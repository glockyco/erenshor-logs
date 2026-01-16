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
    EventFlags? flags = null
  )
  {
    return _inner.CreateDamageEvent(eventType, target, source, amount, damageType, flags);
  }
}
