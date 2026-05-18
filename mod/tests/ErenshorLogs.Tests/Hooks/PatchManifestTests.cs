using ErenshorLogs.Diagnostics;
using ErenshorLogs.Hooks;
using Xunit;

namespace ErenshorLogs.Tests.Hooks;

public sealed class PatchManifestTests
{
  [Fact]
  public void Apply_OptionalFailureRecordsDiagnosticAndContinues()
  {
    var reporter = new DiagnosticReporter(log: null);
    var appliedRequired = false;
    var entries = new[]
    {
      new PatchManifestEntry(
        "optional.missing",
        Required: false,
        () => throw new MissingMethodException("gone")
      ),
      new PatchManifestEntry("required.present", Required: true, () => appliedRequired = true),
    };

    var result = PatchManifest.Apply(entries, reporter);

    Assert.True(appliedRequired);
    Assert.Equal("degraded", result.HealthStatus);
    Assert.Collection(
      result.Statuses,
      status =>
      {
        Assert.Equal("optional.missing", status.Id);
        Assert.False(status.Required);
        Assert.Equal("failed", status.Status);
      },
      status =>
      {
        Assert.Equal("required.present", status.Id);
        Assert.True(status.Required);
        Assert.Equal("active", status.Status);
      }
    );
    Assert.Equal("patch.failed", reporter.RecentDiagnostics[0].Code);
    Assert.Equal(1, reporter.Counters.HookWarnings);
  }

  [Fact]
  public void Apply_RequiredFailureMarksCaptureFatalWithoutThrowing()
  {
    var reporter = new DiagnosticReporter(log: null);
    var entries = new[]
    {
      new PatchManifestEntry(
        "required.missing",
        Required: true,
        () => throw new MissingMethodException("gone")
      ),
    };

    var result = PatchManifest.Apply(entries, reporter);

    Assert.Equal("fatal", result.HealthStatus);
    Assert.Equal("failed", result.Statuses[0].Status);
    Assert.Equal("modFatal", reporter.RecentDiagnostics[0].Impact);
  }
}
