using ErenshorLogs.Events;
using HarmonyLib;

namespace ErenshorLogs.Hooks;

internal static class ResourceEventCapture
{
  internal static void EmitManaEvent(
    EventType eventType,
    Character target,
    Character? source,
    AbilityRef ability,
    int before,
    int after,
    int max
  )
  {
    if (HealMePatch.EventBuilder == null || HealMePatch.Emitter == null)
      return;

    if (!target.IsValid())
      return;

    if (source != null && !source.IsValid())
      source = null;

    if (
      HealMePatch.RelevanceChecker != null
      && !HealMePatch.RelevanceChecker.IsRelevantCombat(source, target)
    )
      return;

    var delta = after - before;
    if (delta == 0)
      return;

    CombatEventDispatcher.PrepareForCapture(
      eventType,
      HealMePatch.SessionManager,
      DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
    );

    var evt = HealMePatch.EventBuilder.CreateResourceEvent(
      eventType,
      target,
      source,
      ability,
      resourceType: "mana",
      delta,
      current: after,
      max
    );

    if (evt == null)
      return;

    HealMePatch.LogDebug?.Invoke(
      $"Mana {delta}: {evt.Source?.Name ?? "Unknown"} -> {evt.Target?.Name ?? "Unknown"}"
    );
    CombatEventDispatcher.Dispatch(
      evt with
      {
        Attribution = AttributionMethod.Verified,
      },
      HealMePatch.Emitter
    );
  }
}

public readonly record struct ManaSnapshot(Character Target, int Before, int Max)
{
  public static ManaSnapshot? FromCharacter(Character? target)
  {
    if (target == null || target.MyStats == null)
      return null;

    return new ManaSnapshot(target, target.MyStats.CurrentMana, target.MyStats.GetCurrentMaxMana());
  }
}

[HarmonyPatch(typeof(AEManaDrainEvent), "Update")]
public static class AEManaDrainEventPatch
{
  [HarmonyPrefix]
  public static void Prefix(
    AEManaDrainEvent __instance,
    NPC ___MyNPC,
    out IReadOnlyList<ManaSnapshot> __state
  )
  {
    __state = SnapshotAggroTable(___MyNPC);
  }

  [HarmonyPostfix]
  public static void Postfix(
    AEManaDrainEvent __instance,
    Character ___MyChar,
    IReadOnlyList<ManaSnapshot> __state
  )
  {
    var ability = new AbilityRef
    {
      Name = __instance.DamageReason ?? "Mana Drain",
      Type = AbilityType.AreaEffect,
      StableKey = "mechanic:mana-drain",
    };

    foreach (var snapshot in __state)
    {
      var stats = snapshot.Target.MyStats;
      if (stats == null)
        continue;

      var after = stats.CurrentMana;
      if (after >= snapshot.Before)
        continue;

      ResourceEventCapture.EmitManaEvent(
        EventType.ManaUse,
        snapshot.Target,
        ___MyChar,
        ability,
        snapshot.Before,
        after,
        snapshot.Max
      );
    }
  }

  private static IReadOnlyList<ManaSnapshot> SnapshotAggroTable(NPC? npc)
  {
    if (npc?.AggroTable == null)
      return Array.Empty<ManaSnapshot>();

    var snapshots = new List<ManaSnapshot>();
    foreach (var slot in npc.AggroTable)
    {
      var snapshot = ManaSnapshot.FromCharacter(slot.Player);
      if (snapshot.HasValue)
        snapshots.Add(snapshot.Value);
    }
    return snapshots;
  }
}

[HarmonyPatch(typeof(FernallaFightEvent), "PhaseHandler")]
public static class FernallaManaRestorePatch
{
  private static readonly AbilityRef Ability = new()
  {
    Name = "Fernalla Mana Burst",
    Type = AbilityType.AreaEffect,
    StableKey = "mechanic:fernalla-mana-burst",
  };

  [HarmonyPrefix]
  public static void Prefix(out IReadOnlyList<ManaSnapshot> __state)
  {
    __state = SnapshotPartyMana();
  }

  [HarmonyPostfix]
  public static void Postfix(FernallaFightEvent __instance, IReadOnlyList<ManaSnapshot> __state)
  {
    var source = __instance?.MyChar;
    foreach (var snapshot in __state)
    {
      var stats = snapshot.Target.MyStats;
      if (stats == null)
        continue;

      var after = stats.CurrentMana;
      if (after <= snapshot.Before)
        continue;

      ResourceEventCapture.EmitManaEvent(
        EventType.ManaRestore,
        snapshot.Target,
        source,
        Ability,
        snapshot.Before,
        after,
        snapshot.Max
      );
    }
  }

  private static IReadOnlyList<ManaSnapshot> SnapshotPartyMana()
  {
    var snapshots = new List<ManaSnapshot>();
    var player = ManaSnapshot.FromCharacter(GameData.PlayerStats?.Myself);
    if (player.HasValue)
      snapshots.Add(player.Value);

    foreach (var member in GameData.GroupMembers)
    {
      var snapshot = ManaSnapshot.FromCharacter(member?.MyAvatar?.MyStats?.Myself);
      if (snapshot.HasValue)
        snapshots.Add(snapshot.Value);
    }
    return snapshots;
  }
}
