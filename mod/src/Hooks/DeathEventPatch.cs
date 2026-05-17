using ErenshorLogs.Context;
using ErenshorLogs.Events;
using ErenshorLogs.Session;
using HarmonyLib;

namespace ErenshorLogs.Hooks;

[HarmonyPatch(typeof(Character), "DoDeath")]
public static class DeathEventPatch
{
  internal static ICombatEventBuilder? EventBuilder { get; set; }
  internal static IEventEmitter? Emitter { get; set; }
  internal static ICombatRelevanceChecker? RelevanceChecker { get; set; }
  internal static ISessionManager? SessionManager { get; set; }
  internal static Action<string>? LogDebug { get; set; }

  public static AbilityRef FallbackAbility { get; } =
    new()
    {
      Name = "Death",
      Type = AbilityType.Unknown,
      StableKey = null,
    };

  [HarmonyPrefix]
  public static void Prefix(Character __instance, out bool __state)
  {
    __state = __instance != null && __instance.Alive;
  }

  [HarmonyPostfix]
  public static void Postfix(Character __instance, bool __state)
  {
    if (!__state || __instance == null || __instance.Alive)
      return;

    if (EventBuilder == null || Emitter == null)
      return;

    if (RelevanceChecker != null && !RelevanceChecker.IsRelevantCombat(null, __instance))
      return;

    var ability = AbilityResolver.FromContext() ?? FallbackAbility;

    CombatEventDispatcher.PrepareForCapture(
      EventType.Death,
      SessionManager,
      DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
    );

    var evt = EventBuilder.CreateDeathEvent(__instance, null, ability, killingBlowEventSeq: null);
    if (evt == null)
      return;

    var killingBlowSeq = KillingBlowTracker.GetLatestDamageEventSeq(evt.Target?.Id);
    if (killingBlowSeq != null)
      evt = evt with { KillingBlowEventSeq = killingBlowSeq };

    CombatEventDispatcher.Dispatch(evt, Emitter);
    LogDebug?.Invoke($"Death event captured for {__instance.name}");
  }
}
