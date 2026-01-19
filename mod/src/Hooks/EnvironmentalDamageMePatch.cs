using ErenshorLogs.Events;
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
  /// Postfix hook that captures environmental damage events after EnvironmentalDamageMe completes.
  /// </summary>
  /// <param name="__instance">The Character that received damage (target).</param>
  /// <param name="__result">The return value from EnvironmentalDamageMe.</param>
  /// <param name="_dmg">The damage amount.</param>
  [HarmonyPostfix]
  public static void Postfix(Character __instance, int __result, int _dmg)
  {
    // Skip if not initialized
    if (EventBuilder == null || Emitter == null)
      return;

    // Skip non-loggable results (-1 = dead/invulnerable)
    if (__result <= 0)
      return;

    // Environmental damage has no source actor and is always physical
    var evt = EventBuilder.CreateDamageEvent(
      EventType.DamageEnvironmental,
      target: __instance,
      source: null,
      amount: __result,
      damageType: DamageType.Physical,
      flags: null
    );

    if (evt != null)
    {
      LogDebug?.Invoke($"Environmental damage: {evt.Target?.Name ?? "Unknown"} took {__result}");
      Emitter.Emit(evt);
    }
  }
}
