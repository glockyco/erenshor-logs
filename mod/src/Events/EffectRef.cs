namespace ErenshorLogs.Events;

/// <summary>
/// Reference to a status effect (buff/debuff) in a combat event.
/// </summary>
public sealed record EffectRef
{
  /// <summary>Effect display name.</summary>
  public required string Name { get; init; }

  /// <summary>Duration in seconds.</summary>
  public int? Duration { get; init; }

  /// <summary>Number of stacks.</summary>
  public int? Stacks { get; init; }
}
