using ErenshorLogs.Events;
using ErenshorLogs.Logging;

namespace ErenshorLogs.Session;

/// <summary>
/// Manages combat session lifecycle, emitting events on state transitions.
/// </summary>
public sealed class SessionManager : ISessionManager
{
  private readonly IEventEmitter _emitter;
  private readonly IGameVersionProvider _gameVersionProvider;
  private readonly string _modVersion;
  private readonly Action<LogLevel, string>? _log;

  private bool _inCombat;
  private CombatSession? _currentSession;

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
  public void OnCombatStateChanged(bool inCombat)
  {
    if (inCombat == _inCombat)
      return;

    _inCombat = inCombat;
    _log?.Invoke(
      LogLevel.Debug,
      $"Combat state changed: {(inCombat ? "entering combat" : "exiting combat")}"
    );

    if (inCombat)
    {
      StartSession();
    }
    else
    {
      EndSession();
    }
  }

  private void StartSession()
  {
    var gameVersion = _gameVersionProvider.GetGameVersion();
    _currentSession = new CombatSession(gameVersion, _modVersion);

    _log?.Invoke(LogLevel.Info, $"Combat session started: {_currentSession.Id}");

    EmitCombatEvent(EventType.CombatStart);
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
    };
    _emitter.Emit(evt);
  }
}
