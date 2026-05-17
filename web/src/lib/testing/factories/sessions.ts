import type { Registries, Session } from "$lib/types";
import { createAbilityRecord } from "./events";
import { createNpc, createPlayer, createSimPlayer } from "./actors";

export function createRegistries(overrides: Partial<Registries> = {}): Registries {
  const player = createPlayer();
  const simPlayer = createSimPlayer();
  const npc = createNpc();

  return {
    revision: 1,
    actors: {
      [player.id]: player,
      [simPlayer.id]: simPlayer,
      [npc.id]: npc,
    },
    abilities: {
      "ability-1": createAbilityRecord({ id: "ability-1", name: "Backstab" }),
      "heal-1": createAbilityRecord({ id: "heal-1", name: "Major Healing", kind: "spell" }),
    },
    effects: {},
    ...overrides,
  };
}

export function createSession(overrides: Partial<Session> = {}): Session {
  return {
    id: crypto.randomUUID(),
    mode: "automatic",
    state: "active",
    startedAtUtcMs: 0,
    producer: { name: "ErenshorLogsMod", modVersion: "2.0.0" },
    registryRevision: 1,
    lastEventSeq: 0,
    eventCount: 0,
    completeness: "complete",
    registries: createRegistries(),
    events: [],
    protocolErrors: [],
    ...overrides,
  };
}

export function createActiveSession(overrides: Partial<Session> = {}): Session {
  return createSession({
    state: "active",
    endedAtUtcMs: undefined,
    durationMs: undefined,
    ...overrides,
  });
}

export function createSessionWithDuration(
  durationMs: number,
  overrides: Partial<Session> = {}
): Session {
  return createSession({
    state: "ended",
    startedAtUtcMs: 0,
    endedAtUtcMs: durationMs,
    durationMs,
    endReason: "manual",
    ...overrides,
  });
}

export function createCompletedSession(overrides: Partial<Session> = {}): Session {
  return createSessionWithDuration(300000, overrides);
}

export function createShortSession(overrides: Partial<Session> = {}): Session {
  return createSessionWithDuration(30000, overrides);
}

export function createLongSession(overrides: Partial<Session> = {}): Session {
  return createSessionWithDuration(3600000, overrides);
}
