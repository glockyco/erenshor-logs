using UnityEngine;

namespace ErenshorLogs.Session;

/// <summary>
/// Extracts game version from Unity Application.version.
/// </summary>
public sealed class GameVersionProvider : IGameVersionProvider
{
  /// <inheritdoc />
  public string GetGameVersion()
  {
    try
    {
      return Application.version ?? "unknown";
    }
    catch
    {
      return "unknown";
    }
  }
}
