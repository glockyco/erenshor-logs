namespace ErenshorLogs.Events;

/// <summary>
/// Pre-computed summary statistics for a session.
/// </summary>
public sealed record SessionSummary
{
  /// <summary>Total damage dealt by player/party.</summary>
  public required long TotalDamageDealt { get; init; }

  /// <summary>Total damage received by player/party.</summary>
  public required long TotalDamageReceived { get; init; }

  /// <summary>Total healing done.</summary>
  public required long TotalHealing { get; init; }

  /// <summary>Damage per second.</summary>
  public required double Dps { get; init; }

  /// <summary>Healing per second.</summary>
  public required double Hps { get; init; }

  /// <summary>Number of deaths.</summary>
  public required int Deaths { get; init; }

  /// <summary>Number of kills.</summary>
  public required int Kills { get; init; }

  /// <summary>Critical hit rate (0.0 - 1.0).</summary>
  public required double CritRate { get; init; }

  /// <summary>Highest single hit.</summary>
  public required int HighestHit { get; init; }

  /// <summary>Damage breakdown by damage type.</summary>
  public required Dictionary<string, long> DamageByType { get; init; }

  /// <summary>Top abilities by damage.</summary>
  public required List<AbilitySummary> TopAbilities { get; init; }
}
