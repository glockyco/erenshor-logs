using ErenshorLogs.Hooks;
using Xunit;

namespace ErenshorLogs.Tests.Hooks;

public sealed class RaidRelevanceInvalidationTests
{
  [Fact]
  public void OnRaidStateChanged_InvokesConfiguredCacheClear()
  {
    var calls = 0;
    RaidRelevanceInvalidation.ClearCache = () => calls += 1;

    try
    {
      RaidRelevanceInvalidationPatches.OnRaidStateChanged();
    }
    finally
    {
      RaidRelevanceInvalidation.ClearCache = null;
    }

    Assert.Equal(1, calls);
  }
}
