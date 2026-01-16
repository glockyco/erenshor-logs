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
  [JsonPropertyName("pet")]
  public bool? Pet { get; init; }

  /// <summary>Triggered by a proc effect.</summary>
  [JsonPropertyName("proc")]
  public bool? Proc { get; init; }

  /// <summary>Ability attribution failed (debug flag).</summary>
  [JsonPropertyName("attributionFailed")]
  public bool? AttributionFailed { get; init; }

  /// <summary>Attack missed (failed hit roll).</summary>
  [JsonPropertyName("missed")]
  public bool? Missed { get; init; }

  /// <summary>Spell was fully resisted.</summary>
  [JsonPropertyName("resisted")]
  public bool? Resisted { get; init; }

  /// <summary>Damage fully absorbed by shield.</summary>
  [JsonPropertyName("absorbed")]
  public bool? Absorbed { get; init; }
}
