namespace ErenshorLogs.Events;

/// <summary>
/// Types of damage that can be dealt.
/// </summary>
public enum DamageType
{
  /// <summary>Unrecognized damage type from game. Indicates mapper needs updating.</summary>
  Unknown,

  /// <summary>Melee/physical damage.</summary>
  Physical,

  /// <summary>Arcane/magic damage.</summary>
  Magic,

  /// <summary>Fire/ice/lightning damage.</summary>
  Elemental,

  /// <summary>Shadow/void damage.</summary>
  Void,

  /// <summary>Poison/nature damage.</summary>
  Poison,
}
