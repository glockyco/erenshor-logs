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

  /// <summary>Proc effect.</summary>
  Proc,

  /// <summary>Damage over time effect.</summary>
  Dot,

  /// <summary>Heal over time effect.</summary>
  Hot,
}
