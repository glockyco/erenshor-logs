using ErenshorLogs.Context;
using ErenshorLogs.Events;
using HarmonyLib;

namespace ErenshorLogs.Hooks;

public enum StatusEffectChangeKind
{
  Apply,
  Refresh,
  Replace,
}

public static class StatusEffectChange
{
  public static StatusEffectChangeKind Classify(string? previousStableKey, string nextStableKey)
  {
    if (string.IsNullOrEmpty(previousStableKey))
      return StatusEffectChangeKind.Apply;

    return string.Equals(previousStableKey, nextStableKey, StringComparison.Ordinal)
      ? StatusEffectChangeKind.Refresh
      : StatusEffectChangeKind.Replace;
  }
}

internal static class AddStatusEffectRegistration
{
  internal static EffectTracker? Tracker { get; set; }

  internal static void Register(
    Stats stats,
    Spell spell,
    int slot,
    Character? source,
    Character? credit,
    AttributionMethod attribution
  )
  {
    if (Tracker == null || stats == null || spell == null)
      return;

    if (slot < 0 || slot >= 30)
      return;

    var previous = Tracker.GetTrackedEffect(stats.Myself, slot);
    var nextStableKey = StableKeyFor(spell);
    var change = StatusEffectChange.Classify(previous?.Context.StableKey, nextStableKey);

    if (change == StatusEffectChangeKind.Refresh)
    {
      Tracker.RegisterEffect(stats.Myself, slot, spell, source, credit);
      StatusEventCapture.EmitRefresh(stats.Myself, spell, source, credit, slot, attribution);
      return;
    }

    if (previous != null)
      StatusEventCapture.EmitFade(previous, "overwritten");

    Tracker.RegisterEffect(stats.Myself, slot, spell, source, credit);
    StatusEventCapture.EmitApply(stats.Myself, spell, source, credit, slot, attribution);
  }

  private static string StableKeyFor(Spell spell)
  {
    return $"spell:{spell.Id}";
  }
}

internal static class StatusEventCapture
{
  internal static void EmitApply(
    Character target,
    Spell spell,
    Character? source,
    Character? credit,
    int slot,
    AttributionMethod attribution
  )
  {
    Emit(
      EventTypeFor(spell, StatusEffectChangeKind.Apply),
      target,
      spell,
      source,
      credit,
      slot,
      "apply",
      null,
      attribution
    );
  }

  internal static void EmitRefresh(
    Character target,
    Spell spell,
    Character? source,
    Character? credit,
    int slot,
    AttributionMethod attribution
  )
  {
    Emit(
      EventTypeFor(spell, StatusEffectChangeKind.Refresh),
      target,
      spell,
      source,
      credit,
      slot,
      "refresh",
      null,
      attribution
    );
  }

  internal static void EmitFade(TrackedEffect tracked, string reason)
  {
    Emit(
      EventTypeFor(tracked.Spell, StatusEffectChangeKind.Replace),
      tracked.Target,
      tracked.Spell,
      tracked.Source,
      tracked.Credit,
      tracked.Slot,
      "fade",
      reason,
      AttributionMethod.EffectTracker
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
    string? reason,
    AttributionMethod attribution
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
      Type = AbilityTypeFor(spell.Type),
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
      CombatEventDispatcher.Dispatch(evt with { Attribution = attribution }, HealMePatch.Emitter);
  }

  internal static AbilityType AbilityTypeFor(Spell.SpellType spellType) =>
    spellType == Spell.SpellType.Beneficial || spellType == Spell.SpellType.Heal
      ? AbilityType.Hot
      : AbilityType.Dot;

  private static EventType EventTypeFor(Spell spell, StatusEffectChangeKind change)
  {
    var isBuff = spell.Type == Spell.SpellType.Beneficial || spell.Type == Spell.SpellType.Heal;
    return (isBuff, change) switch
    {
      (true, StatusEffectChangeKind.Apply) => EventType.BuffApply,
      (true, StatusEffectChangeKind.Refresh) => EventType.BuffRefresh,
      (true, _) => EventType.BuffFade,
      (false, StatusEffectChangeKind.Apply) => EventType.DebuffApply,
      (false, StatusEffectChangeKind.Refresh) => EventType.DebuffRefresh,
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
    AddStatusEffectRegistration.Register(
      __instance,
      spell,
      __result,
      source,
      source,
      AttributionMethod.Context
    );
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
      _specificCaster,
      AttributionMethod.Verified
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
      _specificCaster,
      AttributionMethod.Verified
    );
  }
}
