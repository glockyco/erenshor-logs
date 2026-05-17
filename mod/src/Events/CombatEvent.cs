namespace ErenshorLogs.Events;

/// <summary>
/// A single combat event in the log.
/// </summary>
public sealed record CombatEvent
{
  /// <summary>Unique event identifier (UUID).</summary>
  public required string Id { get; init; }

  /// <summary>Unix timestamp in milliseconds.</summary>
  public required long Timestamp { get; init; }

  /// <summary>Type of combat event.</summary>
  public required EventType EventType { get; init; }

  /// <summary>Actor that caused the event.</summary>
  public ActorRef? Source { get; init; }

  /// <summary>Actor that received the event.</summary>
  public ActorRef? Target { get; init; }

  /// <summary>Ability used. Always present; uses "Unknown" when attribution fails.</summary>
  public required AbilityRef Ability { get; init; }

  /// <summary>Final amount after mitigation.</summary>
  public int? Amount { get; init; }

  /// <summary>Raw amount before mitigation.</summary>
  public int? RawAmount { get; init; }

  /// <summary>Amount mitigated by armor/resists.</summary>
  public int? Mitigated { get; init; }

  /// <summary>Healing that exceeded the target's missing health.</summary>
  public int? OverhealAmount { get; init; }

  /// <summary>Type of damage dealt.</summary>
  public DamageType? DamageType { get; init; }

  /// <summary>Status effect info (for buff/debuff events).</summary>
  public EffectRef? Effect { get; init; }

  /// <summary>Resource type affected by resource events.</summary>
  public string? ResourceType { get; init; }

  /// <summary>Signed resource delta.</summary>
  public int? ResourceDelta { get; init; }

  /// <summary>Resource value after the event.</summary>
  public int? ResourceCurrent { get; init; }

  /// <summary>Maximum resource value at event time.</summary>
  public int? ResourceMax { get; init; }

  /// <summary>Effect action for lifecycle events.</summary>
  public string? EffectAction { get; init; }

  /// <summary>Reason a status effect faded.</summary>
  public string? EffectReason { get; init; }

  /// <summary>Stack count after the effect event.</summary>
  public int? EffectStacks { get; init; }

  /// <summary>Effect duration in milliseconds.</summary>
  public int? EffectDurationMs { get; init; }

  /// <summary>Event sequence of the killing blow.</summary>
  public long? KillingBlowEventSeq { get; init; }

  /// <summary>Health-affecting encounter mechanic details.</summary>
  public MechanicData? Mechanic { get; init; }

  /// <summary>Event flags.</summary>
  public EventFlags? Flags { get; init; }

  /// <summary>
  /// Debug information for attribution troubleshooting.
  /// Only populated when debug capture is enabled and/or attribution fails.
  /// </summary>
  public AttributionDebugInfo? DebugInfo { get; init; }
}
