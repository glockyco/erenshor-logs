using System.Text.Json.Serialization;

namespace ErenshorLogs.Events;

/// <summary>
/// Root container for a combat log file.
/// </summary>
public sealed record CombatLog
{
  /// <summary>Log format version (semver).</summary>
  [JsonPropertyName("version")]
  public required string Version { get; init; }

  /// <summary>Session metadata.</summary>
  [JsonPropertyName("session")]
  public required SessionMetadata Session { get; init; }

  /// <summary>Pre-computed summary statistics.</summary>
  [JsonPropertyName("summary")]
  public required SessionSummary Summary { get; init; }

  /// <summary>Combat events in chronological order.</summary>
  [JsonPropertyName("events")]
  public required List<CombatEvent> Events { get; init; }
}
