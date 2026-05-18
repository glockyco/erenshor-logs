using ErenshorLogs.Diagnostics;
using Xunit;

namespace ErenshorLogs.Tests.Diagnostics;

public sealed class DiagnosticReporterTests
{
  [Fact]
  public void ReportProjectionError_DeduplicatesAndCountsDiagnostics()
  {
    var reporter = new DiagnosticReporter(log: null);

    reporter.ReportProjectionError(
      new ArgumentException("Bad mechanic action"),
      sessionId: "session-1",
      eventType: "Mechanic",
      path: "payload.events.0.action"
    );
    reporter.ReportProjectionError(
      new ArgumentException("Bad mechanic action"),
      sessionId: "session-1",
      eventType: "Mechanic",
      path: "payload.events.0.action"
    );

    var diagnostics = reporter.DrainPendingDiagnostics(maxCount: 4);

    Assert.Equal(2, reporter.Counters.ProjectionErrors);
    Assert.Equal(2, reporter.Counters.DroppedEvents);
    Assert.Single(diagnostics);
    Assert.Equal("projection.failed", diagnostics[0].Code);
    Assert.Equal("error", diagnostics[0].Severity);
    Assert.Equal("eventDropped", diagnostics[0].Impact);
    Assert.Equal(2, diagnostics[0].Count);
    Assert.Equal(1, diagnostics[0].SuppressedCount);
    Assert.Equal("Mechanic", diagnostics[0].Details!["eventType"]);
    Assert.Equal("ArgumentException", diagnostics[0].Details!["exceptionType"]);
    Assert.Equal("payload.events.0.action", diagnostics[0].Details!["path"]);
    Assert.DoesNotContain(" at ", diagnostics[0].Message);
  }

  [Fact]
  public void RecentDiagnostics_AreBoundedToLastThirtyTwoEntries()
  {
    var reporter = new DiagnosticReporter(log: null);

    for (var index = 0; index < 40; index += 1)
    {
      reporter.ReportRecoverable(
        code: $"recoverable.{index}",
        severity: "warning",
        impact: "captureDegraded",
        component: "mod.test",
        operation: "loop",
        message: $"Recoverable diagnostic {index}"
      );
    }

    Assert.Equal(32, reporter.RecentDiagnostics.Count);
    Assert.Equal("recoverable.8", reporter.RecentDiagnostics[0].Code);
    Assert.Equal("recoverable.39", reporter.RecentDiagnostics[^1].Code);
  }

  [Fact]
  public void DrainPendingDiagnostics_LeavesCountersAvailableForStats()
  {
    var reporter = new DiagnosticReporter(log: null);
    reporter.ReportClientSendError(new InvalidOperationException("socket closed"));

    var firstDrain = reporter.DrainPendingDiagnostics(maxCount: 4);
    var secondDrain = reporter.DrainPendingDiagnostics(maxCount: 4);

    Assert.Single(firstDrain);
    Assert.Empty(secondDrain);
    Assert.Equal(1, reporter.Counters.ClientSendErrors);
    Assert.Equal(1, reporter.Counters.DiagnosticsEmitted);
  }
}
