using ErenshorLogs.Events;
using ErenshorLogs.Protocol;
using Xunit;

namespace ErenshorLogs.Tests.Protocol;

public class MessageSerializerTests
{
  [Fact]
  public void Serialize_HandshakeMessage_UsesCamelCase()
  {
    var sessionInfo = new SessionInfo(
      "test-session",
      1234567890,
      new PlayerInfo
      {
        Name = "TestPlayer",
        Class = "Arcanist",
        Level = 10,
      }
    );
    var message = HandshakeMessage.Create("0.1.0", sessionInfo);

    var json = MessageSerializer.Serialize(message);

    Assert.Contains("\"type\":", json);
    Assert.Contains("\"protocolVersion\":", json);
    Assert.Contains("\"modVersion\":", json);
    Assert.Contains("\"session\":", json);
  }

  [Fact]
  public void Serialize_HandshakeMessage_ContainsCorrectValues()
  {
    var sessionInfo = new SessionInfo(
      "test-session",
      1234567890,
      new PlayerInfo
      {
        Name = "TestPlayer",
        Class = "Arcanist",
        Level = 10,
      }
    );
    var message = HandshakeMessage.Create("0.1.0", sessionInfo);

    var json = MessageSerializer.Serialize(message);

    Assert.Contains("\"type\":\"handshake\"", json);
    Assert.Contains("\"protocolVersion\":\"0.1.0\"", json);
    Assert.Contains("\"modVersion\":\"0.1.0\"", json);
    Assert.Contains("\"test-session\"", json);
  }

  [Fact]
  public void Serialize_HandshakeMessage_IsCompact()
  {
    var message = HandshakeMessage.Create("0.1.0", null);

    var json = MessageSerializer.Serialize(message);

    // Should not contain newlines (not indented)
    Assert.DoesNotContain("\n", json);
  }

  [Fact]
  public void Serialize_HandshakeMessage_OmitsNullSession()
  {
    var message = HandshakeMessage.Create("0.1.0", null);

    var json = MessageSerializer.Serialize(message);

    // Null session should be omitted (DefaultIgnoreCondition.WhenWritingNull)
    Assert.DoesNotContain("\"session\"", json);
  }

  [Fact]
  public void Serialize_HandshakeMessage_RoundTrips()
  {
    var sessionInfo = new SessionInfo(
      "test-session",
      1234567890,
      new PlayerInfo
      {
        Name = "TestPlayer",
        Class = "Arcanist",
        Level = 10,
      }
    );
    var original = HandshakeMessage.Create("0.1.0", sessionInfo);

    var json = MessageSerializer.Serialize(original);
    var deserialized = MessageSerializer.Deserialize<HandshakeMessage>(json);

    Assert.NotNull(deserialized);
    Assert.Equal(original.Type, deserialized.Type);
    Assert.Equal(original.ProtocolVersion, deserialized.ProtocolVersion);
    Assert.Equal(original.ModVersion, deserialized.ModVersion);
    Assert.NotNull(deserialized.Session);
    Assert.Equal(original.Session?.Id, deserialized.Session.Id);
  }

  [Fact]
  public void Serialize_SessionStartMessage_UsesCamelCase()
  {
    var sessionInfo = new SessionInfo(
      "test-session",
      1234567890,
      new PlayerInfo
      {
        Name = "TestPlayer",
        Class = "Arcanist",
        Level = 10,
      }
    );
    var message = SessionStartMessage.Create(sessionInfo);

    var json = MessageSerializer.Serialize(message);

    Assert.Contains("\"type\":", json);
    Assert.Contains("\"session\":", json);
  }

  [Fact]
  public void Serialize_SessionEndMessage_UsesCamelCase()
  {
    var message = SessionEndMessage.Create("test-session", 5000);

    var json = MessageSerializer.Serialize(message);

    Assert.Contains("\"type\":", json);
    Assert.Contains("\"sessionId\":", json);
    Assert.Contains("\"duration\":", json);
  }

  [Fact]
  public void Serialize_SessionEndMessage_ContainsCorrectValues()
  {
    var message = SessionEndMessage.Create("test-session", 5000);

    var json = MessageSerializer.Serialize(message);

    Assert.Contains("\"type\":\"sessionEnd\"", json);
    Assert.Contains("\"sessionId\":\"test-session\"", json);
    Assert.Contains("\"duration\":5000", json);
  }

  [Fact]
  public void Serialize_CombatEventsMessage_UsesCamelCase()
  {
    var events = new[]
    {
      new CombatEvent
      {
        Id = "event-1",
        Timestamp = 1234567890,
        EventType = EventType.DamagePhysical,
      },
    };
    var message = CombatEventsMessage.Create("test-session", events);

    var json = MessageSerializer.Serialize(message);

    Assert.Contains("\"type\":", json);
    Assert.Contains("\"sessionId\":", json);
    Assert.Contains("\"events\":", json);
  }

  [Fact]
  public void Serialize_CombatEventsMessage_ContainsEvents()
  {
    var events = new[]
    {
      new CombatEvent
      {
        Id = "event-1",
        Timestamp = 1234567890,
        EventType = EventType.DamagePhysical,
      },
      new CombatEvent
      {
        Id = "event-2",
        Timestamp = 1234567891,
        EventType = EventType.CombatStart,
      },
    };
    var message = CombatEventsMessage.Create("test-session", events);

    var json = MessageSerializer.Serialize(message);

    Assert.Contains("\"event-1\"", json);
    Assert.Contains("\"event-2\"", json);
    Assert.Contains("\"damage_physical\"", json); // EventType serializes as snake_case
    Assert.Contains("\"combat_start\"", json);
  }
}
