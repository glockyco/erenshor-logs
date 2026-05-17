using ErenshorLogs.Context;
using ErenshorLogs.Events;
using HarmonyLib;

namespace ErenshorLogs.Hooks;

public static class HotTickMath
{
  public static int EffectiveAmount(int currentHp, int maxHp, int rawAmount)
  {
    if (rawAmount <= 0 || currentHp >= maxHp)
      return 0;

    return Math.Min(rawAmount, maxHp - currentHp);
  }
}

public sealed record HotTickSnapshot(
  int Slot,
  int BeforeHp,
  int MaxHp,
  int RawAmount,
  Spell Spell,
  Character? Source,
  Character? Credit
);

/// <summary>
/// Hooks Stats.TickEffects to enable sequential slot tracking.
/// Allows damage/heal hooks to determine which StatusEffect slot caused each tick.
/// </summary>
/// <remarks>
/// TickEffects is a private method that processes all 30 StatusEffect slots sequentially.
/// By hooking it with Prefix/Finalizer, we can track the processing lifecycle and
/// enable damage hooks to correlate calls back to specific slots.
/// </remarks>
[HarmonyPatch(typeof(Stats), "TickEffects")]
public static class TickEffectsPatch
{
  /// <summary>
  /// Prefix: Initialize slot tracking before TickEffects processes slots.
  /// </summary>
  [HarmonyPrefix]
  public static void Prefix(Stats __instance, out HotTickSnapshot[] __state)
  {
    __state = CaptureHotTicks(__instance);
    TickEffectsSlotTracker.BeginTickEffects(__instance);
  }

  [HarmonyPostfix]
  public static void Postfix(Stats __instance, HotTickSnapshot[] __state)
  {
    EmitHotTicks(__instance, __state);
  }

  /// <summary>
  /// Finalizer: Clean up slot tracking after TickEffects completes.
  /// Uses Finalizer to ensure cleanup even if TickEffects throws an exception.
  /// </summary>
  [HarmonyFinalizer]
  public static void Finalizer()
  {
    TickEffectsSlotTracker.EndTickEffects();
  }

  private static HotTickSnapshot[] CaptureHotTicks(Stats stats)
  {
    if (stats == null || stats.StatusEffects == null)
      return [];

    List<HotTickSnapshot>? snapshots = null;
    var initialHp = stats.CurrentHP;
    var maxHp = stats.CurrentMaxHP;
    for (var slot = 0; slot < stats.StatusEffects.Length; slot++)
    {
      var status = stats.StatusEffects[slot];
      var spell = status?.Effect;
      if (!IsActiveHot(stats, status, spell))
        continue;

      snapshots ??= new List<HotTickSnapshot>(4);
      snapshots.Add(
        new HotTickSnapshot(
          slot,
          initialHp,
          maxHp,
          CalculateRawHotAmount(status!, spell!),
          spell!,
          status!.Owner,
          status.CreditDPS ?? status.Owner
        )
      );
    }

    return snapshots?.ToArray() ?? [];
  }

  private static bool IsActiveHot(Stats stats, StatusEffect? status, Spell? spell)
  {
    return status != null
      && spell != null
      && spell.TargetHealing > 0
      && status.Duration > 0f
      && spell.MyDamageType == GameData.DamageType.Physical
      && (stats.CombatStance == null || !stats.CombatStance.StopRegen);
  }

  private static int CalculateRawHotAmount(StatusEffect status, Spell spell)
  {
    var amount = spell.TargetHealing;
    var ownerStats = status.Owner?.MyStats;
    if (ownerStats != null && !spell.WornEffect)
    {
      amount += UnityEngine.Mathf.RoundToInt(
        (float)ownerStats.WisScaleMod / 100f * ownerStats.GetCurrentWis() * 10f
      );
      if (ownerStats.CharacterClass == GameData.ClassDB.Druid)
        amount += ownerStats.GetCurrentWis();
    }

    return amount;
  }

  private static void EmitHotTicks(Stats stats, HotTickSnapshot[] snapshots)
  {
    if (snapshots.Length == 0 || HealMePatch.EventBuilder == null || HealMePatch.Emitter == null)
      return;

    var target = stats?.Myself;
    if (target == null || !target.IsValid())
      return;

    var currentHp = snapshots[0].BeforeHp;
    var healthDeltas = TickEffectsSlotTracker.GetHealthDeltas();
    var deltaIndex = 0;

    foreach (var snapshot in snapshots)
    {
      while (deltaIndex < healthDeltas.Count && healthDeltas[deltaIndex].Slot <= snapshot.Slot)
      {
        currentHp = ClampHp(currentHp + healthDeltas[deltaIndex].Amount, snapshot.MaxHp);
        deltaIndex += 1;
      }

      var amount = HotTickMath.EffectiveAmount(currentHp, snapshot.MaxHp, snapshot.RawAmount);
      if (amount <= 0)
        continue;

      var source = snapshot.Credit ?? snapshot.Source;
      if (source != null && !source.IsValid())
        source = null;

      if (
        HealMePatch.RelevanceChecker != null
        && !HealMePatch.RelevanceChecker.IsRelevantCombat(source, target)
      )
        continue;

      var ability = new AbilityRef
      {
        Name = snapshot.Spell.SpellName,
        Type = AbilityType.Hot,
        StableKey = $"spell:{snapshot.Spell.Id}",
      };

      CombatEventDispatcher.PrepareForCapture(
        EventType.HealHot,
        HealMePatch.SessionManager,
        DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
      );
      var evt = HealMePatch.EventBuilder.CreateHealEvent(
        EventType.HealHot,
        target,
        source,
        ability,
        amount,
        snapshot.RawAmount,
        snapshot.RawAmount - amount
      );

      currentHp = ClampHp(currentHp + amount, snapshot.MaxHp);

      if (evt != null)
        CombatEventDispatcher.Dispatch(
          evt with
          {
            Attribution = AttributionMethod.EffectTracker,
          },
          HealMePatch.Emitter
        );
    }
  }

  private static int ClampHp(int value, int maxHp)
  {
    if (value < 0)
      return 0;

    return value > maxHp ? maxHp : value;
  }
}
