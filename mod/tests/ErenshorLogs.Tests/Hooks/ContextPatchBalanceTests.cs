using ErenshorLogs.Context;
using ErenshorLogs.Events;
using ErenshorLogs.Hooks;
using Xunit;

namespace ErenshorLogs.Tests.Hooks;

public sealed class ContextPatchBalanceTests : IDisposable
{
  public ContextPatchBalanceTests()
  {
    CombatContext.Clear();
  }

  [Fact]
  public void DoSkillFinalizer_DoesNotPopOuterContextWhenPrefixDidNotPush()
  {
    PushOuterContext();

    DoSkillPatch.Finalizer(false);

    Assert.Equal("Outer", CombatContext.CurrentAbility()?.Name);
  }

  [Fact]
  public void DoSkillNoChecksFinalizer_DoesNotPopOuterContextWhenPrefixDidNotPush()
  {
    PushOuterContext();

    DoSkillNoChecksPatch.Finalizer(false);

    Assert.Equal("Outer", CombatContext.CurrentAbility()?.Name);
  }

  [Fact]
  public void ResolveSpellFinalizer_DoesNotPopOuterContextWhenPrefixDidNotPush()
  {
    PushOuterContext();

    ResolveSpellPatch.Finalizer(false);

    Assert.Equal("Outer", CombatContext.CurrentAbility()?.Name);
  }

  [Fact]
  public void DeliverDamageFinalizer_DoesNotPopOuterContextWhenPrefixDidNotPush()
  {
    PushOuterContext();

    DeliverDamagePatch.Finalizer(false);

    Assert.Equal("Outer", CombatContext.CurrentAbility()?.Name);
  }

  [Theory]
  [MemberData(nameof(AreaEffectFinalizers))]
  public void AreaEffectFinalizer_DoesNotPopOuterContextWhenPrefixDidNotPush(Action<bool> finalizer)
  {
    PushOuterContext();

    finalizer(false);

    Assert.Equal("Outer", CombatContext.CurrentAbility()?.Name);
  }

  public static TheoryData<Action<bool>> AreaEffectFinalizers =>
    new()
    {
      AEEventUpdatePatch.Finalizer,
      AEEvent2UpdatePatch.Finalizer,
      AstraBreathScriptUpdatePatch.Finalizer,
      NPCFightEventBreathAttackPatch.Finalizer,
      SableheartEventUpdatePatch.Finalizer,
    };

  public void Dispose()
  {
    CombatContext.Clear();
  }

  private static void PushOuterContext()
  {
    CombatContext.PushAbility(
      new AbilityContext
      {
        Name = "Outer",
        Type = AbilityType.Spell,
        StableKey = "spell:outer",
      }
    );
  }
}
