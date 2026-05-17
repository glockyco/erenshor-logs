using ErenshorLogs.Context;
using ErenshorLogs.Events;
using Xunit;

namespace ErenshorLogs.Tests.Hooks;

public sealed class HealingContextTests : IDisposable
{
  public void Dispose() => HealingContext.Clear();

  [Fact]
  public void Current_WhenNoContext_ReturnsNull()
  {
    Assert.Null(HealingContext.Current());
  }

  [Fact]
  public void Push_ReturnedScope_PopsContextOnDispose()
  {
    var ability = new AbilityRef
    {
      Name = "Lifetap",
      Type = AbilityType.Spell,
      StableKey = "spell:10",
    };

    using (HealingContext.Push(null, ability, EventType.HealLifesteal, AttributionMethod.Verified))
    {
      var current = HealingContext.Current();

      Assert.NotNull(current);
      Assert.Same(ability, current!.Ability);
      Assert.Equal(EventType.HealLifesteal, current.EventType);
      Assert.Equal(AttributionMethod.Verified, current.Attribution);
    }

    Assert.Null(HealingContext.Current());
  }

  [Fact]
  public void Push_WhenNested_RestoresOuterContext()
  {
    var outer = new AbilityRef
    {
      Name = "Outer Heal",
      Type = AbilityType.Spell,
      StableKey = "spell:outer",
    };
    var inner = new AbilityRef
    {
      Name = "Inner Heal",
      Type = AbilityType.Spell,
      StableKey = "spell:inner",
    };

    using (HealingContext.Push(null, outer, EventType.HealSpell, AttributionMethod.Context))
    {
      using (HealingContext.Push(null, inner, EventType.HealLifesteal, AttributionMethod.Verified))
      {
        Assert.Same(inner, HealingContext.Current()!.Ability);
      }

      Assert.Same(outer, HealingContext.Current()!.Ability);
    }
  }
}
