using ErenshorLogs.Events;
using ErenshorLogs.Json;
using Newtonsoft.Json;
using Xunit;

namespace ErenshorLogs.Tests.Events;

public class EnumSerializationTests
{
  [Theory]
  [InlineData(EventType.DamageMelee, "\"damageMelee\"")]
  [InlineData(EventType.DamageSpell, "\"damageSpell\"")]
  [InlineData(EventType.HealHot, "\"healHot\"")]
  [InlineData(EventType.CombatStart, "\"combatStart\"")]
  public void EventType_SerializesToCamelCase(EventType value, string expected)
  {
    var json = JsonConvert.SerializeObject(value, JsonSettings.Default);
    Assert.Equal(expected, json);
  }

  [Theory]
  [InlineData(DamageType.Physical, "\"physical\"")]
  [InlineData(DamageType.Magic, "\"magic\"")]
  [InlineData(DamageType.Elemental, "\"elemental\"")]
  public void DamageType_SerializesToCamelCase(DamageType value, string expected)
  {
    var json = JsonConvert.SerializeObject(value, JsonSettings.Default);
    Assert.Equal(expected, json);
  }

  [Theory]
  [InlineData(ActorType.Player, "\"player\"")]
  [InlineData(ActorType.SimPlayer, "\"simPlayer\"")]
  [InlineData(ActorType.Npc, "\"npc\"")]
  public void ActorType_SerializesToCamelCase(ActorType value, string expected)
  {
    var json = JsonConvert.SerializeObject(value, JsonSettings.Default);
    Assert.Equal(expected, json);
  }

  [Theory]
  [InlineData(AbilityType.Skill, "\"skill\"")]
  [InlineData(AbilityType.Spell, "\"spell\"")]
  [InlineData(AbilityType.Dot, "\"dot\"")]
  [InlineData(AbilityType.Hot, "\"hot\"")]
  public void AbilityType_SerializesToCamelCase(AbilityType value, string expected)
  {
    var json = JsonConvert.SerializeObject(value, JsonSettings.Default);
    Assert.Equal(expected, json);
  }
}
