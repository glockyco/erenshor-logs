using ErenshorLogs.Config;
using ErenshorLogs.Diagnostics;
using ErenshorLogs.Events;
using ErenshorLogs.Hooks;
using ErenshorLogs.Protocol;
using ErenshorLogs.Server;
using ErenshorLogs.Session;
using Newtonsoft.Json.Linq;

namespace ErenshorLogs.Broadcast;

/// <summary>
/// Manages periodic broadcasting of protocol v3 frames to WebSocket clients.
/// </summary>
public sealed class CombatEventBroadcaster : ICombatEventBroadcaster
{
  private const int MaxEventsPerBatch = 256;
  private readonly IEventEmitter _eventEmitter;
  private readonly ISessionManager _sessionManager;
  private readonly IWebSocketServer _server;
  private readonly Func<int> _getBroadcastIntervalMs;
  private readonly string _modVersion;
  private readonly Action<string>? _log;
  private readonly IDiagnosticReporter _diagnostics;
  private IReadOnlyList<PatchStatus> _patchStatuses = [];
  private string _captureHealthStatus = "healthy";

  private readonly List<JObject> _eventQueue = [];
  private readonly object _queueLock = new();
  private readonly Dictionary<string, ProtocolSessionState> _sessionStates = new();

  private IDisposable? _eventSubscription;
  private long _frameSeq;
  private float _elapsed;
  private float _statsElapsed;
  private bool _disposed;

  public CombatEventBroadcaster(
    IEventEmitter eventEmitter,
    ISessionManager sessionManager,
    IWebSocketServer server,
    ModConfig config,
    string modVersion,
    Action<string>? log = null,
    IDiagnosticReporter? reporter = null
  )
    : this(
      eventEmitter,
      sessionManager,
      server,
      () => config.BroadcastInterval.Value,
      modVersion,
      log,
      reporter
    ) { }

  public CombatEventBroadcaster(
    IEventEmitter eventEmitter,
    ISessionManager sessionManager,
    IWebSocketServer server,
    int broadcastIntervalMs,
    string modVersion,
    Action<string>? log = null,
    IDiagnosticReporter? reporter = null
  )
    : this(
      eventEmitter,
      sessionManager,
      server,
      () => broadcastIntervalMs,
      modVersion,
      log,
      reporter
    ) { }

  private CombatEventBroadcaster(
    IEventEmitter eventEmitter,
    ISessionManager sessionManager,
    IWebSocketServer server,
    Func<int> getBroadcastIntervalMs,
    string modVersion,
    Action<string>? log,
    IDiagnosticReporter? reporter
  )
  {
    _eventEmitter = eventEmitter;
    _sessionManager = sessionManager;
    _server = server;
    _getBroadcastIntervalMs = getBroadcastIntervalMs;
    _modVersion = modVersion;
    _log = log;
    _diagnostics = reporter ?? new DiagnosticReporter(log);

    _eventSubscription = _eventEmitter.Subscribe(OnCombatEvent);
    _sessionManager.SessionStarted += OnSessionStarted;
    _sessionManager.SessionEnded += OnSessionEnded;
  }

  public void Tick(float deltaTime)
  {
    _elapsed += deltaTime;
    _statsElapsed += deltaTime;

    var intervalSeconds = _getBroadcastIntervalMs() / 1000f;
    if (_elapsed >= intervalSeconds)
    {
      _elapsed = 0f;
      BroadcastQueuedEvents();
    }

    if (_statsElapsed >= 5.0f)
    {
      _statsElapsed = 0f;
      BroadcastStats();
    }
  }

