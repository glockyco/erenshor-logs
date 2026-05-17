import type { ActorRecord } from "$lib/types";

export function createActorRecord(overrides: Partial<ActorRecord> = {}): ActorRecord {
  return {
    id: crypto.randomUUID(),
    name: "Actor",
    kind: "player",
    faction: "friendly",
    ...overrides,
  };
}

export function createPlayer(overrides: Partial<ActorRecord> = {}): ActorRecord {
  return createActorRecord({
    id: "player-1",
    name: "Player",
    kind: "player",
    class: "Duelist",
    level: 35,
    faction: "friendly",
    isPlayerControlled: true,
    ...overrides,
  });
}

export function createSimPlayer(overrides: Partial<ActorRecord> = {}): ActorRecord {
  return createActorRecord({
    id: "sim-1",
    name: "Aeryn",
    kind: "simPlayer",
    class: "Arcanist",
    level: 35,
    faction: "friendly",
    isPlayerControlled: true,
    ...overrides,
  });
}

export function createNpc(overrides: Partial<ActorRecord> = {}): ActorRecord {
  return createActorRecord({
    id: "npc-1",
    name: "A Brittle Skeleton",
    kind: "npc",
    level: 30,
    faction: "hostile",
    ...overrides,
  });
}

export function createPet(overrides: Partial<ActorRecord> = {}): ActorRecord {
  return createActorRecord({
    id: "pet-1",
    name: "Summoned Dire Wolf",
    kind: "pet",
    level: 30,
    ownerActorId: "player-1",
    faction: "friendly",
    ...overrides,
  });
}
