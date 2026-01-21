namespace ErenshorLogs.Events;

/// <summary>
/// Boolean flags for combat events.
/// </summary>
public sealed record EventFlags
{
  /// <summary>Was a critical hit.</summary>
  public bool? Critical { get; init; }

  /// <summary>Damage exceeded target's remaining HP.</summary>
  public bool? Overkill { get; init; }

  /// <summary>Originated from player party (not hostile NPC).</summary>
  public bool? FromPlayer { get; init; }

  /// <summary>Source was a pet.</summary>
  public bool? Pet { get; init; }

  /// <summary>Spell was triggered by resonance mechanic.</summary>
  public bool? Resonating { get; init; }

  /// <summary>Ability attribution failed (debug flag).</summary>
  public bool? AttributionFailed { get; init; }

  /// <summary>Attack missed (failed hit roll).</summary>
  public bool? Missed { get; init; }

  /// <summary>Spell was fully resisted.</summary>
  public bool? Resisted { get; init; }

  /// <summary>Damage fully absorbed by shield.</summary>
  public bool? Absorbed { get; init; }
}
