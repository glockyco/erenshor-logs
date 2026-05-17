using BepInEx.Logging;

namespace ErenshorLogs.Logging;

public sealed class ModLog
{
  private readonly Func<bool> _isEnabled;
  private readonly Action<string> _debug;
  private readonly Action<string> _info;
  private readonly Action<string> _warning;
  private readonly Action<string> _error;

  public ModLog(ManualLogSource source, Func<bool> isEnabled)
    : this(source.LogDebug, source.LogInfo, source.LogWarning, source.LogError, isEnabled) { }

  internal ModLog(
    Func<bool> isEnabled,
    Action<string> debug,
    Action<string> info,
    Action<string> warning,
    Action<string> error
  )
    : this(debug, info, warning, error, isEnabled) { }

  private ModLog(
    Action<string> debug,
    Action<string> info,
    Action<string> warning,
    Action<string> error,
    Func<bool> isEnabled
  )
  {
    _debug = debug;
    _info = info;
    _warning = warning;
    _error = error;
    _isEnabled = isEnabled;
  }

  public bool IsEnabled => _isEnabled();

  public Action<string>? DebugAction => IsEnabled ? Debug : null;

  public void Debug(string message)
  {
    if (IsEnabled)
      _debug(message);
  }

  public void Info(string message)
  {
    if (IsEnabled)
      _info(message);
  }

  public void Warning(string message)
  {
    if (IsEnabled)
      _warning(message);
  }

  public void Error(string message)
  {
    if (IsEnabled)
      _error(message);
  }
}
