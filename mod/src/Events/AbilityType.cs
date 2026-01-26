namespace ErenshorLogs.Events;

/// <summary>
/// Types of abilities that can deal damage or heal.
/// </summary>
public enum AbilityType
{
  /// <summary>Melee/ranged skill.</summary>
  Skill,

  /// <summary>Cast spell.</summary>
  Spell,

  /// <summary>Auto-attack.</summary>
  Auto,

  /// <summary>Damage over time effect.</summary>
  Dot,

  /// <summary>Heal over time effect.</summary>
  Hot,

  /// <summary>Attribution failed - source ability could not be determined.</summary>
  Unknown,

  /// <summary>Environmental damage (fall, fire, drowning, etc.).</summary>
  Environmental,

  /// <summary>Area effect damage from NPC abilities (auras, breath attacks, curses, etc.).</summary>
  AreaEffect,
}
