using ErenshorLogs.Hooks;
using Xunit;

namespace ErenshorLogs.Tests.Hooks;

public class PatchCoverageTests
{
  [Fact]
  public void HooksIncludeSkillContextPatchForSimPlayerNoChecksSkillExecution()
  {
    var patchType = Type.GetType("ErenshorLogs.Hooks.DoSkillNoChecksPatch, ErenshorLogs");

    Assert.NotNull(patchType);
  }

  [Theory]
  [InlineData("ErenshorLogs.Hooks.AddStatusEffectPatch")]
  [InlineData("ErenshorLogs.Hooks.AddStatusEffectThreeArgPatch")]
  [InlineData("ErenshorLogs.Hooks.AddStatusEffectFiveArgPatch")]
  public void HooksIncludeStatusEffectOverloads(string typeName)
  {
    var patchType = Type.GetType($"{typeName}, ErenshorLogs");

    Assert.NotNull(patchType);
  }
}
