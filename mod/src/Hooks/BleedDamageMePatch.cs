using ErenshorLogs.Context;
using ErenshorLogs.Events;
using ErenshorLogs.Session;
using HarmonyLib;

namespace ErenshorLogs.Hooks;

/// <summary>
/// Harmony postfix patch for Character.BleedDamageMe to capture DoT tick damage events.
/// </summary>
[HarmonyPatch(typeof(Character), nameof(Character.BleedDamageMe))]
public static class BleedDamageMePatch
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
  /// Postfix hook that captures DoT tick events after BleedDamageMe completes.
  /// </summary>
  /// <param name="__instance">The Character that received damage (target).</param>
  /// <param name="__result">The return value from BleedDamageMe.</param>
  /// <param name="_incdmg">The incoming DoT tick damage.</param>
  /// <param name="_fromPlayer">Whether damage originated from player faction.</param>
  /// <param name="_attacker">The Character that applied the DoT (source).</param>
  [HarmonyPostfix]
  public static void Postfix(
    Character __instance,
    int __result,
    int _incdmg,
    bool _fromPlayer,
    Character _attacker
  )
  {
    // Skip if not initialized
    if (EventBuilder == null || Emitter == null)
      return;

    // Skip if characters are destroyed Unity objects
    if (!__instance.IsValid())
    {
      LogDebug?.Invoke("[BleedDamageMePatch] Skipping event - target destroyed");
      return;
    }

    if (!_attacker.IsValid())
    {
      LogDebug?.Invoke("[BleedDamageMePatch] Skipping event - attacker destroyed");
      return;
    }

    // Skip if not relevant to player's group
    if (RelevanceChecker != null && !RelevanceChecker.IsRelevantCombat(_attacker, __instance))
      return;

    // Skip non-loggable results (negative values indicate skip conditions)
    // BleedDamageMe only returns positive values for actual damage
    if (__result <= 0)
      return;

    // DoT ticks are always physical damage, no critical hits
    var flags = new EventFlags { FromPlayer = _fromPlayer ? true : null };

    // Resolve ability: try context first, then DoT slot tracking (if in TickEffects)
    var ability = AbilityResolver.FromContext();

    if (ability == null && TickEffectsSlotTracker.IsInTickEffects())
    {
      // We're in TickEffects - try to identify the bleed slot
      var slot = TickEffectsSlotTracker.FindAndAdvanceSlot(
        __instance,
        GameData.DamageType.Physical,
        isBleed: true,
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

    // Fallback: if not in TickEffects or slot tracking failed, mark as Unknown
    ability ??= new AbilityRef
    {
      Name = "Unknown",
      Type = AbilityType.Unknown,
      StableKey = null,
    };

    // Capture debug info if enabled
    var debugInfo = AbilityResolver.CaptureDebugInfoIfEnabled(
      "Character.BleedDamageMe",
      new Dictionary<string, string>
      {
        ["amount"] = __result.ToString(),
        ["incdmg"] = _incdmg.ToString(),
        ["fromPlayer"] = _fromPlayer.ToString(),
      },
      ability,
      CaptureDebugForUnknown,
      CaptureDebugForAll,
      LogDebug
    );

    // Create and emit the event
    CombatEventDispatcher.PrepareForCapture(
      EventType.DamageDot,
      SessionManager,
      DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
    );

    var evt = EventBuilder.CreateDamageEvent(
      EventType.DamageDot,
      target: __instance,
      source: _attacker,
      amount: __result,
      damageType: DamageType.Physical,
      ability: ability,
      flags: flags,
      debugInfo: debugInfo
    );

    if (evt != null)
    {
      LogDebug?.Invoke(
        $"DoT damage: {evt.Source?.Name ?? "Unknown"} -> {evt.Target?.Name ?? "Unknown"} "
          + $"for {__result}"
      );
      CombatEventDispatcher.Dispatch(evt, Emitter);
    }
  }
}
