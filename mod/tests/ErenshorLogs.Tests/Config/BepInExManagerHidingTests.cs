using ErenshorLogs.Config;
using UnityEngine;
using Xunit;

namespace ErenshorLogs.Tests.Config;

public sealed class BepInExManagerHidingTests
{
  [Fact]
  public void EnsureHiddenFlags_WhenFlagsDoNotIncludeHideAndDontSave_AddsFlag()
  {
    var changed = BepInExManagerHiding.EnsureHiddenFlags(HideFlags.None, out var updatedFlags);

    Assert.True(changed);
    Assert.Equal(HideFlags.HideAndDontSave, updatedFlags);
  }

  [Fact]
  public void EnsureHiddenFlags_WhenOtherFlagsExist_PreservesThem()
  {
    var changed = BepInExManagerHiding.EnsureHiddenFlags(
      HideFlags.NotEditable,
      out var updatedFlags
    );

    Assert.True(changed);
    Assert.Equal(HideFlags.NotEditable | HideFlags.HideAndDontSave, updatedFlags);
  }

  [Fact]
  public void EnsureHiddenFlags_WhenAlreadyHidden_ReturnsUnchanged()
  {
    var original = HideFlags.NotEditable | HideFlags.HideAndDontSave;

    var changed = BepInExManagerHiding.EnsureHiddenFlags(original, out var updatedFlags);

    Assert.False(changed);
    Assert.Equal(original, updatedFlags);
  }
}
