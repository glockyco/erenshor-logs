using ErenshorLogs.Context;
using ErenshorLogs.Events;
using HarmonyLib;

namespace ErenshorLogs.Hooks;

[HarmonyPatch(typeof(NPCFightEvent), "FixedUpdate")]
public static class NpcFightEventHealContextPatch
{
  private static readonly AccessTools.FieldRef<NPC, Character> NpcCharacterRef =
    AccessTools.FieldRefAccess<NPC, Character>("Myself");

  private static readonly AbilityRef Ability = new()
  {
    Name = "Boss Regeneration",
    Type = AbilityType.Unknown,
    StableKey = "mechanic:boss-regeneration",
  };

  [HarmonyPrefix]
  public static void Prefix(NPC ___MyNPC, out IDisposable? __state)
  {
    __state = HealingContext.Push(
      ___MyNPC == null ? null : NpcCharacterRef(___MyNPC),
      Ability,
      EventType.HealSpell,
      AttributionMethod.Context
    );
  }

  [HarmonyFinalizer]
  public static void Finalizer(IDisposable? __state)
  {
    __state?.Dispose();
  }
}

[HarmonyPatch(typeof(SiraetheEvent), "Update")]
public static class SiraetheHealContextPatch
{
  private static readonly AccessTools.FieldRef<SiraetheEvent, Character> MyselfRef =
    AccessTools.FieldRefAccess<SiraetheEvent, Character>("Myself");

  private static readonly AbilityRef Ability = new()
  {
    Name = "Siraethe Ward Heal",
    Type = AbilityType.Unknown,
    StableKey = "mechanic:siraethe-ward-heal",
  };

  [HarmonyPrefix]
  public static void Prefix(SiraetheEvent __instance, out IDisposable? __state)
  {
    __state = HealingContext.Push(
      __instance == null ? null : MyselfRef(__instance),
      Ability,
      EventType.HealSpell,
      AttributionMethod.Context
    );
  }

  [HarmonyFinalizer]
  public static void Finalizer(IDisposable? __state)
  {
    __state?.Dispose();
  }
}
