import type { ActorRef } from "$lib/types";

let actorCounter = 0;

/**
 * Creates a generic ActorRef with sequential IDs.
 * Use specific factory functions (createPlayer, createNpc, etc.) for common actor types.
 */
export function createActorRef(overrides: Partial<ActorRef> = {}): ActorRef {
  return {
    id: `actor-${++actorCounter}`,
    name: "Actor",
    type: "player",
    ...overrides,
  };
}

/**
 * Creates a player ActorRef with typical player attributes.
 */
export function createPlayer(overrides: Partial<ActorRef> = {}): ActorRef {
  return {
    id: `player-${++actorCounter}`,
    name: "Player",
    type: "player",
    class: "Duelist",
    level: 35,
    ...overrides,
  };
}

/**
 * Creates a simulated player ActorRef (AI-controlled party member).
 */
export function createSimPlayer(overrides: Partial<ActorRef> = {}): ActorRef {
  return {
    id: `sim-${++actorCounter}`,
    name: "Aeryn",
    type: "simPlayer",
    class: "Arcanist",
    level: 35,
    ...overrides,
  };
}

/**
 * Creates an NPC ActorRef (enemy or friendly NPC).
 */
export function createNpc(overrides: Partial<ActorRef> = {}): ActorRef {
  return {
    id: `npc-${++actorCounter}`,
    name: "A Brittle Skeleton",
    type: "npc",
    level: 30,
    ...overrides,
  };
}

/**
 * Creates a pet ActorRef with a reference to its master.
 */
export function createPet(overrides: Partial<ActorRef> = {}): ActorRef {
  return {
    id: `pet-${++actorCounter}`,
    name: "Summoned Dire Wolf",
    type: "pet",
    level: 30,
    masterId: "player-1",
    ...overrides,
  };
}

/**
 * Resets the actor counter. Useful for deterministic test snapshots.
 */
export function resetActorCounter(): void {
  actorCounter = 0;
}
