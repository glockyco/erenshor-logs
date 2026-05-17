using ErenshorLogs.Events;
using ErenshorLogs.Protocol;
using ErenshorLogs.Session;
using Newtonsoft.Json.Linq;
using Xunit;
using EventMechanicData = ErenshorLogs.Events.MechanicData;

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
  public void CreateSnapshot_PreservesSessionEndReason()
  {
    var manager = new SessionManager(
      new FakeEventEmitter(),
      new FakeGameVersionProvider(),
      new FakeTimeProvider(),
      "2026.5.17.14",
      autoDetectionEnabled: true,
      inactivityTimeoutSeconds: 5,
      sessionStartEvents: "DamagePhysical",
      sessionKeepAliveEvents: "DamagePhysical"
    );
    CombatSession? session = null;
    manager.SessionEnded += (endedSession, _) => session = endedSession;

    manager.StartManualSession();
    manager.EndManualSession();
    var state = new ProtocolSessionState(session!);

    var snapshot = state.CreateSnapshot();

    Assert.Equal("ended", snapshot.State);
    Assert.Equal(SessionEndReasons.Manual, snapshot.EndReason);
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
  public void Append_HealEvent_SerializesHealRecord()
  {
    var session = new CombatSession("playtest-raid", "2026.5.17.1");
    var state = new ProtocolSessionState(session);
    var evt = CreateHealEvent(session.StartTime + 500, EventType.HealSpell) with
    {
      Mechanic = new EventMechanicData { Name = "Grace Echoes", Action = "scripted" },
    };

    var record = state.Append(evt)!;

    Assert.Equal("heal", record.Value<string>("kind"));
    Assert.Equal("scripted", record.Value<string>("action"));
    Assert.Equal(200000, record["data"]!.Value<int>("amount"));
    Assert.Equal(0, record["data"]!.Value<int>("overhealAmount"));
  }

  [Fact]
  public void Append_ResourceDrain_SerializesResourceRecord()
  {
    var session = new CombatSession("playtest-raid", "2026.5.17.1");
    var state = new ProtocolSessionState(session);
    var evt = CreateResourceDrainEvent(session.StartTime + 750);

    var record = state.Append(evt)!;

    Assert.Equal("resource", record.Value<string>("kind"));
    Assert.Equal("drain", record.Value<string>("action"));
    Assert.Equal(-300, record["data"]!.Value<int>("delta"));
  }

  [Fact]
  public void Append_MechanicEvent_SerializesMechanicRecord()
  {
    var session = new CombatSession("playtest-raid", "2026.5.17.1");
    var state = new ProtocolSessionState(session);
    var evt = CreateMechanicEvent(session.StartTime + 1500);

    var record = state.Append(evt)!;

    Assert.Equal("mechanic", record.Value<string>("kind"));
    Assert.Equal("invulnerability", record.Value<string>("action"));
    Assert.Equal("Sprinkles wards", record["data"]!.Value<string>("name"));
    Assert.True(record["data"]!.Value<bool>("value"));
  }

  [Theory]
  [InlineData(EventType.HealHot, "tick")]
  [InlineData(EventType.HealLifesteal, "lifesteal")]
  [InlineData(EventType.HealRegen, "regen")]
  public void Append_HealEvents_MapProtocolActions(EventType eventType, string expectedAction)
  {
    var session = new CombatSession("playtest-raid", "2026.5.17.1");
    var state = new ProtocolSessionState(session);

    var record = state.Append(CreateHealEvent(session.StartTime + 500, eventType))!;

    Assert.Equal(expectedAction, record.Value<string>("action"));
  }

  [Fact]
  public void Append_ManaRestore_MapsToResourceRestore()
  {
    var session = new CombatSession("playtest-raid", "2026.5.17.1");
    var state = new ProtocolSessionState(session);

    var record = state.Append(CreateManaRestoreEvent(session.StartTime + 750))!;

    Assert.Equal("restore", record.Value<string>("action"));
  }

  [Fact]
  public void Append_DebuffApply_MapsToEffectApply()
  {
    var session = new CombatSession("playtest-raid", "2026.5.17.1");
    var state = new ProtocolSessionState(session);

    var record = state.Append(CreateEffectEvent(session.StartTime + 900))!;

    Assert.Equal("effect", record.Value<string>("kind"));
    Assert.Equal("apply", record.Value<string>("action"));
    Assert.Equal(12000, record["data"]!.Value<int>("durationMs"));
    Assert.Equal(12000, state.Registries.Effects["effect:BleedRef"].DefaultDurationMs);
  }

  [Fact]
  public void Append_DebuffApply_RegistersDebuffEffectKind()
  {
    var session = new CombatSession("playtest-raid", "2026.5.17.1");
    var state = new ProtocolSessionState(session);

    state.Append(CreateEffectEvent(session.StartTime + 900));

    Assert.Equal("debuff", state.Registries.Effects["effect:BleedRef"].Kind);
  }

  [Fact]
  public void Append_SerializesExplicitAttribution()
  {
    var session = new CombatSession("playtest-raid", "2026.5.17.1");
    var state = new ProtocolSessionState(session);
    var evt = CreateEffectEvent(session.StartTime + 900) with
    {
      Attribution = AttributionMethod.EffectTracker,
    };

    var record = state.Append(evt)!;

    Assert.Equal("effectTracker", record.Value<string>("attribution"));
  }

  [Fact]
  public void Append_Death_MapsToDieAction()
  {
    var session = new CombatSession("playtest-raid", "2026.5.17.1");
    var state = new ProtocolSessionState(session);

    var record = state.Append(CreateDeathEvent(session.StartTime + 1200))!;

    Assert.Equal("death", record.Value<string>("kind"));
    Assert.Equal("die", record.Value<string>("action"));
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

  private sealed class FakeEventEmitter : IEventEmitter
  {
    public int ListenerCount => 0;
    public long EventCount => 0;

    public void Emit(CombatEvent evt) { }

    public IDisposable Subscribe(Action<CombatEvent> handler) => new NoopDisposable();
  }

  private sealed class FakeGameVersionProvider : IGameVersionProvider
  {
    public string GetGameVersion() => "playtest-23258843";
  }

  private sealed class FakeTimeProvider : ITimeProvider
  {
    public float CurrentTime => 0;
  }

  private sealed class NoopDisposable : IDisposable
  {
    public void Dispose() { }
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

  private static CombatEvent CreateHealEvent(long timestamp, EventType eventType) =>
    new()
    {
      Id = "evt-heal",
      Timestamp = timestamp,
      EventType = eventType,
      Source = new ActorRef
      {
        Id = "npc:grace",
        Name = "Grace",
        Type = ActorType.Npc,
      },
      Target = new ActorRef
      {
        Id = "npc:grace",
        Name = "Grace",
        Type = ActorType.Npc,
      },
      Ability = new AbilityRef
      {
        Name = "Grace Echoes",
        Type = AbilityType.AreaEffect,
        StableKey = "mechanic:grace-echoes",
      },
      Amount = 200000,
      RawAmount = 200000,
      OverhealAmount = 0,
    };

  private static CombatEvent CreateResourceDrainEvent(long timestamp) =>
    new()
    {
      Id = "evt-resource",
      Timestamp = timestamp,
      EventType = EventType.ManaUse,
      Source = new ActorRef
      {
        Id = "npc:mana-drain",
        Name = "Mana Drain",
        Type = ActorType.Npc,
      },
      Target = new ActorRef
      {
        Id = "player:0",
        Name = "Player",
        Type = ActorType.Player,
      },
      Ability = new AbilityRef
      {
        Name = "Mana Drain",
        Type = AbilityType.AreaEffect,
        StableKey = "mechanic:mana-drain",
      },
      ResourceType = "mana",
      ResourceDelta = -300,
      ResourceCurrent = 1200,
      ResourceMax = 1500,
    };

  private static CombatEvent CreateManaRestoreEvent(long timestamp) =>
    CreateResourceDrainEvent(timestamp) with
    {
      EventType = EventType.ManaRestore,
      ResourceDelta = 300,
    };

  private static CombatEvent CreateEffectEvent(long timestamp) =>
    new()
    {
      Id = "evt-effect",
      Timestamp = timestamp,
      EventType = EventType.DebuffApply,
      Source = new ActorRef
      {
        Id = "npc:mizuki",
        Name = "Mizuki",
        Type = ActorType.Npc,
      },
      Target = new ActorRef
      {
        Id = "player:0",
        Name = "Player",
        Type = ActorType.Player,
      },
      Ability = new AbilityRef
      {
        Name = "Dagger Bleed",
        Type = AbilityType.Dot,
        StableKey = "mechanic:mizuki-dagger",
      },
      Effect = new EffectRef
      {
        Name = "BleedRef",
        Duration = 12,
        Stacks = 1,
      },
      EffectAction = "apply",
      EffectStacks = 1,
      EffectDurationMs = 12000,
    };

  private static CombatEvent CreateDeathEvent(long timestamp) =>
    new()
    {
      Id = "evt-death",
      Timestamp = timestamp,
      EventType = EventType.Death,
      Source = new ActorRef
      {
        Id = "npc:death-touch",
        Name = "Death Touch",
        Type = ActorType.Npc,
      },
      Target = new ActorRef
      {
        Id = "sim:cleric",
        Name = "Cleric",
        Type = ActorType.SimPlayer,
      },
      Ability = new AbilityRef
      {
        Name = "Death Touch",
        Type = AbilityType.AreaEffect,
        StableKey = "mechanic:death-touch",
      },
      KillingBlowEventSeq = 1,
    };

  private static CombatEvent CreateMechanicEvent(long timestamp) =>
    new()
    {
      Id = "evt-mechanic",
      Timestamp = timestamp,
      EventType = EventType.Mechanic,
      Source = new ActorRef
      {
        Id = "npc:sprinkles",
        Name = "Sprinkles",
        Type = ActorType.Npc,
      },
      Target = new ActorRef
      {
        Id = "npc:sprinkles",
        Name = "Sprinkles",
        Type = ActorType.Npc,
      },
      Ability = new AbilityRef
      {
        Name = "Sprinkles Wards",
        Type = AbilityType.AreaEffect,
        StableKey = "mechanic:sprinkles-wards",
      },
      Mechanic = new EventMechanicData
      {
        Name = "Sprinkles wards",
        Action = "invulnerability",
        Value = true,
      },
    };
}
