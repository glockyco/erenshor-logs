using System.Collections.Generic;
using ErenshorLogs.Context;
using ErenshorLogs.Events;
using ErenshorLogs.Session;
using HarmonyLib;

namespace ErenshorLogs.Hooks;

/// <summary>
/// Harmony postfix patch for Character.DamageMe to capture physical damage events.
/// </summary>
[HarmonyPatch(typeof(Character), nameof(Character.DamageMe))]
public static class DamageMePatch
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
  /// Effect tracker for resolving DoT tick attribution.
  /// Set by Plugin during initialization.
  /// </summary>
  internal static EffectTracker? EffectTracker { get; set; }

  /// <summary>
  /// Postfix hook that captures damage events after DamageMe completes.
  /// </summary>
  /// <param name="__instance">The Character that received damage (target).</param>
  /// <param name="__result">The return value from DamageMe.</param>
  /// <param name="_incdmg">Original incoming damage before mitigation.</param>
  /// <param name="_fromPlayer">Whether damage originated from player faction.</param>
  /// <param name="_dmgType">The game's damage type enum.</param>
  /// <param name="_attacker">The Character that dealt damage (source).</param>
  /// <param name="_criticalHit">Whether this was a critical hit.</param>
  [HarmonyPostfix]
  public static void Postfix(
    Character __instance,
    int __result,
    int _incdmg,
    bool _fromPlayer,
    GameData.DamageType _dmgType,
    Character _attacker,
    bool _criticalHit
  )
  {
    // Skip if not initialized
    if (EventBuilder == null || Emitter == null)
      return;

    // Skip if characters are destroyed Unity objects
    if (!__instance.IsValid())
    {
      LogDebug?.Invoke("[DamageMePatch] Skipping event - target destroyed");
      return;
    }

    if (!_attacker.IsValid())
    {
      LogDebug?.Invoke("[DamageMePatch] Skipping event - attacker destroyed");
      return;
    }

    // Skip if not relevant to player's group
    if (RelevanceChecker != null && !RelevanceChecker.IsRelevantCombat(_attacker, __instance))
      return;

    // Skip non-loggable results (dead, friendly fire, mining node, treasure chest)
    if (DamageResult.ShouldSkip(__result))
      return;

    // Determine flags based on result and parameters
    var flags = new EventFlags
    {
      FromPlayer = _fromPlayer ? true : null,
      Critical = _criticalHit ? true : null,
      Missed = __result == DamageResult.FullyMitigated ? true : null,
      Absorbed = __result == DamageResult.ShieldAbsorbed ? true : null,
    };

    // Amount is 0 for miss/absorb, otherwise the actual damage dealt
    var amount = __result > 0 ? __result : 0;

    // Convert game damage type to our enum
    var damageType = DamageTypeMapper.FromGame(_dmgType);

    // Resolve ability: try context first, then DoT slot tracking (if in TickEffects), then inference
    var ability = AbilityResolver.FromContext();

    if (ability == null && TickEffectsSlotTracker.IsInTickEffects())
    {
      // We're in TickEffects - try to identify the slot
      var slot = TickEffectsSlotTracker.FindAndAdvanceSlot(
        __instance,
        _dmgType,
        isBleed: false,
        isHeal: false
      );

      if (slot.HasValue && EffectTracker != null)
      {
        // Query EffectTracker with exact slot index to get spell name
        var context = EffectTracker.GetEffectContext(__instance, slot.Value);
        if (context != null)
        {
          ability = new AbilityRef
          {
            Name = context.Name,
            Type = context.Type,
            StableKey = context.StableKey,
            ProcSource = context.ProcSource,
          };
        }
      }
    }

    // Fallback: infer from damage type (auto-attacks, unhooked spells, etc.)
    ability ??= AbilityResolver.InferFromDamageType(_dmgType);

    // Capture debug info if enabled
    var debugInfo = AbilityResolver.CaptureDebugInfoIfEnabled(
      "Character.DamageMe",
      new Dictionary<string, string>
      {
        ["damageType"] = _dmgType.ToString(),
        ["amount"] = __result.ToString(),
        ["incdmg"] = _incdmg.ToString(),
        ["critical"] = _criticalHit.ToString(),
        ["fromPlayer"] = _fromPlayer.ToString(),
      },
      ability,
      CaptureDebugForUnknown,
      CaptureDebugForAll,
      LogDebug
    );

    // Create and emit the event
    var evt = EventBuilder.CreateDamageEvent(
      EventType.DamagePhysical,
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
        $"Physical damage: {evt.Source?.Name ?? "Unknown"} -> {evt.Target?.Name ?? "Unknown"} "
          + $"for {amount} ({damageType})"
          + (flags.Critical == true ? " [CRIT]" : "")
          + (flags.Missed == true ? " [MISS]" : "")
          + (flags.Absorbed == true ? " [ABSORBED]" : "")
      );
      Emitter.Emit(evt);

      // Notify session manager of combat event
      SessionManager?.OnCombatEvent(evt.EventType, evt.Timestamp);
    }
  }
}
