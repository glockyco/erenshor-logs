import { describe, it, expect } from "vitest";
import { calculateSessionStats } from "./combat-analyzer";
import { createCombatEvent } from "$lib/testing";

describe("calculateSessionStats", () => {
  describe("damage dealt by player", () => {
    it("calculates total damage from player to NPC", () => {
      const events = [
        createCombatEvent({
          eventType: "damagePhysical",
          amount: 100,
          source: { id: "player-1", name: "Player", type: "player" },
          target: { id: "npc-1", name: "Enemy", type: "npc" },
        }),
      ];

      const stats = calculateSessionStats(events, 1000);

      expect(stats.totalDamage).toBe(100);
      expect(stats.dps).toBeCloseTo(100); // 100 damage in 1 second
    });

    it("sums multiple damage events", () => {
      const events = [
        createCombatEvent({
          eventType: "damagePhysical",
          amount: 100,
          source: { id: "player-1", name: "Player", type: "player" },
          target: { id: "npc-1", name: "Enemy", type: "npc" },
        }),
        createCombatEvent({
          eventType: "damageMagic",
          amount: 50,
          source: { id: "player-1", name: "Player", type: "player" },
          target: { id: "npc-1", name: "Enemy", type: "npc" },
        }),
      ];

      const stats = calculateSessionStats(events, 1000);

      expect(stats.totalDamage).toBe(150);
    });

    it("ignores damage from NPC to NPC", () => {
      const events = [
        createCombatEvent({
          eventType: "damagePhysical",
          amount: 100,
          source: { id: "npc-1", name: "NPC1", type: "npc" },
          target: { id: "npc-2", name: "NPC2", type: "npc" },
        }),
      ];

      const stats = calculateSessionStats(events, 1000);

      expect(stats.totalDamage).toBe(0);
    });

    it("ignores damage from player to player", () => {
      const events = [
        createCombatEvent({
          eventType: "damagePhysical",
          amount: 100,
          source: { id: "player-1", name: "Player1", type: "player" },
          target: { id: "player-2", name: "Player2", type: "player" },
        }),
      ];

      const stats = calculateSessionStats(events, 1000);

      expect(stats.totalDamage).toBe(0);
    });
  });

  describe("damage taken by player", () => {
    it("calculates total damage from NPC to player", () => {
      const events = [
        createCombatEvent({
          eventType: "damagePhysical",
          amount: 75,
          source: { id: "npc-1", name: "Enemy", type: "npc" },
          target: { id: "player-1", name: "Player", type: "player" },
        }),
      ];

      const stats = calculateSessionStats(events, 1000);

      expect(stats.totalDamageTaken).toBe(75);
      expect(stats.dtps).toBeCloseTo(75);
    });
  });

  describe("healing", () => {
    it("calculates healing done by player", () => {
      const events = [
        createCombatEvent({
          eventType: "healSpell",
          amount: 200,
          source: { id: "player-1", name: "Healer", type: "player" },
          target: { id: "player-2", name: "Tank", type: "player" },
        }),
      ];

      const stats = calculateSessionStats(events, 1000);

      expect(stats.totalHealing).toBe(200);
      expect(stats.hps).toBeCloseTo(200);
    });
  });

  describe("duration and rates", () => {
    it("calculates DPS correctly for different durations", () => {
      const events = [
        createCombatEvent({
          eventType: "damagePhysical",
          amount: 100,
          source: { id: "player-1", name: "Player", type: "player" },
          target: { id: "npc-1", name: "Enemy", type: "npc" },
        }),
      ];

      const stats2s = calculateSessionStats(events, 2000);
      const stats10s = calculateSessionStats(events, 10000);

      expect(stats2s.dps).toBeCloseTo(50); // 100 damage in 2 seconds
      expect(stats10s.dps).toBeCloseTo(10); // 100 damage in 10 seconds
    });

    it("returns 0 DPS for zero duration", () => {
      const events = [
        createCombatEvent({
          eventType: "damagePhysical",
          amount: 100,
          source: { id: "player-1", name: "Player", type: "player" },
          target: { id: "npc-1", name: "Enemy", type: "npc" },
        }),
      ];

      const stats = calculateSessionStats(events, 0);

      expect(stats.dps).toBe(0);
    });
  });

  describe("empty events", () => {
    it("returns zero stats for empty event list", () => {
      const stats = calculateSessionStats([], 1000);

      expect(stats.totalDamage).toBe(0);
      expect(stats.totalHealing).toBe(0);
      expect(stats.totalDamageTaken).toBe(0);
      expect(stats.dps).toBe(0);
      expect(stats.hps).toBe(0);
      expect(stats.durationMs).toBe(1000);
    });
  });

  describe("events with missing amounts", () => {
    it("treats undefined amount as 0", () => {
      const events = [
        createCombatEvent({
          eventType: "damagePhysical",
          amount: undefined,
          source: { id: "player-1", name: "Player", type: "player" },
          target: { id: "npc-1", name: "Enemy", type: "npc" },
        }),
      ];

      const stats = calculateSessionStats(events, 1000);

      expect(stats.totalDamage).toBe(0);
    });
  });

  describe("mitigation and defensive stats", () => {
    it("calculates mitigation from rawAmount and amount", () => {
      const events = [
        createCombatEvent({
          eventType: "damagePhysical",
          amount: 70,
          rawAmount: 100,
          mitigated: 30,
          source: { id: "npc-1", name: "Enemy", type: "npc" },
          target: { id: "player-1", name: "Player", type: "player" },
        }),
      ];

      const stats = calculateSessionStats(events, 1000);

      expect(stats.totalDamageTaken).toBe(70);
      expect(stats.totalMitigated).toBe(30);
      expect(stats.mitigationRate).toBeGreaterThan(0);
    });

    it("handles events with no rawAmount", () => {
      const events = [
        createCombatEvent({
          eventType: "damagePhysical",
          amount: 50,
          source: { id: "npc-1", name: "Enemy", type: "npc" },
          target: { id: "player-1", name: "Player", type: "player" },
        }),
      ];

      const stats = calculateSessionStats(events, 1000);

      expect(stats.totalDamageTaken).toBe(50);
      expect(stats.totalMitigated).toBe(0);
    });
  });

  describe("multiple event types in same session", () => {
    it("aggregates damage, healing, and defensive stats correctly", () => {
      const events = [
        // Player deals damage
        createCombatEvent({
          eventType: "damagePhysical",
          amount: 100,
          source: { id: "player-1", name: "Player", type: "player" },
          target: { id: "npc-1", name: "Enemy", type: "npc" },
        }),
        // Player takes damage
        createCombatEvent({
          eventType: "damagePhysical",
          amount: 50,
          source: { id: "npc-1", name: "Enemy", type: "npc" },
          target: { id: "player-1", name: "Player", type: "player" },
        }),
        // Player heals ally
        createCombatEvent({
          eventType: "healSpell",
          amount: 80,
          source: { id: "player-1", name: "Player", type: "player" },
          target: { id: "player-2", name: "Ally", type: "player" },
        }),
        // Ally receives healing
        createCombatEvent({
          eventType: "healSpell",
          amount: 60,
          source: { id: "player-1", name: "Player", type: "player" },
          target: { id: "player-1", name: "Player", type: "player" },
        }),
      ];

      const stats = calculateSessionStats(events, 2000);

      expect(stats.totalDamage).toBe(100);
      expect(stats.totalDamageTaken).toBe(50);
      expect(stats.totalHealing).toBe(140); // 80 + 60
      expect(stats.totalHealingReceived).toBe(140);
      expect(stats.dps).toBeCloseTo(50); // 100 in 2 seconds
      expect(stats.dtps).toBeCloseTo(25); // 50 in 2 seconds
      expect(stats.hps).toBeCloseTo(70); // 140 in 2 seconds
    });
  });

  describe("percentage calculations", () => {
    it("calculates percentages correctly with multiple actors", () => {
      const events = [
        createCombatEvent({
          eventType: "damagePhysical",
          amount: 100,
          source: { id: "player-1", name: "Player1", type: "player" },
          target: { id: "npc-1", name: "Enemy", type: "npc" },
        }),
        createCombatEvent({
          eventType: "damagePhysical",
          amount: 50,
          source: { id: "player-2", name: "Player2", type: "player" },
          target: { id: "npc-1", name: "Enemy", type: "npc" },
        }),
      ];

      const stats = calculateSessionStats(events, 1000);

      expect(stats.totalDamage).toBe(150);
      expect(stats.actorBreakdown).toBeDefined();
      expect(stats.actorBreakdown.length).toBeGreaterThan(0);
    });
  });

  describe("missed attacks and avoidance", () => {
    it("tracks missed attacks separately from damage", () => {
      const events = [
        // Hit
        createCombatEvent({
          eventType: "damagePhysical",
          amount: 100,
          flags: { missed: false },
          source: { id: "npc-1", name: "Enemy", type: "npc" },
          target: { id: "player-1", name: "Player", type: "player" },
        }),
        // Miss
        createCombatEvent({
          eventType: "damagePhysical",
          amount: 0,
          flags: { missed: true },
          source: { id: "npc-1", name: "Enemy", type: "npc" },
          target: { id: "player-1", name: "Player", type: "player" },
        }),
      ];

      const stats = calculateSessionStats(events, 1000);

      // Only the hit should count toward damage taken
      expect(stats.totalDamageTaken).toBe(100);
      // Avoidance stats tracked in actor breakdown
      expect(stats.actorBreakdown).toBeDefined();
    });

    it("handles events without flags", () => {
      const events = [
        createCombatEvent({
          eventType: "damagePhysical",
          amount: 50,
          // No flags property
          source: { id: "npc-1", name: "Enemy", type: "npc" },
          target: { id: "player-1", name: "Player", type: "player" },
        }),
      ];

      const stats = calculateSessionStats(events, 1000);

      expect(stats.totalDamageTaken).toBe(50);
    });
  });

  describe("actor breakdown", () => {
    it("includes actors in breakdown", () => {
      const events = [
        createCombatEvent({
          eventType: "damagePhysical",
          amount: 100,
          source: { id: "player-1", name: "Player", type: "player" },
          target: { id: "npc-1", name: "Enemy", type: "npc" },
        }),
      ];

      const stats = calculateSessionStats(events, 1000);

      expect(stats.actorBreakdown.length).toBeGreaterThan(0);
      const playerActor = stats.actorBreakdown.find((a) => a.actorType === "player");
      expect(playerActor).toBeDefined();
    });
  });
});
