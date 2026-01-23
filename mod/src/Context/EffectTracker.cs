using ErenshorLogs.Events;

namespace ErenshorLogs.Context;

/// <summary>
/// Tracks active StatusEffects to attribute DoT/HoT ticks to their source spells.
/// Maps effect instances (character + slot index) to source ability context.
/// </summary>
public sealed class EffectTracker
{
  /// <summary>
  /// Map from (character hash, effect slot index) to the ability context.
  /// Character.GetHashCode() provides unique per-instance identification.
  /// Slot index is 0-29 (StatusEffects array size is 30).
  /// </summary>
  private readonly Dictionary<(int, int), AbilityContext> _activeEffects = new();

  /// <summary>
  /// Register an effect when applied.
  /// </summary>
  /// <param name="target">Character receiving the effect.</param>
  /// <param name="slotIndex">Index in StatusEffects array (0-29).</param>
  /// <param name="sourceSpell">Spell that applied the effect.</param>
  public void RegisterEffect(Character target, int slotIndex, Spell sourceSpell)
  {
    if (target == null || sourceSpell == null)
      return;

    var key = (target.GetHashCode(), slotIndex);

    // Determine ability type based on spell type
    var abilityType =
      sourceSpell.Type == Spell.SpellType.Beneficial ? AbilityType.Hot : AbilityType.Dot;

    var context = new AbilityContext
    {
      Name = sourceSpell.SpellName,
      Type = abilityType,
      StableKey = $"spell:{sourceSpell.Id}",
    };

    _activeEffects[key] = context;
  }

  /// <summary>
  /// Get ability context for an effect tick.
  /// Returns null if the effect is not tracked (e.g., already expired).
  /// </summary>
  /// <param name="target">Character with the effect.</param>
  /// <param name="slotIndex">Index in StatusEffects array (0-29).</param>
  public AbilityContext? GetEffectContext(Character target, int slotIndex)
  {
    if (target == null)
      return null;

    var key = (target.GetHashCode(), slotIndex);
    return _activeEffects.TryGetValue(key, out var context) ? context : null;
  }

  /// <summary>
  /// Remove effect when it expires or is removed.
  /// Prevents memory leaks and ensures stale effects don't cause wrong attribution.
  /// </summary>
  /// <param name="target">Character that had the effect.</param>
  /// <param name="slotIndex">Index in StatusEffects array (0-29).</param>
  public void UnregisterEffect(Character target, int slotIndex)
  {
    if (target == null)
      return;

    var key = (target.GetHashCode(), slotIndex);
    _activeEffects.Remove(key);
  }

  /// <summary>
  /// Clear all tracked effects.
  /// Useful for testing or session cleanup.
  /// </summary>
  public void Clear()
  {
    _activeEffects.Clear();
  }

  /// <summary>
  /// Get count of tracked effects (for diagnostics/testing).
  /// </summary>
  internal int Count() => _activeEffects.Count;
}
