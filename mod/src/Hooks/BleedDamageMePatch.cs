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

    // Skip if not relevant to player's group
    if (RelevanceChecker != null && !RelevanceChecker.IsRelevantCombat(_attacker, __instance))
      return;

    // Ensure session exists before emitting event
    SessionManager?.EnsureSessionStarted(EventType.DamageDot);

    // Skip non-loggable results (negative values indicate skip conditions)
    // BleedDamageMe only returns positive values for actual damage
    if (__result <= 0)
      return;

    // DoT ticks are always physical damage, no critical hits
    var flags = new EventFlags { FromPlayer = _fromPlayer ? true : null };

    // Try to resolve ability from context (if TickEffects hook provides it)
    // Otherwise fall back to generic "DoT Tick"
    // Issue #11 tracks full DoT attribution with EffectTracker integration
    var ability =
      AbilityResolver.FromContext() ?? AbilityResolver.CreateFixed("DoT Tick", AbilityType.Dot);

    // Create and emit the event
    var evt = EventBuilder.CreateDamageEvent(
      EventType.DamageDot,
      target: __instance,
      source: _attacker,
      amount: __result,
      damageType: DamageType.Physical,
      ability: ability,
      flags: flags
    );

    if (evt != null)
    {
      LogDebug?.Invoke(
        $"DoT damage: {evt.Source?.Name ?? "Unknown"} -> {evt.Target?.Name ?? "Unknown"} "
          + $"for {__result}"
      );
      Emitter.Emit(evt);
    }
  }
}
