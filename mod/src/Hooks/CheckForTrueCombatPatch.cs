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
  /// Optional debug logging callback. Set by Plugin during initialization.
  /// </summary>
  internal static Action<string>? LogDebug { get; set; }

  /// <summary>
  /// Postfix hook that notifies the session manager of combat state changes.
  /// </summary>
  /// <param name="__result">The return value from CheckForTrueCombat (true if in combat).</param>
  /// <remarks>
  /// To see detailed combat state logging, enable Debug log level in BepInEx.cfg:
  /// [Logging.Console] LogLevels = Fatal, Error, Warning, Message, Info, Debug
  /// </remarks>
  [HarmonyPostfix]
  public static void Postfix(bool __result)
  {
    LogDebug?.Invoke($"CheckForTrueCombat returned: {__result}");
    SessionManager?.OnCombatStateChanged(__result);
  }
}
