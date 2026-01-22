// Pure functions for aggregating combat event statistics

import type { CombatEvent, EventType, SessionStats, ActorStats, AbilityStats } from "$lib/types";
import {
  isDamageDealtByPlayer,
  isDamageTakenByPlayer,
  isHealingDoneByPlayer,
  isHealingReceivedByPlayer,
} from "$lib/utils/event-filters";

const MS_PER_SECOND = 1000;

const DAMAGE_EVENTS: Set<EventType> = new Set([
  "damagePhysical",
  "damageMagic",
  "damageMelee",
  "damageSkill",
  "damageSpell",
  "damageDot",
  "damageProc",
  "damagePet",
  "damageReflect",
  "damageEnvironmental",
]);

const HEAL_EVENTS: Set<EventType> = new Set(["healSpell", "healHot", "healLifesteal", "healRegen"]);

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
 * Tracks both outgoing (dealt) and incoming (taken) metrics for player faction.
 */
export function calculateSessionStats(events: CombatEvent[], durationMs: number): SessionStats {
  // Outgoing metrics (damage/healing dealt by player faction)
  const damageDealtEvents = events.filter(isDamageDealtByPlayer);
  const healingDoneEvents = events.filter(isHealingDoneByPlayer);

  const totalDamage = damageDealtEvents.reduce((sum, e) => sum + (e.amount ?? 0), 0);
  const totalHealing = healingDoneEvents.reduce((sum, e) => sum + (e.amount ?? 0), 0);

  // Incoming metrics (damage/healing taken by player faction)
  const damageTakenEvents = events.filter(isDamageTakenByPlayer);
  const healingReceivedEvents = events.filter(isHealingReceivedByPlayer);

  const totalDamageTaken = damageTakenEvents.reduce((sum, e) => sum + (e.amount ?? 0), 0);
  const totalHealingReceived = healingReceivedEvents.reduce((sum, e) => sum + (e.amount ?? 0), 0);

  // Defense metrics
  const totalRawDamage = damageTakenEvents.reduce(
    (sum, e) => sum + (e.rawAmount ?? e.amount ?? 0),
    0
  );
  const totalMitigated = damageTakenEvents.reduce((sum, e) => sum + (e.mitigated ?? 0), 0);

  // Calculate rates
  const dps = calculateRate(totalDamage, durationMs);
  const hps = calculateRate(totalHealing, durationMs);
  const dtps = calculateRate(totalDamageTaken, durationMs);
  const hrps = calculateRate(totalHealingReceived, durationMs);

  // Calculate mitigation rate
  const mitigationRate = calculatePercentage(totalMitigated, totalRawDamage);

  // Aggregate by actor (bidirectional)
  const actorBreakdown = aggregateByActor(events, durationMs);

  return {
    totalDamage,
    totalHealing,
    dps,
    hps,
    totalDamageTaken,
    totalHealingReceived,
    dtps,
    hrps,
    totalMitigated,
    mitigationRate,
    durationMs,
    actorBreakdown,
  };
}

/**
 * Aggregate combat events by actor with bidirectional tracking.
 * Tracks both outgoing (dealt) and incoming (taken) metrics for all actors.
 */
