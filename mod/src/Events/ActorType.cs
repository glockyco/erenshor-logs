namespace ErenshorLogs.Events;

/// <summary>
/// Types of actors that can participate in combat.
/// Serializes to camelCase (e.g. SimPlayer -> "simPlayer").
/// </summary>
public enum ActorType
{
  /// <summary>The player character.</summary>
  Player,

  /// <summary>Simulated players (AI companions).</summary>
  SimPlayer,

  /// <summary>Non-player characters (enemies, NPCs).</summary>
  Npc,

  /// <summary>Player or SimPlayer pets.</summary>
  Pet,
}
