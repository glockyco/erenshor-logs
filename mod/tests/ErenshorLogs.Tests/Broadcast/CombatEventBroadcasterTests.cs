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
  public void SendHandshakeToNewClient_BroadcastsV2HelloEnvelope()
  {
    using var harness = BroadcasterHarness.Create(clientCount: 1);

    harness.Broadcaster.SendHandshakeToNewClient();

    var frame = ParseLastFrame(harness.Server);
    Assert.Equal("erenshor.logs.live", frame.Value<string>("protocol"));
    Assert.Equal("hello", frame.Value<string>("kind"));
    Assert.Equal(1, frame.Value<long>("frameSeq"));
    Assert.Equal("ErenshorLogsMod", frame["payload"]!["producer"]!.Value<string>("name"));
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
  public void Tick_BroadcastsQueuedEventsAsV2Batch()
  {
    using var harness = BroadcasterHarness.Create(clientCount: 1);
    var session = new CombatSession("playtest-23258843", "2026.5.17.14");
    harness.SessionManager.Start(session);
    harness.Server.Messages.Clear();

    harness.Emitter.Emit(CreateDamageEvent(session.StartTime + 100));
    harness.Broadcaster.Tick(1.0f);

    var frame = ParseLastFrame(harness.Server);
    Assert.Equal("events", frame.Value<string>("kind"));
    Assert.Equal(session.Id, frame.Value<string>("sessionId"));
    Assert.Equal(1, frame["payload"]!.Value<long>("eventSeqStart"));
    Assert.Equal(1, frame["payload"]!.Value<long>("eventSeqEnd"));
    Assert.Equal("damage", frame["payload"]!["events"]![0]!.Value<string>("kind"));
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
    public event Action? ClientConnected;

    public void Start() { }

    public void Stop() { }

    public void Broadcast(string message) => Messages.Add(message);

    public void Dispose() { }

    public void RaiseClientConnected() => ClientConnected?.Invoke();
  }

  private sealed class FakeSessionManager : ISessionManager
  {
    public CombatSession? CurrentSession { get; private set; }
    public event Action<CombatSession>? SessionStarted;
    public event Action<CombatSession>? SessionEnded;

    public void OnCombatEvent(EventType eventType, long eventTimestamp) { }

    public void CheckInactivityTimeout(float currentTime) { }

    public void StartManualSession() { }

    public void EndManualSession() { }

    public void Start(CombatSession session)
    {
      CurrentSession = session;
      SessionStarted?.Invoke(session);
    }

    public void End()
    {
      if (CurrentSession == null)
        return;

      var session = CurrentSession;
      CurrentSession = null;
      SessionEnded?.Invoke(session);
    }
  }
}
