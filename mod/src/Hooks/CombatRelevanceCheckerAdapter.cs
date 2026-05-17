namespace ErenshorLogs.Hooks;

/// <summary>
/// Adapter that wires CombatRelevanceChecker to actual Unity game types.
/// </summary>
public sealed class CombatRelevanceCheckerAdapter : ICombatRelevanceChecker
{
  private readonly CombatRelevanceChecker<Character, NPC> _inner;

  private static readonly System.Reflection.FieldInfo? MyRaidSlotField = typeof(NPC).GetField(
    "MyRaidSlot"
  );

  /// <summary>
  /// Creates a new CombatRelevanceCheckerAdapter.
  /// </summary>
  public CombatRelevanceCheckerAdapter()
  {
    _inner = new CombatRelevanceChecker<Character, NPC>(
      // GetInstanceId
      c => c.GetInstanceID(),
      // GetTransformName
      c => c.transform.name,
      // GetMyNpc
      c => c.MyNPC,
      // IsSimPlayer
      npc => npc.SimPlayer,
      // IsInGroup
      IsInPlayerGroupOrRaid,
      // GetMaster
      c => c.Master,
      // GetAttackingPlayer
      () => GameData.AttackingPlayer,
      // GetGroupMatesInCombat
      () => GameData.GroupMatesInCombat,
      // GetGroupTargets
      () => GameData.SimPlayerGrouping?.GroupTargets,
      // GetGroupMembers
      () =>
      {
        // Extract Characters from the 4 group member slots
        var members = new List<Character>();
        for (int i = 0; i < 4; i++)
        {
          var member = GameData.GroupMembers[i];
          var character = member?.MyAvatar?.MyStats?.Myself;
          if (character != null)
            members.Add(character);
        }
        return members.ToArray();
      },
      // GetAggroTablePlayers
      npc =>
      {
        if (npc.AggroTable == null)
          return Array.Empty<Character>();

        var players = new List<Character>();
        foreach (var slot in npc.AggroTable)
        {
          if (slot.Player != null)
            players.Add(slot.Player);
        }
        return players;
      }
    );
  }

  private static bool IsInPlayerGroupOrRaid(NPC npc)
  {
    return npc.InGroup || MyRaidSlotField?.GetValue(npc) != null;
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
