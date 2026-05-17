using ErenshorLogs.Events;
using ErenshorLogs.Registry;

namespace ErenshorLogs.Hooks;

/// <summary>
/// Adapter that wires CombatEventBuilder to game types and services.
/// Registered in DI container for production use.
/// </summary>
public sealed class CombatEventBuilderAdapter : ICombatEventBuilder
{
  private readonly CombatEventBuilder<Character> _inner;

  /// <summary>
  /// Creates a new adapter with the specified actor registry.
  /// </summary>
  /// <param name="actorRegistry">Registry for resolving characters to ActorRefs.</param>
  public CombatEventBuilderAdapter(IActorRegistry actorRegistry)
  {
    _inner = new CombatEventBuilder<Character>(
      resolveActor: actorRegistry.GetOrCreate,
      generateId: () => Guid.NewGuid().ToString(),
      getTimestamp: () => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
    );
  }

  /// <inheritdoc />
  public CombatEvent? CreateDamageEvent(
    EventType eventType,
    Character target,
    Character? source,
    int amount,
    DamageType damageType,
    AbilityRef ability,
    EventFlags? flags = null,
    AttributionDebugInfo? debugInfo = null
  )
  {
    return _inner.CreateDamageEvent(
      eventType,
      target,
      source,
      amount,
      damageType,
      ability,
      flags,
      debugInfo
    );
  }

  /// <inheritdoc />
  public CombatEvent? CreateHealEvent(
    EventType eventType,
    Character target,
    Character? source,
    AbilityRef ability,
    int amount,
    int? rawAmount,
    int? overhealAmount,
    EventFlags? flags = null
  )
  {
    return _inner.CreateHealEvent(
      eventType,
      target,
      source,
      ability,
      amount,
      rawAmount,
      overhealAmount,
      flags
    );
  }

  /// <inheritdoc />
  public CombatEvent? CreateResourceEvent(
    EventType eventType,
    Character target,
    Character? source,
    AbilityRef ability,
    string resourceType,
    int delta,
    int? current,
    int? max
  )
  {
    return _inner.CreateResourceEvent(
      eventType,
      target,
      source,
      ability,
      resourceType,
      delta,
      current,
      max
    );
  }

  /// <inheritdoc />
  public CombatEvent? CreateEffectEvent(
    EventType eventType,
    Character target,
    Character? source,
    AbilityRef ability,
    EffectRef effect,
    string action,
    string? reason
  )
  {
    return _inner.CreateEffectEvent(eventType, target, source, ability, effect, action, reason);
  }

  /// <inheritdoc />
  public CombatEvent? CreateDeathEvent(
    Character target,
    Character? source,
    AbilityRef ability,
    long? killingBlowEventSeq
  )
  {
    return _inner.CreateDeathEvent(target, source, ability, killingBlowEventSeq);
  }

  /// <inheritdoc />
  public CombatEvent? CreateMechanicEvent(
    Character? target,
    Character? source,
    AbilityRef ability,
    MechanicData mechanic
  )
  {
    return _inner.CreateMechanicEvent(target, source, ability, mechanic);
  }
}