export function aggregateByActor(events: CombatEvent[], durationMs: number): ActorStats[] {
  const actorMap = new Map<string, ActorStats>();

  // Helper to ensure actor exists in map
  const ensureActor = (actorId: string, actorName: string, actorType: string) => {
    if (!actorMap.has(actorId)) {
      actorMap.set(actorId, {
        actorId,
        actorName,
        actorType: actorType as ActorStats["actorType"],
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
      });
    }
  };

  // First pass: collect all unique actors (from both source and target)
  for (const event of events) {
    if (event.source) {
      ensureActor(event.source.id, event.source.name, event.source.type);
    }
    if (event.target) {
      ensureActor(event.target.id, event.target.name, event.target.type);
    }
  }

  // Second pass: aggregate dealt and taken metrics
  // Track hit counters per actor for avoidance calculation
  const actorHitCounts = new Map<string, { hits: number; misses: number }>();

  for (const event of events) {
    // Track damage/healing DEALT by source
    if (event.source && actorMap.has(event.source.id)) {
      const actor = actorMap.get(event.source.id)!;
      if (isDamageEvent(event.eventType)) {
        actor.totalDamage += event.amount ?? 0;
      } else if (isHealEvent(event.eventType)) {
        actor.totalHealing += event.amount ?? 0;
      }
    }

    // Track damage/healing TAKEN by target
    if (event.target && actorMap.has(event.target.id)) {
      const actor = actorMap.get(event.target.id)!;
      if (isDamageEvent(event.eventType)) {
        const mitigated = event.mitigated ?? 0;
        const isMiss = event.flags?.missed ?? false;

        // Initialize hit counter for this actor if needed
        if (!actorHitCounts.has(actor.actorId)) {
          actorHitCounts.set(actor.actorId, { hits: 0, misses: 0 });
        }
        const hitCount = actorHitCounts.get(actor.actorId)!;

        if (isMiss) {
          hitCount.misses++;
          actor.totalMissedAgainst++;
        } else {
          hitCount.hits++;
          actor.damageTaken += event.amount ?? 0;
          actor.totalMitigated += mitigated;
        }
      } else if (isHealEvent(event.eventType)) {
        actor.healingReceived += event.amount ?? 0;
      }
    }
  }

  // Calculate totals for percentage calculations
  const actors = Array.from(actorMap.values());
  const totalDamageDealt = actors.reduce((sum, a) => sum + a.totalDamage, 0);
  const totalHealingDone = actors.reduce((sum, a) => sum + a.totalHealing, 0);
  const totalDamageTaken = actors.reduce((sum, a) => sum + a.damageTaken, 0);
  const totalHealingReceived = actors.reduce((sum, a) => sum + a.healingReceived, 0);

  // Calculate per-actor rates and percentages
  for (const actor of actors) {
    // Outgoing rates
    actor.dps = calculateRate(actor.totalDamage, durationMs);
    actor.hps = calculateRate(actor.totalHealing, durationMs);
    actor.percentOfTotalDamage = calculatePercentage(actor.totalDamage, totalDamageDealt);
    actor.percentOfTotalHealing = calculatePercentage(actor.totalHealing, totalHealingDone);

    // Incoming rates
    actor.dtps = calculateRate(actor.damageTaken, durationMs);
    actor.hrps = calculateRate(actor.healingReceived, durationMs);
    actor.percentOfTotalDamageTaken = calculatePercentage(actor.damageTaken, totalDamageTaken);
    actor.percentOfTotalHealingReceived = calculatePercentage(
      actor.healingReceived,
      totalHealingReceived
    );

    // Defense rates
    const actorRawDamage = actor.damageTaken + actor.totalMitigated;
    actor.mitigationRate = calculatePercentage(actor.totalMitigated, actorRawDamage);

    // Avoidance rate: misses / (hits + misses)
    const hitCount = actorHitCounts.get(actor.actorId);
    const totalAttackAttempts = hitCount ? hitCount.hits + hitCount.misses : 0;
    actor.avoidanceRate = calculatePercentage(actor.totalMissedAgainst, totalAttackAttempts);

    // Ability breakdowns
    actor.abilityBreakdown = aggregateByAbility(events, actor.actorId, "dealt");
    actor.abilitiesReceivedFrom = aggregateByAbility(events, actor.actorId, "taken");
  }

  return actors;
}

/**
 * Aggregate combat events by ability for a specific actor.
 * @param perspective - "dealt" for abilities used by actor, "taken" for abilities that hit actor
 */
export function aggregateByAbility(
  events: CombatEvent[],
  actorId: string,
  perspective: "dealt" | "taken" = "dealt"
): AbilityStats[] {
  const abilityMap = new Map<string, AbilityStats>();

  for (const event of events) {
    if (!event.ability) continue;

    // Filter by perspective
    const matchesActor =
      perspective === "dealt" ? event.source?.id === actorId : event.target?.id === actorId;

    if (!matchesActor) continue;

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
