import type {
  AbilityStats,
  ActorRecord,
  ActorStats,
  DamageEvent,
  HealEvent,
  Registries,
  Session,
  SessionStats,
} from "$lib/types";
import {
  getEventSource,
  getEventTarget,
  isDamageDealtByPlayer,
  isDamageEvent,
  isDamageTakenByPlayer,
  isHealEvent,
  isHealingDoneByPlayer,
  isHealingReceivedByPlayer,
} from "$lib/utils/event-filters";

const MS_PER_SECOND = 1000;

export function calculateRate(total: number, durationMs: number): number {
  return durationMs > 0 ? (total / durationMs) * MS_PER_SECOND : 0;
}

export function calculatePercentage(part: number, whole: number): number {
  return whole > 0 ? (part / whole) * 100 : 0;
}

export function calculateSessionStats(
  session: Session,
  durationMs = getDurationMs(session)
): SessionStats {
  const { events, registries } = session;
  const damageDealtEvents = events.filter((event): event is DamageEvent =>
    isDamageDealtByPlayer(event, registries)
  );
  const healingDoneEvents = events.filter((event): event is HealEvent =>
    isHealingDoneByPlayer(event, registries)
  );
  const damageTakenEvents = events.filter((event): event is DamageEvent =>
    isDamageTakenByPlayer(event, registries)
  );
  const healingReceivedEvents = events.filter((event): event is HealEvent =>
    isHealingReceivedByPlayer(event, registries)
  );

  const totalDamage = sumDamage(damageDealtEvents);
  const totalHealing = sumHealing(healingDoneEvents);
  const totalDamageTaken = sumDamage(damageTakenEvents);
  const totalHealingReceived = sumHealing(healingReceivedEvents);
  const totalMitigated = damageTakenEvents.reduce(
    (sum, event) => sum + (event.data.mitigatedAmount ?? 0),
    0
  );
  const totalRawDamage = damageTakenEvents.reduce(
    (sum, event) =>
      sum + (event.data.rawAmount ?? event.data.amount + (event.data.mitigatedAmount ?? 0)),
    0
  );

  return {
    totalDamage,
    totalHealing,
    dps: calculateRate(totalDamage, durationMs),
    hps: calculateRate(totalHealing, durationMs),
    totalDamageTaken,
    totalHealingReceived,
    dtps: calculateRate(totalDamageTaken, durationMs),
    hrps: calculateRate(totalHealingReceived, durationMs),
    totalMitigated,
    mitigationRate: calculatePercentage(totalMitigated, totalRawDamage),
    durationMs,
    actorBreakdown: aggregateByActor(session, durationMs),
  };
}

export function aggregateByActor(
  session: Session,
  durationMs = getDurationMs(session)
): ActorStats[] {
  const actorMap = new Map<string, ActorStats>();
  const actorHitCounts = new Map<string, { hits: number; misses: number }>();

  for (const event of session.events) {
    ensureEventActor(actorMap, getEventSource(event, session.registries));
    ensureEventActor(actorMap, getEventTarget(event, session.registries));
  }

  for (const event of session.events) {
    const source = getEventSource(event, session.registries);
    const target = getEventTarget(event, session.registries);

    if (source) {
      const actor = actorMap.get(source.id);
      if (actor && isDamageEvent(event)) actor.totalDamage += event.data.amount;
      if (actor && isHealEvent(event)) actor.totalHealing += event.data.amount;
    }

    if (target) {
      const actor = actorMap.get(target.id);
      if (!actor) continue;

      if (isDamageEvent(event)) {
        const hitCount = actorHitCounts.get(actor.actorId) ?? { hits: 0, misses: 0 };
        actorHitCounts.set(actor.actorId, hitCount);

        if (event.data.outcome.result === "missed") {
          hitCount.misses += 1;
          actor.totalMissedAgainst += 1;
        } else {
          hitCount.hits += 1;
          actor.damageTaken += event.data.amount;
          actor.totalMitigated += event.data.mitigatedAmount ?? 0;
        }
      }

      if (isHealEvent(event)) actor.healingReceived += event.data.amount;
    }
  }

  const actors = Array.from(actorMap.values());
  const totalDamageDealt = actors.reduce((sum, actor) => sum + actor.totalDamage, 0);
  const totalHealingDone = actors.reduce((sum, actor) => sum + actor.totalHealing, 0);
  const totalDamageTaken = actors.reduce((sum, actor) => sum + actor.damageTaken, 0);
  const totalHealingReceived = actors.reduce((sum, actor) => sum + actor.healingReceived, 0);

  for (const actor of actors) {
    actor.dps = calculateRate(actor.totalDamage, durationMs);
    actor.hps = calculateRate(actor.totalHealing, durationMs);
    actor.percentOfTotalDamage = calculatePercentage(actor.totalDamage, totalDamageDealt);
    actor.percentOfTotalHealing = calculatePercentage(actor.totalHealing, totalHealingDone);
    actor.dtps = calculateRate(actor.damageTaken, durationMs);
    actor.hrps = calculateRate(actor.healingReceived, durationMs);
    actor.percentOfTotalDamageTaken = calculatePercentage(actor.damageTaken, totalDamageTaken);
    actor.percentOfTotalHealingReceived = calculatePercentage(
      actor.healingReceived,
      totalHealingReceived
    );

    const actorRawDamage = actor.damageTaken + actor.totalMitigated;
    actor.mitigationRate = calculatePercentage(actor.totalMitigated, actorRawDamage);

    const hitCount = actorHitCounts.get(actor.actorId);
    const totalAttackAttempts = hitCount ? hitCount.hits + hitCount.misses : 0;
    actor.avoidanceRate = calculatePercentage(actor.totalMissedAgainst, totalAttackAttempts);

    actor.abilityBreakdown = aggregateByAbility(session, actor.actorId, "dealt");
    actor.abilitiesReceivedFrom = aggregateByAbility(session, actor.actorId, "taken");
  }

  return actors;
}

