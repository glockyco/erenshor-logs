using ErenshorLogs.Events;
using ErenshorLogs.Session;
using Xunit;

namespace ErenshorLogs.Tests.Session;

public class SessionManagerTests
{
  private class FakeGameVersionProvider : IGameVersionProvider
  {
    public string GameVersion { get; set; } = "1.0.0";

    public string GetGameVersion() => GameVersion;
  }

  private class FakeEventEmitter : IEventEmitter
  {
    public List<CombatEvent> EmittedEvents { get; } = [];
    public int ListenerCount => 0;
    public long EventCount => EmittedEvents.Count;

    public void Emit(CombatEvent evt) => EmittedEvents.Add(evt);

    public IDisposable Subscribe(Action<CombatEvent> handler) =>
      throw new NotImplementedException();
  }

  [Fact]
  public void OnCombatStateChanged_ToTrue_CreatesSession()
  {
    var emitter = new FakeEventEmitter();
    var provider = new FakeGameVersionProvider();
    var manager = new SessionManager(emitter, provider, "0.1.0");

    manager.OnCombatStateChanged(true);

    Assert.NotNull(manager.CurrentSession);
    Assert.True(manager.InCombat);
  }

  [Fact]
  public void OnCombatStateChanged_ToTrue_EmitsCombatStartEvent()
  {
    var emitter = new FakeEventEmitter();
    var provider = new FakeGameVersionProvider();
    var manager = new SessionManager(emitter, provider, "0.1.0");

    manager.OnCombatStateChanged(true);

    Assert.Single(emitter.EmittedEvents);
    Assert.Equal(EventType.CombatStart, emitter.EmittedEvents[0].EventType);
  }

  [Fact]
  public void OnCombatStateChanged_ToTrue_RaisesSessionStartedEvent()
  {
    var emitter = new FakeEventEmitter();
    var provider = new FakeGameVersionProvider();
    var manager = new SessionManager(emitter, provider, "0.1.0");
    CombatSession? receivedSession = null;
    manager.SessionStarted += s => receivedSession = s;

    manager.OnCombatStateChanged(true);

    Assert.NotNull(receivedSession);
    Assert.Same(manager.CurrentSession, receivedSession);
  }

  [Fact]
  public void OnCombatStateChanged_ToFalse_DoesNotEndSessionImmediately()
  {
    var emitter = new FakeEventEmitter();
    var provider = new FakeGameVersionProvider();
    var manager = new SessionManager(emitter, provider, "0.1.0");
    manager.OnCombatStateChanged(true);

    manager.OnCombatStateChanged(false);

    // Session should still exist (not ended immediately)
    Assert.NotNull(manager.CurrentSession);
    Assert.False(manager.InCombat);
  }

  [Fact]
  public void OnCombatStateChanged_ToFalse_DoesNotEmitCombatEndImmediately()
  {
    var emitter = new FakeEventEmitter();
    var provider = new FakeGameVersionProvider();
    var manager = new SessionManager(emitter, provider, "0.1.0");
    manager.OnCombatStateChanged(true);
    emitter.EmittedEvents.Clear();

    manager.OnCombatStateChanged(false);

    // No immediate combatEnd event - waits for inactivity timeout
    Assert.Empty(emitter.EmittedEvents);
  }

  [Fact]
  public void OnCombatStateChanged_ToFalse_DoesNotRaiseSessionEndedImmediately()
  {
    var emitter = new FakeEventEmitter();
    var provider = new FakeGameVersionProvider();
    var manager = new SessionManager(emitter, provider, "0.1.0");
    manager.OnCombatStateChanged(true);
    var startedSession = manager.CurrentSession;
    CombatSession? endedSession = null;
    manager.SessionEnded += s => endedSession = s;

    manager.OnCombatStateChanged(false);

    // Session not ended immediately - waits for inactivity timeout
    Assert.Null(endedSession);
    Assert.Same(startedSession, manager.CurrentSession);
  }

