namespace ErenshorLogs.Session;

/// <summary>
/// Manages combat session lifecycle and state transitions.
/// </summary>
public interface ISessionManager
{
  /// <summary>
  /// The currently active combat session, or null if not in combat.
  /// </summary>
  CombatSession? CurrentSession { get; }

  /// <summary>
  /// Whether currently in combat.
  /// </summary>
  bool InCombat { get; }

  /// <summary>
  /// Called when combat state changes. Handles session start/end transitions.
  /// </summary>
  /// <param name="inCombat">The new combat state.</param>
  void OnCombatStateChanged(bool inCombat);

  /// <summary>
  /// Raised when a new combat session starts.
  /// </summary>
  event Action<CombatSession>? SessionStarted;

  /// <summary>
  /// Raised when a combat session ends.
  /// </summary>
  event Action<CombatSession>? SessionEnded;
}
