// Pure functions for aggregating combat event statistics

import type { CombatEvent, EventType, SessionStats, ActorStats, AbilityStats } from "$lib/types";

const MS_PER_SECOND = 1000;

const DAMAGE_EVENTS: Set<EventType> = new Set([
  "damage_physical",
  "damage_magic",
  "damage_melee",
  "damage_skill",
  "damage_spell",
  "damage_dot",
  "damage_proc",
  "damage_pet",
  "damage_reflect",
  "damage_environmental",
]);

const HEAL_EVENTS: Set<EventType> = new Set([
  "heal_spell",
  "heal_hot",
  "heal_lifesteal",
  "heal_regen",
]);

function isDamageEvent(eventType: EventType): boolean {
  return DAMAGE_EVENTS.has(eventType);
}

function isHealEvent(eventType: EventType): boolean {
  return HEAL_EVENTS.has(eventType);
}

function calculateRate(total: number, durationMs: number): number {
  return durationMs > 0 ? (total / durationMs) * MS_PER_SECOND : 0;
}

function calculatePercentage(part: number, whole: number): number {
  return whole > 0 ? (part / whole) * 100 : 0;
}

/**
 * Calculate overall session statistics from combat events.
 */
export function calculateSessionStats(events: CombatEvent[], durationMs: number): SessionStats {
  const totalDamage = events
    .filter((e) => isDamageEvent(e.eventType))
    .reduce((sum, e) => sum + (e.amount ?? 0), 0);

  const totalHealing = events
    .filter((e) => isHealEvent(e.eventType))
    .reduce((sum, e) => sum + (e.amount ?? 0), 0);

  const dps = calculateRate(totalDamage, durationMs);
  const hps = calculateRate(totalHealing, durationMs);

  const actorBreakdown = aggregateByActor(events, durationMs);

  return {
    totalDamage,
    totalHealing,
    durationMs,
    dps,
    hps,
    actorBreakdown,
  };
}

/**
 * Aggregate combat events by actor (source).
 */
export function aggregateByActor(events: CombatEvent[], durationMs: number): ActorStats[] {
  const actorMap = new Map<string, ActorStats>();

  for (const event of events) {
    if (!event.source) continue;

    const actorId = event.source.id;
    if (!actorMap.has(actorId)) {
      actorMap.set(actorId, {
        actorId,
        actorName: event.source.name,
        actorType: event.source.type,
        totalDamage: 0,
        totalHealing: 0,
        dps: 0,
        hps: 0,
        percentOfTotalDamage: 0,
        percentOfTotalHealing: 0,
        abilityBreakdown: [],
      });
    }

    const actor = actorMap.get(actorId)!;

    if (isDamageEvent(event.eventType)) {
      actor.totalDamage += event.amount ?? 0;
    } else if (isHealEvent(event.eventType)) {
      actor.totalHealing += event.amount ?? 0;
    }
  }

  // Calculate totals for percentage calculation
  const actors = Array.from(actorMap.values());
  const totalDamage = actors.reduce((sum, a) => sum + a.totalDamage, 0);
  const totalHealing = actors.reduce((sum, a) => sum + a.totalHealing, 0);

  // Calculate per-actor DPS/HPS and percentages
  for (const actor of actors) {
    actor.dps = calculateRate(actor.totalDamage, durationMs);
    actor.hps = calculateRate(actor.totalHealing, durationMs);
    actor.percentOfTotalDamage = calculatePercentage(actor.totalDamage, totalDamage);
    actor.percentOfTotalHealing = calculatePercentage(actor.totalHealing, totalHealing);
    actor.abilityBreakdown = aggregateByAbility(events, actor.actorId);
  }

  return actors;
}

/**
 * Aggregate combat events by ability for a specific actor.
 */
export function aggregateByAbility(events: CombatEvent[], actorId: string): AbilityStats[] {
  const abilityMap = new Map<string, AbilityStats>();

  for (const event of events) {
    if (!event.source || event.source.id !== actorId) continue;
    if (!event.ability) continue;

    const abilityName = event.ability.name;
    if (!abilityMap.has(abilityName)) {
      abilityMap.set(abilityName, {
        abilityName,
        abilityType: event.ability.type,
        damage: 0,
        healing: 0,
        hits: 0,
        crits: 0,
        misses: 0,
        avgDamage: 0,
        avgHealing: 0,
        critRate: 0,
        missRate: 0,
      });
    }

    const ability = abilityMap.get(abilityName)!;

    const isMiss = event.flags?.missed ?? false;
    const isCrit = event.flags?.critical ?? false;

    if (isMiss) {
      ability.misses++;
    } else {
      ability.hits++;
      if (isCrit) ability.crits++;

      if (isDamageEvent(event.eventType)) {
        ability.damage += event.amount ?? 0;
      } else if (isHealEvent(event.eventType)) {
        ability.healing += event.amount ?? 0;
      }
    }
  }

  // Calculate averages and rates
  const abilities = Array.from(abilityMap.values());
  for (const ability of abilities) {
    const totalAttempts = ability.hits + ability.misses;
    ability.avgDamage = ability.hits > 0 ? ability.damage / ability.hits : 0;
    ability.avgHealing = ability.hits > 0 ? ability.healing / ability.hits : 0;
    ability.critRate = calculatePercentage(ability.crits, ability.hits);
    ability.missRate = calculatePercentage(ability.misses, totalAttempts);
  }

  return abilities;
}
