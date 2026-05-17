using System.Collections.Generic;
using System.Reflection;
using ErenshorLogs.Context;
using ErenshorLogs.Events;
using ErenshorLogs.Hooks;
using Xunit;

namespace ErenshorLogs.Tests.Hooks;

public sealed class StatusEventBuilderTests
{
  [Fact]
  public void CreateEffectEvent_BuildsDebuffApply()
  {
    var source = new object();
    var target = new object();
    var actors = new Dictionary<object, ActorRef>
    {
      [source] = new()
      {
        Id = "npc:mizuki",
        Name = "Mizuki",
        Type = ActorType.Npc,
      },
      [target] = new()
      {
        Id = "player:0",
        Name = "Player",
        Type = ActorType.Player,
      },
    };
    var builder = new CombatEventBuilder<object>(
      actor => actor == null ? null : actors[actor],
      () => "evt-1",
      () => 1_800_000_000_000
    );

    var evt = builder.CreateEffectEvent(
      eventType: EventType.DebuffApply,
      target: target,
      source: source,
      ability: new AbilityRef
      {
        Name = "Dagger Bleed",
        Type = AbilityType.Dot,
        StableKey = "mechanic:mizuki-dagger",
      },
      effect: new EffectRef
      {
        Name = "BleedRef",
        Duration = 12,
        Stacks = 1,
      },
      action: "apply",
      reason: null
    );

    Assert.NotNull(evt);
    Assert.Equal(EventType.DebuffApply, evt.EventType);
    Assert.Equal("BleedRef", evt.Effect?.Name);
    Assert.Equal("npc:mizuki", evt.Source?.Id);
    Assert.Equal("player:0", evt.Target?.Id);
    Assert.Equal("apply", evt.EffectAction);
    Assert.Equal(1, evt.EffectStacks);
    Assert.Equal(12000, evt.EffectDurationMs);
  }

  [Fact]
  public void EffectTracker_ExposesTrackedOwnershipLookup()
  {
    LoadGameAssemblies();
    var register = typeof(EffectTracker).GetMethod(nameof(EffectTracker.RegisterEffect));
    var lookup = typeof(EffectTracker).GetMethod(nameof(EffectTracker.GetTrackedEffect));

    Assert.NotNull(register);
    Assert.NotNull(lookup);
    Assert.Contains(register!.GetParameters(), parameter => parameter.Name == "source");
    Assert.Contains(register.GetParameters(), parameter => parameter.Name == "credit");
  }

  [Theory]
  [InlineData(null, "spell:1", StatusEffectChangeKind.Apply)]
  [InlineData("spell:1", "spell:1", StatusEffectChangeKind.Refresh)]
  [InlineData("spell:1", "spell:2", StatusEffectChangeKind.Replace)]
  public void StatusEffectChange_ClassifiesSlotReapplication(
    string? previousStableKey,
    string nextStableKey,
    StatusEffectChangeKind expected
  )
  {
    Assert.Equal(expected, StatusEffectChange.Classify(previousStableKey, nextStableKey));
  }

  private static void LoadGameAssemblies()
  {
    var libPath = Path.GetFullPath(
      Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "lib")
    );
    var path = Path.Combine(libPath, "Assembly-CSharp.dll");
    if (File.Exists(path))
      Assembly.LoadFrom(path);
  }
}
