using ErenshorLogs.Protocol;
using Newtonsoft.Json.Linq;
using Xunit;

namespace ErenshorLogs.Tests.Protocol;

public class ProtocolFixtureTests
{
  public static TheoryData<string> LiveFixtures =>
    new() { "hello", "session-snapshot", "registry-delta", "events", "session-ended", "error" };

  [Theory]
  [MemberData(nameof(LiveFixtures))]
  public void Deserialize_LiveFixture_RoundTrips(string fixtureName)
  {
    var json = ReadFixture($"live/{fixtureName}.json");

    var frame = MessageSerializer.Deserialize<LiveEnvelope>(json);
    var serialized = MessageSerializer.Serialize(frame!);
    var reparsed = MessageSerializer.Deserialize<LiveEnvelope>(serialized);

    Assert.True(JToken.DeepEquals(JToken.Parse(json), JToken.Parse(serialized)));

    Assert.NotNull(frame);
    Assert.NotNull(reparsed);
    Assert.Equal("erenshor.logs.live", frame.Protocol);
    Assert.StartsWith("2.", frame.ProtocolVersion);
    Assert.StartsWith("2.", frame.SchemaVersion);
    AssertLivePayloadMatchesKind(frame);
  }

  [Theory]
  [InlineData("export/single-session.json", 1)]
  [InlineData("export/multi-session.json", 2)]
  public void Deserialize_ExportFixture_RoundTrips(string fixturePath, int expectedSessions)
  {
    var json = ReadFixture(fixturePath);

    var file = MessageSerializer.Deserialize<CombatLogFile>(json);
    var serialized = MessageSerializer.Serialize(file!);
    var reparsed = MessageSerializer.Deserialize<CombatLogFile>(serialized);

    Assert.True(JToken.DeepEquals(JToken.Parse(json), JToken.Parse(serialized)));

    Assert.NotNull(file);
    Assert.NotNull(reparsed);
    Assert.Equal("erenshor.logs.export", file.Format);
    Assert.Equal(expectedSessions, file.Sessions.Count);
    foreach (var session in file.Sessions)
    {
      if (session.Events.Count > 0)
      {
        Assert.NotEmpty(session.Snapshot.Registries.Actors);
      }
      foreach (var evt in session.Events)
      {
        AssertCombatEventMatchesKind(evt);
      }
    }
  }

  private static void AssertLivePayloadMatchesKind(LiveEnvelope frame)
  {
    var payload = Assert.IsType<JObject>(frame.Payload);
    switch (frame.Kind)
    {
      case "hello":
        var hello = payload.ToObject<HelloPayload>()!;
        Assert.Equal("ErenshorLogsMod", hello.Producer.Name);
        Assert.Contains("sessionSnapshot", hello.Capabilities);
        break;
      case "sessionSnapshot":
        var snapshot = payload.ToObject<SessionSnapshotPayload>()!;
        Assert.Equal(frame.SessionId, snapshot.SessionId);
        Assert.NotEmpty(snapshot.Registries.Actors);
        break;
      case "registryDelta":
        var delta = payload.ToObject<RegistryDeltaPayload>()!;
        Assert.True(delta.Revision > 0);
        Assert.NotNull(delta.Abilities);
        break;
      case "events":
        var events = payload.ToObject<EventsPayload>()!;
        Assert.Equal(frame.SessionId, events.SessionId);
        Assert.NotEmpty(events.Events);
        for (var index = 0; index < events.Events.Count; index += 1)
        {
          Assert.Equal(events.EventSeqStart + index, events.Events[index].Value<long>("eventSeq"));
          AssertCombatEventMatchesKind(events.Events[index]);
        }
        break;
      case "sessionEnded":
        var ended = payload.ToObject<SessionEndedPayload>()!;
        Assert.Equal(frame.SessionId, ended.SessionId);
        Assert.True(ended.EndedAtEventSeq > 0);
        break;
      case "error":
        var error = payload.ToObject<ErrorPayload>()!;
        Assert.False(string.IsNullOrWhiteSpace(error.Code));
        Assert.False(string.IsNullOrWhiteSpace(error.Severity));
        break;
      default:
        throw new InvalidOperationException($"Unhandled fixture kind: {frame.Kind}");
    }
  }

  private static void AssertCombatEventMatchesKind(JObject evt)
  {
    switch (evt.Value<string>("kind"))
    {
      case "damage":
        var damage = evt.ToObject<DamageEventRecord>()!;
        Assert.Equal("damage", damage.Kind);
        Assert.True(damage.Data.Amount > 0);
        Assert.NotNull(damage.Data.Outcome);
        break;
      case "heal":
        var heal = evt.ToObject<HealEventRecord>()!;
        Assert.Equal("heal", heal.Kind);
        Assert.True(heal.Data.Amount > 0);
        break;
      case "resource":
        var resource = evt.ToObject<ResourceEventRecord>()!;
        Assert.Equal("resource", resource.Kind);
        Assert.Equal("mana", resource.Data.Resource);
        break;
      case "effect":
        var effect = evt.ToObject<EffectEventRecord>()!;
        Assert.Equal("effect", effect.Kind);
        Assert.NotNull(effect.EffectId);
        break;
      case "death":
        var death = evt.ToObject<DeathEventRecord>()!;
        Assert.Equal("death", death.Kind);
        Assert.Equal("die", death.Action);
        break;
      case "interrupt":
        var interrupt = evt.ToObject<InterruptEventRecord>()!;
        Assert.Equal("interrupt", interrupt.Kind);
        Assert.NotNull(interrupt.Data.InterruptedAbilityId);
        break;
      default:
        throw new InvalidOperationException($"Unhandled event kind: {evt.Value<string>("kind")}");
    }
  }

  private static string ReadFixture(string relativePath)
  {
    var path = Path.Combine(
      AppContext.BaseDirectory,
      "..",
      "..",
      "..",
      "..",
      "..",
      "..",
      "shared",
      "protocol",
      "fixtures",
      relativePath
    );
    return File.ReadAllText(Path.GetFullPath(path));
  }
}
