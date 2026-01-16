namespace ErenshorLogs.Hooks;

/// <summary>
/// Return value constants from Character.DamageMe and related damage methods.
/// These mirror the game's undocumented return value semantics.
/// </summary>
public static class DamageResult
{
  /// <summary>Damage was fully mitigated by armor (DamageMe).</summary>
  public const int FullyMitigated = 0;

  /// <summary>Spell was fully resisted (MagicDamageMe). Same value as FullyMitigated.</summary>
  public const int FullyResisted = 0;

  /// <summary>Target is dead or invulnerable.</summary>
  public const int DeadOrInvulnerable = -1;

  /// <summary>Shield absorbed all damage.</summary>
  public const int ShieldAbsorbed = -2;

  /// <summary>Friendly fire was blocked.</summary>
  public const int FriendlyFire = -3;

  /// <summary>Target is a mining node (not a combat target).</summary>
  public const int MiningNode = -5;

  /// <summary>Target is a treasure chest (not a combat target).</summary>
  public const int TreasureChest = -6;

  /// <summary>
  /// Checks if the result indicates the event should be logged.
  /// Logs damage dealt (positive), full mitigation (0), and shield absorb (-2).
  /// </summary>
  public static bool ShouldLog(int result) => result >= 0 || result == ShieldAbsorbed;

  /// <summary>
  /// Checks if the result indicates a skip condition (no event should be emitted).
  /// </summary>
  public static bool ShouldSkip(int result) =>
    result == DeadOrInvulnerable
    || result == FriendlyFire
    || result == MiningNode
    || result == TreasureChest;
}
