using ErenshorLogs.Events;
using ErenshorLogs.Hooks;
using Xunit;

namespace ErenshorLogs.Tests.Hooks;

public class CombatEventBuilderTests
{
  private sealed class MockCharacter
  {
    public int InstanceId { get; init; }
    public string Name { get; init; } = "";
  }

  private static ActorRef CreateActorRef(MockCharacter c) =>
    new()
    {
      Id = $"mock:{c.InstanceId}",
      Name = c.Name,
      Type = ActorType.Npc,
    };

  [Fact]
  public void CreateDamageEvent_WithValidTargetAndSource_ReturnsEvent()
  {
    var idCounter = 0;
    var builder = new CombatEventBuilder<MockCharacter>(
      resolveActor: c => c == null ? null : CreateActorRef(c),
      generateId: () => $"event-{++idCounter}",
      getTimestamp: () => 1000L
    );

    var target = new MockCharacter { InstanceId = 1, Name = "Goblin" };
    var source = new MockCharacter { InstanceId = 2, Name = "Player" };

    var evt = builder.CreateDamageEvent(
      EventType.DamagePhysical,
      target,
      source,
      amount: 100,
      DamageType.Physical,
      new EventFlags { Critical = true }
    );

    Assert.NotNull(evt);
    Assert.Equal("event-1", evt.Id);
    Assert.Equal(1000L, evt.Timestamp);
    Assert.Equal(EventType.DamagePhysical, evt.EventType);
    Assert.Equal("mock:1", evt.Target?.Id);
    Assert.Equal("Goblin", evt.Target?.Name);
    Assert.Equal("mock:2", evt.Source?.Id);
    Assert.Equal("Player", evt.Source?.Name);
    Assert.Equal(100, evt.Amount);
    Assert.Equal(DamageType.Physical, evt.DamageType);
    Assert.True(evt.Flags?.Critical);
  }

  [Fact]
  public void CreateDamageEvent_WithNullSource_ReturnsEventWithNullSource()
  {
    var builder = new CombatEventBuilder<MockCharacter>(
      resolveActor: c => c == null ? null : CreateActorRef(c),
      generateId: () => "event-1",
      getTimestamp: () => 1000L
    );

    var target = new MockCharacter { InstanceId = 1, Name = "Player" };

    var evt = builder.CreateDamageEvent(
      EventType.DamageEnvironmental,
      target,
      source: null,
      amount: 50,
      DamageType.Physical
    );

    Assert.NotNull(evt);
    Assert.Null(evt.Source);
    Assert.NotNull(evt.Target);
    Assert.Equal(EventType.DamageEnvironmental, evt.EventType);
  }

  [Fact]
  public void CreateDamageEvent_WhenTargetCannotBeResolved_ReturnsNull()
  {
    var builder = new CombatEventBuilder<MockCharacter>(
      resolveActor: _ => null, // Always fails to resolve
      generateId: () => "event-1",
      getTimestamp: () => 1000L
    );

    var target = new MockCharacter { InstanceId = 1, Name = "Unknown" };

    var evt = builder.CreateDamageEvent(
      EventType.DamagePhysical,
      target,
      source: null,
      amount: 100,
      DamageType.Physical
    );

    Assert.Null(evt);
  }

  [Fact]
  public void CreateDamageEvent_WithZeroAmount_ReturnsEventWithZeroAmount()
  {
    var builder = new CombatEventBuilder<MockCharacter>(
      resolveActor: c => c == null ? null : CreateActorRef(c),
      generateId: () => "event-1",
      getTimestamp: () => 1000L
    );

    var target = new MockCharacter { InstanceId = 1, Name = "Goblin" };
    var source = new MockCharacter { InstanceId = 2, Name = "Player" };

    var evt = builder.CreateDamageEvent(
      EventType.DamagePhysical,
      target,
      source,
      amount: 0,
      DamageType.Physical,
      new EventFlags { Missed = true }
    );

    Assert.NotNull(evt);
    Assert.Equal(0, evt.Amount);
    Assert.True(evt.Flags?.Missed);
  }

  [Fact]
  public void CreateDamageEvent_WithMagicDamage_SetsCorrectDamageType()
  {
    var builder = new CombatEventBuilder<MockCharacter>(
      resolveActor: c => c == null ? null : CreateActorRef(c),
      generateId: () => "event-1",
      getTimestamp: () => 1000L
    );

    var target = new MockCharacter { InstanceId = 1, Name = "Goblin" };
    var source = new MockCharacter { InstanceId = 2, Name = "Mage" };

    var evt = builder.CreateDamageEvent(
      EventType.DamageMagic,
      target,
      source,
      amount: 200,
      DamageType.Magic,
      new EventFlags { Resisted = true }
    );

    Assert.NotNull(evt);
    Assert.Equal(EventType.DamageMagic, evt.EventType);
    Assert.Equal(DamageType.Magic, evt.DamageType);
    Assert.True(evt.Flags?.Resisted);
  }
}
