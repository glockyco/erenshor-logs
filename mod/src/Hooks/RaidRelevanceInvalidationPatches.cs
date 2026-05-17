using HarmonyLib;

namespace ErenshorLogs.Hooks;

public static class RaidRelevanceInvalidation
{
  public static Action? ClearCache { get; set; }
}

public static class RaidRelevanceInvalidationPatches
{
  public static void OnRaidStateChanged()
  {
    RaidRelevanceInvalidation.ClearCache?.Invoke();
  }

  [HarmonyPatch(typeof(RaidManager), "AddToRoster")]
  private static class AddToRosterPatch
  {
    [HarmonyPostfix]
    private static void Postfix() => OnRaidStateChanged();
  }

  [HarmonyPatch(typeof(RaidManager), "AssignToSpecificSlot")]
  private static class AssignToSpecificSlotPatch
  {
    [HarmonyPostfix]
    private static void Postfix() => OnRaidStateChanged();
  }

  [HarmonyPatch(typeof(RaidManager), "DismissRaider")]
  private static class DismissRaiderPatch
  {
    [HarmonyPostfix]
    private static void Postfix() => OnRaidStateChanged();
  }

  [HarmonyPatch(typeof(RaidManager), "DismissAllRaiders")]
  private static class DismissAllRaidersPatch
  {
    [HarmonyPostfix]
    private static void Postfix() => OnRaidStateChanged();
  }

  [HarmonyPatch(typeof(RaidManager), "AssignTargetToGroup")]
  private static class AssignTargetToGroupPatch
  {
    [HarmonyPostfix]
    private static void Postfix() => OnRaidStateChanged();
  }

  [HarmonyPatch(typeof(RaidManager), "AssignUrgentTarget")]
  private static class AssignUrgentTargetPatch
  {
    [HarmonyPostfix]
    private static void Postfix() => OnRaidStateChanged();
  }

  [HarmonyPatch(typeof(RaidManager), "ClearBurnTarg")]
  private static class ClearBurnTargPatch
  {
    [HarmonyPostfix]
    private static void Postfix() => OnRaidStateChanged();
  }
}
