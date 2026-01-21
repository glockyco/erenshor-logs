namespace ErenshorLogs.Events;

/// <summary>
/// Reference to an ability (skill, spell, proc, etc.) in a combat event.
/// </summary>
public sealed record AbilityRef
{
  /// <summary>Display name.</summary>
  public required string Name { get; init; }

  /// <summary>Ability type.</summary>
  public required AbilityType Type { get; init; }

  /// <summary>Game's stable key for linking (e.g. skill:Backstab).</summary>
  public string? StableKey { get; init; }

  /// <summary>What triggered this ability, if it was proc'd.</summary>
  public ProcSource? ProcSource { get; init; }
}
