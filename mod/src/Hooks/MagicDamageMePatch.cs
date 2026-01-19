using ErenshorLogs.Events;
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

    // Create and emit the event
    var evt = EventBuilder.CreateDamageEvent(
      EventType.DamageMagic,
      target: __instance,
      source: _attacker,
      amount: amount,
      damageType: damageType,
      flags: flags
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
