import { describe, expect, it } from "vitest";
import { calculateSessionStats } from "./combat-analyzer";
import {
  createDamageEvent,
  createBuffEvent,
  createHealEvent,
  createMechanicEvent,
  createDeathEvent,
  createInterruptEvent,
  createResourceEvent,
  createSession,
} from "$lib/testing";

function sessionWith(events = [] as ReturnType<typeof createDamageEvent>[]) {
  return createSession({
    durationMs: 1000,
    lastEventSeq: events.length,
    eventCount: events.length,
    events,
  });
}

describe("calculateSessionStats", () => {
  it("counts friendly damage to hostile targets as outgoing damage", () => {
    const session = sessionWith([
      createDamageEvent({
        data: { amount: 100, damageType: "physical", outcome: { result: "landed" } },
      }),
    ]);

    const stats = calculateSessionStats(session, 1000);

    expect(stats.totalDamage).toBe(100);
    expect(stats.dps).toBeCloseTo(100);
  });

  it("ignores hostile damage to hostile targets for player totals", () => {
    const session = sessionWith([
      createDamageEvent({ sourceActorId: "npc-1", targetActorId: "npc-1" }),
    ]);

    const stats = calculateSessionStats(session, 1000);

    expect(stats.totalDamage).toBe(0);
    expect(stats.totalDamageTaken).toBe(0);
  });

  it("counts hostile damage to friendly targets as incoming damage", () => {
    const session = sessionWith([
      createDamageEvent({ sourceActorId: "npc-1", targetActorId: "player-1" }),
    ]);

    const stats = calculateSessionStats(session, 1000);

    expect(stats.totalDamageTaken).toBe(1000);
    expect(stats.dtps).toBeCloseTo(1000);
  });

  it("counts friendly healing as outgoing and received healing", () => {
    const session = createSession({
      durationMs: 1000,
      lastEventSeq: 1,
      eventCount: 1,
      events: [createHealEvent({ data: { amount: 250 } })],
    });

    const stats = calculateSessionStats(session, 1000);

    expect(stats.totalHealing).toBe(250);
    expect(stats.totalHealingReceived).toBe(250);
    expect(stats.hps).toBeCloseTo(250);
  });

  it("counts healing without counting resource or mechanics as damage", () => {
    const session = createSession({
      durationMs: 1000,
      lastEventSeq: 7,
      eventCount: 7,
      events: [
        createDamageEvent({
          eventSeq: 1,
          data: { amount: 1000, damageType: "physical", outcome: { result: "landed" } },
        }),
        createHealEvent({
          eventSeq: 2,
          data: { amount: 250 },
          sourceActorId: "sim-1",
          targetActorId: "player-1",
        }),
        createResourceEvent({ eventSeq: 3 }),
        createBuffEvent({ eventSeq: 4 }),
        createDeathEvent({ eventSeq: 5 }),
        createInterruptEvent({ eventSeq: 6 }),
        createMechanicEvent({ eventSeq: 7 }),
      ],
    });

    const stats = calculateSessionStats(session, 1000);

    expect(stats.totalDamage).toBe(1000);
    expect(stats.totalHealing).toBe(250);
    expect(stats.eventCounts.resource).toBe(1);
    expect(stats.eventCounts.mechanic).toBe(1);
    expect(stats.eventCounts.effect).toBe(1);
    expect(stats.eventCounts.death).toBe(1);
    expect(stats.eventCounts.interrupt).toBe(1);
  });

  it("uses mitigation fields from v2 damage data", () => {
    const session = sessionWith([
      createDamageEvent({
        sourceActorId: "npc-1",
        targetActorId: "player-1",
        data: {
          amount: 70,
          rawAmount: 100,
          mitigatedAmount: 30,
          damageType: "physical",
          outcome: { result: "landed" },
        },
      }),
    ]);

    const stats = calculateSessionStats(session, 1000);

    expect(stats.totalDamageTaken).toBe(70);
    expect(stats.totalMitigated).toBe(30);
    expect(stats.mitigationRate).toBe(30);
  });

  it("builds actor and ability breakdowns from registries", () => {
    const session = sessionWith([
      createDamageEvent({
        data: { amount: 100, damageType: "physical", outcome: { result: "landed" } },
      }),
    ]);

    const stats = calculateSessionStats(session, 1000);

    const player = stats.actorBreakdown.find((actor) => actor.actorId === "player-1");
    expect(player?.actorName).toBe("Player");
    expect(player?.actorType).toBe("player");
    expect(player?.abilityBreakdown[0]?.abilityName).toBe("Backstab");
  });
});
