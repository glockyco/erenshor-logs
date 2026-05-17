using ErenshorLogs.Context;
using ErenshorLogs.Events;
using HarmonyLib;

namespace ErenshorLogs.Hooks;

/// <summary>
/// Harmony patch for AEEvent2.Update to track area effect damage context.
/// AEEvent2 is a variant of AEEvent that deals periodic area damage to nearby enemies.
/// </summary>
[HarmonyPatch(typeof(AEEvent2), "Update")]
public static class AEEvent2UpdatePatch
{
  /// <summary>
  /// Prefix: Push area effect context onto stack before Update processes damage ticks.
  /// Uses DamageReason field for ability name, falling back to "Area Effect" if not set.
  /// </summary>
  /// <param name="__instance">The AEEvent2 instance processing damage.</param>
  /// <param name="__state">Whether this prefix pushed context.</param>
  [HarmonyPrefix]
  public static void Prefix(AEEvent2 __instance, out bool __state)
  {
    __state = false;
    if (__instance == null)
      return;
    var context = new AbilityContext
    {
      Name = __instance.DamageReason ?? "Area Effect",
      Type = AbilityType.AreaEffect,
      StableKey = null,
    };

    CombatContext.PushAbility(context);
    __state = true;
  }

  /// <summary>
  /// Finalizer: Pop context from stack after Update completes.
  /// Uses Finalizer instead of Postfix to ensure cleanup even if Update throws.
  /// </summary>
  [HarmonyFinalizer]
  public static void Finalizer(bool __state)
  {
    if (__state)
      CombatContext.PopAbility();
  }
}
