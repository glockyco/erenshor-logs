using ErenshorLogs.Events;
using ErenshorLogs.Json;
using Newtonsoft.Json;
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

    var json = JsonConvert.SerializeObject(actor, JsonSettings.Default);

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

    var json = JsonConvert.SerializeObject(actor, JsonSettings.Default);

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

    var json = JsonConvert.SerializeObject(ability, JsonSettings.Default);

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

    var json = JsonConvert.SerializeObject(effect, JsonSettings.Default);

    Assert.Contains("\"name\":\"Battle Shout\"", json);
    Assert.Contains("\"duration\":300", json);
    Assert.Contains("\"stacks\":1", json);
  }

  [Fact]
  public void EventFlags_OmitsFalseAndNullValues()
  {
    var flags = new EventFlags { Critical = true, FromPlayer = true };

    var json = JsonConvert.SerializeObject(flags, JsonSettings.Default);

    Assert.Contains("\"critical\":true", json);
    Assert.Contains("\"fromPlayer\":true", json);
    Assert.DoesNotContain("overkill", json);
    Assert.DoesNotContain("isPet", json);
  }
}
