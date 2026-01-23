namespace ErenshorLogs.Events;

/// <summary>
/// Debug information captured when ability attribution fails or when detailed
/// debugging is enabled. Helps identify missing hooks and context issues.
/// </summary>
public sealed record AttributionDebugInfo
{
  /// <summary>Source method where the event originated (e.g., "Character.DamageMe").</summary>
  public required string SourceMethod { get; init; }

  /// <summary>Key parameter values that help understand the event context.</summary>
  public Dictionary<string, string>? Parameters { get; init; }

  /// <summary>Top frames of the call stack showing the code path to this event.</summary>
  public string[]? StackTrace { get; init; }

  /// <summary>State of the CombatContext stack at the time of the event.</summary>
  public ContextSnapshot? Context { get; init; }
}

/// <summary>
/// Snapshot of the CombatContext stack state at the time of an event.
/// Helps diagnose whether attribution failed due to missing context, wrong context, or timing issues.
/// </summary>
public sealed record ContextSnapshot
{
  /// <summary>Number of items on the context stack (0 = no context available).</summary>
  public int StackDepth { get; init; }

  /// <summary>Name of the ability at the top of the context stack, if any.</summary>
  public string? TopContextName { get; init; }

  /// <summary>Type of the ability at the top of the context stack, if any.</summary>
  public AbilityType? TopContextType { get; init; }
}
