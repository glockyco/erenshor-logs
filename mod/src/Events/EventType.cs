namespace ErenshorLogs.Events;

/// <summary>
/// Types of combat events that can be logged.
/// Serializes to snake_case (e.g. DamageMelee -> "damage_melee").
/// </summary>
public enum EventType
{
  /// <summary>Physical damage (pre-attribution, from Character.DamageMe).</summary>
  DamagePhysical,

  /// <summary>Magic damage (pre-attribution, from Character.MagicDamageMe).</summary>
  DamageMagic,

  /// <summary>Auto-attack damage.</summary>
  DamageMelee,

  /// <summary>Melee/ranged skill damage.</summary>
  DamageSkill,

  /// <summary>Direct damage spell.</summary>
  DamageSpell,

  /// <summary>Damage over time tick.</summary>
  DamageDot,

  /// <summary>Weapon/ability proc damage.</summary>
  DamageProc,

  /// <summary>Pet damage (attributed to owner).</summary>
  DamagePet,

  /// <summary>Damage shield reflection.</summary>
  DamageReflect,

  /// <summary>Environmental damage.</summary>
  DamageEnvironmental,

  /// <summary>Direct healing spell.</summary>
  HealSpell,

  /// <summary>Heal over time tick.</summary>
  HealHot,

  /// <summary>Lifesteal healing.</summary>
  HealLifesteal,

  /// <summary>Buff applied.</summary>
  BuffApply,

  /// <summary>Buff removed/expired.</summary>
  BuffFade,

  /// <summary>Debuff applied.</summary>
  DebuffApply,

  /// <summary>Debuff removed/expired.</summary>
  DebuffFade,

  /// <summary>Entity died.</summary>
  Death,

  /// <summary>Combat began.</summary>
  CombatStart,

  /// <summary>Combat ended.</summary>
  CombatEnd,
}
