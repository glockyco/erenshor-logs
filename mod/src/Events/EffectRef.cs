using System.Text.Json.Serialization;

namespace ErenshorLogs.Events;

/// <summary>
/// Reference to a status effect (buff/debuff) in a combat event.
/// </summary>
public sealed record EffectRef
{
  /// <summary>Effect display name.</summary>
  [JsonPropertyName("name")]
  public required string Name { get; init; }

  /// <summary>Duration in seconds.</summary>
  [JsonPropertyName("duration")]
  public int? Duration { get; init; }

  /// <summary>Number of stacks.</summary>
  [JsonPropertyName("stacks")]
  public int? Stacks { get; init; }
}
