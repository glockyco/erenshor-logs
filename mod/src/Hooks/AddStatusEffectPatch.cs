using ErenshorLogs.Context;
using ErenshorLogs.Events;
using HarmonyLib;

namespace ErenshorLogs.Hooks;

internal static class AddStatusEffectRegistration
{
  internal static EffectTracker? Tracker { get; set; }

  internal static void Register(
    Stats stats,
    Spell spell,
    int slot,
    Character? source,
    Character? credit
  )
  {
    if (Tracker == null || stats == null || spell == null)
      return;

    if (slot >= 0 && slot < 30)
    {
      Tracker.RegisterEffect(stats.Myself, slot, spell, source, credit);
      StatusEventCapture.EmitApply(stats.Myself, spell, source, credit, slot);
    }
  }
}

internal static class StatusEventCapture
{
  internal static void EmitApply(
    Character target,
    Spell spell,
    Character? source,
    Character? credit,
    int slot
  )
  {
    Emit(EventTypeFor(spell, apply: true), target, spell, source, credit, slot, "apply", null);
  }

  internal static void EmitFade(TrackedEffect tracked, string reason)
  {
    Emit(
      EventTypeFor(tracked.Spell, apply: false),
      tracked.Target,
      tracked.Spell,
      tracked.Source,
      tracked.Credit,
      tracked.Slot,
      "fade",
      reason
    );
  }

  private static void Emit(
    EventType eventType,
    Character target,
    Spell spell,
    Character? source,
    Character? credit,
    int slot,
    string action,
    string? reason
  )
  {
    _ = slot;
    if (HealMePatch.EventBuilder == null || HealMePatch.Emitter == null)
      return;

    if (!target.IsValid())
      return;

    source ??= credit;
    if (source != null && !source.IsValid())
      source = null;

    if (
      HealMePatch.RelevanceChecker != null
      && !HealMePatch.RelevanceChecker.IsRelevantCombat(source, target)
    )
      return;

    var ability = new AbilityRef
    {
      Name = spell.SpellName,
      Type = spell.Type == Spell.SpellType.Beneficial ? AbilityType.Hot : AbilityType.Dot,
      StableKey = $"spell:{spell.Id}",
    };
    var effect = new EffectRef
    {
      Name = spell.SpellName,
      Duration = spell.SpellDurationInTicks,
      Stacks = 1,
    };

    CombatEventDispatcher.PrepareForCapture(
      eventType,
      HealMePatch.SessionManager,
      DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
    );

    var evt = HealMePatch.EventBuilder.CreateEffectEvent(
      eventType,
      target,
      source,
      ability,
      effect,
      action,
      reason
    );

    if (evt != null)
      CombatEventDispatcher.Dispatch(evt, HealMePatch.Emitter);
  }

  private static EventType EventTypeFor(Spell spell, bool apply)
  {
    var isBuff = spell.Type == Spell.SpellType.Beneficial || spell.Type == Spell.SpellType.Heal;
    return (isBuff, apply) switch
    {
      (true, true) => EventType.BuffApply,
      (true, false) => EventType.BuffFade,
      (false, true) => EventType.DebuffApply,
      _ => EventType.DebuffFade,
    };
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
    var source = RaidAuraContext.Resolve(spell);
    AddStatusEffectRegistration.Register(__instance, spell, __result, source, source);
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
  public static void Postfix(Stats __instance, Spell spell, Character _specificCaster, int __result)
  {
    AddStatusEffectRegistration.Register(
      __instance,
      spell,
      __result,
      _specificCaster,
      _specificCaster
    );
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
  public static void Postfix(Stats __instance, Spell spell, Character _specificCaster, int __result)
  {
    AddStatusEffectRegistration.Register(
      __instance,
      spell,
      __result,
      _specificCaster,
      _specificCaster
    );
  }
}
