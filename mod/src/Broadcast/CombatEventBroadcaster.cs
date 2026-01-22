using ErenshorLogs.Config;
using ErenshorLogs.Events;
using ErenshorLogs.Protocol;
using ErenshorLogs.Server;
using ErenshorLogs.Session;

namespace ErenshorLogs.Broadcast;

/// <summary>
/// Manages periodic broadcasting of combat events to WebSocket clients.
/// Batches events for efficiency and handles session boundary messages.
/// </summary>
public sealed class CombatEventBroadcaster : ICombatEventBroadcaster
{
  private readonly IEventEmitter _eventEmitter;
  private readonly ISessionManager _sessionManager;
  private readonly IWebSocketServer _server;
  private readonly ModConfig _config;
  private readonly string _modVersion;
  private readonly Action<string>? _log;

  private readonly List<CombatEvent> _eventQueue = [];
  private readonly object _queueLock = new();

  private IDisposable? _eventSubscription;
  private float _elapsed;
  private bool _disposed;

  /// <summary>
  /// Creates a new CombatEventBroadcaster.
  /// </summary>
  /// <param name="eventEmitter">Event emitter to subscribe to for combat events.</param>
  /// <param name="sessionManager">Session manager for session boundary events.</param>
  /// <param name="server">WebSocket server for broadcasting.</param>
  /// <param name="config">Configuration for broadcast interval.</param>
  /// <param name="modVersion">Mod version string for handshake.</param>
  /// <param name="log">Optional logging callback.</param>
  public CombatEventBroadcaster(
    IEventEmitter eventEmitter,
    ISessionManager sessionManager,
    IWebSocketServer server,
    ModConfig config,
    string modVersion,
    Action<string>? log = null
  )
  {
    _eventEmitter = eventEmitter;
    _sessionManager = sessionManager;
    _server = server;
    _config = config;
    _modVersion = modVersion;
    _log = log;

    // Subscribe to combat events
    _eventSubscription = _eventEmitter.Subscribe(OnCombatEvent);

    // Subscribe to session events
    _sessionManager.SessionStarted += OnSessionStarted;
    _sessionManager.SessionEnded += OnSessionEnded;
  }

  /// <inheritdoc />
  public void Tick(float deltaTime)
  {
    _elapsed += deltaTime;

    // Convert interval from milliseconds to seconds
    var intervalSeconds = _config.BroadcastInterval.Value / 1000f;
    if (_elapsed < intervalSeconds)
      return;

    _elapsed = 0f;
    BroadcastQueuedEvents();
  }

  /// <inheritdoc />
  public void SendHandshakeToNewClient()
  {
    // Skip if no clients connected
    if (_server.ClientCount == 0)
      return;

    try
    {
      var session = _sessionManager.CurrentSession;
      var sessionInfo = session != null ? new SessionInfo(session.Id, session.StartTime) : null;

      var handshake = HandshakeMessage.Create(_modVersion, sessionInfo);
      var json = MessageSerializer.Serialize(handshake);
      _server.Broadcast(json);
    }
    catch (Exception ex)
    {
      _log?.Invoke($"Error sending handshake: {ex.Message}");
    }
  }

  /// <inheritdoc />
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
    // Queue event for next broadcast
    lock (_queueLock)
    {
      _eventQueue.Add(evt);
    }
  }

  private void OnSessionStarted(CombatSession session)
  {
    // Skip if no clients connected
    if (_server.ClientCount == 0)
      return;

    try
    {
      var sessionInfo = new SessionInfo(session.Id, session.StartTime);
      var message = SessionStartMessage.Create(sessionInfo);
      var json = MessageSerializer.Serialize(message);
      _server.Broadcast(json);
    }
    catch (Exception ex)
    {
      _log?.Invoke($"Error broadcasting session start: {ex.Message}");
    }
  }

  private void OnSessionEnded(CombatSession session)
  {
    // Broadcast any remaining events before sending session end
    BroadcastQueuedEvents();

    // Skip if no clients connected
    if (_server.ClientCount == 0)
      return;

    try
    {
      var message = SessionEndMessage.Create(session.Id, session.Duration);
      var json = MessageSerializer.Serialize(message);
      _server.Broadcast(json);
    }
    catch (Exception ex)
    {
      _log?.Invoke($"Error broadcasting session end: {ex.Message}");
    }
  }

  private void BroadcastQueuedEvents()
  {
    // Skip if no clients connected
    if (_server.ClientCount == 0)
    {
      // Clear queue even if no clients - don't accumulate events
      lock (_queueLock)
      {
        _eventQueue.Clear();
      }
      return;
    }

    // Skip if no events queued
    CombatEvent[] events;
    lock (_queueLock)
    {
      if (_eventQueue.Count == 0)
        return;

      events = [.. _eventQueue];
      _eventQueue.Clear();
    }

    // Skip if no active session
    var session = _sessionManager.CurrentSession;
    if (session == null)
      return;

    try
    {
      var message = CombatEventsMessage.Create(session.Id, events);
      var json = MessageSerializer.Serialize(message);
      _server.Broadcast(json);
    }
    catch (Exception ex)
    {
      _log?.Invoke($"Error broadcasting events: {ex.Message}");
    }
  }
}
