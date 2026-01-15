using System.Text.Json.Serialization;

namespace ErenshorLogs.Events;

/// <summary>
/// A single combat event in the log.
/// </summary>
public sealed record CombatEvent
{
  /// <summary>Unique event identifier (UUID).</summary>
  [JsonPropertyName("id")]
  public required string Id { get; init; }

  /// <summary>Unix timestamp in milliseconds.</summary>
  [JsonPropertyName("timestamp")]
  public required long Timestamp { get; init; }

  /// <summary>Type of combat event.</summary>
  [JsonPropertyName("eventType")]
  public required EventType EventType { get; init; }

  /// <summary>Actor that caused the event.</summary>
  [JsonPropertyName("source")]
  public ActorRef? Source { get; init; }

  /// <summary>Actor that received the event.</summary>
  [JsonPropertyName("target")]
  public ActorRef? Target { get; init; }

  /// <summary>Ability used (null for auto-attacks without named ability).</summary>
  [JsonPropertyName("ability")]
  public AbilityRef? Ability { get; init; }

  /// <summary>Final amount after mitigation.</summary>
  [JsonPropertyName("amount")]
  public int? Amount { get; init; }

  /// <summary>Raw amount before mitigation.</summary>
  [JsonPropertyName("rawAmount")]
  public int? RawAmount { get; init; }

  /// <summary>Amount mitigated by armor/resists.</summary>
  [JsonPropertyName("mitigated")]
  public int? Mitigated { get; init; }

  /// <summary>Type of damage dealt.</summary>
  [JsonPropertyName("damageType")]
  public DamageType? DamageType { get; init; }

  /// <summary>Status effect info (for buff/debuff events).</summary>
  [JsonPropertyName("effect")]
  public EffectRef? Effect { get; init; }

  /// <summary>Event flags.</summary>
  [JsonPropertyName("flags")]
  public EventFlags? Flags { get; init; }
}
