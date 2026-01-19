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
  /// Player information captured at session start.
  /// </summary>
  public PlayerInfo Player { get; }

  /// <summary>
  /// Game version at session start.
  /// </summary>
  public string GameVersion { get; }

  /// <summary>
  /// Mod version at session start.
  /// </summary>
  public string ModVersion { get; }

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
  public CombatSession(PlayerInfo player, string gameVersion, string modVersion)
  {
    Id = Guid.NewGuid().ToString();
    StartTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    Player = player;
    GameVersion = gameVersion;
    ModVersion = modVersion;
  }

  /// <summary>
  /// Marks the session as ended.
  /// </summary>
  internal void End()
  {
    if (EndTime == null)
    {
      EndTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
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
      Player = Player,
      GameVersion = GameVersion,
      ModVersion = ModVersion,
    };
  }
}
