using ErenshorLogs.Events;

namespace ErenshorLogs.Hooks;

/// <summary>
/// Interface for creating CombatEvent instances from game data.
/// Uses concrete Character type for production code.
/// </summary>
public interface ICombatEventBuilder
{
  /// <summary>
  /// Creates a damage event from the given parameters.
  /// </summary>
  /// <param name="eventType">The type of damage event.</param>
  /// <param name="target">The character receiving damage.</param>
  /// <param name="source">The character dealing damage (null for environmental).</param>
  /// <param name="amount">The final damage amount after mitigation.</param>
  /// <param name="damageType">The type of damage dealt.</param>
  /// <param name="ability">The ability that caused the damage.</param>
  /// <param name="flags">Event flags (critical, missed, etc.).</param>
  /// <param name="debugInfo">Optional debug information for attribution troubleshooting.</param>
  /// <returns>A new CombatEvent, or null if target cannot be resolved.</returns>
  CombatEvent? CreateDamageEvent(
    EventType eventType,
    Character target,
    Character? source,
    int amount,
    DamageType damageType,
    AbilityRef ability,
    EventFlags? flags = null,
    AttributionDebugInfo? debugInfo = null
  );

  CombatEvent? CreateHealEvent(
    EventType eventType,
    Character target,
    Character? source,
    AbilityRef ability,
    int amount,
    int? rawAmount,
    int? overhealAmount
  );

  CombatEvent? CreateResourceEvent(
    EventType eventType,
    Character target,
    Character? source,
    AbilityRef ability,
    string resourceType,
    int delta,
    int? current,
    int? max
  );

  CombatEvent? CreateEffectEvent(
    EventType eventType,
    Character target,
    Character? source,
    AbilityRef ability,
    EffectRef effect,
    string action,
    string? reason
  );

  CombatEvent? CreateDeathEvent(
    Character target,
    Character? source,
    AbilityRef ability,
    long? killingBlowEventSeq
  );

  CombatEvent? CreateMechanicEvent(
    Character? target,
    Character? source,
    AbilityRef ability,
    MechanicData mechanic
  );
}
