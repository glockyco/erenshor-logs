using System.Text.Json.Serialization;

namespace ErenshorLogs.Events;

/// <summary>
/// Reference to an actor (player, NPC, pet, etc.) in a combat event.
/// </summary>
public sealed record ActorRef
{
  /// <summary>Stable identifier (type:instanceId).</summary>
  [JsonPropertyName("id")]
  public required string Id { get; init; }

  /// <summary>Display name.</summary>
  [JsonPropertyName("name")]
  public required string Name { get; init; }

  /// <summary>Actor type.</summary>
  [JsonPropertyName("type")]
  public required ActorType Type { get; init; }

  /// <summary>Character class (Arcanist, Paladin, etc.). Null for NPCs.</summary>
  [JsonPropertyName("class")]
  public string? Class { get; init; }

  /// <summary>Character level (1-35). Null if unknown.</summary>
  [JsonPropertyName("level")]
  public int? Level { get; init; }

  /// <summary>Owner's ID for pets. Null for non-pets.</summary>
  [JsonPropertyName("masterId")]
  public string? MasterId { get; init; }
}
