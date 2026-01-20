namespace ErenshorLogs.Events;

/// <summary>
/// Player information stored in session metadata.
/// </summary>
public sealed record PlayerInfo
{
  /// <summary>Player character name.</summary>
  public required string Name { get; init; }

  /// <summary>Character class (Arcanist, Paladin, Duelist, Druid, Stormcaller).</summary>
  public required string Class { get; init; }

  /// <summary>Character level (1-35).</summary>
  public required int Level { get; init; }
}
