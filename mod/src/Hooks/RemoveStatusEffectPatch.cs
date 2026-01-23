using ErenshorLogs.Context;
using HarmonyLib;

namespace ErenshorLogs.Hooks;

/// <summary>
/// Harmony patch for Stats.RemoveStatusEffect to track when effects are removed.
/// Unregisters the effect from EffectTracker to prevent memory leaks.
/// </summary>
[HarmonyPatch(typeof(Stats), nameof(Stats.RemoveStatusEffect))]
public static class RemoveStatusEffectPatch
{
  /// <summary>
  /// Effect tracker instance. Set by Plugin during initialization.
  /// </summary>
  internal static EffectTracker? Tracker { get; set; }

  /// <summary>
  /// Prefix: Unregister the effect before it is removed from StatusEffects array.
  /// Must run before removal so we still have access to the effect data.
  /// </summary>
  /// <param name="__instance">The Stats instance losing the effect.</param>
  /// <param name="index">The StatusEffects array index being cleared.</param>
  [HarmonyPrefix]
  public static void Prefix(Stats __instance, int index)
  {
    if (Tracker == null || __instance == null)
      return;

    // Unregister before the effect is cleared
    Tracker.UnregisterEffect(__instance.Myself, index);
  }
}
