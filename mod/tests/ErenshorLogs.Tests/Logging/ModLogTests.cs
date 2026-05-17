using ErenshorLogs.Logging;
using Xunit;

namespace ErenshorLogs.Tests.Logging;

public class ModLogTests
{
  [Fact]
  public void Write_WhenDisabled_DoesNotForwardMessages()
  {
    var entries = new List<(LogLevel Level, string Message)>();
    var log = new ModLog(
      () => false,
      message => entries.Add((LogLevel.Debug, message)),
      message => entries.Add((LogLevel.Info, message)),
      message => entries.Add((LogLevel.Warning, message)),
      message => entries.Add((LogLevel.Error, message))
    );

    log.Debug("debug");
    log.Info("info");
    log.Warning("warning");
    log.Error("error");

    Assert.Empty(entries);
  }

  [Fact]
  public void Write_WhenEnabled_ForwardsMessagesAtRequestedLevel()
  {
    var entries = new List<(LogLevel Level, string Message)>();
    var log = new ModLog(
      () => true,
      message => entries.Add((LogLevel.Debug, message)),
      message => entries.Add((LogLevel.Info, message)),
      message => entries.Add((LogLevel.Warning, message)),
      message => entries.Add((LogLevel.Error, message))
    );

    log.Debug("debug");
    log.Info("info");
    log.Warning("warning");
    log.Error("error");

    Assert.Equal(
      [
        (LogLevel.Debug, "debug"),
        (LogLevel.Info, "info"),
        (LogLevel.Warning, "warning"),
        (LogLevel.Error, "error"),
      ],
      entries
    );
  }

  [Fact]
  public void DebugAction_WhenDisabled_ReturnsNull()
  {
    var entries = new List<string>();
    var log = new ModLog(() => false, entries.Add, entries.Add, entries.Add, entries.Add);

    log.DebugAction?.Invoke("debug");

    Assert.Null(log.DebugAction);
    Assert.Empty(entries);
  }
}
