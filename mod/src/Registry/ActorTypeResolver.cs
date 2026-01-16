using ErenshorLogs.Events;

namespace ErenshorLogs.Registry;

/// <summary>
/// Production implementation that determines ActorType from game state.
/// </summary>
public sealed class ActorTypeResolver : IActorTypeResolver
{
  public ActorType Resolve(Character character)
  {
    // Player check first - most specific
    if (GameData.PlayerControl != null && character == GameData.PlayerControl.Myself)
      return ActorType.Player;

    // Pet check - has a master
    if (character.Master != null)
      return ActorType.Pet;

    // SimPlayer check - has SimPlayer component
    if (character.GetComponent<SimPlayer>() != null)
      return ActorType.SimPlayer;

    // Default to NPC
    return ActorType.Npc;
  }
}
