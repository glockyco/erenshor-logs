using ErenshorLogs.Events;
using ErenshorLogs.Session;
using Xunit;

namespace ErenshorLogs.Tests.Events;

public sealed class CombatEventDispatcherTests
{
  [Fact]
  public void PrepareForCapture_NotifiesSessionManagerBeforeActorResolution()
  {
    var calls = new List<string>();
    var sessionManager = new RecordingSessionManager(calls);

    var timestamp = CombatEventDispatcher.PrepareForCapture(
      EventType.DamagePhysical,
      sessionManager,
      123
    );
    calls.Add("resolve-actors");

    Assert.Equal(["session", "resolve-actors"], calls);
    Assert.Equal(EventType.DamagePhysical, sessionManager.EventType);
    Assert.Equal(123, sessionManager.Timestamp);
    Assert.Equal(123, timestamp);
  }

  [Fact]
  public void Dispatch_EmitsPreparedEventWithoutRenotifyingSessionManager()
  {
    var calls = new List<string>();
    var sessionManager = new RecordingSessionManager(calls);
    var emitter = new RecordingEmitter(calls);
    var evt = new CombatEvent
    {
      Id = "evt-1",
      Timestamp = 123,
      EventType = EventType.DamagePhysical,
      Ability = new AbilityRef { Name = "Backstab", Type = AbilityType.Skill },
    };

    CombatEventDispatcher.Dispatch(evt, emitter);

    Assert.Equal(["emit"], calls);
    Assert.Null(sessionManager.EventType);
    Assert.Null(sessionManager.Timestamp);
    Assert.Same(evt, emitter.Event);
  }

  [Fact]
  public void PrepareBeforeActorResolution_KeepsActorIdsStableAcrossSessionStart()
  {
    var player = new MockCharacter(1, "Ceevia", ActorType.Player);
    var bear = new MockCharacter(2, "A Brown Bear Cub", ActorType.Npc);
    var drone = new MockCharacter(3, "A Faerie Drone", ActorType.Npc);
    var registry = new ErenshorLogs.Registry.ActorRegistry<MockCharacter>(
      c => c.InstanceId,
      c => c.Type,
      c => new ErenshorLogs.Registry.ActorData { Name = c.Name }
    );
    var sessionManager = new RecordingSessionManager([]);
    sessionManager.SessionStarted += _ => registry.Clear();
    long currentTimestamp = 0;
    var builder = new ErenshorLogs.Hooks.CombatEventBuilder<MockCharacter>(
      registry.GetOrCreate,
      () => "event",
      () => currentTimestamp
    );

    currentTimestamp = CombatEventDispatcher.PrepareForCapture(
      EventType.DamagePhysical,
      sessionManager,
      100
    );
    var first = builder.CreateDamageEvent(
      EventType.DamagePhysical,
      target: drone,
      source: bear,
      amount: 4,
      DamageType.Physical,
      new AbilityRef { Name = "Scratch", Type = AbilityType.Auto }
    )!;
    registry.GetOrCreate(player);
    var second = builder.CreateDamageEvent(
      EventType.DamagePhysical,
      target: drone,
      source: player,
      amount: 314,
      DamageType.Physical,
      new AbilityRef { Name = "Attack", Type = AbilityType.Auto }
    )!;

    Assert.Equal(first.Target!.Id, second.Target!.Id);
  }

  private sealed record MockCharacter(int InstanceId, string Name, ActorType Type);

  private sealed class RecordingEmitter(List<string> calls) : IEventEmitter
  {
    public CombatEvent? Event { get; private set; }
    public int ListenerCount => 0;
    public long EventCount => Event == null ? 0 : 1;

    public void Emit(CombatEvent evt)
    {
      calls.Add("emit");
      Event = evt;
    }

    public IDisposable Subscribe(Action<CombatEvent> handler) => new NoopDisposable();
  }

  private sealed class RecordingSessionManager(List<string> calls) : ISessionManager
  {
    public EventType? EventType { get; private set; }
    public long? Timestamp { get; private set; }
    public CombatSession? CurrentSession => null;
    public event Action<CombatSession>? SessionStarted;
    public event Action<CombatSession, string>? SessionEnded;

    public void OnCombatEvent(EventType eventType, long eventTimestamp)
    {
      calls.Add("session");
      EventType = eventType;
      Timestamp = eventTimestamp;
    }

    public void CheckInactivityTimeout(float currentTime) { }

    public void StartManualSession() => SessionStarted?.Invoke(new CombatSession("test", "test"));

    public void EndManualSession()
    {
      SessionEnded?.Invoke(new CombatSession("test", "test"), SessionEndReasons.Manual);
    }

    public void EndCurrentSessionForShutdown() { }
  }

  private sealed class NoopDisposable : IDisposable
  {
    public void Dispose() { }
  }
}
