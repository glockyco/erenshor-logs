using ErenshorLogs.Events;
using ErenshorLogs.Session;
using Xunit;

namespace ErenshorLogs.Tests.Events;

public sealed class CombatEventDispatcherTests
{
  [Fact]
  public void Dispatch_NotifiesSessionManagerBeforeEmittingEvent()
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

    CombatEventDispatcher.Dispatch(evt, emitter, sessionManager);

    Assert.Equal(["session", "emit"], calls);
    Assert.Equal(EventType.DamagePhysical, sessionManager.EventType);
    Assert.Equal(123, sessionManager.Timestamp);
    Assert.Same(evt, emitter.Event);
  }

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
  }

  private sealed class NoopDisposable : IDisposable
  {
    public void Dispose() { }
  }
}
