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

    // All remaining Characters are NPCs (enemies, friendly NPCs, etc.)
    // In Erenshor, every Character is one of: Player, Pet, SimPlayer, or NPC.
    // This is not a fallback - NPC is the correct type for anything not matching above.
    return ActorType.Npc;
  }
}
