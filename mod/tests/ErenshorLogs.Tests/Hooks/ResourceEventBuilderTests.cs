using System.Collections.Generic;
using ErenshorLogs.Events;
using ErenshorLogs.Hooks;
using Xunit;

namespace ErenshorLogs.Tests.Hooks;

public sealed class ResourceEventBuilderTests
{
  [Fact]
  public void CreateResourceEvent_BuildsManaDrain()
  {
    var source = new object();
    var target = new object();
    var actors = new Dictionary<object, ActorRef>
    {
      [source] = new()
      {
        Id = "npc:mana-drain",
        Name = "Mana Drain",
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

    var evt = builder.CreateResourceEvent(
      eventType: EventType.ManaUse,
      target: target,
      source: source,
      ability: new AbilityRef
      {
        Name = "Mana Drain",
        Type = AbilityType.AreaEffect,
        StableKey = "mechanic:mana-drain",
      },
      resourceType: "mana",
      delta: -300,
      current: 1200,
      max: 1500
    );

    Assert.NotNull(evt);
    Assert.Equal(EventType.ManaUse, evt.EventType);
    Assert.Equal("mana", evt.ResourceType);
    Assert.Equal(-300, evt.ResourceDelta);
    Assert.Equal(1200, evt.ResourceCurrent);
    Assert.Equal(1500, evt.ResourceMax);
  }
}
