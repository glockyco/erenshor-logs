using ErenshorLogs.Context;
using ErenshorLogs.Events;
using HarmonyLib;

namespace ErenshorLogs.Hooks;

/// <summary>
/// Harmony patch for AstraBreathScriot.Update to track cosmic breath damage context.
/// AstraBreathScriot (note: typo is in the game code) is a boss-specific MonoBehaviour
/// that deals periodic magic damage to nearby enemies via breath attacks.
/// </summary>
[HarmonyPatch(typeof(AstraBreathScriot), "Update")]
public static class AstraBreathScriptUpdatePatch
{
  /// <summary>
  /// Prefix: Push area effect context onto stack before Update processes damage.
  /// Uses OverrideBreath field if set, otherwise defaults to "Astra's Cosmic Breath"
  /// to match the in-game combat log.
  /// </summary>
  /// <param name="__instance">The AstraBreathScriot instance processing damage.</param>
  [HarmonyPrefix]
  public static void Prefix(AstraBreathScriot __instance)
  {
    if (__instance == null)
      return;

    var abilityName = string.IsNullOrEmpty(__instance.OverrideBreath)
      ? "Astra's Cosmic Breath"
      : __instance.OverrideBreath;

    var context = new AbilityContext
    {
      Name = abilityName,
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
