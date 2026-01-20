namespace ErenshorLogs.Events;

/// <summary>
/// Metadata about a combat logging session.
/// </summary>
public sealed record SessionMetadata
{
  /// <summary>Session UUID.</summary>
  public required string Id { get; init; }

  /// <summary>Unix timestamp (ms) of first event.</summary>
  public required long StartTime { get; init; }

  /// <summary>Unix timestamp (ms) of last event.</summary>
  public required long EndTime { get; init; }

  /// <summary>Session duration in milliseconds.</summary>
  public required long Duration { get; init; }

  /// <summary>Player information.</summary>
  public required PlayerInfo Player { get; init; }

  /// <summary>Erenshor game version.</summary>
  public required string GameVersion { get; init; }

  /// <summary>Combat Logger mod version.</summary>
  public required string ModVersion { get; init; }
}
