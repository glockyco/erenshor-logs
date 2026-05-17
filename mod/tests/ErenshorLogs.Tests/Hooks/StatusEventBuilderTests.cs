using System.Collections.Generic;
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
    Assert.Equal("apply", evt.EffectAction);
    Assert.Equal(1, evt.EffectStacks);
    Assert.Equal(12000, evt.EffectDurationMs);
  }
}
