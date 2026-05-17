using System.Reflection;
using ErenshorLogs.Context;
using ErenshorLogs.Events;
using HarmonyLib;

namespace ErenshorLogs.Hooks;

[HarmonyPatch]
public static class MizukiEventPatch
{
  public static MethodBase? TargetMethod()
  {
    return typeof(MizukiEvent)
      .GetNestedTypes(BindingFlags.NonPublic)
      .FirstOrDefault(type => type.Name.Contains("SetNewAggro", StringComparison.Ordinal))
      ?.GetMethod("MoveNext", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
  }

  [HarmonyPrefix]
  public static void Prefix(out bool __state)
  {
    CombatContext.PushAbility(
      new AbilityContext
      {
        Name = "Mizuki Dagger",
        Type = AbilityType.AreaEffect,
        StableKey = "mechanic:mizuki-dagger",
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
