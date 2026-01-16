namespace ErenshorLogs.Registry;

/// <summary>
/// Raw data extracted from a game character, before conversion to ActorRef.
/// </summary>
public sealed record ActorData
{
  /// <summary>Display name of the character.</summary>
  public required string Name { get; init; }

  /// <summary>Character class name (e.g., "Duelist", "Arcanist"). Null for NPCs.</summary>
  public string? ClassName { get; init; }

  /// <summary>Character level (1-35). Null if unknown.</summary>
  public int? Level { get; init; }

  /// <summary>
  /// Instance ID of the master character, if this is a pet.
  /// Used to establish the pet-owner relationship.
  /// </summary>
  public int? MasterInstanceId { get; init; }
}
