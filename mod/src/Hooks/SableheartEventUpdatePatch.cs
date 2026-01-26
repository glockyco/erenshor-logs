using ErenshorLogs.Context;
using ErenshorLogs.Events;
using HarmonyLib;

namespace ErenshorLogs.Hooks;

/// <summary>
/// Harmony patch for SableheartEvent.Update to track Sableheart's Curse damage context.
/// SableheartEvent is a boss-specific MonoBehaviour that deals scaling void damage
/// to nearby enemies when the boss reaches a health threshold.
/// </summary>
[HarmonyPatch(typeof(SableheartEvent), "Update")]
public static class SableheartEventUpdatePatch
{
  /// <summary>
  /// Prefix: Push area effect context onto stack before Update processes damage ticks.
  /// Uses hardcoded "Sableheart's Curse" name to match the in-game combat log.
  /// </summary>
  /// <param name="__instance">The SableheartEvent instance processing damage.</param>
  [HarmonyPrefix]
  public static void Prefix(SableheartEvent __instance)
  {
    if (__instance == null)
      return;

    var context = new AbilityContext
    {
      Name = "Sableheart's Curse",
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
