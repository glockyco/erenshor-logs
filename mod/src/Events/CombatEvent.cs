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

  /// <summary>Type of damage dealt.</summary>
  public DamageType? DamageType { get; init; }

  /// <summary>Status effect info (for buff/debuff events).</summary>
  public EffectRef? Effect { get; init; }

  /// <summary>Event flags.</summary>
  public EventFlags? Flags { get; init; }

  /// <summary>
  /// Debug information for attribution troubleshooting.
  /// Only populated when debug capture is enabled and/or attribution fails.
  /// </summary>
  public AttributionDebugInfo? DebugInfo { get; init; }
}
