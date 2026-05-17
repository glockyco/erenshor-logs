using ErenshorLogs.Context;
using HarmonyLib;

namespace ErenshorLogs.Hooks;

internal static class AddStatusEffectRegistration
{
  internal static EffectTracker? Tracker { get; set; }

  internal static void Register(Stats stats, Spell spell, int slot)
  {
    if (Tracker == null || stats == null || spell == null)
      return;

    if (slot >= 0 && slot < 30)
    {
      Tracker.RegisterEffect(stats.Myself, slot, spell);
    }
  }
}

/// <summary>
/// Harmony patch for the 3-parameter Stats.AddStatusEffect overload.
/// </summary>
[HarmonyPatch(
  typeof(Stats),
  nameof(Stats.AddStatusEffect),
  new[] { typeof(Spell), typeof(bool), typeof(int) }
)]
public static class AddStatusEffectThreeArgPatch
{
  [HarmonyPostfix]
  public static void Postfix(Stats __instance, Spell spell, int __result)
  {
    AddStatusEffectRegistration.Register(__instance, spell, __result);
  }
}

/// <summary>
/// Harmony patch for the 4-parameter Stats.AddStatusEffect overload.
/// </summary>
[HarmonyPatch(
  typeof(Stats),
  nameof(Stats.AddStatusEffect),
  new[] { typeof(Spell), typeof(bool), typeof(int), typeof(Character) }
)]
public static class AddStatusEffectPatch
{
  internal static EffectTracker? Tracker
  {
    get => AddStatusEffectRegistration.Tracker;
    set => AddStatusEffectRegistration.Tracker = value;
  }

  [HarmonyPostfix]
  public static void Postfix(Stats __instance, Spell spell, int __result)
  {
    AddStatusEffectRegistration.Register(__instance, spell, __result);
  }
}

/// <summary>
/// Harmony patch for the 5-parameter Stats.AddStatusEffect overload.
/// </summary>
[HarmonyPatch(
  typeof(Stats),
  nameof(Stats.AddStatusEffect),
  new[] { typeof(Spell), typeof(bool), typeof(int), typeof(Character), typeof(float) }
)]
public static class AddStatusEffectFiveArgPatch
{
  [HarmonyPostfix]
  public static void Postfix(Stats __instance, Spell spell, int __result)
  {
    AddStatusEffectRegistration.Register(__instance, spell, __result);
  }
}
