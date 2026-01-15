using System.Text.Json.Serialization;

namespace ErenshorLogs.Events;

/// <summary>
/// Summary statistics for a single ability.
/// </summary>
public sealed record AbilitySummary
{
  /// <summary>Ability display name.</summary>
  [JsonPropertyName("name")]
  public required string Name { get; init; }

  /// <summary>Total damage dealt by this ability.</summary>
  [JsonPropertyName("damage")]
  public required long Damage { get; init; }

  /// <summary>Number of hits.</summary>
  [JsonPropertyName("hits")]
  public required int Hits { get; init; }
}
