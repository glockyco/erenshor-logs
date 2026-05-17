using ErenshorLogs.Context;
using ErenshorLogs.Events;
using ErenshorLogs.Session;
using HarmonyLib;

namespace ErenshorLogs.Hooks;

/// <summary>
/// Harmony postfix patch for Character.EnvironmentalDamageMe to capture environmental damage events.
/// </summary>
[HarmonyPatch(typeof(Character), nameof(Character.EnvironmentalDamageMe))]
public static class EnvironmentalDamageMePatch
{
  /// <summary>
  /// Event builder for creating combat events. Set by Plugin during initialization.
  /// </summary>
  internal static ICombatEventBuilder? EventBuilder { get; set; }

  /// <summary>
  /// Event emitter for dispatching events. Set by Plugin during initialization.
  /// </summary>
  internal static IEventEmitter? Emitter { get; set; }

  /// <summary>
  /// Optional debug logging callback. Set by Plugin during initialization.
  /// </summary>
  internal static Action<string>? LogDebug { get; set; }

  /// <summary>
  /// Combat relevance checker for filtering events. Set by Plugin during initialization.
  /// </summary>
  internal static ICombatRelevanceChecker? RelevanceChecker { get; set; }

  /// <summary>
  /// Session manager for ensuring session exists before emitting events.
  /// Set by Plugin during initialization.
  /// </summary>
  internal static ISessionManager? SessionManager { get; set; }

  /// <summary>
  /// Configuration for debug capture settings.
  /// Set by Plugin during initialization.
  /// </summary>
  internal static bool CaptureDebugForUnknown { get; set; } = true;

  /// <summary>
  /// Configuration for capturing debug info for all events.
  /// Set by Plugin during initialization.
  /// </summary>
  internal static bool CaptureDebugForAll { get; set; } = false;

  /// <summary>
  /// Postfix hook that captures environmental damage after EnvironmentalDamageMe completes.
  /// </summary>
  /// <param name="__instance">The Character that received damage (target).</param>
  /// <param name="__result">The return value from EnvironmentalDamageMe.</param>
  /// <param name="_dmg">The environmental damage amount.</param>
  [HarmonyPostfix]
  public static void Postfix(Character __instance, int __result, int _dmg)
  {
    // Skip if not initialized
    if (EventBuilder == null || Emitter == null)
      return;

    // Skip if target is a destroyed Unity object
    if (!__instance.IsValid())
    {
      LogDebug?.Invoke("[EnvironmentalDamageMePatch] Skipping event - target destroyed");
      return;
    }

    // Skip if target not relevant to player's group (no source for environmental damage)
    if (RelevanceChecker != null && !RelevanceChecker.IsRelevantCombat(null, __instance))
      return;

    // Skip non-loggable results (-1 = dead/invulnerable)
    if (__result <= 0)
      return;

    // Environmental damage has no source actor and is always physical
    var ability = AbilityResolver.CreateFixed("Environmental", AbilityType.Environmental);

    // Capture debug info if enabled (unlikely for environmental but supports CaptureDebugForAll)
    var debugInfo = AbilityResolver.CaptureDebugInfoIfEnabled(
      "Character.EnvironmentalDamageMe",
      new Dictionary<string, string>
      {
        ["amount"] = __result.ToString(),
        ["dmg"] = _dmg.ToString(),
      },
      ability,
      CaptureDebugForUnknown,
      CaptureDebugForAll,
      LogDebug
    );

    var evt = EventBuilder.CreateDamageEvent(
      EventType.DamageEnvironmental,
      target: __instance,
      source: null,
      amount: __result,
      damageType: DamageType.Physical,
      ability: ability,
      flags: null,
      debugInfo: debugInfo
    );

    if (evt != null)
    {
      LogDebug?.Invoke($"Environmental damage: {evt.Target?.Name ?? "Unknown"} took {__result}");
      Emitter.Emit(evt);

      // Notify session manager of combat event
      // Note: Environmental damage will not start sessions (filtered by config)
      SessionManager?.OnCombatEvent(evt.EventType, evt.Timestamp);
    }
  }
}
