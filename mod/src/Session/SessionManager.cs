using ErenshorLogs.Context;
using ErenshorLogs.Events;
using ErenshorLogs.Logging;

namespace ErenshorLogs.Session;

/// <summary>
/// Manages combat session lifecycle with lazy session creation.
///
/// Sessions start when the first combat event occurs (lazy start) rather than
/// waiting for the game's combat state confirmation. This ensures all events,
/// including the first attack, are captured within a session context.
///
/// Unconfirmed sessions timeout after 1 second and are automatically cleaned up
/// to prevent false positives from non-combat events.
/// </summary>
public sealed class SessionManager : ISessionManager
{
  private readonly IEventEmitter _emitter;
  private readonly IGameVersionProvider _gameVersionProvider;
  private readonly string _modVersion;
  private readonly Action<LogLevel, string>? _log;

  private bool _inCombat;
  private CombatSession? _currentSession;

  /// <summary>
  /// Timeout duration for pending sessions awaiting combat confirmation.
  /// If CheckForTrueCombat doesn't confirm within this time, session is ended.
  /// </summary>
  private const float PENDING_SESSION_TIMEOUT_SECONDS = 1.0f;

  /// <summary>
  /// Unity Time.time when pending session was created.
  /// Null when session is confirmed or no session exists.
  /// </summary>
  private float? _pendingSessionStartTime;

  /// <inheritdoc />
  public CombatSession? CurrentSession => _currentSession;

  /// <inheritdoc />
  public bool InCombat => _inCombat;

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
  /// <param name="log">Optional logging callback.</param>
  public SessionManager(
    IEventEmitter emitter,
    IGameVersionProvider gameVersionProvider,
    string modVersion,
    Action<LogLevel, string>? log = null
  )
  {
    _emitter = emitter;
    _gameVersionProvider = gameVersionProvider;
    _modVersion = modVersion;
    _log = log;
  }

  /// <inheritdoc />
  public void EnsureSessionStarted(EventType eventType)
  {
    // Don't start sessions for environmental damage (fall damage, etc.)
    if (eventType == EventType.DamageEnvironmental)
      return;

    // Idempotent - session already exists
    if (_currentSession != null)
      return;

    _log?.Invoke(LogLevel.Debug, "Starting session from combat event (lazy start)");

    StartSession();

    // Start timeout timer - will be cleared when combat state confirms
    _pendingSessionStartTime = UnityEngine.Time.time;

    // Emit CombatStart immediately for clean event ordering
    EmitCombatEvent(EventType.CombatStart);
  }

  /// <inheritdoc />
  public void CheckPendingSessionTimeout(float currentTime)
  {
    // No pending session to check
    if (_pendingSessionStartTime == null)
      return;

    // Session doesn't exist anymore (shouldn't happen, defensive)
    if (_currentSession == null)
    {
      _pendingSessionStartTime = null;
      return;
    }

    // Check if timeout elapsed
    float elapsed = currentTime - _pendingSessionStartTime.Value;
    if (elapsed >= PENDING_SESSION_TIMEOUT_SECONDS)
    {
      _log?.Invoke(
        LogLevel.Warning,
        $"Session {_currentSession.Id} timed out without combat confirmation, ending"
      );

      _pendingSessionStartTime = null;
      EndSession();
    }
  }

  /// <inheritdoc />
  public void OnCombatStateChanged(bool inCombat)
  {
    if (inCombat)
    {
      // Create session if it doesn't exist (shouldn't happen with lazy start)
      if (_currentSession == null)
      {
        _log?.Invoke(LogLevel.Warning, "Session started from combat state (expected lazy start)");
        StartSession();
        EmitCombatEvent(EventType.CombatStart);
      }
      else
      {
        // Session exists (lazy start) - this is the confirmation
        _log?.Invoke(LogLevel.Debug, "Combat state confirmed lazy-started session");
      }

      // Clear pending timeout - session is confirmed
      _pendingSessionStartTime = null;
      _inCombat = true;
    }
    else
    {
      // Combat ended - clean up session if exists
      if (_currentSession != null)
      {
        _inCombat = false;
        _pendingSessionStartTime = null;
        EndSession();
      }
    }
  }

  private void StartSession()
  {
    var gameVersion = _gameVersionProvider.GetGameVersion();
    _currentSession = new CombatSession(gameVersion, _modVersion);

    _log?.Invoke(LogLevel.Info, $"Combat session started: {_currentSession.Id}");

    SessionStarted?.Invoke(_currentSession);
  }

  private void EndSession()
  {
    if (_currentSession == null)
      return;

    _currentSession.End();

    _log?.Invoke(
      LogLevel.Info,
      $"Combat session ended: {_currentSession.Id} " + $"(Duration: {_currentSession.Duration}ms)"
    );

    EmitCombatEvent(EventType.CombatEnd);
    SessionEnded?.Invoke(_currentSession);
    _currentSession = null;
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
}
