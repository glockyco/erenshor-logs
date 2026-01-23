// Factory functions for creating mock statistics objects

import type { ActorStats, SessionStats, AbilityStats } from "$lib/types";

/**
 * Creates a mock ActorStats object with bidirectional metrics.
 * All numeric fields default to 0 and can be overridden.
 */
export function createActorStats(overrides: Partial<ActorStats> = {}): ActorStats {
  return {
    actorId: "actor-1",
    actorName: "Actor",
    actorType: "player",
    actorClass: undefined,
    // Outgoing
    totalDamage: 0,
    totalHealing: 0,
    dps: 0,
    hps: 0,
    percentOfTotalDamage: 0,
    percentOfTotalHealing: 0,
    // Incoming
    damageTaken: 0,
    healingReceived: 0,
    dtps: 0,
    hrps: 0,
    percentOfTotalDamageTaken: 0,
    percentOfTotalHealingReceived: 0,
    // Defense
    totalMitigated: 0,
    mitigationRate: 0,
    totalMissedAgainst: 0,
    avoidanceRate: 0,
    // Breakdowns
    abilityBreakdown: [],
    abilitiesReceivedFrom: [],
    ...overrides,
  };
}

/**
 * Creates a mock SessionStats object with bidirectional metrics.
 * All numeric fields default to 0 and can be overridden.
 */
export function createSessionStats(overrides: Partial<SessionStats> = {}): SessionStats {
  return {
    // Outgoing
    totalDamage: 0,
    totalHealing: 0,
    dps: 0,
    hps: 0,
    // Incoming
    totalDamageTaken: 0,
    totalHealingReceived: 0,
    dtps: 0,
    hrps: 0,
    // Defense
    totalMitigated: 0,
    mitigationRate: 0,
    // Meta
    durationMs: 0,
    actorBreakdown: [],
    ...overrides,
  };
}

/**
 * Creates a mock AbilityStats object.
 * All numeric fields default to 0 and can be overridden.
 */
export function createAbilityStats(overrides: Partial<AbilityStats> = {}): AbilityStats {
  return {
    abilityName: "Ability",
    abilityType: "skill",
    damage: 0,
    healing: 0,
    hits: 0,
    crits: 0,
    misses: 0,
    avgDamage: 0,
    avgHealing: 0,
    critRate: 0,
    missRate: 0,
    ...overrides,
  };
}
