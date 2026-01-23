import type { AbilityRef, EffectRef, CombatEvent } from "$lib/types";
import { createPlayer, createNpc } from "./actors";

/**
 * Creates an AbilityRef with default values.
 */
export function createAbilityRef(overrides: Partial<AbilityRef> = {}): AbilityRef {
  return {
    name: "Ability",
    type: "skill",
    ...overrides,
  };
}

/**
 * Creates an EffectRef with default values.
 */
export function createEffectRef(overrides: Partial<EffectRef> = {}): EffectRef {
  return {
    name: "Effect",
    duration: 10000,
    stacks: 1,
    ...overrides,
  };
}

/**
 * Creates a generic CombatEvent with unique ID and deterministic timestamp.
 * For specific event types, use createDamageEvent or createHealEvent.
 */
export function createCombatEvent(overrides: Partial<CombatEvent> = {}): CombatEvent {
  return {
    id: crypto.randomUUID(),
    timestamp: 0,
    eventType: "damagePhysical",
    ...overrides,
  };
}

/**
 * Creates a damage event with typical damage attributes.
 */
export function createDamageEvent(overrides: Partial<CombatEvent> = {}): CombatEvent {
  return {
    id: crypto.randomUUID(),
    timestamp: 0,
    eventType: "damagePhysical",
    source: createPlayer(),
    target: createNpc(),
    ability: createAbilityRef({ name: "Backstab", type: "skill" }),
    amount: 1000,
    damageType: "physical",
    ...overrides,
  };
}

/**
 * Creates a heal event with typical healing attributes.
 */
export function createHealEvent(overrides: Partial<CombatEvent> = {}): CombatEvent {
  return {
    id: crypto.randomUUID(),
    timestamp: 0,
    eventType: "healSpell",
    source: createPlayer(),
    target: createPlayer(),
    amount: 500,
    ability: createAbilityRef({ name: "Major Healing", type: "spell" }),
    ...overrides,
  };
}

/**
 * Creates a critical damage event.
 */
export function createCriticalDamageEvent(overrides: Partial<CombatEvent> = {}): CombatEvent {
  return createDamageEvent({
    amount: 2000,
    flags: { critical: true },
    ...overrides,
  });
}

/**
 * Creates a buff application event.
 */
export function createBuffEvent(overrides: Partial<CombatEvent> = {}): CombatEvent {
  return {
    id: crypto.randomUUID(),
    timestamp: 0,
    eventType: "buffApply",
    source: createPlayer(),
    target: createPlayer(),
    effect: createEffectRef({ name: "Blessed Quiver", duration: 30000 }),
    ...overrides,
  };
}

/**
 * Creates multiple combat events with sequential timestamps.
 * Useful for testing time-based logic like DPS calculations.
 *
 * @param count Number of events to create
 * @param intervalMs Time interval between events in milliseconds
 * @returns Array of combat events with sequential timestamps
 */
export function createTimedEvents(count: number, intervalMs: number): CombatEvent[] {
  return Array.from({ length: count }, (_, i) => createCombatEvent({ timestamp: i * intervalMs }));
}
