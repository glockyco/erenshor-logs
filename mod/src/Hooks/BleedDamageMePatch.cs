using ErenshorLogs.Events;
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

    // Skip non-loggable results (negative values indicate skip conditions)
    // BleedDamageMe only returns positive values for actual damage
    if (__result <= 0)
      return;

    // DoT ticks are always physical damage, no critical hits
    var flags = new EventFlags { FromPlayer = _fromPlayer ? true : null };

    // Create and emit the event
    var evt = EventBuilder.CreateDamageEvent(
      EventType.DamageDot,
      target: __instance,
      source: _attacker,
      amount: __result,
      damageType: DamageType.Physical,
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
