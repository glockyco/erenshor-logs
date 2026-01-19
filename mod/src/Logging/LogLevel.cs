namespace ErenshorLogs.Logging;

/// <summary>
/// Log severity levels.
/// </summary>
public enum LogLevel
{
  /// <summary>
  /// Detailed tracing for debugging.
  /// </summary>
  Debug,

  /// <summary>
  /// Informational messages about normal operation.
  /// </summary>
  Info,

  /// <summary>
  /// Warning about potential issues.
  /// </summary>
  Warning,

  /// <summary>
  /// Error conditions that need attention.
  /// </summary>
  Error,
}
