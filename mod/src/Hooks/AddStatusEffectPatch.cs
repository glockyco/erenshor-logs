using ErenshorLogs.Context;
using HarmonyLib;

namespace ErenshorLogs.Hooks;

/// <summary>
/// Harmony patch for Stats.AddStatusEffectNoChecks to track when effects are applied.
/// Registers the effect with EffectTracker so DoT/HoT ticks can be attributed.
/// </summary>
[HarmonyPatch(typeof(Stats), "AddStatusEffectNoChecks")]
public static class AddStatusEffectPatch
{
  /// <summary>
  /// Effect tracker instance. Set by Plugin during initialization.
  /// </summary>
  internal static EffectTracker? Tracker { get; set; }

  /// <summary>
  /// Postfix: Register the effect after it has been added to StatusEffects array.
  /// Scans the array to find which slot was just filled with this spell.
  /// </summary>
  /// <param name="__instance">The Stats instance receiving the effect.</param>
  /// <param name="spell">The spell providing the effect.</param>
  [HarmonyPostfix]
  public static void Postfix(Stats __instance, Spell spell)
  {
    if (Tracker == null || __instance == null || spell == null)
      return;

    // StatusEffects is an array of size 30
    // Find which slot was just filled with this spell
    for (int i = 0; i < __instance.StatusEffects.Length; i++)
    {
      if (__instance.StatusEffects[i].Effect == spell)
      {
        Tracker.RegisterEffect(__instance.Myself, i, spell);
        // Only register the first match (in case of duplicates)
        break;
      }
    }
  }
}
