using System.Text.Json.Serialization;

namespace ErenshorLogs.Events;

/// <summary>
/// Boolean flags for combat events.
/// </summary>
public sealed record EventFlags
{
  /// <summary>Was a critical hit.</summary>
  [JsonPropertyName("critical")]
  public bool? Critical { get; init; }

  /// <summary>Damage exceeded target's remaining HP.</summary>
  [JsonPropertyName("overkill")]
  public bool? Overkill { get; init; }

  /// <summary>Originated from player party (not hostile NPC).</summary>
  [JsonPropertyName("fromPlayer")]
  public bool? FromPlayer { get; init; }

  /// <summary>Source was a pet.</summary>
  [JsonPropertyName("isPet")]
  public bool? IsPet { get; init; }

  /// <summary>Triggered by a proc effect.</summary>
  [JsonPropertyName("isProc")]
  public bool? IsProc { get; init; }

  /// <summary>Ability attribution failed (debug flag).</summary>
  [JsonPropertyName("attributionFailed")]
  public bool? AttributionFailed { get; init; }
}
