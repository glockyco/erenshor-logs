using System.Reflection;
using ErenshorLogs.Context;
using ErenshorLogs.Events;
using ErenshorLogs.Session;
using HarmonyLib;

namespace ErenshorLogs.Hooks;

[HarmonyPatch]
public static class HealMePatch
{
  internal static ICombatEventBuilder? EventBuilder { get; set; }
  internal static IEventEmitter? Emitter { get; set; }
  internal static ICombatRelevanceChecker? RelevanceChecker { get; set; }
  internal static ISessionManager? SessionManager { get; set; }
  internal static Action<string>? LogDebug { get; set; }

  public static MethodBase TargetMethod()
  {
    return typeof(Stats).GetMethod(
      nameof(Stats.HealMe),
      BindingFlags.Instance | BindingFlags.Public,
      null,
      [typeof(int)],
      null
    );
  }

  [HarmonyPrefix]
  public static void Prefix(Stats __instance, int _amt, out HealthSnapshot __state)
  {
    __state = HealthSnapshot.FromStats(__instance, _amt);
  }

  [HarmonyPostfix]
  public static void Postfix(Stats __instance, HealthSnapshot __state)
  {
    if (!__state.IsValid)
      return;

    var healingContext =
      HealingContext.Current() ?? TickEffectsSlotTracker.ConsumePendingHealingContext();

    var target = __instance.Myself;
    if (!target.IsValid())
      return;

    var after = __instance.CurrentHP;
    var effective = Math.Max(0, after - __state.BeforeHp);
    if (effective <= 0)
      return;

    var contextAbility = AbilityResolver.FromContext();
    var ability =
      healingContext?.Ability
      ?? contextAbility
      ?? new AbilityRef
      {
        Name = "Healing",
        Type = AbilityType.Spell,
        StableKey = null,
      };
    var eventType = healingContext?.EventType ?? GetHealEventType(ability);
    var source = healingContext?.Source;
    if (source != null && !source.IsValid())
      source = null;
    var overheal = Math.Max(0, __state.RawAmount - effective);

    HealthEventCapture.EmitHeal(
      eventType,
      target,
      source,
      ability,
      effective,
      __state.RawAmount,
      overheal,
      mechanic: null,
      flags: null,
      attribution: ResolveAttribution(healingContext, contextAbility)
    );
  }

  internal static AttributionMethod ResolveAttribution(
    HealingContextFrame? healingContext,
    AbilityRef? contextAbility
  )
  {
    if (healingContext != null)
      return healingContext.Attribution;

    return contextAbility != null ? AttributionMethod.Context : AttributionMethod.Unknown;
  }

  private static EventType GetHealEventType(AbilityRef ability)
  {
    return ability.Type == AbilityType.Hot ? EventType.HealHot : EventType.HealSpell;
  }
}

[HarmonyPatch]
public static class SpellHealMePatch
{
  public static MethodBase TargetMethod()
  {
    return typeof(Stats).GetMethod(
      nameof(Stats.HealMe),
      BindingFlags.Instance | BindingFlags.Public,
      null,
      [typeof(Spell), typeof(int), typeof(bool), typeof(bool), typeof(Character)],
      null
    );
  }

  [HarmonyPrefix]
  public static void Prefix(
    Stats __instance,
    Spell _spell,
    int _amt,
    bool _isMana,
    out SpellHealSnapshot __state
  )
  {
    __state = SpellHealSnapshot.FromStats(__instance, _spell, _amt, _isMana);
  }

  [HarmonyPostfix]
  public static void Postfix(
    Stats __instance,
    Spell _spell,
    bool _isCrit,
    bool _isMana,
    Character _source,
    SpellHealSnapshot __state
  )
  {
    if (!__state.IsValid)
      return;

    var target = __instance.Myself;
    if (!target.IsValid())
      return;

    var ability = new AbilityRef
    {
      Name = _spell.SpellName,
      Type = AbilityType.Spell,
      StableKey = $"spell:{_spell.Id}",
    };

    if (_isMana)
    {
      ResourceEventCapture.EmitManaEvent(
        EventType.ManaRestore,
        target,
        _source,
        ability,
        __state.BeforeResource,
        __instance.CurrentMana,
        __state.MaxResource
      );
      return;
    }

    var effective = Math.Max(0, __instance.CurrentHP - __state.BeforeResource);
    if (effective <= 0)
      return;

    HealthEventCapture.EmitHeal(
      EventType.HealSpell,
      target,
      _source,
      ability,
      effective,
      __state.RawAmount,
      Math.Max(0, __state.RawAmount - effective),
      mechanic: null,
      flags: new EventFlags { Critical = _isCrit ? true : null },
      attribution: AttributionMethod.Verified
    );
  }
}

public readonly record struct SpellHealSnapshot(
  int BeforeResource,
  int MaxResource,
  int RawAmount,
  bool IsMana
)
{
  public bool IsValid => MaxResource > 0;

  public static SpellHealSnapshot FromStats(Stats? stats, Spell? spell, int rawAmount, bool isMana)
  {
    if (stats == null || spell == null)
      return default;

    return isMana
      ? new SpellHealSnapshot(stats.CurrentMana, stats.GetCurrentMaxMana(), rawAmount, true)
      : new SpellHealSnapshot(stats.CurrentHP, stats.CurrentMaxHP, rawAmount, false);
  }
}

public readonly record struct HealthSnapshot(int BeforeHp, int MaxHp, int RawAmount)
{
  public bool IsValid => MaxHp > 0;

  public static HealthSnapshot FromStats(Stats? stats, int rawAmount)
  {
    if (stats == null)
      return default;

    return new HealthSnapshot(stats.CurrentHP, stats.CurrentMaxHP, rawAmount);
  }
}
