using System.Collections.Generic;
using ErenshorLogs.Events;
using ErenshorLogs.Hooks;
using Xunit;

namespace ErenshorLogs.Tests.Hooks;

public sealed class MechanicEventBuilderTests
{
  [Fact]
  public void CreateMechanicEvent_BuildsInvulnerabilityMechanic()
  {
    var source = new object();
    var target = new object();
    var actors = new Dictionary<object, ActorRef>
    {
      [source] = new()
      {
        Id = "npc:sprinkles",
        Name = "Sprinkles",
        Type = ActorType.Npc,
      },
      [target] = new()
      {
        Id = "npc:sprinkles",
        Name = "Sprinkles",
        Type = ActorType.Npc,
      },
    };
    var builder = new CombatEventBuilder<object>(
      actor => actor == null ? null : actors[actor],
      () => "evt-1",
      () => 1_800_000_000_000
    );

    var evt = builder.CreateMechanicEvent(
      target: target,
      source: source,
      ability: new AbilityRef
      {
        Name = "Sprinkles Wards",
        Type = AbilityType.AreaEffect,
        StableKey = "mechanic:sprinkles-wards",
      },
      mechanic: new MechanicData
      {
        Action = "invulnerability",
        Name = "Sprinkles wards",
        Value = true,
      }
    );

    Assert.NotNull(evt);
    Assert.Equal(EventType.Mechanic, evt.EventType);
    Assert.Equal("Sprinkles wards", evt.Mechanic?.Name);
    Assert.Equal("invulnerability", evt.Mechanic?.Action);
    Assert.Equal(true, evt.Mechanic?.Value);
  }
}
