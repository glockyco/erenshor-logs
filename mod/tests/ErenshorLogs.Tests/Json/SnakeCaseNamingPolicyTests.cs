using ErenshorLogs.Json;
using Xunit;

namespace ErenshorLogs.Tests.Json;

public class SnakeCaseNamingPolicyTests
{
  private readonly SnakeCaseNamingPolicy _policy = SnakeCaseNamingPolicy.Instance;

  [Theory]
  [InlineData("DamageMelee", "damage_melee")]
  [InlineData("DamageSkill", "damage_skill")]
  [InlineData("DamageDot", "damage_dot")]
  [InlineData("HealHot", "heal_hot")]
  [InlineData("Physical", "physical")]
  [InlineData("SimPlayer", "sim_player")]
  [InlineData("BuffApply", "buff_apply")]
  [InlineData("CombatStart", "combat_start")]
  [InlineData("", "")]
  public void ConvertName_PascalCase_ReturnsSnakeCase(string input, string expected)
  {
    var result = _policy.ConvertName(input);

    Assert.Equal(expected, result);
  }
}
