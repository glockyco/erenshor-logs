using System.Text.Json.Serialization;

namespace ErenshorLogs.Events;

/// <summary>
/// Pre-computed summary statistics for a session.
/// </summary>
public sealed record SessionSummary
{
  /// <summary>Total damage dealt by player/party.</summary>
  [JsonPropertyName("totalDamageDealt")]
  public required long TotalDamageDealt { get; init; }

  /// <summary>Total damage received by player/party.</summary>
  [JsonPropertyName("totalDamageReceived")]
  public required long TotalDamageReceived { get; init; }

  /// <summary>Total healing done.</summary>
  [JsonPropertyName("totalHealing")]
  public required long TotalHealing { get; init; }

  /// <summary>Damage per second.</summary>
  [JsonPropertyName("dps")]
  public required double Dps { get; init; }

  /// <summary>Healing per second.</summary>
  [JsonPropertyName("hps")]
  public required double Hps { get; init; }

  /// <summary>Number of deaths.</summary>
  [JsonPropertyName("deaths")]
  public required int Deaths { get; init; }

  /// <summary>Number of kills.</summary>
  [JsonPropertyName("kills")]
  public required int Kills { get; init; }

  /// <summary>Critical hit rate (0.0 - 1.0).</summary>
  [JsonPropertyName("critRate")]
  public required double CritRate { get; init; }

  /// <summary>Highest single hit.</summary>
  [JsonPropertyName("highestHit")]
  public required int HighestHit { get; init; }

  /// <summary>Damage breakdown by damage type.</summary>
  [JsonPropertyName("damageByType")]
  public required Dictionary<string, long> DamageByType { get; init; }

  /// <summary>Top abilities by damage.</summary>
  [JsonPropertyName("topAbilities")]
  public required List<AbilitySummary> TopAbilities { get; init; }
}
