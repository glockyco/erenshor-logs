using System.Collections.Generic;
using ErenshorLogs.Context;
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

  [Fact]
  public void CreateHealEvent_WhenSourceIsUnknown_DoesNotUseTargetAsSource()
  {
    var target = new object();
    var builder = CreateBuilder(
      new Dictionary<object, ActorRef>
      {
        [target] = new()
        {
          Id = "sim:cleric",
          Name = "Cleric",
          Type = ActorType.SimPlayer,
        },
      }
    );

    var evt = builder.CreateHealEvent(
      eventType: EventType.HealSpell,
      target: target,
      source: null,
      ability: new AbilityRef
      {
        Name = "Healing",
        Type = AbilityType.Spell,
        StableKey = null,
      },
      amount: 50,
      rawAmount: 80,
      overhealAmount: 30
    );

    Assert.NotNull(evt);
    Assert.Null(evt!.Source);
    Assert.Equal("sim:cleric", evt.Target!.Id);
  }

  [Fact]
  public void HealingContext_CarriesSourceForIntHealEmission()
  {
    var source = new object();
    var target = new object();
    var builder = CreateBuilder(
      new Dictionary<object, ActorRef>
      {
        [source] = new()
        {
          Id = "sim:druid",
          Name = "Druid",
          Type = ActorType.SimPlayer,
        },
        [target] = new()
        {
          Id = "sim:tank",
          Name = "Tank",
          Type = ActorType.SimPlayer,
        },
      }
    );

    var evt = builder.CreateHealEvent(
      eventType: EventType.HealLifesteal,
      target: target,
      source: source,
      ability: new AbilityRef
      {
        Name = "Lifetap",
        Type = AbilityType.Spell,
        StableKey = "spell:lifetap",
      },
      amount: 30,
      rawAmount: 30,
      overhealAmount: 0
    );

    Assert.NotNull(evt);
    Assert.Equal("sim:druid", evt!.Source!.Id);
    Assert.Equal("sim:tank", evt.Target!.Id);
    Assert.Equal(EventType.HealLifesteal, evt.EventType);
  }

  [Fact]
  public void CreateHealEvent_WhenCriticalFlagProvided_StoresFlags()
  {
    var target = new object();
    var builder = CreateBuilder(
      new Dictionary<object, ActorRef>
      {
        [target] = new()
        {
          Id = "sim:cleric",
          Name = "Cleric",
          Type = ActorType.SimPlayer,
        },
      }
    );

    var evt = builder.CreateHealEvent(
      eventType: EventType.HealSpell,
      target: target,
      source: null,
      ability: new AbilityRef
      {
        Name = "Greater Heal",
        Type = AbilityType.Spell,
        StableKey = "spell:greater-heal",
      },
      amount: 100,
      rawAmount: 120,
      overhealAmount: 20,
      flags: new EventFlags { Critical = true }
    );

    Assert.NotNull(evt);
    Assert.True(evt!.Flags!.Critical);
  }

  [Fact]
  public void HealingContext_CanClassifyLifestealWithoutNameHeuristic()
  {
    var ability = new AbilityRef
    {
      Name = "Vampiric Return",
      Type = AbilityType.Unknown,
      StableKey = "proc:vampiric-return",
    };

    using (HealingContext.Push(null, ability, EventType.HealLifesteal, AttributionMethod.Verified))
    {
      var current = HealingContext.Current();

      Assert.NotNull(current);
      Assert.Equal(EventType.HealLifesteal, current!.EventType);
      Assert.Equal("Vampiric Return", current.Ability.Name);
    }
  }

  [Fact]
  public void HealingContext_CanCarryReapAndRenewEffectAbility()
  {
    var ability = new AbilityRef
    {
      Name = "Reap and Renew",
      Type = AbilityType.Hot,
      StableKey = "spell:reap-and-renew",
    };

    using (HealingContext.Push(null, ability, EventType.HealSpell, AttributionMethod.EffectTracker))
    {
      var current = HealingContext.Current();

      Assert.NotNull(current);
      Assert.Equal(EventType.HealSpell, current!.EventType);
      Assert.Equal(AbilityType.Hot, current.Ability.Type);
      Assert.Equal(AttributionMethod.EffectTracker, current.Attribution);
    }
  }

  [Fact]
  public void CreateHealEvent_WhenRawAndOverhealAreUnknown_LeavesThemNull()
  {
    var target = new object();
    var builder = CreateBuilder(
      new Dictionary<object, ActorRef>
      {
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
      source: null,
      ability: new AbilityRef
      {
        Name = "Grace Echoes",
        Type = AbilityType.AreaEffect,
        StableKey = "mechanic:grace-echoes",
      },
      amount: 200000,
      rawAmount: null,
      overhealAmount: null
    );

    Assert.NotNull(evt);
    Assert.Null(evt!.RawAmount);
    Assert.Null(evt.OverhealAmount);
  }

  private static CombatEventBuilder<object> CreateBuilder(Dictionary<object, ActorRef> actors) =>
    new(actor => actor == null ? null : actors[actor], () => "evt-1", () => 1_800_000_000_000);
}
