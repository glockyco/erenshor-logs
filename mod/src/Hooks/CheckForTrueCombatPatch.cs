using ErenshorLogs.Session;
using HarmonyLib;

namespace ErenshorLogs.Hooks;

/// <summary>
/// Harmony postfix patch for PlayerCombat.CheckForTrueCombat to detect combat state changes.
/// </summary>
[HarmonyPatch(typeof(PlayerCombat), "CheckForTrueCombat")]
public static class CheckForTrueCombatPatch
{
  /// <summary>
  /// Session manager for handling combat state transitions. Set by Plugin during initialization.
  /// </summary>
  internal static ISessionManager? SessionManager { get; set; }

  /// <summary>
  /// Postfix hook that notifies the session manager of combat state changes.
  /// </summary>
  /// <param name="__result">The return value from CheckForTrueCombat (true if in combat).</param>
  [HarmonyPostfix]
  public static void Postfix(bool __result)
  {
    SessionManager?.OnCombatStateChanged(__result);
  }
}
