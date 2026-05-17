using ErenshorLogs.Context;
using ErenshorLogs.Events;
using HarmonyLib;

namespace ErenshorLogs.Hooks;

/// <summary>
/// Harmony patch for UseSkill.DoSkillNoChecks to track SimPlayer skill execution context.
/// </summary>
[HarmonyPatch(typeof(UseSkill), nameof(UseSkill.DoSkillNoChecks))]
public static class DoSkillNoChecksPatch
{
  /// <summary>
  /// Prefix: Push skill context onto stack before execution.
  /// This allows subsequent damage hooks to attribute SimPlayer damage to this skill.
  /// </summary>
  /// <param name="_skill">The skill being executed.</param>
  [HarmonyPrefix]
  public static void Prefix(Skill _skill)
  {
    if (_skill == null)
      return;

    var context = new AbilityContext
    {
      Name = _skill.SkillName,
      Type = AbilityType.Skill,
      StableKey = $"skill:{_skill.Id}",
    };

    CombatContext.PushAbility(context);
  }

  /// <summary>
  /// Finalizer: Pop context from stack after execution completes.
  /// Uses Finalizer instead of Postfix to ensure cleanup even if DoSkillNoChecks throws.
  /// </summary>
  [HarmonyFinalizer]
  public static void Finalizer()
  {
    CombatContext.PopAbility();
  }
}
