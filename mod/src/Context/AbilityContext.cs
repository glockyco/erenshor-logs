using ErenshorLogs.Events;

namespace ErenshorLogs.Context;

/// <summary>
/// Context about the currently executing ability.
/// Immutable to ensure thread safety when stored on the stack.
/// </summary>
public sealed record AbilityContext
{
  /// <summary>Display name of the ability.</summary>
  public required string Name { get; init; }

  /// <summary>Type of ability (skill, spell, auto, dot, hot).</summary>
  public required AbilityType Type { get; init; }

  /// <summary>
  /// Game's stable key for linking events to abilities.
  /// Format: "skill:SkillID" or "spell:SpellID".
  /// Null for auto-attacks or when no stable identifier exists.
  /// </summary>
  public string? StableKey { get; init; }

  /// <summary>
  /// What triggered this ability, if it was proc'd.
  /// Null for directly-activated abilities.
  /// </summary>
  public ProcSource? ProcSource { get; init; }
}
