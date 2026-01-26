using ErenshorLogs.Context;
using ErenshorLogs.Events;
using HarmonyLib;

namespace ErenshorLogs.Hooks;

/// <summary>
/// Harmony patch for AEEvent.Update to track area effect damage context.
/// AEEvent is a MonoBehaviour attached to NPCs that deals periodic area damage
/// to nearby enemies (e.g., poison clouds, auras).
/// </summary>
[HarmonyPatch(typeof(AEEvent), "Update")]
public static class AEEventUpdatePatch
{
  /// <summary>
  /// Prefix: Push area effect context onto stack before Update processes damage ticks.
  /// Uses DamageReason field for ability name, falling back to "Area Effect" if not set.
  /// </summary>
  /// <param name="__instance">The AEEvent instance processing damage.</param>
  [HarmonyPrefix]
  public static void Prefix(AEEvent __instance)
  {
    if (__instance == null)
      return;

    var context = new AbilityContext
    {
      Name = __instance.DamageReason ?? "Area Effect",
      Type = AbilityType.AreaEffect,
      StableKey = null,
    };

    CombatContext.PushAbility(context);
  }

  /// <summary>
  /// Finalizer: Pop context from stack after Update completes.
  /// Uses Finalizer instead of Postfix to ensure cleanup even if Update throws.
  /// </summary>
  [HarmonyFinalizer]
  public static void Finalizer()
  {
    CombatContext.PopAbility();
  }
}
