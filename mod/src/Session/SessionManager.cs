using ErenshorLogs.Events;

namespace ErenshorLogs.Session;

/// <summary>
/// Manages combat session lifecycle, emitting events on state transitions.
/// </summary>
public sealed class SessionManager : ISessionManager
{
  private readonly IEventEmitter _emitter;
  private readonly IPlayerInfoProvider _playerInfoProvider;
  private readonly string _modVersion;

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
  /// <param name="playerInfoProvider">Provider for player information.</param>
  /// <param name="modVersion">Current mod version string.</param>
  public SessionManager(
    IEventEmitter emitter,
    IPlayerInfoProvider playerInfoProvider,
    string modVersion
  )
  {
    _emitter = emitter;
    _playerInfoProvider = playerInfoProvider;
    _modVersion = modVersion;
  }

  /// <inheritdoc />
  public void OnCombatStateChanged(bool inCombat)
  {
    if (inCombat == _inCombat)
      return;

    _inCombat = inCombat;

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
    var playerInfo = _playerInfoProvider.GetCurrentPlayerInfo();
    if (playerInfo == null)
    {
      // Can't start session without player info - game not fully initialized
      return;
    }

    var gameVersion = _playerInfoProvider.GetGameVersion();
    _currentSession = new CombatSession(playerInfo, gameVersion, _modVersion);

    EmitCombatEvent(EventType.CombatStart);
    SessionStarted?.Invoke(_currentSession);
  }

  private void EndSession()
  {
    if (_currentSession == null)
      return;

    _currentSession.End();
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
