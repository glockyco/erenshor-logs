namespace ErenshorLogs.Hooks;

/// <summary>
/// Adapter that wires CombatRelevanceChecker to actual Unity game types.
/// </summary>
public sealed class CombatRelevanceCheckerAdapter : ICombatRelevanceChecker
{
  private readonly CombatRelevanceChecker<Character, NPC> _inner;

  /// <summary>
  /// Creates a new CombatRelevanceCheckerAdapter.
  /// </summary>
  public CombatRelevanceCheckerAdapter()
  {
    _inner = new CombatRelevanceChecker<Character, NPC>(
      // GetInstanceId - returns -1 for invalid/destroyed objects (never matches valid IDs)
      c => c.GetSafeInstanceID() ?? -1,
      // GetTransformName - returns empty string for invalid/destroyed objects
      c => c.GetSafeTransformName(),
      // GetMyNpc - null-conditional prevents NullRef on destroyed objects
      c => c?.MyNPC,
      // IsSimPlayer - safe even if npc is null (returns false)
      npc => npc?.SimPlayer ?? false,
      // IsInGroup - safe even if npc is null
      npc => npc?.InGroup ?? false,
      // GetMaster - null-conditional chain
      c => c?.Master,
      // GetAttackingPlayer - safe, GameData won't be null
      () => GameData.AttackingPlayer,
      // GetGroupMatesInCombat - safe, GameData won't be null
      () => GameData.GroupMatesInCombat,
      // GetGroupTargets - safe, returns null if SimPlayerGrouping is null
      () => GameData.SimPlayerGrouping?.GroupTargets,
      // GetGroupMembers - safe extraction with validity checks
      () =>
      {
        // Extract Characters from the 4 group member slots
        var members = new List<Character>();
        for (int i = 0; i < 4; i++)
        {
          var member = GameData.GroupMembers[i];
          // Check each member in the chain for validity
          var character = member?.MyAvatar?.MyStats?.Myself;
          if (character != null && character.IsValid())
            members.Add(character);
        }
        return members.ToArray();
      },
      // GetAggroTablePlayers - safe iteration with validity checks
      npc =>
      {
        if (npc?.AggroTable == null)
          return Array.Empty<Character>();

        var players = new List<Character>();
        foreach (var slot in npc.AggroTable)
        {
          // Check slot player is valid before adding
          if (slot.Player != null && slot.Player.IsValid())
            players.Add(slot.Player);
        }
        return players;
      }
    );
  }

  /// <inheritdoc />
  public bool IsRelevantCombat(Character? source, Character? target)
  {
    return _inner.IsRelevantCombat(source, target);
  }

  /// <inheritdoc />
  public void ClearCache()
  {
    _inner.ClearCache();
  }
}
