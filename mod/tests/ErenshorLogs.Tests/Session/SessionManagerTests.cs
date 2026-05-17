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

  private class FakeTimeProvider : ITimeProvider
  {
    public float CurrentTime { get; set; } = 0.0f;
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

  /// <summary>
  /// Default session start events for tests (matches typical config).
  /// </summary>
  private const string DefaultStartEvents = "DamagePhysical,DamageMagic,DamageDot";

  /// <summary>
  /// Default session keep-alive events for tests.
  /// </summary>
  private const string DefaultKeepAliveEvents =
    "DamagePhysical,DamageMagic,DamageDot,DamageEnvironmental";

  /// <summary>
  /// Default inactivity timeout in seconds.
  /// </summary>
  private const float DefaultTimeout = 5.0f;

  private static SessionManager CreateManager(
    FakeEventEmitter emitter,
    FakeGameVersionProvider versionProvider,
    FakeTimeProvider timeProvider,
    string modVersion = "0.1.0",
    bool autoDetectionEnabled = true,
    float inactivityTimeout = DefaultTimeout,
    string? startEvents = null,
    string? keepAliveEvents = null
  )
  {
    return new SessionManager(
      emitter,
      versionProvider,
      timeProvider,
      modVersion,
      autoDetectionEnabled,
      inactivityTimeout,
      startEvents ?? DefaultStartEvents,
      keepAliveEvents ?? DefaultKeepAliveEvents
    );
  }

  [Fact]
  public void OnCombatEvent_WithStartEvent_CreatesSession()
  {
    var emitter = new FakeEventEmitter();
    var versionProvider = new FakeGameVersionProvider();
    var timeProvider = new FakeTimeProvider();
    var manager = CreateManager(emitter, versionProvider, timeProvider);

    manager.OnCombatEvent(EventType.DamagePhysical, 1000);

    Assert.NotNull(manager.CurrentSession);
  }

  [Fact]
  public void OnCombatEvent_WithStartEvent_EmitsCombatStartEvent()
  {
    var emitter = new FakeEventEmitter();
    var versionProvider = new FakeGameVersionProvider();
    var timeProvider = new FakeTimeProvider();
    var manager = CreateManager(emitter, versionProvider, timeProvider);

    manager.OnCombatEvent(EventType.DamagePhysical, 1000);

    Assert.Single(emitter.EmittedEvents);
    Assert.Equal(EventType.CombatStart, emitter.EmittedEvents[0].EventType);
  }

  [Fact]
  public void OnCombatEvent_WithStartEvent_RaisesSessionStartedEvent()
  {
    var emitter = new FakeEventEmitter();
    var versionProvider = new FakeGameVersionProvider();
    var timeProvider = new FakeTimeProvider();
    var manager = CreateManager(emitter, versionProvider, timeProvider);
    CombatSession? receivedSession = null;
    manager.SessionStarted += s => receivedSession = s;

    manager.OnCombatEvent(EventType.DamagePhysical, 1000);

    Assert.NotNull(receivedSession);
    Assert.Same(manager.CurrentSession, receivedSession);
  }

  [Fact]
  public void OnCombatEvent_NonStartEvent_DoesNotCreateSession()
  {
    var emitter = new FakeEventEmitter();
    var versionProvider = new FakeGameVersionProvider();
    var timeProvider = new FakeTimeProvider();
    var manager = CreateManager(emitter, versionProvider, timeProvider);

    // Environmental damage is not a start event by default
    manager.OnCombatEvent(EventType.DamageEnvironmental, 1000);

    Assert.Null(manager.CurrentSession);
    Assert.Empty(emitter.EmittedEvents);
  }

  [Fact]
  public void OnCombatEvent_WhenSessionExists_DoesNotCreateNewSession()
  {
    var emitter = new FakeEventEmitter();
    var versionProvider = new FakeGameVersionProvider();
    var timeProvider = new FakeTimeProvider();
    var manager = CreateManager(emitter, versionProvider, timeProvider);

    manager.OnCombatEvent(EventType.DamagePhysical, 1000);
    var firstSession = manager.CurrentSession;
    emitter.EmittedEvents.Clear();

    timeProvider.CurrentTime = 1.0f;
    manager.OnCombatEvent(EventType.DamagePhysical, 2000);

    Assert.Same(firstSession, manager.CurrentSession);
    Assert.Empty(emitter.EmittedEvents);
  }

  [Fact]
  public void OnCombatEvent_WhenHealEventIsConfiguredAsKeepAlive_ExtendsSession()
  {
    var emitter = new FakeEventEmitter();
    var versionProvider = new FakeGameVersionProvider();
    var timeProvider = new FakeTimeProvider();
    var manager = CreateManager(
      emitter,
      versionProvider,
      timeProvider,
      startEvents: EventType.DamagePhysical.ToString(),
      keepAliveEvents: $"{EventType.DamagePhysical},{EventType.HealSpell}"
    );

    manager.OnCombatEvent(EventType.DamagePhysical, eventTimestamp: 1_000);
    timeProvider.CurrentTime = 4.0f;
    manager.OnCombatEvent(EventType.HealSpell, eventTimestamp: 4_000);
    manager.CheckInactivityTimeout(currentTime: 8.0f);

    Assert.NotNull(manager.CurrentSession);
  }

  [Fact]
  public void OnCombatEvent_CapturesVersionInfo()
  {
    var emitter = new FakeEventEmitter();
    var versionProvider = new FakeGameVersionProvider { GameVersion = "2.0.0" };
    var timeProvider = new FakeTimeProvider();
    var manager = CreateManager(emitter, versionProvider, timeProvider, modVersion: "0.5.0");

    manager.OnCombatEvent(EventType.DamagePhysical, 1000);

    Assert.Equal("2.0.0", manager.CurrentSession!.GameVersion);
    Assert.Equal("0.5.0", manager.CurrentSession.ModVersion);
  }

  [Fact]
  public void CheckInactivityTimeout_BeforeTimeout_DoesNotEndSession()
  {
    var emitter = new FakeEventEmitter();
    var versionProvider = new FakeGameVersionProvider();
    var timeProvider = new FakeTimeProvider { CurrentTime = 0.0f };
    var manager = CreateManager(emitter, versionProvider, timeProvider, inactivityTimeout: 5.0f);

    // Start session at t=0
    manager.OnCombatEvent(EventType.DamagePhysical, 1000);
    emitter.EmittedEvents.Clear();

    // Check at t=3s (before 5s timeout)
    manager.CheckInactivityTimeout(3.0f);

    Assert.NotNull(manager.CurrentSession);
    Assert.Empty(emitter.EmittedEvents);
  }

  [Fact]
  public void CheckInactivityTimeout_AfterTimeout_EndsSession()
  {
    var emitter = new FakeEventEmitter();
    var versionProvider = new FakeGameVersionProvider();
    var timeProvider = new FakeTimeProvider { CurrentTime = 0.0f };
    var manager = CreateManager(emitter, versionProvider, timeProvider, inactivityTimeout: 5.0f);

    // Start session at t=0
    manager.OnCombatEvent(EventType.DamagePhysical, 1000);
    emitter.EmittedEvents.Clear();

    // Check at t=6s (after 5s timeout)
    manager.CheckInactivityTimeout(6.0f);

    Assert.Null(manager.CurrentSession);
    Assert.Single(emitter.EmittedEvents);
    Assert.Equal(EventType.CombatEnd, emitter.EmittedEvents[0].EventType);
  }

  [Fact]
  public void CheckInactivityTimeout_AfterTimeout_RaisesSessionEndedEvent()
  {
    var emitter = new FakeEventEmitter();
    var versionProvider = new FakeGameVersionProvider();
    var timeProvider = new FakeTimeProvider { CurrentTime = 0.0f };
    var manager = CreateManager(emitter, versionProvider, timeProvider, inactivityTimeout: 5.0f);
    CombatSession? endedSession = null;
    manager.SessionEnded += (session, _) => endedSession = session;

    manager.OnCombatEvent(EventType.DamagePhysical, 1000);
    var startedSession = manager.CurrentSession;

    manager.CheckInactivityTimeout(6.0f);

    Assert.NotNull(endedSession);
    Assert.Same(startedSession, endedSession);
    Assert.Equal(SessionEndReasons.Inactivity, endedSession.EndReason);
  }

  [Fact]
  public void CheckInactivityTimeout_WithNoSession_DoesNothing()
  {
    var emitter = new FakeEventEmitter();
    var versionProvider = new FakeGameVersionProvider();
    var timeProvider = new FakeTimeProvider();
    var manager = CreateManager(emitter, versionProvider, timeProvider);

    manager.CheckInactivityTimeout(10.0f);

    Assert.Null(manager.CurrentSession);
    Assert.Empty(emitter.EmittedEvents);
  }

  [Fact]
  public void StartManualSession_CreatesSession()
  {
    var emitter = new FakeEventEmitter();
    var versionProvider = new FakeGameVersionProvider();
    var timeProvider = new FakeTimeProvider();
    var manager = CreateManager(emitter, versionProvider, timeProvider);

    manager.StartManualSession();

    Assert.NotNull(manager.CurrentSession);
    Assert.True(manager.CurrentSession.IsManual);
  }

  [Fact]
  public void StartManualSession_EmitsCombatStartEvent()
  {
    var emitter = new FakeEventEmitter();
    var versionProvider = new FakeGameVersionProvider();
    var timeProvider = new FakeTimeProvider();
    var manager = CreateManager(emitter, versionProvider, timeProvider);

    manager.StartManualSession();

    Assert.Single(emitter.EmittedEvents);
    Assert.Equal(EventType.CombatStart, emitter.EmittedEvents[0].EventType);
  }

  [Fact]
  public void StartManualSession_WhenSessionExists_EndsExistingFirst()
  {
    var emitter = new FakeEventEmitter();
    var versionProvider = new FakeGameVersionProvider();
    var timeProvider = new FakeTimeProvider();
    var manager = CreateManager(emitter, versionProvider, timeProvider);
    CombatSession? endedSession = null;
    manager.SessionEnded += (session, _) => endedSession = session;

    manager.StartManualSession();
    var firstSession = manager.CurrentSession;
    emitter.EmittedEvents.Clear();

    manager.StartManualSession();
    // Should have ended first session and started second
    Assert.NotSame(firstSession, manager.CurrentSession);
    Assert.Same(firstSession, endedSession);
    Assert.Equal(SessionEndReasons.NewSession, endedSession!.EndReason);
    Assert.Equal(2, emitter.EmittedEvents.Count);
    Assert.Equal(EventType.CombatEnd, emitter.EmittedEvents[0].EventType);
    Assert.Equal(EventType.CombatStart, emitter.EmittedEvents[1].EventType);
  }

  [Fact]
  public void EndManualSession_EndsSession()
  {
    var emitter = new FakeEventEmitter();
    var versionProvider = new FakeGameVersionProvider();
    var timeProvider = new FakeTimeProvider();
    var manager = CreateManager(emitter, versionProvider, timeProvider);
    CombatSession? endedSession = null;
    manager.SessionEnded += (session, _) => endedSession = session;

    manager.StartManualSession();
    var activeSession = manager.CurrentSession;
    emitter.EmittedEvents.Clear();

    manager.EndManualSession();

    Assert.Null(manager.CurrentSession);
    Assert.Same(activeSession, endedSession);
    Assert.Equal(SessionEndReasons.Manual, endedSession!.EndReason);
    Assert.Single(emitter.EmittedEvents);
    Assert.Equal(EventType.CombatEnd, emitter.EmittedEvents[0].EventType);
  }

  [Fact]
  public void EndCurrentSessionForShutdown_EndsWithShutdownReason()
  {
    var emitter = new FakeEventEmitter();
    var versionProvider = new FakeGameVersionProvider();
    var timeProvider = new FakeTimeProvider();
    var manager = CreateManager(emitter, versionProvider, timeProvider);
    CombatSession? endedSession = null;
    manager.SessionEnded += (session, _) => endedSession = session;

    manager.StartManualSession();
    var activeSession = manager.CurrentSession;
    emitter.EmittedEvents.Clear();

    manager.EndCurrentSessionForShutdown();

    Assert.Null(manager.CurrentSession);
    Assert.Same(activeSession, endedSession);
    Assert.Equal(SessionEndReasons.Shutdown, endedSession!.EndReason);
    Assert.Single(emitter.EmittedEvents);
    Assert.Equal(EventType.CombatEnd, emitter.EmittedEvents[0].EventType);
  }

  [Fact]
  public void EndManualSession_WithNoSession_DoesNothing()
  {
    var emitter = new FakeEventEmitter();
    var versionProvider = new FakeGameVersionProvider();
    var timeProvider = new FakeTimeProvider();
    var manager = CreateManager(emitter, versionProvider, timeProvider);

    manager.EndManualSession();

    Assert.Null(manager.CurrentSession);
    Assert.Empty(emitter.EmittedEvents);
  }

  [Fact]
  public void ManualSession_DoesNotTimeout()
  {
    var emitter = new FakeEventEmitter();
    var versionProvider = new FakeGameVersionProvider();
    var timeProvider = new FakeTimeProvider();
    var manager = CreateManager(emitter, versionProvider, timeProvider, inactivityTimeout: 1.0f);

    manager.StartManualSession();
    emitter.EmittedEvents.Clear();

    // Even with long time passed, manual session should not timeout
    manager.CheckInactivityTimeout(100.0f);

    Assert.NotNull(manager.CurrentSession);
    Assert.Empty(emitter.EmittedEvents);
  }

  [Fact]
  public void AutoDetectionDisabled_DoesNotAutoStartSession()
  {
    var emitter = new FakeEventEmitter();
    var versionProvider = new FakeGameVersionProvider();
    var timeProvider = new FakeTimeProvider();
    var manager = CreateManager(
      emitter,
      versionProvider,
      timeProvider,
      autoDetectionEnabled: false
    );

    manager.OnCombatEvent(EventType.DamagePhysical, 1000);

    Assert.Null(manager.CurrentSession);
    Assert.Empty(emitter.EmittedEvents);
  }

  [Fact]
  public void AutoDetectionDisabled_ManualSessionStillWorks()
  {
    var emitter = new FakeEventEmitter();
    var versionProvider = new FakeGameVersionProvider();
    var timeProvider = new FakeTimeProvider();
    var manager = CreateManager(
      emitter,
      versionProvider,
      timeProvider,
      autoDetectionEnabled: false
    );

    manager.StartManualSession();

    Assert.NotNull(manager.CurrentSession);
    Assert.True(manager.CurrentSession.IsManual);
  }
}
