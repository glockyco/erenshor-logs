using ErenshorLogs.Context;
using ErenshorLogs.Events;
using HarmonyLib;

namespace ErenshorLogs.Hooks;

/// <summary>
/// Harmony patch for WandBolt.DeliverDamage (private method) to track wand/bow attack context.
/// Pushes ability context before damage delivery and pops after completion.
/// </summary>
[HarmonyPatch(typeof(WandBolt), "DeliverDamage")]
public static class DeliverDamagePatch
{
  /// <summary>
  /// Prefix: Push wand/bow attack context onto stack before damage delivery.
  /// Distinguishes between wand (magic) and bow (physical) based on DmgType field.
  /// </summary>
  /// <param name="__instance">The WandBolt instance delivering damage.</param>
  /// <param name="__state">Whether this prefix pushed context.</param>
  [HarmonyPrefix]
  public static void Prefix(WandBolt __instance, out bool __state)
  {
    __state = false;
    if (__instance == null)
      return;
    // Determine ability name based on damage type
    // WandBolt.DmgType is Magic for wands, Physical for bows
    var abilityName =
      __instance.DmgType == GameData.DamageType.Magic ? "Wand Attack" : "Bow Attack";

    var context = new AbilityContext
    {
      Name = abilityName,
      Type = AbilityType.Auto,
      StableKey = null,
    };

    CombatContext.PushAbility(context);
    __state = true;
  }

  /// <summary>
  /// Finalizer: Pop context from stack after damage delivery completes.
  /// Uses Finalizer instead of Postfix to ensure cleanup even if DeliverDamage throws.
  /// </summary>
  [HarmonyFinalizer]
  public static void Finalizer(bool __state)
  {
    if (__state)
      CombatContext.PopAbility();
  }
}
