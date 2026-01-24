using ErenshorLogs.Events;

namespace ErenshorLogs.Session;

/// <summary>
/// Manages combat session lifecycle with event-driven detection and manual control.
/// </summary>
public interface ISessionManager
{
  /// <summary>
  /// The currently active combat session, or null if not in combat.
  /// </summary>
  CombatSession? CurrentSession { get; }

  /// <summary>
  /// Notifies the session manager that a combat event occurred.
  /// Creates a new session if needed (auto-detection mode only).
  /// Updates last event timestamp for inactivity tracking.
  /// </summary>
  /// <param name="eventType">The type of combat event that occurred.</param>
  /// <param name="eventTimestamp">Unix timestamp (ms) of the event.</param>
  void OnCombatEvent(EventType eventType, long eventTimestamp);

  /// <summary>
  /// Checks for session inactivity timeout and ends sessions if needed.
  /// Should be called every frame from Plugin.Update() with Time.time.
  /// Only affects automatic sessions (manual sessions don't timeout).
  /// </summary>
  /// <param name="currentTime">Current Unity Time.time value.</param>
  void CheckInactivityTimeout(float currentTime);

  /// <summary>
  /// Manually starts a new combat session.
  /// If a session is already active, it will be ended first (manual or automatic).
  /// </summary>
  void StartManualSession();

  /// <summary>
  /// Manually ends the current combat session.
  /// Only ends manually-started sessions. Does nothing for automatic sessions.
  /// </summary>
  void EndManualSession();

  /// <summary>
  /// Raised when a new combat session starts (automatic or manual).
  /// </summary>
  event Action<CombatSession>? SessionStarted;

  /// <summary>
  /// Raised when a combat session ends (timeout or manual).
  /// </summary>
  event Action<CombatSession>? SessionEnded;
}
