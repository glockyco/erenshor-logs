using ErenshorLogs.Events;
using ErenshorLogs.Json;
using Newtonsoft.Json;
using Xunit;

namespace ErenshorLogs.Tests.Events;

public class CombatEventTests
{
  [Fact]
  public void CombatEvent_Serializes_DamageEvent()
  {
    var evt = new CombatEvent
    {
      Id = "test-id",
      Timestamp = 1704067200000,
      EventType = EventType.DamageSkill,
      Source = new ActorRef
      {
        Id = "player:0",
        Name = "Valdris",
        Type = ActorType.Player,
      },
      Target = new ActorRef
      {
        Id = "npc:123",
        Name = "Goblin",
        Type = ActorType.Npc,
      },
      Ability = new AbilityRef { Name = "Backstab", Type = AbilityType.Skill },
      Amount = 1500,
      RawAmount = 2000,
      Mitigated = 500,
      DamageType = DamageType.Physical,
      Flags = new EventFlags { Critical = true, FromPlayer = true },
    };

    var json = JsonConvert.SerializeObject(evt, JsonSettings.Default);

    Assert.Contains("\"eventType\":\"damage_skill\"", json);
    Assert.Contains("\"amount\":1500", json);
    Assert.Contains("\"damageType\":\"physical\"", json);
    Assert.Contains("\"critical\":true", json);
  }

  [Fact]
  public void CombatEvent_OmitsNullFields()
  {
    var evt = new CombatEvent
    {
      Id = "test-id",
      Timestamp = 1704067200000,
      EventType = EventType.CombatStart,
    };

    var json = JsonConvert.SerializeObject(evt, JsonSettings.Default);

    Assert.DoesNotContain("source", json);
    Assert.DoesNotContain("target", json);
    Assert.DoesNotContain("ability", json);
    Assert.DoesNotContain("amount", json);
  }
}
