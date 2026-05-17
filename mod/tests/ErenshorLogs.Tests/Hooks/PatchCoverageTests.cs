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
}
