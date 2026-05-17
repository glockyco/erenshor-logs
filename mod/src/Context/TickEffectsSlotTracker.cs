namespace ErenshorLogs.Context;

/// <summary>
/// Tracks sequential slot processing during Stats.TickEffects.
/// Allows damage/heal hooks to determine which StatusEffect slot caused each call.
/// </summary>
/// <remarks>
/// TickEffects processes slots sequentially (0→29). When a slot triggers damage/heal,
/// we scan forward from our last position to find the matching slot. This works because:
/// 1. Processing order is deterministic (0→29)
/// 2. Resisted damage doesn't call DamageMe (so we don't advance)
/// 3. We stay in perfect sync with the loop
/// </remarks>
public readonly record struct TickHealthDelta(int Slot, int Amount);

public static class TickEffectsSlotTracker
{
  /// <summary>
  /// The character currently being processed by TickEffects.
  /// Thread-local to handle concurrent ticking across multiple characters.
  /// </summary>
  [ThreadStatic]
  private static Character? _currentCharacter;

  /// <summary>
  /// Last slot that was processed (starts at -1).
  /// We scan forward from this position to find the next match.
  /// </summary>
  [ThreadStatic]
  private static int _lastProcessedSlot;

  /// <summary>
  /// Whether we're currently inside a TickEffects call.
  /// </summary>
  [ThreadStatic]
  private static bool _isInTickEffects;

  [ThreadStatic]
  private static List<TickHealthDelta>? _healthDeltas;

  /// <summary>
  /// Called by TickEffectsPatch.Prefix when TickEffects starts.
  /// Initializes tracking state for this character's tick processing.
  /// </summary>
  public static void BeginTickEffects(Stats stats)
  {
    _currentCharacter = stats.Myself;
    _lastProcessedSlot = -1;
    _isInTickEffects = true;
    _healthDeltas = null;
  }

  /// <summary>
  /// Called by damage/heal hooks to find which slot caused this call.
  /// Scans forward from last processed slot to find the next matching effect.
  /// </summary>
  /// <param name="target">Character that received damage/heal.</param>
  /// <param name="damageType">Type of damage dealt (for matching).</param>
  /// <param name="isBleed">Whether this is bleed damage (uses BleedDamagePercent).</param>
  /// <param name="isHeal">Whether this is healing (uses TargetHealing).</param>
  /// <returns>Slot index if found, null if not in TickEffects or no match.</returns>
  public static int? FindAndAdvanceSlot(
    Character target,
    GameData.DamageType damageType,
    bool isBleed = false,
    bool isHeal = false
  )
  {
    // Only works if we're in TickEffects for this character
    if (!_isInTickEffects || _currentCharacter != target)
      return null;

    // Scan forward from last processed position
    for (int i = _lastProcessedSlot + 1; i < 30; i++)
    {
      var statusEffect = target.MyStats.StatusEffects[i];
      var spell = statusEffect?.Effect;

      if (spell == null || statusEffect == null || statusEffect.Duration <= 0f)
        continue;

      // Check if this slot matches the effect type we're looking for
      bool matches = false;

      if (isHeal)
      {
        // Heal-over-time: TargetHealing > 0 and Physical damage type
        // (See Stats.TickEffects line 1347)
        matches = spell.TargetHealing > 0 && spell.MyDamageType == GameData.DamageType.Physical;
      }
      else if (isBleed)
      {
        // Bleed damage: BleedDamagePercent > 0
        // (See Stats.TickEffects line 1324)
        matches = spell.BleedDamagePercent > 0;
      }
      else
      {
        // Regular DoT damage: TargetDamage > 0 with matching damage type
        // (See Stats.TickEffects line 1240)
        matches = spell.TargetDamage > 0 && spell.MyDamageType == damageType;
      }

      if (matches)
      {
        // Found it! Advance our position and return
        _lastProcessedSlot = i;
        return i;
      }
    }

    // No matching slot found
    // This can happen if effect expired between scan and call
    return null;
  }

  public static int? GetCurrentReapAndRenewSlot(Character target)
  {
    if (!_isInTickEffects || _currentCharacter != target || _lastProcessedSlot < 0)
      return null;

    var statusEffect = target.MyStats.StatusEffects[_lastProcessedSlot];
    var spell = statusEffect?.Effect;
    if (spell?.ReapAndRenew == true && statusEffect!.Duration > 0f)
      return _lastProcessedSlot;

    return null;
  }

  public static void RecordHealthDelta(int slot, int amount)
  {
    if (!_isInTickEffects || amount == 0)
      return;

    _healthDeltas ??= [];
    _healthDeltas.Add(new TickHealthDelta(slot, amount));
  }

  public static IReadOnlyList<TickHealthDelta> GetHealthDeltas()
  {
    return _healthDeltas ?? [];
  }

  /// <summary>
  /// Called by TickEffectsPatch.Finalizer when TickEffects completes.
  /// Cleans up tracking state.
  /// </summary>
  public static void EndTickEffects()
  {
    _currentCharacter = null;
    _lastProcessedSlot = -1;
    _isInTickEffects = false;
    _healthDeltas = null;
  }

  /// <summary>
  /// Get whether we're currently in TickEffects (for diagnostics).
  /// </summary>
  internal static bool IsInTickEffects() => _isInTickEffects;
}
