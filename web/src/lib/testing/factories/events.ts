import type { AbilityRef, EffectRef, CombatEvent } from "$lib/types";
import { createPlayer, createNpc } from "./actors";

let eventCounter = 0;
let abilityCounter = 0;
let effectCounter = 0;

/**
 * Creates an AbilityRef with sequential naming.
 */
export function createAbilityRef(overrides: Partial<AbilityRef> = {}): AbilityRef {
  return {
    name: `Ability ${++abilityCounter}`,
    type: "skill",
    ...overrides,
  };
}

/**
 * Creates an EffectRef with sequential naming.
 */
export function createEffectRef(overrides: Partial<EffectRef> = {}): EffectRef {
  return {
    name: `Effect ${++effectCounter}`,
    duration: 10000,
    stacks: 1,
    ...overrides,
  };
}

/**
 * Creates a generic CombatEvent with sequential IDs and relative timestamps.
 * For specific event types, use createDamageEvent or createHealEvent.
 */
export function createCombatEvent(overrides: Partial<CombatEvent> = {}): CombatEvent {
  return {
    id: `event-${++eventCounter}`,
    timestamp: Date.now(),
    eventType: "damagePhysical",
    ...overrides,
  };
}

/**
 * Creates a damage event with typical damage attributes.
 */
export function createDamageEvent(overrides: Partial<CombatEvent> = {}): CombatEvent {
  return {
    id: `event-${++eventCounter}`,
    timestamp: Date.now(),
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
    id: `event-${++eventCounter}`,
    timestamp: Date.now(),
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
    id: `event-${++eventCounter}`,
    timestamp: Date.now(),
    eventType: "buffApply",
    source: createPlayer(),
    target: createPlayer(),
    effect: createEffectRef({ name: "Blessed Quiver", duration: 30000 }),
    ...overrides,
  };
}

/**
 * Resets the event counter. Useful for deterministic test snapshots.
 */
export function resetEventCounter(): void {
  eventCounter = 0;
}

/**
 * Resets the ability counter. Useful for deterministic test snapshots.
 */
export function resetAbilityCounter(): void {
  abilityCounter = 0;
}

/**
 * Resets the effect counter. Useful for deterministic test snapshots.
 */
export function resetEffectCounter(): void {
  effectCounter = 0;
}
