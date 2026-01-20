namespace ErenshorLogs.Events;

/// <summary>
/// Summary statistics for a single ability.
/// </summary>
public sealed record AbilitySummary
{
  /// <summary>Ability display name.</summary>
  public required string Name { get; init; }

  /// <summary>Total damage dealt by this ability.</summary>
  public required long Damage { get; init; }

  /// <summary>Number of hits.</summary>
  public required int Hits { get; init; }
}
