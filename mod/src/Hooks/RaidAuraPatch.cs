using HarmonyLib;

namespace ErenshorLogs.Hooks;

internal static class RaidAuraContext
{
  private static readonly Dictionary<Spell, Character> Owners = new();

  internal static Character? Resolve(Spell? spell)
  {
    if (spell == null)
      return null;

    return Owners.TryGetValue(spell, out var owner) ? owner : null;
  }

  internal static void Begin(RaidManager raid)
  {
    Owners.Clear();
    Register(GameData.PlayerStats?.MyAura, GameData.PlayerStats?.Myself);
    RegisterGroup(raid.Group1);
    RegisterGroup(raid.Group2);
    RegisterGroup(raid.Group3);
  }

  internal static void End()
  {
    Owners.Clear();
  }

  private static void RegisterGroup(IEnumerable<RaidMemberSlot> slots)
  {
    foreach (var slot in slots)
      Register(slot?.AssignedAvatar?.MyStats?.MyAura, slot?.AssignedAvatar?.MyStats?.Myself);
  }

  private static void Register(Spell? spell, Character? owner)
  {
    if (spell != null && owner != null && !Owners.ContainsKey(spell))
      Owners.Add(spell, owner);
  }
}

[HarmonyPatch(typeof(RaidManager), "UpdateGroupAuras")]
public static class RaidAuraPatch
{
  [HarmonyPrefix]
  public static void Prefix(RaidManager __instance, out bool __state)
  {
    __state = false;
    if (__instance == null)
      return;

    RaidAuraContext.Begin(__instance);
    __state = true;
  }

  [HarmonyFinalizer]
  public static void Finalizer(bool __state)
  {
    if (__state)
      RaidAuraContext.End();
  }
}
