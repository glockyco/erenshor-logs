namespace ErenshorLogs.Hooks;

/// <summary>
/// Generic implementation of combat relevance checking with caching.
/// </summary>
/// <typeparam name="TCharacter">The character type (Unity Character class).</typeparam>
/// <typeparam name="TNpc">The NPC type (Unity NPC class).</typeparam>
public sealed class CombatRelevanceChecker<TCharacter, TNpc>
  where TCharacter : class
  where TNpc : class
{
  private readonly Func<TCharacter, int> _getInstanceId;
  private readonly Func<TCharacter, string> _getTransformName;
  private readonly Func<TCharacter, TNpc?> _getMyNpc;
  private readonly Func<TNpc, bool> _isSimPlayer;
  private readonly Func<TNpc, bool> _isInGroup;
  private readonly Func<TCharacter, TCharacter?> _getMaster;
  private readonly Func<IReadOnlyList<TNpc>> _getAttackingPlayer;
  private readonly Func<IReadOnlyList<TNpc>> _getGroupMatesInCombat;
  private readonly Func<IReadOnlyList<TCharacter>?> _getGroupTargets;
  private readonly Func<TCharacter[]> _getGroupMembers;
  private readonly Func<TNpc, IReadOnlyList<TCharacter>> _getAggroTablePlayers;

  private readonly HashSet<int> _relevantCharacterIds = [];

  /// <summary>
  /// Creates a new CombatRelevanceChecker with the required delegates for accessing game state.
  /// </summary>
  public CombatRelevanceChecker(
    Func<TCharacter, int> getInstanceId,
    Func<TCharacter, string> getTransformName,
    Func<TCharacter, TNpc?> getMyNpc,
    Func<TNpc, bool> isSimPlayer,
    Func<TNpc, bool> isInGroup,
    Func<TCharacter, TCharacter?> getMaster,
    Func<IReadOnlyList<TNpc>> getAttackingPlayer,
    Func<IReadOnlyList<TNpc>> getGroupMatesInCombat,
    Func<IReadOnlyList<TCharacter>?> getGroupTargets,
    Func<TCharacter[]> getGroupMembers,
    Func<TNpc, IReadOnlyList<TCharacter>> getAggroTablePlayers
  )
  {
    _getInstanceId = getInstanceId;
    _getTransformName = getTransformName;
    _getMyNpc = getMyNpc;
    _isSimPlayer = isSimPlayer;
    _isInGroup = isInGroup;
    _getMaster = getMaster;
    _getAttackingPlayer = getAttackingPlayer;
    _getGroupMatesInCombat = getGroupMatesInCombat;
    _getGroupTargets = getGroupTargets;
    _getGroupMembers = getGroupMembers;
    _getAggroTablePlayers = getAggroTablePlayers;
  }

  /// <summary>
  /// Checks if a combat event involving the given source and target is relevant to the player's group.
  /// Uses caching for performance - once a character is found relevant, they stay cached for the session.
  /// </summary>
  public bool IsRelevantCombat(TCharacter? source, TCharacter? target)
  {
    // Check source relevance
    if (source != null && IsRelevantCharacter(source))
      return true;

    // Check target relevance
    if (target != null && IsRelevantCharacter(target))
      return true;

    return false;
  }

  /// <summary>
  /// Clears the relevance cache. Should be called when a combat session ends.
  /// </summary>
  public void ClearCache()
  {
    _relevantCharacterIds.Clear();
  }

  /// <summary>
  /// Checks if a character is relevant to the player's group.
  /// First checks the cache for O(1) lookup, then does full relevance check if not cached.
  /// </summary>
  private bool IsRelevantCharacter(TCharacter character)
  {
    // Get instance ID - will be -1 or negative for invalid/destroyed objects
    var instanceId = _getInstanceId(character);

    // Invalid objects (negative IDs) are never relevant
    if (instanceId < 0)
      return false;

    // Check cache first (O(1))
    if (_relevantCharacterIds.Contains(instanceId))
      return true;

    // Not in cache - do full relevance check
    var isRelevant =
      IsPlayer(character)
      || IsGroupedSimPlayer(character)
      || IsPetOfPlayerOrGroup(character)
      || IsNpcAttackingPlayer(character)
      || IsGroupMateInCombat(character)
      || IsGroupTarget(character)
      || IsNpcWithGroupOnAggroTable(character)
      || IsPetOfEngagedNpc(character);

    // Cache if relevant
    if (isRelevant)
    {
      _relevantCharacterIds.Add(instanceId);
    }

    return isRelevant;
  }

  /// <summary>
  /// Checks if the character is the player.
  /// </summary>
  private bool IsPlayer(TCharacter character)
  {
    var name = _getTransformName(character);
    // Empty string indicates destroyed object - not a player
    return !string.IsNullOrEmpty(name) && name == "Player";
  }

  /// <summary>
  /// Checks if the character is a SimPlayer in the player's group.
  /// </summary>
  private bool IsGroupedSimPlayer(TCharacter character)
  {
    var npc = _getMyNpc(character);
    if (npc == null)
      return false;

    return _isSimPlayer(npc) && _isInGroup(npc);
  }

  /// <summary>
  /// Checks if the character is a pet of the player or a grouped SimPlayer.
  /// </summary>
  private bool IsPetOfPlayerOrGroup(TCharacter character)
  {
    var master = _getMaster(character);
    if (master == null)
      return false;

    // Check if master is player or grouped SimPlayer
    return IsPlayer(master) || IsGroupedSimPlayer(master);
  }

  /// <summary>
  /// Checks if the character is an NPC in the AttackingPlayer list.
  /// </summary>
  private bool IsNpcAttackingPlayer(TCharacter character)
  {
    var npc = _getMyNpc(character);
    if (npc == null)
      return false;

    var attackingPlayer = _getAttackingPlayer();
    return attackingPlayer.Contains(npc);
  }

  /// <summary>
  /// Checks if the character is an NPC in the GroupMatesInCombat list.
  /// </summary>
  private bool IsGroupMateInCombat(TCharacter character)
  {
    var npc = _getMyNpc(character);
    if (npc == null)
      return false;

    var groupMatesInCombat = _getGroupMatesInCombat();
    return groupMatesInCombat.Contains(npc);
  }

  /// <summary>
  /// Checks if the character is in the GroupTargets list.
  /// </summary>
  private bool IsGroupTarget(TCharacter character)
  {
    var groupTargets = _getGroupTargets();
    if (groupTargets == null)
      return false;

    return groupTargets.Contains(character);
  }

  /// <summary>
  /// Checks if the character is an NPC with the player or group members on its AggroTable.
  /// </summary>
  private bool IsNpcWithGroupOnAggroTable(TCharacter character)
  {
    var npc = _getMyNpc(character);
    if (npc == null)
      return false;

    var aggroTablePlayers = _getAggroTablePlayers(npc);
    if (aggroTablePlayers.Count == 0)
      return false;

    // Check if any player on the aggro table is the player or a grouped SimPlayer
    foreach (var player in aggroTablePlayers)
    {
      if (IsPlayer(player) || IsGroupedSimPlayer(player))
        return true;
    }

    return false;
  }

  /// <summary>
  /// Checks if the character is a pet of an NPC that's already engaged with the group.
  /// </summary>
  private bool IsPetOfEngagedNpc(TCharacter character)
  {
    var master = _getMaster(character);
    if (master == null)
      return false;

    var masterNpc = _getMyNpc(master);
    if (masterNpc == null)
      return false;

    // Check if the master is in any of the engaged NPC lists (but don't recurse into IsRelevantCharacter)
    var attackingPlayer = _getAttackingPlayer();
    if (attackingPlayer.Contains(masterNpc))
      return true;

    var groupMatesInCombat = _getGroupMatesInCombat();
    if (groupMatesInCombat.Contains(masterNpc))
      return true;

    var groupTargets = _getGroupTargets();
    if (groupTargets != null && groupTargets.Contains(master))
      return true;

    return false;
  }
}
