using ErenshorLogs.Events;

namespace ErenshorLogs.Hooks;

/// <summary>
/// Builder for creating CombatEvent instances from damage hook data.
/// Uses generics to allow testing without game type dependencies.
/// </summary>
/// <typeparam name="TCharacter">The character type (Character in production, mock in tests).</typeparam>
public sealed class CombatEventBuilder<TCharacter>
  where TCharacter : class
{
  private readonly Func<TCharacter?, ActorRef?> _resolveActor;
  private readonly Func<string> _generateId;
  private readonly Func<long> _getTimestamp;

  /// <summary>
  /// Creates a new CombatEventBuilder with the specified delegates.
  /// </summary>
  /// <param name="resolveActor">Function to resolve a character to an ActorRef.</param>
  /// <param name="generateId">Function to generate unique event IDs.</param>
  /// <param name="getTimestamp">Function to get current timestamp in milliseconds.</param>
  public CombatEventBuilder(
    Func<TCharacter?, ActorRef?> resolveActor,
    Func<string> generateId,
    Func<long> getTimestamp
  )
  {
    _resolveActor = resolveActor;
    _generateId = generateId;
    _getTimestamp = getTimestamp;
  }

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
  public CombatEvent? CreateDamageEvent(
    EventType eventType,
    TCharacter target,
    TCharacter? source,
    int amount,
    DamageType damageType,
    AbilityRef ability,
    EventFlags? flags = null,
    AttributionDebugInfo? debugInfo = null
  )
  {
    var targetRef = _resolveActor(target);
    if (targetRef == null)
      return null;

    var sourceRef = _resolveActor(source);

    return new CombatEvent
    {
      Id = _generateId(),
      Timestamp = _getTimestamp(),
      EventType = eventType,
      Source = sourceRef,
      Target = targetRef,
      Amount = amount,
      DamageType = damageType,
      Ability = ability,
      Flags = flags,
      DebugInfo = debugInfo,
    };
  }

  public CombatEvent? CreateHealEvent(
    EventType eventType,
    TCharacter target,
    TCharacter? source,
    AbilityRef ability,
    int amount,
    int? rawAmount,
    int? overhealAmount
  )
  {
    var targetRef = _resolveActor(target);
    if (targetRef == null)
      return null;

    return new CombatEvent
    {
      Id = _generateId(),
      Timestamp = _getTimestamp(),
      EventType = eventType,
      Source = _resolveActor(source),
      Target = targetRef,
      Ability = ability,
      Amount = amount,
      RawAmount = rawAmount,
      OverhealAmount = overhealAmount,
    };
  }

  public CombatEvent? CreateResourceEvent(
    EventType eventType,
    TCharacter target,
    TCharacter? source,
    AbilityRef ability,
    string resourceType,
    int delta,
    int? current,
    int? max
  )
  {
    var targetRef = _resolveActor(target);
    if (targetRef == null)
      return null;

    return new CombatEvent
    {
      Id = _generateId(),
      Timestamp = _getTimestamp(),
      EventType = eventType,
      Source = _resolveActor(source),
      Target = targetRef,
      Ability = ability,
      ResourceType = resourceType,
      ResourceDelta = delta,
      ResourceCurrent = current,
      ResourceMax = max,
    };
  }

  public CombatEvent? CreateEffectEvent(
    EventType eventType,
    TCharacter target,
    TCharacter? source,
    AbilityRef ability,
    EffectRef effect,
    string action,
    string? reason
  )
  {
    var targetRef = _resolveActor(target);
    if (targetRef == null)
      return null;

    return new CombatEvent
    {
      Id = _generateId(),
      Timestamp = _getTimestamp(),
      EventType = eventType,
      Source = _resolveActor(source),
      Target = targetRef,
      Ability = ability,
      Effect = effect,
      EffectAction = action,
      EffectReason = reason,
      EffectStacks = effect.Stacks,
      EffectDurationMs = effect.Duration * 1000,
    };
  }

  public CombatEvent? CreateDeathEvent(
    TCharacter target,
    TCharacter? source,
    AbilityRef ability,
    long? killingBlowEventSeq
  )
  {
    var targetRef = _resolveActor(target);
    if (targetRef == null)
      return null;

    return new CombatEvent
    {
      Id = _generateId(),
      Timestamp = _getTimestamp(),
      EventType = EventType.Death,
      Source = _resolveActor(source),
      Target = targetRef,
      Ability = ability,
      KillingBlowEventSeq = killingBlowEventSeq,
    };
  }

  public CombatEvent? CreateMechanicEvent(
    TCharacter? target,
    TCharacter? source,
    AbilityRef ability,
    MechanicData mechanic
  )
  {
    var targetRef = target == null ? null : _resolveActor(target);
    if (target != null && targetRef == null)
      return null;

    return new CombatEvent
    {
      Id = _generateId(),
      Timestamp = _getTimestamp(),
      EventType = EventType.Mechanic,
      Source = _resolveActor(source),
      Target = targetRef,
      Ability = ability,
      Mechanic = mechanic,
    };
  }
}
