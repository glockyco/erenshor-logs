using ErenshorLogs.Events;
using ErenshorLogs.Protocol;
using ErenshorLogs.Session;
using Newtonsoft.Json.Linq;
using Xunit;

namespace ErenshorLogs.Tests.Protocol;

public sealed class ProtocolSessionStateTests
{
  [Fact]
  public void AppendDamageEvent_AssignsSequenceAndRegistersReferences()
  {
    var session = new CombatSession("playtest-23258843", "2026.5.17.14");
    var state = new ProtocolSessionState(session);
    var evt = CreateDamageEvent(session.StartTime + 250);

    var protocolEvent = state.Append(evt);

    Assert.NotNull(protocolEvent);
    Assert.Equal(1, protocolEvent.Value<long>("eventSeq"));
    Assert.Equal(250, protocolEvent.Value<long>("offsetMs"));
    Assert.Equal("damage", protocolEvent.Value<string>("kind"));
    Assert.Equal("hit", protocolEvent.Value<string>("action"));
    Assert.Equal("player:0", protocolEvent.Value<string>("sourceActorId"));
    Assert.Equal("npc:1", protocolEvent.Value<string>("targetActorId"));
    Assert.Equal("skill:101", protocolEvent.Value<string>("abilityId"));
    Assert.Equal(350, protocolEvent["data"]!.Value<long>("amount"));
    Assert.Equal("physical", protocolEvent["data"]!.Value<string>("damageType"));

    Assert.Equal(3, state.RegistryRevision);
    Assert.Equal("Player", state.Registries.Actors["player:0"].Name);
    Assert.Equal("Raid Boss", state.Registries.Actors["npc:1"].Name);
    Assert.Equal("Backstab", state.Registries.Abilities["skill:101"].Name);
  }

  [Fact]
  public void CreateSnapshot_IncludesFullRegistriesAndLastEventSeq()
  {
    var session = new CombatSession("playtest-23258843", "2026.5.17.14", isManual: true);
    var state = new ProtocolSessionState(session);
    state.Append(CreateDamageEvent(session.StartTime + 250));

    var snapshot = state.CreateSnapshot();

    Assert.Equal(session.Id, snapshot.SessionId);
    Assert.Equal("active", snapshot.State);
    Assert.Equal("manual", snapshot.Mode);
    Assert.Equal(1, snapshot.LastEventSeq);
    Assert.Equal(1, snapshot.EventCount);
    Assert.Equal(state.RegistryRevision, snapshot.RegistryRevision);
    Assert.NotEmpty(snapshot.Registries.Actors);
    Assert.NotEmpty(snapshot.Registries.Abilities);
  }

  [Fact]
  public void Append_AreaEffectAbilityMapsToAreaEffectKind()
  {
    var session = new CombatSession("playtest-23258843", "2026.5.17.14");
    var state = new ProtocolSessionState(session);
    var evt = CreateDamageEvent(session.StartTime + 250) with
    {
      Ability = new AbilityRef
      {
        Name = "Dragon Breath",
        Type = AbilityType.AreaEffect,
        StableKey = "area:dragon-breath",
      },
    };

    state.Append(evt);

    Assert.Equal("areaEffect", state.Registries.Abilities["area:dragon-breath"].Kind);
  }

  [Fact]
  public void Append_IgnoresSyntheticCombatLifecycleEvents()
  {
    var session = new CombatSession("main-22374607", "2026.5.17.14");
    var state = new ProtocolSessionState(session);

    var protocolEvent = state.Append(
      new CombatEvent
      {
        Id = "evt-1",
        Timestamp = session.StartTime,
        EventType = EventType.CombatStart,
        Ability = new AbilityRef { Name = "Combat Start", Type = AbilityType.Unknown },
      }
    );

    Assert.Null(protocolEvent);
    Assert.Equal(0, state.LastEventSeq);
    Assert.Empty(state.Events);
  }

  private static CombatEvent CreateDamageEvent(long timestamp) =>
    new()
    {
      Id = "evt-1",
      Timestamp = timestamp,
      EventType = EventType.DamagePhysical,
      Source = new ActorRef
      {
        Id = "player:0",
        Name = "Player",
        Type = ActorType.Player,
        Class = "Duelist",
        Level = 20,
      },
      Target = new ActorRef
      {
        Id = "npc:1",
        Name = "Raid Boss",
        Type = ActorType.Npc,
        Level = 25,
      },
      Ability = new AbilityRef
      {
        Name = "Backstab",
        Type = AbilityType.Skill,
        StableKey = "skill:101",
      },
      Amount = 350,
      RawAmount = 400,
      Mitigated = 50,
      DamageType = DamageType.Physical,
      Flags = new EventFlags { Critical = true },
    };
}
