using System.Reflection;
using ErenshorLogs.Events;
using HarmonyLib;

namespace ErenshorLogs.Hooks;

[HarmonyPatch]
public static class RegenEffectsPatch
{
  public static MethodBase TargetMethod()
  {
    return typeof(Stats).GetMethod(
      "RegenEffects",
      BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
      null,
      [typeof(float)],
      null
    );
  }

  private static readonly AbilityRef Ability = new()
  {
    Name = "Natural Regeneration",
    Type = AbilityType.Unknown,
    StableKey = "system:hp-regen",
  };

  [HarmonyPrefix]
  public static void Prefix(Stats __instance, out HealthSnapshot __state)
  {
    __state = HealthSnapshot.FromStats(__instance, rawAmount: 0);
  }

  [HarmonyPostfix]
  public static void Postfix(Stats __instance, HealthSnapshot __state)
  {
    if (!__state.IsValid)
      return;

    var target = __instance.Myself;
    if (!target.IsValid())
      return;

    var effective = Math.Max(0, __instance.CurrentHP - __state.BeforeHp);
    if (effective <= 0)
      return;

    HealthEventCapture.EmitHeal(
      EventType.HealRegen,
      target,
      source: null,
      ability: Ability,
      amount: effective,
      rawAmount: null,
      overhealAmount: null,
      mechanic: null,
      flags: null,
      attribution: AttributionMethod.Verified
    );
  }
}
