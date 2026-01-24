using ErenshorLogs.Context;
using ErenshorLogs.Events;
using ErenshorLogs.Logging;

namespace ErenshorLogs.Session;

/// <summary>
/// Manages combat session lifecycle with event-driven detection and manual control.
///
/// Sessions can be created in two modes:
/// 1. Automatic: Triggered by configured combat events, end on inactivity timeout
/// 2. Manual: Started/stopped by hotkeys, never timeout
///
/// Session end times are backdated to the last event timestamp for accuracy.
/// </summary>
public sealed class SessionManager : ISessionManager
{
  private readonly IEventEmitter _emitter;
  private readonly IGameVersionProvider _gameVersionProvider;
  private readonly string _modVersion;
  private readonly bool _autoDetectionEnabled;
  private readonly float _inactivityTimeoutSeconds;
  private readonly HashSet<EventType> _sessionStartEvents;
  private readonly HashSet<EventType> _sessionKeepAliveEvents;
  private readonly Action<LogLevel, string>? _log;

  private CombatSession? _currentSession;
  private float? _lastEventTime; // Unity Time.time of last keep-alive event
  private long? _lastEventTimestamp; // Unix timestamp (ms) of last keep-alive event

  /// <inheritdoc />
  public CombatSession? CurrentSession => _currentSession;

  /// <inheritdoc />
  public event Action<CombatSession>? SessionStarted;

  /// <inheritdoc />
  public event Action<CombatSession>? SessionEnded;

  /// <summary>
  /// Creates a new SessionManager.
  /// </summary>
  /// <param name="emitter">Event emitter for CombatStart/CombatEnd events.</param>
  /// <param name="gameVersionProvider">Provider for game version information.</param>
  /// <param name="modVersion">Current mod version string.</param>
  /// <param name="autoDetectionEnabled">Whether automatic session detection is enabled.</param>
  /// <param name="inactivityTimeoutSeconds">Seconds of inactivity before ending automatic sessions.</param>
  /// <param name="sessionStartEvents">Comma-separated list of event types that can start sessions.</param>
  /// <param name="sessionKeepAliveEvents">Comma-separated list of event types that extend sessions.</param>
  /// <param name="log">Optional logging callback.</param>
  public SessionManager(
    IEventEmitter emitter,
    IGameVersionProvider gameVersionProvider,
    string modVersion,
    bool autoDetectionEnabled,
    float inactivityTimeoutSeconds,
    string sessionStartEvents,
    string sessionKeepAliveEvents,
    Action<LogLevel, string>? log = null
  )
  {
    _emitter = emitter;
    _gameVersionProvider = gameVersionProvider;
    _modVersion = modVersion;
    _autoDetectionEnabled = autoDetectionEnabled;
    _inactivityTimeoutSeconds = inactivityTimeoutSeconds;
    _log = log;

    _sessionStartEvents = ParseEventTypes(sessionStartEvents, "SessionStartEvents");
    _sessionKeepAliveEvents = ParseEventTypes(sessionKeepAliveEvents, "SessionKeepAliveEvents");

    ValidateConfiguration();
  }

  /// <inheritdoc />
  public void OnCombatEvent(EventType eventType, long eventTimestamp)
  {
    // Check if this event keeps session alive
    if (_sessionKeepAliveEvents.Contains(eventType))
    {
      _lastEventTime = UnityEngine.Time.time;
      _lastEventTimestamp = eventTimestamp;
    }

    // Check if this event can start a session (auto mode only)
    if (_autoDetectionEnabled && _currentSession == null && _sessionStartEvents.Contains(eventType))
    {
      _log?.Invoke(LogLevel.Debug, $"Auto-starting session from event: {eventType}");
      StartSession(isManual: false);
    }
  }

  /// <inheritdoc />
  public void CheckInactivityTimeout(float currentTime)
  {
    // Only check automatic sessions
    if (_currentSession == null || _currentSession.IsManual)
      return;

    // Only check if we have event timing data
    if (_lastEventTime == null)
      return;

    float elapsed = currentTime - _lastEventTime.Value;
    if (elapsed >= _inactivityTimeoutSeconds)
    {
      _log?.Invoke(
        LogLevel.Info,
        $"Session {_currentSession.Id} ended after {elapsed:F1}s inactivity"
      );

      // Backdate end time to last event
      EndSession(_lastEventTimestamp);
    }
  }

