using ErenshorLogs.Events;
using ErenshorLogs.Json;
using Newtonsoft.Json;
using Xunit;

namespace ErenshorLogs.Tests.Events;

public class EnumSerializationTests
{
  [Theory]
  [InlineData(EventType.DamageMelee, "\"damage_melee\"")]
  [InlineData(EventType.DamageSpell, "\"damage_spell\"")]
  [InlineData(EventType.HealHot, "\"heal_hot\"")]
  [InlineData(EventType.CombatStart, "\"combat_start\"")]
  public void EventType_SerializesToSnakeCase(EventType value, string expected)
  {
    var json = JsonConvert.SerializeObject(value, JsonSettings.Default);
    Assert.Equal(expected, json);
  }

  [Theory]
  [InlineData(DamageType.Physical, "\"physical\"")]
  [InlineData(DamageType.Magic, "\"magic\"")]
  [InlineData(DamageType.Elemental, "\"elemental\"")]
  public void DamageType_SerializesToSnakeCase(DamageType value, string expected)
  {
    var json = JsonConvert.SerializeObject(value, JsonSettings.Default);
    Assert.Equal(expected, json);
  }

  [Theory]
  [InlineData(ActorType.Player, "\"player\"")]
  [InlineData(ActorType.SimPlayer, "\"sim_player\"")]
  [InlineData(ActorType.Npc, "\"npc\"")]
  public void ActorType_SerializesToSnakeCase(ActorType value, string expected)
  {
    var json = JsonConvert.SerializeObject(value, JsonSettings.Default);
    Assert.Equal(expected, json);
  }

  [Theory]
  [InlineData(AbilityType.Skill, "\"skill\"")]
  [InlineData(AbilityType.Spell, "\"spell\"")]
  [InlineData(AbilityType.Dot, "\"dot\"")]
  [InlineData(AbilityType.Hot, "\"hot\"")]
  public void AbilityType_SerializesToSnakeCase(AbilityType value, string expected)
  {
    var json = JsonConvert.SerializeObject(value, JsonSettings.Default);
    Assert.Equal(expected, json);
  }
}
