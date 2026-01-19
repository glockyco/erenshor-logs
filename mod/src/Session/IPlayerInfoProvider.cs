using ErenshorLogs.Events;

namespace ErenshorLogs.Session;

/// <summary>
/// Provides player information for session metadata.
/// </summary>
public interface IPlayerInfoProvider
{
  /// <summary>
  /// Gets the current player's information.
  /// </summary>
  /// <returns>Player info, or null if not available.</returns>
  PlayerInfo? GetCurrentPlayerInfo();

  /// <summary>
  /// Gets the current game version.
  /// </summary>
  /// <returns>Game version string, or "unknown" if not available.</returns>
  string GetGameVersion();
}
