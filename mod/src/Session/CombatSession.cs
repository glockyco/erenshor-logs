using ErenshorLogs.Events;

namespace ErenshorLogs.Session;

/// <summary>
/// Represents an active or completed combat session.
/// </summary>
public sealed class CombatSession
{
  /// <summary>
  /// Unique session identifier.
  /// </summary>
  public string Id { get; }

  /// <summary>
  /// Unix timestamp (ms) when combat started.
  /// </summary>
  public long StartTime { get; }

  /// <summary>
  /// Unix timestamp (ms) when combat ended, or null if still active.
  /// </summary>
  public long? EndTime { get; private set; }

  /// <summary>
  /// Game version at session start.
  /// </summary>
  public string GameVersion { get; }

  /// <summary>
  /// Mod version at session start.
  /// </summary>
  public string ModVersion { get; }

  /// <summary>
  /// Whether this session was started manually via hotkey.
  /// Manual sessions don't auto-end on inactivity timeout.
  /// </summary>
  public bool IsManual { get; }

  /// <summary>
  /// Whether the session is still active (EndTime is null).
  /// </summary>
  public bool IsActive => EndTime == null;

  /// <summary>
  /// Duration in milliseconds, or time since start if still active.
  /// </summary>
  public long Duration => (EndTime ?? DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()) - StartTime;

  /// <summary>
  /// Creates a new combat session.
  /// </summary>
  /// <param name="gameVersion">Game version string.</param>
  /// <param name="modVersion">Mod version string.</param>
  /// <param name="isManual">Whether this session was manually started.</param>
  public CombatSession(string gameVersion, string modVersion, bool isManual = false)
  {
    Id = Guid.NewGuid().ToString();
    StartTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    GameVersion = gameVersion;
    ModVersion = modVersion;
    IsManual = isManual;
  }

  /// <summary>
  /// Marks the session as ended at the current time.
  /// </summary>
  internal void End()
  {
    if (EndTime == null)
    {
      EndTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }
  }

  /// <summary>
  /// Marks the session as ended at a specific timestamp.
  /// Used for backdating session end to last event time.
  /// </summary>
  /// <param name="endTime">Unix timestamp (ms) when session ended.</param>
  internal void EndAt(long endTime)
  {
    if (EndTime == null)
    {
      EndTime = endTime;
    }
  }

  /// <summary>
  /// Converts this session to SessionMetadata for export.
  /// </summary>
  public SessionMetadata ToMetadata()
  {
    var endTime = EndTime ?? DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    return new SessionMetadata
    {
      Id = Id,
      StartTime = StartTime,
      EndTime = endTime,
      Duration = endTime - StartTime,
      GameVersion = GameVersion,
      ModVersion = ModVersion,
    };
  }
}
