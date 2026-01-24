using System.Collections.Generic;
using ErenshorLogs.Context;
using ErenshorLogs.Events;
using ErenshorLogs.Session;
using HarmonyLib;

namespace ErenshorLogs.Hooks;

/// <summary>
/// Harmony postfix patch for Character.MagicDamageMe to capture magic damage events.
/// </summary>
[HarmonyPatch(typeof(Character), nameof(Character.MagicDamageMe))]
public static class MagicDamageMePatch
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
  /// Postfix hook that captures magic damage events after MagicDamageMe completes.
  /// </summary>
  /// <param name="__instance">The Character that received damage (target).</param>
  /// <param name="__result">The return value from MagicDamageMe.</param>
  /// <param name="_dmg">The damage amount.</param>
  /// <param name="_fromPlayer">Whether damage originated from player faction.</param>
  /// <param name="_dmgType">The game's damage type enum.</param>
  /// <param name="_attacker">The Character that dealt damage (source).</param>
  [HarmonyPostfix]
  public static void Postfix(
    Character __instance,
    int __result,
    int _dmg,
    bool _fromPlayer,
    GameData.DamageType _dmgType,
    Character _attacker
  )
  {
    // Skip if not initialized
    if (EventBuilder == null || Emitter == null)
      return;

    // Skip if characters are destroyed Unity objects
    if (!__instance.IsValid())
    {
      LogDebug?.Invoke("[MagicDamageMePatch] Skipping event - target destroyed");
      return;
    }

    if (!_attacker.IsValid())
    {
      LogDebug?.Invoke("[MagicDamageMePatch] Skipping event - attacker destroyed");
      return;
    }

    // Skip if not relevant to player's group
    if (RelevanceChecker != null && !RelevanceChecker.IsRelevantCombat(_attacker, __instance))
      return;

    // Ensure session exists before emitting event
    SessionManager?.EnsureSessionStarted(EventType.DamageMagic);

    // Skip non-loggable results (dead, invulnerable, mining node, treasure chest)
    if (DamageResult.ShouldSkip(__result))
      return;

    // Determine flags based on result and parameters
    // For MagicDamageMe, return 0 means fully resisted (not mitigated like DamageMe)
    var flags = new EventFlags
    {
      FromPlayer = _fromPlayer ? true : null,
      Resisted = __result == DamageResult.FullyResisted ? true : null,
    };

    // Amount is 0 for resist, otherwise the actual damage dealt
    var amount = __result > 0 ? __result : 0;

    // Convert game damage type to our enum
    var damageType = DamageTypeMapper.FromGame(_dmgType);

    // Resolve ability from context with smart fallback
    var ability = AbilityResolver.ResolveWithFallback(_dmgType);

    // Capture debug info if enabled
    var debugInfo = AbilityResolver.CaptureDebugInfoIfEnabled(
      "Character.MagicDamageMe",
      new Dictionary<string, string>
      {
        ["damageType"] = _dmgType.ToString(),
        ["amount"] = __result.ToString(),
        ["dmg"] = _dmg.ToString(),
        ["fromPlayer"] = _fromPlayer.ToString(),
      },
      ability,
      CaptureDebugForUnknown,
      CaptureDebugForAll,
      LogDebug
    );

    // Create and emit the event
    var evt = EventBuilder.CreateDamageEvent(
      EventType.DamageMagic,
      target: __instance,
      source: _attacker,
      amount: amount,
      damageType: damageType,
      ability: ability,
      flags: flags,
      debugInfo: debugInfo
    );

    if (evt != null)
    {
      LogDebug?.Invoke(
        $"Magic damage: {evt.Source?.Name ?? "Unknown"} -> {evt.Target?.Name ?? "Unknown"} "
          + $"for {amount} ({damageType})"
          + (flags.Resisted == true ? " [RESISTED]" : "")
      );
      Emitter.Emit(evt);
    }
  }
}
