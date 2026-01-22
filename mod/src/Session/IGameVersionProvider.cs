namespace ErenshorLogs.Session;

/// <summary>
/// Provides the current game version.
/// </summary>
public interface IGameVersionProvider
{
  /// <summary>
  /// Gets the current game version.
  /// </summary>
  /// <returns>Game version string, or "unknown" if not available.</returns>
  string GetGameVersion();
}
