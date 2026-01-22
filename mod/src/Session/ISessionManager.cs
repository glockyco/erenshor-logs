using ErenshorLogs.Events;

namespace ErenshorLogs.Session;

/// <summary>
/// Manages combat session lifecycle and state transitions with lazy session creation.
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
  /// Ensures a combat session exists, creating one if needed.
  /// Idempotent - safe to call multiple times.
  /// </summary>
  /// <param name="eventType">The type of event triggering session start.
  /// Environmental damage does not start sessions.</param>
  void EnsureSessionStarted(EventType eventType);

  /// <summary>
  /// Checks if pending session has timed out and ends it if needed.
  /// Should be called from Plugin.Update() with Time.time.
  /// </summary>
  /// <param name="currentTime">Current Unity Time.time value.</param>
  void CheckPendingSessionTimeout(float currentTime);

  /// <summary>
  /// Raised when a new combat session starts.
  /// </summary>
  event Action<CombatSession>? SessionStarted;

  /// <summary>
  /// Raised when a combat session ends.
  /// </summary>
  event Action<CombatSession>? SessionEnded;
}
