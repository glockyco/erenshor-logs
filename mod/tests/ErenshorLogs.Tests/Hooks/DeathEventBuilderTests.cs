using ErenshorLogs.Events;
using ErenshorLogs.Hooks;
using Xunit;

namespace ErenshorLogs.Tests.Hooks;

public sealed class DeathEventBuilderTests
{
  [Fact]
  public void KillingBlowTracker_ReturnsLatestDamageSeqForTarget()
  {
    KillingBlowTracker.Clear();

    KillingBlowTracker.RecordDamage("sim:cleric", 41);
    KillingBlowTracker.RecordDamage("sim:cleric", 42);

    Assert.Equal(42, KillingBlowTracker.GetLatestDamageEventSeq("sim:cleric"));
  }

  [Fact]
  public void DeathFallbackAbility_IsUnknownWhenNoCauseIsKnown()
  {
    var ability = DeathEventPatch.FallbackAbility;

    Assert.Equal("Death", ability.Name);
    Assert.Equal(AbilityType.Unknown, ability.Type);
    Assert.Null(ability.StableKey);
  }
}
