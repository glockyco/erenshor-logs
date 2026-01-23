using ErenshorLogs.Context;
using HarmonyLib;

namespace ErenshorLogs.Hooks;

/// <summary>
/// Harmony patch for Stats.AddStatusEffect to track when effects are applied.
/// Registers the effect with EffectTracker so DoT/HoT ticks can be attributed.
/// </summary>
/// <remarks>
/// Hooks the 4-parameter overload which is called by SpellVessel and other game systems.
/// Uses the return value (slot index) directly instead of scanning the array.
/// </remarks>
[HarmonyPatch(
  typeof(Stats),
  nameof(Stats.AddStatusEffect),
  new[] { typeof(Spell), typeof(bool), typeof(int), typeof(Character) }
)]
public static class AddStatusEffectPatch
{
  /// <summary>
  /// Effect tracker instance. Set by Plugin during initialization.
  /// </summary>
  internal static EffectTracker? Tracker { get; set; }

  /// <summary>
  /// Postfix: Register the effect after it has been added to StatusEffects array.
  /// Uses the method's return value (slot index) directly - no scanning needed!
  /// </summary>
  /// <param name="__instance">The Stats instance receiving the effect.</param>
  /// <param name="spell">The spell providing the effect.</param>
  /// <param name="__result">The slot index where effect was added, or -1 if failed.</param>
  [HarmonyPostfix]
  public static void Postfix(Stats __instance, Spell spell, int __result)
  {
    if (Tracker == null || __instance == null || spell == null)
      return;

    // __result is the slot index returned by AddStatusEffect
    // -1 means effect was not added (failed resist check, etc.)
    if (__result >= 0 && __result < 30)
    {
      Tracker.RegisterEffect(__instance.Myself, __result, spell);
    }
  }
}