export function aggregateByAbility(
  session: Session,
  actorId: string,
  perspective: "dealt" | "taken" = "dealt"
): AbilityStats[] {
  const abilityMap = new Map<string, AbilityStats>();

  for (const event of session.events) {
    const eventActorId = perspective === "dealt" ? event.sourceActorId : event.targetActorId;
    if (eventActorId !== actorId) continue;
    if (!isDamageEvent(event) && !isHealEvent(event)) continue;

    const ability = ensureAbility(abilityMap, session.registries, event.abilityId);
    const missed = isDamageEvent(event) && event.data.outcome.result === "missed";
    const critical = isDamageEvent(event)
      ? event.data.outcome.critical === true
      : event.data.critical === true;

    if (missed) {
      ability.misses += 1;
      continue;
    }

    if (critical) ability.crits += 1;
    else ability.hits += 1;

    if (isDamageEvent(event)) ability.damage += event.data.amount;
    if (isHealEvent(event)) ability.healing += event.data.amount;
  }

  const abilities = Array.from(abilityMap.values());
  for (const ability of abilities) {
    const totalSuccessfulHits = ability.hits + ability.crits;
    const totalAttempts = totalSuccessfulHits + ability.misses;
    ability.avgDamage =
      totalSuccessfulHits > 0 ? Math.round(ability.damage / totalSuccessfulHits) : 0;
    ability.avgHealing =
      totalSuccessfulHits > 0 ? Math.round(ability.healing / totalSuccessfulHits) : 0;
    ability.critRate = calculatePercentage(ability.crits, totalSuccessfulHits);
    ability.missRate = calculatePercentage(ability.misses, totalAttempts);
  }

  return abilities;
}

function getDurationMs(session: Session): number {
  if (typeof session.durationMs === "number") return session.durationMs;
  if (typeof session.endedAtUtcMs === "number")
    return session.endedAtUtcMs - session.startedAtUtcMs;
  return 0;
}

function sumDamage(events: DamageEvent[]): number {
  return events.reduce((sum, event) => sum + event.data.amount, 0);
}

function sumHealing(events: HealEvent[]): number {
  return events.reduce((sum, event) => sum + event.data.amount, 0);
}

function ensureEventActor(actorMap: Map<string, ActorStats>, actor?: ActorRecord): void {
  if (!actor || actorMap.has(actor.id)) return;

  actorMap.set(actor.id, {
    actorId: actor.id,
    actorName: actor.name,
    actorType: actor.kind,
    actorClass: actor.class,
    totalDamage: 0,
    totalHealing: 0,
    dps: 0,
    hps: 0,
    percentOfTotalDamage: 0,
    percentOfTotalHealing: 0,
    damageTaken: 0,
    healingReceived: 0,
    dtps: 0,
    hrps: 0,
    percentOfTotalDamageTaken: 0,
    percentOfTotalHealingReceived: 0,
    totalMitigated: 0,
    mitigationRate: 0,
    totalMissedAgainst: 0,
    avoidanceRate: 0,
    abilityBreakdown: [],
    abilitiesReceivedFrom: [],
  });
}

function ensureAbility(
  abilityMap: Map<string, AbilityStats>,
  registries: Registries,
  abilityId?: string
): AbilityStats {
  const key = abilityId ?? "unknown";
  const existing = abilityMap.get(key);
  if (existing) return existing;

  const record = abilityId ? registries.abilities[abilityId] : undefined;
  const ability: AbilityStats = {
    abilityName: record?.name ?? "Unknown",
    abilityType: record?.kind ?? "unknown",
    damage: 0,
    healing: 0,
    hits: 0,
    crits: 0,
    misses: 0,
    avgDamage: 0,
    avgHealing: 0,
    critRate: 0,
    missRate: 0,
  };
  abilityMap.set(key, ability);
  return ability;
}
