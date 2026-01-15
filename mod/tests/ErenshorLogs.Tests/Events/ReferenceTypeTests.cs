using System.Text.Json;
using ErenshorLogs.Events;
using ErenshorLogs.Json;
using Xunit;

namespace ErenshorLogs.Tests.Events;

public class ReferenceTypeTests
{
  [Fact]
  public void ActorRef_Serializes_WithCorrectPropertyNames()
  {
    var actor = new ActorRef
    {
      Id = "player:0",
      Name = "Valdris",
      Type = ActorType.Player,
      Class = "Duelist",
      Level = 35,
    };

    var json = JsonSerializer.Serialize(actor, JsonContext.Options);

    Assert.Contains("\"id\":\"player:0\"", json);
    Assert.Contains("\"name\":\"Valdris\"", json);
    Assert.Contains("\"type\":\"player\"", json);
    Assert.Contains("\"class\":\"Duelist\"", json);
    Assert.Contains("\"level\":35", json);
  }

  [Fact]
  public void ActorRef_OmitsNullProperties()
  {
    var actor = new ActorRef
    {
      Id = "npc:123",
      Name = "Goblin",
      Type = ActorType.Npc,
    };

    var json = JsonSerializer.Serialize(actor, JsonContext.Options);

    Assert.DoesNotContain("class", json);
    Assert.DoesNotContain("level", json);
    Assert.DoesNotContain("masterId", json);
  }

  [Fact]
  public void AbilityRef_Serializes_WithSnakeCaseType()
  {
    var ability = new AbilityRef
    {
      Name = "Backstab",
      Type = AbilityType.Skill,
      StableKey = "skill:Backstab",
    };

    var json = JsonSerializer.Serialize(ability, JsonContext.Options);

    Assert.Contains("\"type\":\"skill\"", json);
    Assert.Contains("\"stableKey\":\"skill:Backstab\"", json);
  }

  [Fact]
  public void EffectRef_Serializes_Correctly()
  {
    var effect = new EffectRef
    {
      Name = "Battle Shout",
      Duration = 300,
      Stacks = 1,
    };

    var json = JsonSerializer.Serialize(effect, JsonContext.Options);

    Assert.Contains("\"name\":\"Battle Shout\"", json);
    Assert.Contains("\"duration\":300", json);
    Assert.Contains("\"stacks\":1", json);
  }

  [Fact]
  public void EventFlags_OmitsFalseAndNullValues()
  {
    var flags = new EventFlags { Critical = true, FromPlayer = true };

    var json = JsonSerializer.Serialize(flags, JsonContext.Options);

    Assert.Contains("\"critical\":true", json);
    Assert.Contains("\"fromPlayer\":true", json);
    Assert.DoesNotContain("overkill", json);
    Assert.DoesNotContain("isPet", json);
  }
}
