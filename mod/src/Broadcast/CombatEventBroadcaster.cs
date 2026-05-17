using ErenshorLogs.Config;
using ErenshorLogs.Events;
using ErenshorLogs.Protocol;
using ErenshorLogs.Server;
using ErenshorLogs.Session;
using Newtonsoft.Json.Linq;

namespace ErenshorLogs.Broadcast;

/// <summary>
/// Manages periodic broadcasting of protocol v2 frames to WebSocket clients.
/// </summary>
public sealed class CombatEventBroadcaster : ICombatEventBroadcaster
{
  private readonly IEventEmitter _eventEmitter;
  private readonly ISessionManager _sessionManager;
  private readonly IWebSocketServer _server;
  private readonly Func<int> _getBroadcastIntervalMs;
  private readonly string _modVersion;
  private readonly Action<string>? _log;

  private readonly List<JObject> _eventQueue = [];
  private readonly object _queueLock = new();
  private readonly Dictionary<string, ProtocolSessionState> _sessionStates = new();

  private IDisposable? _eventSubscription;
  private long _frameSeq;
  private float _elapsed;
  private bool _disposed;

  public CombatEventBroadcaster(
    IEventEmitter eventEmitter,
    ISessionManager sessionManager,
    IWebSocketServer server,
    ModConfig config,
    string modVersion,
    Action<string>? log = null
  )
    : this(
      eventEmitter,
      sessionManager,
      server,
      () => config.BroadcastInterval.Value,
      modVersion,
      log
    ) { }

  public CombatEventBroadcaster(
    IEventEmitter eventEmitter,
    ISessionManager sessionManager,
    IWebSocketServer server,
    int broadcastIntervalMs,
    string modVersion,
    Action<string>? log = null
  )
    : this(eventEmitter, sessionManager, server, () => broadcastIntervalMs, modVersion, log) { }

  private CombatEventBroadcaster(
    IEventEmitter eventEmitter,
    ISessionManager sessionManager,
    IWebSocketServer server,
    Func<int> getBroadcastIntervalMs,
    string modVersion,
    Action<string>? log
  )
  {
    _eventEmitter = eventEmitter;
    _sessionManager = sessionManager;
    _server = server;
    _getBroadcastIntervalMs = getBroadcastIntervalMs;
    _modVersion = modVersion;
    _log = log;

    _eventSubscription = _eventEmitter.Subscribe(OnCombatEvent);
    _sessionManager.SessionStarted += OnSessionStarted;
    _sessionManager.SessionEnded += OnSessionEnded;
  }

  public void Tick(float deltaTime)
  {
    _elapsed += deltaTime;

    var intervalSeconds = _getBroadcastIntervalMs() / 1000f;
    if (_elapsed < intervalSeconds)
      return;

    _elapsed = 0f;
    BroadcastQueuedEvents();
  }

  public void SendHandshakeToNewClient()
  {
    if (_server.ClientCount == 0)
      return;

    try
    {
      BroadcastEnvelope(
        "hello",
        null,
        new HelloPayload
        {
          Producer = new ProducerInfo { Name = "ErenshorLogsMod", ModVersion = _modVersion },
          ActiveSessionId = _sessionManager.CurrentSession?.Id,
          Capabilities = ["sessionSnapshot", "registryDelta"],
        }
      );

      var currentSession = _sessionManager.CurrentSession;
      if (currentSession != null && _sessionStates.TryGetValue(currentSession.Id, out var state))
      {
        BroadcastEnvelope("sessionSnapshot", currentSession.Id, state.CreateSnapshot());

        if (state.Events.Count > 0)
        {
          BroadcastEnvelope(
            "events",
            currentSession.Id,
            state.CreateEventsPayload([.. state.Events])
          );
        }
      }
    }
    catch (Exception ex)
    {
      _log?.Invoke($"Error sending handshake: {ex.Message}");
    }
  }

  public void Dispose()
  {
    if (_disposed)
      return;

    _eventSubscription?.Dispose();
    _sessionManager.SessionStarted -= OnSessionStarted;
    _sessionManager.SessionEnded -= OnSessionEnded;

    _disposed = true;
  }

  private void OnCombatEvent(CombatEvent evt)
  {
    var session = _sessionManager.CurrentSession;
    if (session == null)
      return;

    var state = GetOrCreateState(session);
    var protocolEvent = state.Append(evt);
    if (protocolEvent == null)
      return;

    lock (_queueLock)
    {
      _eventQueue.Add(protocolEvent);
    }
  }

  private void OnSessionStarted(CombatSession session)
  {
    var state = GetOrCreateState(session);

    if (_server.ClientCount == 0)
      return;

    try
    {
      BroadcastEnvelope("sessionSnapshot", session.Id, state.CreateSnapshot());
    }
    catch (Exception ex)
    {
      _log?.Invoke($"Error broadcasting session snapshot: {ex.Message}");
    }
  }

  private void OnSessionEnded(CombatSession session)
  {
    BroadcastQueuedEvents();

    if (_server.ClientCount == 0)
      return;

    try
    {
      var durationMs = session.EndTime.HasValue
        ? session.EndTime.Value - session.StartTime
        : session.Duration;
      BroadcastEnvelope(
        "sessionEnded",
        session.Id,
        new SessionEndedPayload
        {
          SessionId = session.Id,
          EndedAtUtcMs = session.EndTime ?? DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
          EndedAtEventSeq = GetOrCreateState(session).LastEventSeq,
          Reason = "inactivity",
          DurationMs = durationMs,
          Diagnostics = new SessionDiagnostics
          {
            HookWarnings = [],
            AttributionFailures = 0,
            DroppedEvents = 0,
            DroppedFrames = 0,
            SerializationErrors = 0,
          },
        }
      );
    }
    catch (Exception ex)
    {
      _log?.Invoke($"Error broadcasting session end: {ex.Message}");
    }
  }

  private void BroadcastQueuedEvents()
  {
    if (_server.ClientCount == 0)
      return;

    JObject[] events;
    lock (_queueLock)
    {
      if (_eventQueue.Count == 0)
        return;

      events = [.. _eventQueue];
      _eventQueue.Clear();
    }

    var session = _sessionManager.CurrentSession;
    if (session == null)
      return;

    try
    {
      var state = GetOrCreateState(session);
      BroadcastEnvelope("events", session.Id, state.CreateEventsPayload(events));
    }
    catch (Exception ex)
    {
      _log?.Invoke($"Error broadcasting events: {ex.Message}");
    }
  }

  private ProtocolSessionState GetOrCreateState(CombatSession session)
  {
    if (_sessionStates.TryGetValue(session.Id, out var state))
      return state;

    state = new ProtocolSessionState(session);
    _sessionStates.Add(session.Id, state);
    return state;
  }

  private void BroadcastEnvelope(string kind, string? sessionId, object payload)
  {
    var envelope = new LiveEnvelope
    {
      Kind = kind,
      FrameSeq = ++_frameSeq,
      SessionId = sessionId,
      SentAtMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
      Payload = payload,
    };

    _server.Broadcast(MessageSerializer.Serialize(envelope));
  }
}
