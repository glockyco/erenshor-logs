using ErenshorLogs.Events;
using UnityEngine;

namespace ErenshorLogs.Session;

/// <summary>
/// Extracts player information from game state.
/// </summary>
public sealed class PlayerInfoProvider : IPlayerInfoProvider
{
  /// <inheritdoc />
  public PlayerInfo? GetCurrentPlayerInfo()
  {
    var stats = GameData.PlayerStats;
    if (stats == null)
      return null;

    return new PlayerInfo
    {
      Name = stats.MyName ?? "Unknown",
      Class = stats.CharacterClass?.name ?? "Unknown",
      Level = stats.Level,
    };
  }

  /// <inheritdoc />
  public string GetGameVersion()
  {
    try
    {
      return Application.version ?? "unknown";
    }
    catch
    {
      return "unknown";
    }
  }
}
