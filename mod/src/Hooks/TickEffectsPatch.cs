using ErenshorLogs.Context;
using HarmonyLib;

namespace ErenshorLogs.Hooks;

/// <summary>
/// Hooks Stats.TickEffects to enable sequential slot tracking.
/// Allows damage/heal hooks to determine which StatusEffect slot caused each tick.
/// </summary>
/// <remarks>
/// TickEffects is a private method that processes all 30 StatusEffect slots sequentially.
/// By hooking it with Prefix/Finalizer, we can track the processing lifecycle and
/// enable damage hooks to correlate calls back to specific slots.
/// </remarks>
[HarmonyPatch(typeof(Stats), "TickEffects")]
public static class TickEffectsPatch
{
  /// <summary>
  /// Prefix: Initialize slot tracking before TickEffects processes slots.
  /// </summary>
  [HarmonyPrefix]
  public static void Prefix(Stats __instance)
  {
    TickEffectsSlotTracker.BeginTickEffects(__instance);
  }

  /// <summary>
  /// Finalizer: Clean up slot tracking after TickEffects completes.
  /// Uses Finalizer to ensure cleanup even if TickEffects throws an exception.
  /// </summary>
  [HarmonyFinalizer]
  public static void Finalizer()
  {
    TickEffectsSlotTracker.EndTickEffects();
  }
}