  public void SendHandshakeToNewClient(IWebSocketClient client)
  {
    try
    {
      SendEnvelope(client, "hello", null, CreateHelloPayload());

      SendEnvelope(client, "stats", null, CreateStatsPayload());

      var currentSession = _sessionManager.CurrentSession;
      if (currentSession != null && _sessionStates.TryGetValue(currentSession.Id, out var state))
      {
        SendEnvelope(client, "sessionOpened", currentSession.Id, state.CreateSnapshot());

        if (state.Events.Count > 0)
          SendEventBatches(client, currentSession.Id, state);
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

  public void SetPatchManifestResult(PatchManifestResult result)
  {
    _patchStatuses = result.Statuses;
    _captureHealthStatus = result.HealthStatus;
  }

  private void OnCombatEvent(CombatEvent evt)
  {
    var session = _sessionManager.CurrentSession;
    if (session == null)
      return;

    _diagnostics.Counters.CapturedEvents += 1;
    var state = GetOrCreateState(session);
    if (!state.TryAppend(evt, out var protocolEvent, out var errorPath))
    {
      if (errorPath != null)
      {
        _diagnostics.ReportProjectionError(
          new InvalidOperationException("Protocol projection failed validation."),
          session.Id,
          evt.EventType.ToString(),
          errorPath
        );
      }
      state.RecordDroppedEvent();
      return;
    }

    _diagnostics.Counters.ProjectedEvents += 1;
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
      BroadcastEnvelope("sessionOpened", session.Id, state.CreateSnapshot());
    }
    catch (Exception ex)
    {
      _log?.Invoke($"Error broadcasting session snapshot: {ex.Message}");
    }
  }

  private void OnSessionEnded(CombatSession session, string reason)
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
        "sessionClosed",
        session.Id,
        new SessionEndedPayload
        {
          SessionId = session.Id,
          EndedAtUtcMs = session.EndTime ?? DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
          EndedAtEventSeq = GetOrCreateState(session).LastEventSeq,
          Reason = reason,
          DurationMs = durationMs,
          Diagnostics = GetOrCreateState(session).CreateDiagnostics(),
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
    {
      ClearEventQueue();
      return;
    }

    JObject[] events;
    var hadQueuedEvents = false;
    lock (_queueLock)
    {
      if (_eventQueue.Count > 0)
      {
        hadQueuedEvents = true;
        events = [.. _eventQueue];
        _eventQueue.Clear();
      }
      else
      {
        events = [];
      }
    }

    var session = _sessionManager.CurrentSession;
    if (session == null)
      return;

    var state = GetOrCreateState(session);
    if (hadQueuedEvents)
    {
      try
      {
        BroadcastRegistryDelta(session.Id, state);
        BroadcastEnvelope("eventBatch", session.Id, state.CreateEventsPayload(events));
        _diagnostics.Counters.SentEvents += events.Length;
      }
      catch (Exception ex)
      {
        _diagnostics.ReportSerializationError(ex, "broadcastEvents", session.Id);
        _log?.Invoke($"Error broadcasting events: {ex.Message}");
      }
    }

    BroadcastDiagnostics(session.Id);
  }

  private void BroadcastRegistryDelta(string sessionId, ProtocolSessionState state)
  {
    if (state.RegistryRevision == 0)
      return;

    BroadcastEnvelope(
      "registryDelta",
      sessionId,
      new RegistryDeltaPayload
      {
        Revision = state.RegistryRevision,
        Actors = state.Registries.Actors,
        Abilities = state.Registries.Abilities,
        Effects = state.Registries.Effects,
      }
    );
  }

  private void BroadcastDiagnostics(string? sessionId)
  {
    var diagnostics = _diagnostics.DrainPendingDiagnostics(maxCount: 4);
    if (diagnostics.Count == 0)
      return;

    try
    {
      BroadcastEnvelope(
        "diagnosticBatch",
        sessionId,
        new DiagnosticBatchPayload { Diagnostics = diagnostics }
      );
    }
    catch (Exception ex)
    {
      _diagnostics.Counters.SerializationErrors += 1;
      _log?.Invoke($"Error broadcasting diagnostics: {ex.Message}");
    }
  }

  private void BroadcastStats()
  {
    if (_server.ClientCount == 0)
      return;

    try
    {
      BroadcastEnvelope("stats", null, CreateStatsPayload());
    }
    catch (Exception ex)
    {
      _diagnostics.ReportSerializationError(ex, "broadcastStats");
      _log?.Invoke($"Error broadcasting stats: {ex.Message}");
    }
  }

  private void ClearEventQueue()
  {
    lock (_queueLock)
    {
      _eventQueue.Clear();
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
    _server.Broadcast(SerializeEnvelope(kind, sessionId, payload));
    _diagnostics.Counters.SentFrames += 1;
  }

  private void SendEnvelope(IWebSocketClient client, string kind, string? sessionId, object payload)
  {
    client.Send(SerializeEnvelope(kind, sessionId, payload));
  }

  private string SerializeEnvelope(string kind, string? sessionId, object payload)
  {
    var envelope = new LiveEnvelope
    {
      Kind = kind,
      FrameId = ++_frameSeq,
      SessionId = sessionId,
      SentAtMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
      Producer = CreateProducer(),
      Payload = payload,
    };

    return MessageSerializer.Serialize(envelope);
  }

  private void SendEventBatches(
    IWebSocketClient client,
    string sessionId,
    ProtocolSessionState state
  )
  {
    for (var index = 0; index < state.Events.Count; index += MaxEventsPerBatch)
    {
      var batch = state.Events.Skip(index).Take(MaxEventsPerBatch).ToArray();
      SendEnvelope(client, "eventBatch", sessionId, state.CreateEventsPayload(batch));
    }
  }

  private StatsPayload CreateStatsPayload()
  {
    var counters = _diagnostics.Counters;
    var currentState =
      _sessionManager.CurrentSession == null
        ? null
        : GetOrCreateState(_sessionManager.CurrentSession);
    return new StatsPayload
    {
      UptimeMs = 0,
      ConnectedClients = _server.ClientCount,
      CapturedEvents = counters.CapturedEvents,
      ProjectedEvents = counters.ProjectedEvents,
      SentEvents = counters.SentEvents,
      SentFrames = counters.SentFrames,
      DroppedEvents = counters.DroppedEvents,
      DroppedFrames = counters.DroppedFrames,
      ProjectionErrors = counters.ProjectionErrors,
      SerializationErrors = counters.SerializationErrors,
      ClientSendErrors = counters.ClientSendErrors,
      HookWarnings = counters.HookWarnings,
      AttributionFailures = counters.AttributionFailures,
      DiagnosticsEmitted = counters.DiagnosticsEmitted,
      DiagnosticsSuppressed = counters.DiagnosticsSuppressed,
      QueueDepth = GetQueueDepth(),
      RegistryRevision = currentState?.RegistryRevision ?? 0,
      HealthStatus =
        counters.DroppedEvents > 0
        || counters.DroppedFrames > 0
        || counters.ProjectionErrors > 0
        || counters.SerializationErrors > 0
          ? "degraded"
          : "healthy",
    };
  }

  private int GetQueueDepth()
  {
    lock (_queueLock)
    {
      return _eventQueue.Count;
    }
  }

  private HelloPayload CreateHelloPayload() =>
    new()
    {
      ActiveSessionId = _sessionManager.CurrentSession?.Id,
      Capabilities = ["eventBatch", "diagnosticBatch", "stats"],
      RequiredWebCapabilities = ["protocolV3"],
      Health = new HealthPayload
      {
        Status = _captureHealthStatus,
        CaptureAvailable = _captureHealthStatus != "fatal",
      },
      Patches = _patchStatuses
        .Select(status => new PatchStatusPayload
        {
          Id = status.Id,
          Required = status.Required,
          Status = status.Status,
        })
        .ToArray(),
      Limits = new LimitsPayload
      {
        MaxFrameBytes = 262144,
        MaxEventsPerBatch = 256,
        DiagnosticRingSize = 32,
      },
      DiagnosticSummary = new SeverityCountsPayload
      {
        Fatal = 0,
        Error = 0,
        Warning = 0,
        Info = 0,
      },
    };

  private ProducerInfo CreateProducer() =>
    new()
    {
      Name = "ErenshorLogsMod",
      ModVersion = _modVersion,
      GameVersion = "playtest",
    };

  private static SessionDiagnostics CreateEmptyDiagnostics() =>
    new()
    {
      HookWarnings = [],
      AttributionFailures = 0,
      DroppedEvents = 0,
      DroppedFrames = 0,
      SerializationErrors = 0,
    };
}
