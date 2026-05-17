using ErenshorLogs.Context;
using ErenshorLogs.Events;
using HarmonyLib;

namespace ErenshorLogs.Hooks;

[HarmonyPatch(typeof(AEEvent), "TriggerAE")]
public static class AEEventTriggerPatch
{
  private static readonly AccessTools.FieldRef<AEEvent, Character> MyCharRef =
    AccessTools.FieldRefAccess<AEEvent, Character>("MyChar");

  [HarmonyPrefix]
  public static void Prefix(AEEvent __instance, out AeEventTriggerContextState __state)
  {
    __state = default;
    if (__instance == null)
      return;

    var ability = new AbilityRef
    {
      Name = __instance.DamageReason ?? "Area Effect",
      Type = AbilityType.AreaEffect,
      StableKey = null,
    };

    CombatContext.PushAbility(
      new AbilityContext
      {
        Name = ability.Name,
        Type = ability.Type,
        StableKey = ability.StableKey,
      }
    );
    __state = __state with { PushedCombatContext = true };

    if (!__instance.isLifetap)
      return;

    __state = __state with
    {
      HealingScope = HealingContext.Push(
        MyCharRef(__instance),
        ability,
        EventType.HealLifesteal,
        AttributionMethod.Verified
      ),
    };
  }

  [HarmonyFinalizer]
  public static void Finalizer(AeEventTriggerContextState __state)
  {
    __state.HealingScope?.Dispose();
    if (__state.PushedCombatContext)
      CombatContext.PopAbility();
  }
}

public readonly record struct AeEventTriggerContextState(
  bool PushedCombatContext,
  IDisposable? HealingScope
);
