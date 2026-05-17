using ErenshorLogs.Context;
using ErenshorLogs.Events;
using HarmonyLib;

namespace ErenshorLogs.Hooks;

/// <summary>
/// Harmony patch for NPCFightEvent.BreathAttack to track generic NPC breath attack damage context.
/// NPCFightEvent is a MonoBehaviour attached to various NPCs that provides configurable
/// fight mechanics including periodic breath attacks that damage nearby enemies.
/// </summary>
[HarmonyPatch(typeof(NPCFightEvent), "BreathAttack")]
public static class NPCFightEventBreathAttackPatch
{
  /// <summary>
  /// Prefix: Push area effect context onto stack before BreathAttack processes damage.
  /// Uses generic "Breath Attack" name as the game doesn't expose a configurable name.
  /// </summary>
  /// <param name="__instance">The NPCFightEvent instance processing the breath attack.</param>
  /// <param name="__state">Whether this prefix pushed context.</param>
  [HarmonyPrefix]
  public static void Prefix(NPCFightEvent __instance, out bool __state)
  {
    __state = false;
    if (__instance == null)
      return;
    var context = new AbilityContext
    {
      Name = "Breath Attack",
      Type = AbilityType.AreaEffect,
      StableKey = null,
    };

    CombatContext.PushAbility(context);
    __state = true;
  }

  /// <summary>
  /// Finalizer: Pop context from stack after BreathAttack completes.
  /// Uses Finalizer instead of Postfix to ensure cleanup even if BreathAttack throws.
  /// </summary>
  [HarmonyFinalizer]
  public static void Finalizer(bool __state)
  {
    if (__state)
      CombatContext.PopAbility();
  }
}
