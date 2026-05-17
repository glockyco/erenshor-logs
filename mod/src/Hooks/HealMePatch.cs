using ErenshorLogs.Context;
using ErenshorLogs.Events;
using ErenshorLogs.Session;
using HarmonyLib;

namespace ErenshorLogs.Hooks;

[HarmonyPatch(typeof(Stats), nameof(Stats.HealMe))]
public static class HealMePatch
{
  internal static ICombatEventBuilder? EventBuilder { get; set; }
  internal static IEventEmitter? Emitter { get; set; }
  internal static ICombatRelevanceChecker? RelevanceChecker { get; set; }
  internal static ISessionManager? SessionManager { get; set; }
  internal static Action<string>? LogDebug { get; set; }

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

    var target = __instance.Myself;
    if (!target.IsValid())
      return;

    var after = __instance.CurrentHP;
    var effective = Math.Max(0, after - __state.BeforeHp);
    if (effective <= 0)
      return;

    var ability =
      AbilityResolver.FromContext()
      ?? new AbilityRef
      {
        Name = "Healing",
        Type = AbilityType.Spell,
        StableKey = null,
      };
    var eventType = GetHealEventType(ability);
    var overheal = Math.Max(0, __state.RawAmount - effective);

    HealthEventCapture.EmitHeal(
      eventType,
      target,
      source: target,
      ability,
      effective,
      __state.RawAmount,
      overheal,
      mechanic: null
    );
  }

  private static EventType GetHealEventType(AbilityRef ability)
  {
    if (ability.Type == AbilityType.Hot)
      return EventType.HealHot;

    if (
      ability.Name.Contains("lifesteal", StringComparison.OrdinalIgnoreCase)
      || ability.Name.Contains("lifetap", StringComparison.OrdinalIgnoreCase)
    )
      return EventType.HealLifesteal;

    return EventType.HealSpell;
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
