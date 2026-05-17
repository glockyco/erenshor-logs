using System.Reflection;
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

  [Fact]
  public void CoversPlaytestRaidMechanicContextMethods()
  {
    LoadGameAssemblies();
    AssertPatchTargetExists("AEEvent", "TriggerAE", "ErenshorLogs.Hooks.AEEventTriggerPatch");
    AssertPatchTargetExists("DeathTouch", "Update", "ErenshorLogs.Hooks.DeathTouchPatch");

    var patchType = RequireModType("ErenshorLogs.Hooks.MizukiEventPatch");
    var mizukiTarget = patchType.GetMethod("TargetMethod")?.Invoke(null, null) as MethodBase;
    Assert.NotNull(mizukiTarget);
    Assert.Equal("MoveNext", mizukiTarget!.Name);
    Assert.Contains("SetNewAggro", mizukiTarget.DeclaringType?.Name);
  }

  private static void AssertPatchTargetExists(
    string targetTypeName,
    string methodName,
    string patchTypeName
  )
  {
    var targetType = RequireGameType(targetTypeName);
    var patchType = RequireModType(patchTypeName);
    Assert.NotNull(
      targetType.GetMethod(
        methodName,
        BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic
      )
    );
    Assert.NotNull(patchType.GetMethod("Prefix", BindingFlags.Public | BindingFlags.Static));
    Assert.NotNull(patchType.GetMethod("Finalizer", BindingFlags.Public | BindingFlags.Static));
  }

  private static void LoadGameAssemblies()
  {
    var libPath = Path.GetFullPath(
      Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "lib")
    );
    foreach (
      var dll in new[]
      {
        "UnityEngine.CoreModule.dll",
        "UnityEngine.InputLegacyModule.dll",
        "UnityEngine.dll",
        "Assembly-CSharp.dll",
      }
    )
    {
      var path = Path.Combine(libPath, dll);
      if (File.Exists(path))
        Assembly.LoadFrom(path);
    }
  }

  private static Type RequireGameType(string typeName)
  {
    var type = AppDomain
      .CurrentDomain.GetAssemblies()
      .Select(assembly => assembly.GetType(typeName))
      .FirstOrDefault(type => type != null);
    Assert.NotNull(type);
    return type!;
  }

  private static Type RequireModType(string typeName)
  {
    var type = Type.GetType($"{typeName}, ErenshorLogs");
    Assert.NotNull(type);
    return type!;
  }
}
