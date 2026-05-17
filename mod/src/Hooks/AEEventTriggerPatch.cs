using ErenshorLogs.Context;
using ErenshorLogs.Events;
using HarmonyLib;

namespace ErenshorLogs.Hooks;

[HarmonyPatch(typeof(AEEvent), "TriggerAE")]
public static class AEEventTriggerPatch
{
  [HarmonyPrefix]
  public static void Prefix(AEEvent __instance, out bool __state)
  {
    __state = false;
    if (__instance == null)
      return;

    CombatContext.PushAbility(
      new AbilityContext
      {
        Name = __instance.DamageReason ?? "Area Effect",
        Type = AbilityType.AreaEffect,
        StableKey = null,
      }
    );
    __state = true;
  }

  [HarmonyFinalizer]
  public static void Finalizer(bool __state)
  {
    if (__state)
      CombatContext.PopAbility();
  }
}
