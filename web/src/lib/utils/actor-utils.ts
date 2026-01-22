// Utilities for actor faction identification and classification
// Used to distinguish between friendly (player/sims/pets) and hostile (NPC) actors

import type { ActorRef, ActorType } from "$lib/types";

/**
 * Faction type for combat analytics filtering
 */
export type Faction = "friendly" | "hostile";

/**
 * Check if an actor type belongs to the player faction.
 * Player faction includes: player, simulated players, and pets.
 */
export function isPlayerFaction(actorType: ActorType): boolean {
  return ["player", "simPlayer", "pet"].includes(actorType);
}

/**
 * Check if an actor type belongs to the enemy faction.
 * Enemy faction includes: NPCs and their pets.
 */
export function isEnemyFaction(actorType: ActorType): boolean {
  return actorType === "npc";
}

/**
 * Get the faction classification for an actor.
 * Returns null if the actor reference is missing.
 */
export function getActorFaction(actor?: ActorRef): Faction | null {
  if (!actor) return null;
  return isPlayerFaction(actor.type) ? "friendly" : "hostile";
}

/**
 * Filter actors by faction.
 * Preserves the full type of the input array.
 */
export function filterByFaction<T extends { actorType: ActorType }>(
  actors: T[],
  faction: "all" | Faction
): T[] {
  if (faction === "all") return actors;

  return actors.filter((actor) => {
    if (faction === "friendly") return isPlayerFaction(actor.actorType);
    return isEnemyFaction(actor.actorType);
  });
}
