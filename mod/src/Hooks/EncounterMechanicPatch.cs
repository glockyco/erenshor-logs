using System.Collections;
using ErenshorLogs.Events;
using ErenshorLogs.Session;
using HarmonyLib;

namespace ErenshorLogs.Hooks;

public static class MechanicAffectedStats
{
  public static string? Normalize(string? gameStat)
  {
    return gameStat switch
    {
      "tickDmg" => "damage",
      "ResistMod" => "resist",
      "hp" or "mana" or "damage" or "resist" or "armorPen" => gameStat,
      _ => null,
    };
  }
}

internal static class EncounterMechanicEmitter
{
  internal static ICombatEventBuilder? EventBuilder { get; set; }
  internal static IEventEmitter? Emitter { get; set; }
  internal static ICombatRelevanceChecker? RelevanceChecker { get; set; }
  internal static ISessionManager? SessionManager { get; set; }
  internal static Action<string>? LogDebug { get; set; }

  internal static void Emit(
    Character? source,
    Character? target,
    string name,
    string action,
    object? value = null,
    object? previousValue = null,
    string? affectedStat = null,
    int? amount = null,
    string? stableKey = null
  )
  {
    if (EventBuilder == null || Emitter == null)
      return;

    if (source != null && !source.IsValid())
      source = null;
    if (target != null && !target.IsValid())
      target = null;

    if (
      RelevanceChecker != null
      && (source != null || target != null)
      && !RelevanceChecker.IsRelevantCombat(source, target)
    )
      return;

    var ability = new AbilityRef
    {
      Name = name,
      Type = AbilityType.AreaEffect,
      StableKey = stableKey,
    };
    var mechanic = new MechanicData
    {
      Name = name,
      Action = action,
      Value = value,
      PreviousValue = previousValue,
      AffectedStat = MechanicAffectedStats.Normalize(affectedStat),
      Amount = amount,
    };

    CombatEventDispatcher.PrepareForCapture(
      EventType.Mechanic,
      SessionManager,
      DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
    );

    var evt = EventBuilder.CreateMechanicEvent(target, source, ability, mechanic);
    if (evt == null)
      return;

    CombatEventDispatcher.Dispatch(evt with { Attribution = AttributionMethod.Verified }, Emitter);
    LogDebug?.Invoke($"Mechanic event captured: {name}/{action}");
  }
}

public sealed record AeSnapshot(int TickDamage, int ResistMod, float TickTime, bool TriggerOnly)
{
  public static AeSnapshot From(AEEvent? aeEvent)
  {
    return aeEvent == null
      ? new AeSnapshot(0, 0, 0, false)
      : new AeSnapshot(aeEvent.tickDmg, aeEvent.ResistMod, aeEvent.TickTime, aeEvent.TriggerOnly);
  }
}

public sealed record SprinklesSnapshot(
  bool Invulnerable,
  AeSnapshot Offensive,
  AeSnapshot Lifetap,
  Character[] LivingWards
);

[HarmonyPatch(typeof(SprinklesEvent), "Update")]
public static class SprinklesInvulnerabilityPatch
{
  [HarmonyPrefix]
  public static void Prefix(SprinklesEvent __instance, out bool __state)
  {
    __state = __instance?.Sprinkles != null && __instance.Sprinkles.Invulnerable;
  }

  [HarmonyPostfix]
  public static void Postfix(SprinklesEvent __instance, bool __state)
  {
    var sprinkles = __instance?.Sprinkles;
    if (sprinkles == null || sprinkles.Invulnerable == __state)
      return;

    EncounterMechanicEmitter.Emit(
      sprinkles,
      sprinkles,
      "Sprinkles Wards",
      "invulnerability",
      sprinkles.Invulnerable,
      __state,
      stableKey: "mechanic:sprinkles-wards"
    );
  }
}

[HarmonyPatch(typeof(SprinklesEvent), "CleanList")]
public static class SprinklesMechanicPatch
{
  [HarmonyPrefix]
  public static void Prefix(
    SprinklesEvent __instance,
    List<Character> ___LivingWards,
    out SprinklesSnapshot __state
  )
  {
    __state = new SprinklesSnapshot(
      __instance?.Sprinkles?.Invulnerable ?? false,
      AeSnapshot.From(__instance?.SprinklesOffensiveEvent),
      AeSnapshot.From(__instance?.SprinklesLifetapEvent),
      ___LivingWards?.Where(ward => ward != null).ToArray() ?? []
    );
  }

  [HarmonyPostfix]
  public static void Postfix(SprinklesEvent __instance, SprinklesSnapshot __state)
  {
    var source = __instance?.Sprinkles;
    EmitAeChanges(
      source,
      "Sprinkles Offensive Growth",
      "mechanic:sprinkles-offensive-growth",
      __state.Offensive,
      AeSnapshot.From(__instance?.SprinklesOffensiveEvent)
    );
    EmitAeChanges(
      source,
      "Sprinkles Lifetap Growth",
      "mechanic:sprinkles-lifetap-growth",
      __state.Lifetap,
      AeSnapshot.From(__instance?.SprinklesLifetapEvent)
    );

    foreach (var ward in __state.LivingWards)
    {
      if (ward != null && !ward.Alive)
      {
        EncounterMechanicEmitter.Emit(
          source,
          ward,
          "Sprinkles Ward",
          "despawn",
          stableKey: "mechanic:sprinkles-ward"
        );
      }
    }
  }

  private static void EmitAeChanges(
    Character? source,
    string name,
    string stableKey,
    AeSnapshot before,
    AeSnapshot after
  )
  {
    EmitStatChange(source, name, stableKey, "tickDmg", before.TickDamage, after.TickDamage);
    EmitStatChange(source, name, stableKey, "ResistMod", before.ResistMod, after.ResistMod);
  }

