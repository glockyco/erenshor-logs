using ErenshorLogs.Context;
using ErenshorLogs.Events;
using HarmonyLib;

namespace ErenshorLogs.Hooks;

[HarmonyPatch(typeof(DeathTouch), "Update")]
public static class DeathTouchPatch
{
  [HarmonyPrefix]
  public static void Prefix(out bool __state)
  {
    CombatContext.PushAbility(
      new AbilityContext
      {
        Name = "Death Touch",
        Type = AbilityType.AreaEffect,
        StableKey = "mechanic:death-touch",
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
