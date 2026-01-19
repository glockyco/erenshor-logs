namespace ErenshorLogs.Hooks;

/// <summary>
/// Checks if combat events are relevant to the player's group.
/// </summary>
public interface ICombatRelevanceChecker
{
  /// <summary>
  /// Checks if a combat event involving the given source and target is relevant to the player's group.
  /// </summary>
  /// <param name="source">The source character (attacker), or null for environmental damage.</param>
  /// <param name="target">The target character (victim).</param>
  /// <returns>True if either the source or target is relevant to the player's group.</returns>
  bool IsRelevantCombat(Character? source, Character? target);

  /// <summary>
  /// Clears the relevance cache. Called when a combat session ends.
  /// </summary>
  void ClearCache();
}