  private static void EmitStatChange(
    Character? source,
    string name,
    string stableKey,
    string stat,
    int before,
    int after
  )
  {
    if (before == after)
      return;

    EncounterMechanicEmitter.Emit(
      source,
      source,
      name,
      "statChange",
      after,
      before,
      stat,
      after - before,
      stableKey
    );
  }
}

[HarmonyPatch(typeof(SprinklesEvent), "spawnWards")]
public static class SprinklesWardSpawnPatch
{
  [HarmonyPrefix]
  public static void Prefix() { }

  [HarmonyPostfix]
  public static void Postfix(SprinklesEvent __instance, ref IEnumerator __result)
  {
    __result = Wrap(__instance, __result);
  }

  private static IEnumerator Wrap(SprinklesEvent source, IEnumerator inner)
  {
    while (true)
    {
      var looseAdds = GameData.RaidManager?.LooseAdds;
      var before = looseAdds?.Count ?? 0;
      if (!inner.MoveNext())
        yield break;

      if (looseAdds != null && looseAdds.Count > before)
      {
        for (var index = before; index < looseAdds.Count; index++)
        {
          EncounterMechanicEmitter.Emit(
            source?.Sprinkles,
            looseAdds[index],
            "Sprinkles Ward",
            "spawn",
            stableKey: "mechanic:sprinkles-ward"
          );
        }
      }

      yield return inner.Current;
    }
  }
}

[HarmonyPatch(typeof(DPSCheckAEEvent), "Update")]
public static class DpsCheckAeMechanicPatch
{
  [HarmonyPrefix]
  public static void Prefix(AEEvent ___myEvent, out AeSnapshot __state)
  {
    __state = AeSnapshot.From(___myEvent);
  }

  [HarmonyPostfix]
  public static void Postfix(AEEvent ___myEvent, Stats ___myStats, AeSnapshot __state)
  {
    var after = AeSnapshot.From(___myEvent);
    EmitStatChange(
      ___myStats?.Myself,
      "DPS Check Growth",
      "tickDmg",
      __state.TickDamage,
      after.TickDamage
    );
    EmitStatChange(
      ___myStats?.Myself,
      "DPS Check Growth",
      "ResistMod",
      __state.ResistMod,
      after.ResistMod
    );
  }

  private static void EmitStatChange(
    Character? source,
    string name,
    string stat,
    int before,
    int after
  )
  {
    if (before == after)
      return;

    EncounterMechanicEmitter.Emit(
      source,
      source,
      name,
      "statChange",
      after,
      before,
      stat,
      after - before,
      "mechanic:dps-check-growth"
    );
  }
}

[HarmonyPatch(typeof(FaithEvent), "DoEventScript")]
public static class FaithEventMechanicPatch
{
  [HarmonyPrefix]
  public static void Prefix(out int __state)
  {
    __state = GameData.RaidManager?.LooseAdds?.Count ?? 0;
  }

  [HarmonyPostfix]
  public static void Postfix(FaithEvent __instance, int __state)
  {
    var looseAdds = GameData.RaidManager?.LooseAdds;
    if (looseAdds == null || looseAdds.Count <= __state)
      return;

    for (var index = __state; index < looseAdds.Count; index++)
    {
      EncounterMechanicEmitter.Emit(
        __instance?.Faith,
        looseAdds[index],
        "Faith Heal Add",
        "spawn",
        stableKey: "mechanic:faith-heal-add"
      );
    }
  }
}

[HarmonyPatch(typeof(MizukiEvent), "DoFinal")]
public static class MizukiFinalPhasePatch
{
  [HarmonyPrefix]
  public static void Prefix(MizukiEvent __instance, out AeSnapshot __state)
  {
    __state = AeSnapshot.From(__instance?.MizAE);
  }

  [HarmonyPostfix]
  public static void Postfix(MizukiEvent __instance, AeSnapshot __state)
  {
    var source = __instance?.MizChar;
    var after = AeSnapshot.From(__instance?.MizAE);
    EncounterMechanicEmitter.Emit(
      source,
      source,
      "Mizuki Final Phase",
      "phase",
      "final",
      stableKey: "mechanic:mizuki-final"
    );
    EmitStatChange(source, "Mizuki Final Phase", "tickDmg", __state.TickDamage, after.TickDamage);
    EmitStatChange(source, "Mizuki Final Phase", "TickTime", __state.TickTime, after.TickTime);
    if (__state.TriggerOnly != after.TriggerOnly)
    {
      EncounterMechanicEmitter.Emit(
        source,
        source,
        "Mizuki Final Phase",
        "statChange",
        after.TriggerOnly,
        __state.TriggerOnly,
        "TriggerOnly",
        stableKey: "mechanic:mizuki-final"
      );
    }
  }

  private static void EmitStatChange(
    Character? source,
    string name,
    string stat,
    int before,
    int after
  )
  {
    if (before == after)
      return;

    EncounterMechanicEmitter.Emit(
      source,
      source,
      name,
      "statChange",
      after,
      before,
      stat,
      after - before,
      "mechanic:mizuki-final"
    );
  }

  private static void EmitStatChange(
    Character? source,
    string name,
    string stat,
    float before,
    float after
  )
  {
    if (Math.Abs(before - after) < 0.001f)
      return;

    EncounterMechanicEmitter.Emit(
      source,
      source,
      name,
      "statChange",
      after,
      before,
      stat,
      stableKey: "mechanic:mizuki-final"
    );
  }
}
