using ErenshorLogs.Broadcast;
using ErenshorLogs.Events;
using ErenshorLogs.Server;
using ErenshorLogs.Session;
using Newtonsoft.Json.Linq;
using Xunit;

namespace ErenshorLogs.Tests.Broadcast;

public sealed class CombatEventBroadcasterTests
{
  [Fact]
  public void SendHandshakeToNewClient_SendsCatchupOnlyToTargetClient()
  {
    using var harness = BroadcasterHarness.Create(clientCount: 2);
    var session = new CombatSession("playtest-23258843", "2026.5.17.14");
    var newClient = new FakeClient();
    harness.SessionManager.Start(session);
    harness.Emitter.Emit(CreateDamageEvent(session.StartTime + 100));
    harness.Broadcaster.Tick(1.0f);
    harness.Server.Messages.Clear();

    harness.Broadcaster.SendHandshakeToNewClient(newClient);

    Assert.Empty(harness.Server.Messages);
    Assert.Equal(["hello", "sessionSnapshot", "events"], newClient.Messages.Select(ReadKind));
  }

  [Fact]
  public void SessionStarted_BroadcastsSnapshotEnvelope()
  {
    using var harness = BroadcasterHarness.Create(clientCount: 1);

    harness.SessionManager.Start(new CombatSession("playtest-23258843", "2026.5.17.14"));

    var frame = ParseLastFrame(harness.Server);
    Assert.Equal("sessionSnapshot", frame.Value<string>("kind"));
    Assert.Equal(harness.SessionManager.CurrentSession!.Id, frame.Value<string>("sessionId"));
    Assert.Equal("active", frame["payload"]!.Value<string>("state"));
    Assert.NotNull(frame["payload"]!["registries"]);
  }

  [Fact]
  public void SessionEnded_BroadcastsSessionEndReason()
  {
    using var harness = BroadcasterHarness.Create(clientCount: 1);
    var session = new CombatSession("playtest-23258843", "2026.5.17.14");
    harness.SessionManager.Start(session);
    harness.Server.Messages.Clear();

    harness.SessionManager.EndManualSession();

    var frame = ParseLastFrame(harness.Server);
    Assert.Equal("sessionEnded", frame.Value<string>("kind"));
    Assert.Equal(SessionEndReasons.Manual, frame["payload"]!.Value<string>("reason"));
  }

  [Fact]
  public void Shutdown_BroadcastsShutdownSessionEndReason()
  {
    using var harness = BroadcasterHarness.Create(clientCount: 1);
    var session = new CombatSession("playtest-23258843", "2026.5.17.14");
    harness.SessionManager.Start(session);
    harness.Server.Messages.Clear();

    harness.SessionManager.EndCurrentSessionForShutdown();

    var frame = ParseLastFrame(harness.Server);
    Assert.Equal("sessionEnded", frame.Value<string>("kind"));
    Assert.Equal(SessionEndReasons.Shutdown, frame["payload"]!.Value<string>("reason"));
  }

  [Fact]
  public void Tick_BroadcastsRegistryDeltaBeforeFirstEventBatch()
  {
    using var harness = BroadcasterHarness.Create(clientCount: 1);
    var session = new CombatSession("playtest-23258843", "2026.5.17.14");
    harness.SessionManager.Start(session);
    harness.Server.Messages.Clear();

    harness.Emitter.Emit(CreateDamageEvent(session.StartTime + 100));
    harness.Broadcaster.Tick(1.0f);

    Assert.Equal(["registryDelta", "events"], harness.Server.Messages.Select(ReadKind));
    var delta = JObject.Parse(harness.Server.Messages[0]);
    Assert.Equal("Player", delta["payload"]!["actors"]!["player:0"]!.Value<string>("name"));
  }

  [Fact]
  public void Tick_DropsQueuedEventsWhenNoClientsAreConnected()
  {
    using var harness = BroadcasterHarness.Create(clientCount: 0);
    var unwatched = new CombatSession("playtest-23258843", "2026.5.17.14");
    harness.SessionManager.Start(unwatched);
    harness.Emitter.Emit(CreateDamageEvent(unwatched.StartTime + 100));
    harness.Broadcaster.Tick(1.0f);

    harness.Server.ClientCount = 1;
    var watched = new CombatSession("playtest-23258843", "2026.5.17.14");
    harness.SessionManager.Start(watched);
    harness.Server.Messages.Clear();
    harness.Broadcaster.Tick(1.0f);

    Assert.Empty(harness.Server.Messages);
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
      },
      Target = new ActorRef
      {
        Id = "npc:1",
        Name = "Raid Boss",
        Type = ActorType.Npc,
      },
      Ability = new AbilityRef
      {
        Name = "Backstab",
        Type = AbilityType.Skill,
        StableKey = "skill:101",
      },
      Amount = 350,
      DamageType = DamageType.Physical,
    };

  private static string? ReadKind(string message) => JObject.Parse(message).Value<string>("kind");

  private static JObject ParseLastFrame(FakeServer server)
  {
    Assert.NotEmpty(server.Messages);
    return JObject.Parse(server.Messages[^1]);
  }

  private sealed class BroadcasterHarness : IDisposable
  {
    private BroadcasterHarness(int clientCount)
    {
      Emitter = new EventEmitter();
      SessionManager = new FakeSessionManager();
      Server = new FakeServer(clientCount);
      Broadcaster = new CombatEventBroadcaster(
        Emitter,
        SessionManager,
        Server,
        broadcastIntervalMs: 100,
        modVersion: "2026.5.17.14"
      );
    }

    public EventEmitter Emitter { get; }
    public FakeSessionManager SessionManager { get; }
    public FakeServer Server { get; }
    public CombatEventBroadcaster Broadcaster { get; }

    public static BroadcasterHarness Create(int clientCount) => new(clientCount);

    public void Dispose()
    {
      Broadcaster.Dispose();
    }
  }

  private sealed class FakeServer(int clientCount) : IWebSocketServer
  {
    public List<string> Messages { get; } = [];
    public int ClientCount { get; set; } = clientCount;
    public event Action<IWebSocketClient>? ClientConnected;

    public void Start() { }

    public void Stop() { }

    public void Broadcast(string message) => Messages.Add(message);

    public void Dispose() { }

    public void RaiseClientConnected(IWebSocketClient client) => ClientConnected?.Invoke(client);
  }

  private sealed class FakeClient : IWebSocketClient
  {
    public Guid Id { get; } = Guid.NewGuid();
    public string ClientIpAddress => "127.0.0.1";
    public List<string> Messages { get; } = [];

    public void Send(string message) => Messages.Add(message);
  }

  private sealed class FakeSessionManager : ISessionManager
  {
    public CombatSession? CurrentSession { get; private set; }
    public event Action<CombatSession>? SessionStarted;
    public event Action<CombatSession, string>? SessionEnded;

    public void OnCombatEvent(EventType eventType, long eventTimestamp) { }

    public void CheckInactivityTimeout(float currentTime) { }

    public void StartManualSession() { }

    public void EndManualSession()
    {
      if (CurrentSession == null)
        return;

      SessionEnded?.Invoke(CurrentSession, SessionEndReasons.Manual);
      CurrentSession = null;
    }

    public void EndCurrentSessionForShutdown()
    {
      if (CurrentSession == null)
        return;

      SessionEnded?.Invoke(CurrentSession, SessionEndReasons.Shutdown);
      CurrentSession = null;
    }

    public void Start(CombatSession session)
    {
      CurrentSession = session;
      SessionStarted?.Invoke(session);
    }
  }
}
