namespace ErenshorLogs.Session;

/// <summary>
/// Provides the current time for session management.
/// Abstracted to enable unit testing without Unity dependencies.
/// </summary>
public interface ITimeProvider
{
  /// <summary>
  /// Gets the current time in seconds since application start.
  /// Equivalent to Unity's Time.time in production.
  /// </summary>
  float CurrentTime { get; }
}
