using ErenshorLogs.Context;
using ErenshorLogs.Events;
using HarmonyLib;

namespace ErenshorLogs.Hooks;

/// <summary>
/// Harmony patch for SpellVessel.ResolveSpell (private method) to track spell execution context.
/// Pushes ability context before spell resolution and pops after completion.
/// </summary>
[HarmonyPatch(typeof(SpellVessel), "ResolveSpell")]
public static class ResolveSpellPatch
{
  /// <summary>
  /// Prefix: Push spell context onto stack before resolution.
  /// This allows subsequent damage/heal hooks to attribute effects to this spell.
  /// </summary>
  /// <param name="__instance">The SpellVessel instance containing the spell being resolved.</param>
  [HarmonyPrefix]
  public static void Prefix(SpellVessel __instance)
  {
    // SpellVessel has public field: Spell spell
    if (__instance.spell == null)
      return;

    var context = new AbilityContext
    {
      Name = __instance.spell.SpellName,
      Type = AbilityType.Spell,
      StableKey = $"spell:{__instance.spell.Id}",
    };

    CombatContext.PushAbility(context);
  }

  /// <summary>
  /// Finalizer: Pop context from stack after resolution completes.
  /// Uses Finalizer instead of Postfix to ensure cleanup even if ResolveSpell throws.
  /// </summary>
  [HarmonyFinalizer]
  public static void Finalizer()
  {
    CombatContext.PopAbility();
  }
}