  /// <inheritdoc />
  public void StartManualSession()
  {
    // End ANY existing session (manual or automatic)
    if (_currentSession != null)
    {
      var sessionType = _currentSession.IsManual ? "manual" : "automatic";
      _log?.Invoke(
        LogLevel.Info,
        $"Ending existing {sessionType} session {_currentSession.Id} for manual start"
      );
      EndSession(null); // End at current time
    }

    _log?.Invoke(LogLevel.Info, "Starting manual session via hotkey");
    StartSession(isManual: true);
  }

  /// <inheritdoc />
  public void EndManualSession()
  {
    if (_currentSession == null)
    {
      _log?.Invoke(LogLevel.Debug, "No active session to end");
      return;
    }

    // Only allow ending manual sessions
    if (!_currentSession.IsManual)
    {
      _log?.Invoke(
        LogLevel.Warning,
        $"Cannot manually stop automatic session {_currentSession.Id}. "
          + "Use manual start to begin a new session, or wait for inactivity timeout."
      );
      return;
    }

    _log?.Invoke(LogLevel.Info, $"Manually ending session {_currentSession.Id} via hotkey");
    EndSession(null); // End at current time
  }

  private void StartSession(bool isManual)
  {
    var gameVersion = _gameVersionProvider.GetGameVersion();
    _currentSession = new CombatSession(gameVersion, _modVersion, isManual);

    _log?.Invoke(
      LogLevel.Info,
      $"{(isManual ? "Manual" : "Automatic")} session started: {_currentSession.Id}"
    );

    SessionStarted?.Invoke(_currentSession);
    EmitCombatEvent(EventType.CombatStart);
  }

  private void EndSession(long? backdatedEndTime)
  {
    if (_currentSession == null)
      return;

    if (backdatedEndTime.HasValue)
    {
      _currentSession.EndAt(backdatedEndTime.Value);
    }
    else
    {
      _currentSession.End(); // Use current time
    }

    _log?.Invoke(
      LogLevel.Info,
      $"Session ended: {_currentSession.Id} (Duration: {_currentSession.Duration}ms)"
    );

    EmitCombatEvent(EventType.CombatEnd);
    SessionEnded?.Invoke(_currentSession);

    // Clear all state
    _currentSession = null;
    _lastEventTime = null;
    _lastEventTimestamp = null;
  }

  private void EmitCombatEvent(EventType eventType)
  {
    var evt = new CombatEvent
    {
      Id = Guid.NewGuid().ToString(),
      Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
      EventType = eventType,
      Ability = AbilityResolver.CreateFixed(
        eventType == EventType.CombatStart ? "Combat Start" : "Combat End",
        AbilityType.Unknown
      ),
    };
    _emitter.Emit(evt);
  }

  private HashSet<EventType> ParseEventTypes(string configValue, string configName)
  {
    var result = new HashSet<EventType>();

    if (string.IsNullOrWhiteSpace(configValue))
    {
      _log?.Invoke(LogLevel.Warning, $"Config '{configName}' is empty - no events configured");
      return result;
    }

    var parts = configValue.Split(',', StringSplitOptions.RemoveEmptyEntries);

    foreach (var part in parts)
    {
      var trimmed = part.Trim();

      // Try parsing case-insensitively
      if (Enum.TryParse<EventType>(trimmed, ignoreCase: true, out var eventType))
      {
        result.Add(eventType);
      }
      else
      {
        _log?.Invoke(
          LogLevel.Warning,
          $"Unknown event type in {configName}: '{trimmed}' (ignoring)"
        );
      }
    }

    return result;
  }

  private void ValidateConfiguration()
  {
    // Log configuration summary
    _log?.Invoke(
      LogLevel.Info,
      $"Session auto-detection: {(_autoDetectionEnabled ? "enabled" : "disabled")}"
    );

    if (_autoDetectionEnabled)
    {
      _log?.Invoke(
        LogLevel.Info,
        $"Session start events: {string.Join(", ", _sessionStartEvents)}"
      );
      _log?.Invoke(
        LogLevel.Info,
        $"Session keep-alive events: {string.Join(", ", _sessionKeepAliveEvents)}"
      );
      _log?.Invoke(LogLevel.Info, $"Inactivity timeout: {_inactivityTimeoutSeconds}s");
    }

    // Warn if start events not in keep-alive events
    var startNotInKeepAlive = _sessionStartEvents.Except(_sessionKeepAliveEvents).ToList();
    if (startNotInKeepAlive.Any())
    {
      _log?.Invoke(
        LogLevel.Warning,
        $"SessionStartEvents contains events not in SessionKeepAliveEvents: "
          + $"{string.Join(", ", startNotInKeepAlive)}. "
          + "Sessions may end immediately after starting."
      );
    }
  }
}