  [Fact]
  public void OnCombatStateChanged_SameState_NoAction()
  {
    var emitter = new FakeEventEmitter();
    var provider = new FakeGameVersionProvider();
    var manager = new SessionManager(emitter, provider, "0.1.0");

    manager.OnCombatStateChanged(false);
    manager.OnCombatStateChanged(false);

    Assert.Empty(emitter.EmittedEvents);
    Assert.Null(manager.CurrentSession);
  }

  [Fact]
  public void OnCombatStateChanged_TrueWhenAlreadyTrue_NoAction()
  {
    var emitter = new FakeEventEmitter();
    var provider = new FakeGameVersionProvider();
    var manager = new SessionManager(emitter, provider, "0.1.0");
    manager.OnCombatStateChanged(true);
    var firstSession = manager.CurrentSession;
    emitter.EmittedEvents.Clear();

    manager.OnCombatStateChanged(true);

    Assert.Empty(emitter.EmittedEvents);
    Assert.Same(firstSession, manager.CurrentSession);
  }

  [Fact]
  public void OnCombatStateChanged_CapturesVersionInfo()
  {
    var emitter = new FakeEventEmitter();
    var provider = new FakeGameVersionProvider { GameVersion = "2.0.0" };
    var manager = new SessionManager(emitter, provider, "0.5.0");

    manager.OnCombatStateChanged(true);

    Assert.Equal("2.0.0", manager.CurrentSession!.GameVersion);
    Assert.Equal("0.5.0", manager.CurrentSession.ModVersion);
  }

  [Fact]
  public void MultipleTransitions_SessionPersistsAcrossCombatStateChanges()
  {
    var emitter = new FakeEventEmitter();
    var provider = new FakeGameVersionProvider();
    var manager = new SessionManager(emitter, provider, "0.1.0");

    manager.OnCombatStateChanged(true);
    var firstSessionId = manager.CurrentSession!.Id;

    // Combat ends - but session continues (no immediate end)
    manager.OnCombatStateChanged(false);
    Assert.NotNull(manager.CurrentSession);

    // Combat starts again - session is reused (not ended)
    manager.OnCombatStateChanged(true);
    var secondSessionId = manager.CurrentSession!.Id;

    // Same session (inactivity timeout hasn't triggered without events)
    Assert.Equal(firstSessionId, secondSessionId);
  }

  [Fact]
  public void OnCombatStateChanged_EndWithoutStart_NoAction()
  {
    var emitter = new FakeEventEmitter();
    var provider = new FakeGameVersionProvider();
    var manager = new SessionManager(emitter, provider, "0.1.0");

    // Force state to true without a session
    manager.OnCombatStateChanged(false);

    Assert.Empty(emitter.EmittedEvents);
  }

  [Fact]
  public void CheckSessionTimeouts_SessionWithoutEvents_DoesNotTimeout()
  {
    var emitter = new FakeEventEmitter();
    var provider = new FakeGameVersionProvider();
    var manager = new SessionManager(emitter, provider, "0.1.0");

    // Start session via combat state (no events, so no _lastEventTime)
    manager.OnCombatStateChanged(true);

    // Check at t=10s - session won't timeout without _lastEventTime
    manager.CheckSessionTimeouts(10.0f);

    // Session remains open (inactivity timeout requires events)
    Assert.NotNull(manager.CurrentSession);
  }

  [Fact]
  public void CheckSessionTimeouts_CombatEndDoesNotEndSessionImmediately()
  {
    var emitter = new FakeEventEmitter();
    var provider = new FakeGameVersionProvider();
    var manager = new SessionManager(emitter, provider, "0.1.0");

    // Start session
    manager.OnCombatStateChanged(true);
    var startedSession = manager.CurrentSession;

    // Combat ends - session continues (no immediate end)
    manager.OnCombatStateChanged(false);

    Assert.NotNull(manager.CurrentSession);
    Assert.Same(startedSession, manager.CurrentSession);
    Assert.False(manager.InCombat);
  }
}
