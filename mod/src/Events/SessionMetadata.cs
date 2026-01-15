using System.Text.Json.Serialization;

namespace ErenshorLogs.Events;

/// <summary>
/// Metadata about a combat logging session.
/// </summary>
public sealed record SessionMetadata
{
  /// <summary>Session UUID.</summary>
  [JsonPropertyName("id")]
  public required string Id { get; init; }

  /// <summary>Unix timestamp (ms) of first event.</summary>
  [JsonPropertyName("startTime")]
  public required long StartTime { get; init; }

  /// <summary>Unix timestamp (ms) of last event.</summary>
  [JsonPropertyName("endTime")]
  public required long EndTime { get; init; }

  /// <summary>Session duration in milliseconds.</summary>
  [JsonPropertyName("duration")]
  public required long Duration { get; init; }

  /// <summary>Player information.</summary>
  [JsonPropertyName("player")]
  public required PlayerInfo Player { get; init; }

  /// <summary>Erenshor game version.</summary>
  [JsonPropertyName("gameVersion")]
  public required string GameVersion { get; init; }

  /// <summary>Combat Logger mod version.</summary>
  [JsonPropertyName("modVersion")]
  public required string ModVersion { get; init; }
}
