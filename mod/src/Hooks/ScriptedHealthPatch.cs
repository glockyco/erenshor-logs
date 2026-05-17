using ErenshorLogs.Events;
using ErenshorLogs.Session;
using HarmonyLib;

namespace ErenshorLogs.Hooks;

internal static class HealthEventCapture
{
  internal static ICombatEventBuilder? EventBuilder => HealMePatch.EventBuilder;
  internal static IEventEmitter? Emitter => HealMePatch.Emitter;
  internal static ICombatRelevanceChecker? RelevanceChecker => HealMePatch.RelevanceChecker;
  internal static ISessionManager? SessionManager => HealMePatch.SessionManager;
  internal static Action<string>? LogDebug => HealMePatch.LogDebug;

  internal static void EmitHeal(
    EventType eventType,
    Character target,
    Character? source,
    AbilityRef ability,
    int amount,
    int rawAmount,
    int overhealAmount,
    MechanicData? mechanic
  )
  {
    if (EventBuilder == null || Emitter == null)
      return;

    if (!target.IsValid())
      return;

    if (source != null && !source.IsValid())
      source = null;

    if (RelevanceChecker != null && !RelevanceChecker.IsRelevantCombat(source, target))
      return;

    CombatEventDispatcher.PrepareForCapture(
      eventType,
      SessionManager,
      DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
    );

    var evt = EventBuilder.CreateHealEvent(
      eventType,
      target,
      source,
      ability,
      amount,
      rawAmount,
      overhealAmount
    );

    if (evt == null)
      return;

    evt = evt with
    {
      Mechanic = mechanic,
      Attribution = mechanic != null ? AttributionMethod.Verified : AttributionMethod.Context,
    };

    LogDebug?.Invoke(
      $"Heal: {evt.Source?.Name ?? "Unknown"} -> {evt.Target?.Name ?? "Unknown"} for {amount}"
    );
    CombatEventDispatcher.Dispatch(evt, Emitter);
  }

  internal static void EmitScriptedHeal(
    Character target,
    AbilityRef ability,
    HealthSnapshot before,
    string mechanicName
  )
  {
    if (!before.IsValid || !target.IsValid())
      return;

    var stats = target.MyStats;
    if (stats == null)
      return;

    var after = stats.CurrentHP;
    var effective = Math.Max(0, after - before.BeforeHp);
    if (effective <= 0)
      return;

    var raw = Math.Max(before.RawAmount, effective);
    EmitHeal(
      EventType.HealSpell,
      target,
      source: target,
      ability,
      effective,
      raw,
      Math.Max(0, raw - effective),
      new MechanicData { Name = mechanicName, Action = "scripted" }
    );
  }
}

[HarmonyPatch(typeof(GraceEvent), "DoEventScript")]
public static class GraceEventHealthPatch
{
  private static readonly AbilityRef Ability = new()
  {
    Name = "Grace Echoes",
    Type = AbilityType.AreaEffect,
    StableKey = "mechanic:grace-echoes",
  };

  [HarmonyPrefix]
  public static void Prefix(GraceEvent __instance, out HealthSnapshot __state)
  {
    __state = HealthSnapshot.FromStats(__instance?.Grace?.MyStats, rawAmount: 0);
  }

  [HarmonyPostfix]
  public static void Postfix(GraceEvent __instance, HealthSnapshot __state)
  {
    var grace = __instance?.Grace;
    if (grace == null)
      return;

    HealthEventCapture.EmitScriptedHeal(grace, Ability, __state, "Grace Echoes");
  }
}

[HarmonyPatch(typeof(FernallaFightEvent), "PhaseHandler")]
public static class FernallaPhaseHealthPatch
{
  private static readonly AbilityRef Ability = new()
  {
    Name = "Fernalla Phase Shift",
    Type = AbilityType.AreaEffect,
    StableKey = "mechanic:fernalla-phase-shift",
  };

  [HarmonyPrefix]
  public static void Prefix(FernallaFightEvent __instance, out HealthSnapshot __state)
  {
    __state = HealthSnapshot.FromStats(__instance?.MyChar?.MyStats, rawAmount: 0);
  }

  [HarmonyPostfix]
  public static void Postfix(FernallaFightEvent __instance, HealthSnapshot __state)
  {
    var fernalla = __instance?.MyChar;
    if (fernalla == null)
      return;

    HealthEventCapture.EmitScriptedHeal(fernalla, Ability, __state, "Fernalla phase shift");
  }
}

[HarmonyPatch(typeof(LighthouseHealBox), "OnTriggerEnter")]
public static class LighthouseHealPatch
{
  private static readonly AbilityRef Ability = new()
  {
    Name = "Lighthouse Heal",
    Type = AbilityType.AreaEffect,
    StableKey = "mechanic:lighthouse-heal",
  };

  [HarmonyPrefix]
  public static void Prefix(object other, out HealthSnapshot __state)
  {
    __state = HealthSnapshot.FromStats(GetStatsFromCollider(other), rawAmount: 0);
  }

  [HarmonyPostfix]
  public static void Postfix(object other, HealthSnapshot __state)
  {
    var target = GetStatsFromCollider(other)?.Myself;
    if (target == null)
      return;

    HealthEventCapture.EmitScriptedHeal(target, Ability, __state, "Lighthouse heal");
  }

  private static Stats? GetStatsFromCollider(object? collider)
  {
    if (collider == null)
      return null;

    var method = collider
      .GetType()
      .GetMethods()
      .FirstOrDefault(method =>
        method.Name == "GetComponent"
        && method.IsGenericMethodDefinition
        && method.GetParameters().Length == 0
      );

    return method?.MakeGenericMethod(typeof(Stats)).Invoke(collider, null) as Stats;
  }
}
