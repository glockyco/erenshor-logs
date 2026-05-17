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
  private static readonly AccessTools.FieldRef<SpellVessel, CastSpell> SpellSourceRef =
    AccessTools.FieldRefAccess<SpellVessel, CastSpell>("SpellSource");

  /// <summary>
  /// Prefix: Push spell context onto stack before resolution.
  /// This allows subsequent damage/heal hooks to attribute effects to this spell.
  /// </summary>
  /// <param name="__instance">The SpellVessel instance containing the spell being resolved.</param>
  /// <param name="__state">Whether this prefix pushed context.</param>
  [HarmonyPrefix]
  public static void Prefix(SpellVessel __instance, out ResolveSpellContextState __state)
  {
    __state = default;
    if (__instance.spell == null)
      return;

    var context = new AbilityContext
    {
      Name = __instance.spell.SpellName,
      Type = AbilityType.Spell,
      StableKey = $"spell:{__instance.spell.Id}",
    };

    CombatContext.PushAbility(context);
    __state = __state with { PushedCombatContext = true };

    if (!__instance.spell.Lifetap)
      return;

    __state = __state with
    {
      HealingScope = HealingContext.Push(
        SpellSourceRef(__instance)?.MyChar,
        new AbilityRef
        {
          Name = __instance.spell.SpellName,
          Type = AbilityType.Spell,
          StableKey = $"spell:{__instance.spell.Id}",
        },
        EventType.HealLifesteal,
        AttributionMethod.Verified
      ),
    };
  }

  /// <summary>
  /// Finalizer: Pop context from stack after resolution completes.
  /// Uses Finalizer instead of Postfix to ensure cleanup even if ResolveSpell throws.
  /// </summary>
  [HarmonyFinalizer]
  public static void Finalizer(ResolveSpellContextState __state)
  {
    __state.HealingScope?.Dispose();
    if (__state.PushedCombatContext)
      CombatContext.PopAbility();
  }
}

public readonly record struct ResolveSpellContextState(
  bool PushedCombatContext,
  IDisposable? HealingScope
);
