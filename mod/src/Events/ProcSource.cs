namespace ErenshorLogs.Events;

/// <summary>
/// What triggered a proc effect.
/// Procs are abilities triggered by other actions (weapon hits, buffs, skills).
/// </summary>
public enum ProcSource
{
  /// <summary>Weapon on-hit proc (Item.WeaponProcOnHit).</summary>
  Weapon,

  /// <summary>Wand projectile proc (Item.WandEffect).</summary>
  Wand,

  /// <summary>Bow projectile proc (Item.BowEffect).</summary>
  Bow,

  /// <summary>Buff/status effect proc (Spell.AddProc).</summary>
  Buff,

  /// <summary>Skill-triggered spell (Skill.CastOnTarget).</summary>
  Skill,
}
