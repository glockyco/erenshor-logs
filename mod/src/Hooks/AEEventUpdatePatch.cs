using ErenshorLogs.Context;
using HarmonyLib;

namespace ErenshorLogs.Hooks;

/// <summary>
/// Harmony patch for AEEvent.Update to track area effect damage context.
/// AEEvent is a MonoBehaviour attached to NPCs that deals periodic area damage
/// to nearby enemies (e.g., poison clouds, auras).
/// </summary>
[HarmonyPatch(typeof(AEEvent), "Update")]
public static class AEEventUpdatePatch
{
  /// <summary>
  /// Prefix: Do not push context here. AEEvent.Update delegates damage to TriggerAE,
  /// which has its own patch and also covers direct TriggerAE calls.
  /// </summary>
  /// <param name="__state">Always false so the finalizer does not pop.</param>
  [HarmonyPrefix]
  public static void Prefix(out bool __state)
  {
    __state = false;
  }

  /// <summary>
  /// Finalizer: Pop context from stack after Update completes.
  /// Uses Finalizer instead of Postfix to ensure cleanup even if Update throws.
  /// </summary>
  [HarmonyFinalizer]
  public static void Finalizer(bool __state)
  {
    if (__state)
      CombatContext.PopAbility();
  }
}
