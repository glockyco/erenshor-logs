using System.Collections.Generic;
using ErenshorLogs.Events;
using ErenshorLogs.Hooks;
using Xunit;

namespace ErenshorLogs.Tests.Hooks;

public sealed class HealthEventBuilderTests
{
  [Fact]
  public void CreateHealEvent_BuildsScriptedHeal()
  {
    var source = new object();
    var target = new object();
    var builder = CreateBuilder(
      new Dictionary<object, ActorRef>
      {
        [source] = new()
        {
          Id = "npc:grace",
          Name = "Grace",
          Type = ActorType.Npc,
        },
        [target] = new()
        {
          Id = "npc:grace",
          Name = "Grace",
          Type = ActorType.Npc,
        },
      }
    );

    var evt = builder.CreateHealEvent(
      eventType: EventType.HealSpell,
      target: target,
      source: source,
      ability: new AbilityRef
      {
        Name = "Grace Echoes",
        Type = AbilityType.AreaEffect,
        StableKey = "mechanic:grace-echoes",
      },
      amount: 200000,
      rawAmount: 200000,
      overhealAmount: 0
    );

    Assert.NotNull(evt);
    Assert.Equal(EventType.HealSpell, evt.EventType);
    Assert.Equal(200000, evt.Amount);
    Assert.Equal(200000, evt.RawAmount);
    Assert.Equal(0, evt.OverhealAmount);
    Assert.Equal("Grace Echoes", evt.Ability.Name);
  }

  [Fact]
  public void CreateDeathEvent_BuildsDeathWithKillingBlowLink()
  {
    var source = new object();
    var target = new object();
    var builder = CreateBuilder(
      new Dictionary<object, ActorRef>
      {
        [source] = new()
        {
          Id = "npc:death-touch",
          Name = "Death Touch",
          Type = ActorType.Npc,
        },
        [target] = new()
        {
          Id = "sim:cleric",
          Name = "Cleric",
          Type = ActorType.Npc,
        },
      }
    );

    var evt = builder.CreateDeathEvent(
      target: target,
      source: source,
      ability: new AbilityRef
      {
        Name = "Death Touch",
        Type = AbilityType.AreaEffect,
        StableKey = "mechanic:death-touch",
      },
      killingBlowEventSeq: 42
    );

    Assert.NotNull(evt);
    Assert.Equal(EventType.Death, evt.EventType);
    Assert.Equal("sim:cleric", evt.Target?.Id);
    Assert.Equal("Death Touch", evt.Ability.Name);
    Assert.Equal(42, evt.KillingBlowEventSeq);
  }

  [Theory]
  [InlineData(900, 1000, 150, 100)]
  [InlineData(500, 1000, 150, 150)]
  [InlineData(1000, 1000, 150, 0)]
  public void HotTickMath_ClampsEffectiveHealing(
    int currentHp,
    int maxHp,
    int rawAmount,
    int expectedEffective
  )
  {
    Assert.Equal(expectedEffective, HotTickMath.EffectiveAmount(currentHp, maxHp, rawAmount));
  }

  private static CombatEventBuilder<object> CreateBuilder(Dictionary<object, ActorRef> actors) =>
    new(actor => actor == null ? null : actors[actor], () => "evt-1", () => 1_800_000_000_000);
}
