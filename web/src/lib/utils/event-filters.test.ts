import { describe, it, expect } from "vitest";
import {
  isDamageEvent,
  isHealEvent,
  isPlayerPerspectiveEvent,
  isDamageDealtByPlayer,
  isDamageTakenByPlayer,
  isHealingDoneByPlayer,
  isHealingReceivedByPlayer,
} from "./event-filters";
import { createCombatEvent } from "$lib/testing";

describe("event-filters", () => {
  describe("isDamageEvent", () => {
    it("returns true for damage event types", () => {
      const event = createCombatEvent({ eventType: "damagePhysical" });
      expect(isDamageEvent(event)).toBe(true);
    });

    it("returns true for magic damage", () => {
      const event = createCombatEvent({ eventType: "damageMagic" });
      expect(isDamageEvent(event)).toBe(true);
    });

    it("returns false for heal event types", () => {
      const event = createCombatEvent({ eventType: "healSpell" });
      expect(isDamageEvent(event)).toBe(false);
    });
  });

  describe("isHealEvent", () => {
    it("returns true for heal event types", () => {
      const event = createCombatEvent({ eventType: "healSpell" });
      expect(isHealEvent(event)).toBe(true);
    });

    it("returns false for damage event types", () => {
      const event = createCombatEvent({ eventType: "damagePhysical" });
      expect(isHealEvent(event)).toBe(false);
    });
  });

  describe("isPlayerPerspectiveEvent", () => {
    it("returns true when source is player faction", () => {
      const event = createCombatEvent({
        source: { id: "1", name: "Player", type: "player" },
        target: { id: "2", name: "Enemy", type: "npc" },
      });
      expect(isPlayerPerspectiveEvent(event)).toBe(true);
    });

    it("returns true when target is player faction", () => {
      const event = createCombatEvent({
        source: { id: "1", name: "Enemy", type: "npc" },
        target: { id: "2", name: "Player", type: "player" },
      });
      expect(isPlayerPerspectiveEvent(event)).toBe(true);
    });

    it("returns false when neither source nor target is player faction", () => {
      const event = createCombatEvent({
        source: { id: "1", name: "Enemy1", type: "npc" },
        target: { id: "2", name: "Enemy2", type: "npc" },
      });
      expect(isPlayerPerspectiveEvent(event)).toBe(false);
    });
  });

  describe("isDamageDealtByPlayer", () => {
    it("returns true for player damage to NPC", () => {
      const event = createCombatEvent({
        eventType: "damagePhysical",
        source: { id: "1", name: "Player", type: "player" },
        target: { id: "2", name: "Enemy", type: "npc" },
      });
      expect(isDamageDealtByPlayer(event)).toBe(true);
    });

    it("returns false for NPC damage to player", () => {
      const event = createCombatEvent({
        eventType: "damagePhysical",
        source: { id: "1", name: "Enemy", type: "npc" },
        target: { id: "2", name: "Player", type: "player" },
      });
      expect(isDamageDealtByPlayer(event)).toBe(false);
    });

    it("returns false for player damage to player", () => {
      const event = createCombatEvent({
        eventType: "damagePhysical",
        source: { id: "1", name: "Player1", type: "player" },
        target: { id: "2", name: "Player2", type: "player" },
      });
      expect(isDamageDealtByPlayer(event)).toBe(false);
    });

    it("returns false for heal event", () => {
      const event = createCombatEvent({
        eventType: "healSpell",
        source: { id: "1", name: "Player", type: "player" },
        target: { id: "2", name: "Enemy", type: "npc" },
      });
      expect(isDamageDealtByPlayer(event)).toBe(false);
    });
  });

  describe("isDamageTakenByPlayer", () => {
    it("returns true for NPC damage to player", () => {
      const event = createCombatEvent({
        eventType: "damagePhysical",
        source: { id: "1", name: "Enemy", type: "npc" },
        target: { id: "2", name: "Player", type: "player" },
      });
      expect(isDamageTakenByPlayer(event)).toBe(true);
    });

    it("returns false for player damage to NPC", () => {
      const event = createCombatEvent({
        eventType: "damagePhysical",
        source: { id: "1", name: "Player", type: "player" },
        target: { id: "2", name: "Enemy", type: "npc" },
      });
      expect(isDamageTakenByPlayer(event)).toBe(false);
    });

    it("returns false for heal event", () => {
      const event = createCombatEvent({
        eventType: "healSpell",
        source: { id: "1", name: "Enemy", type: "npc" },
        target: { id: "2", name: "Player", type: "player" },
      });
      expect(isDamageTakenByPlayer(event)).toBe(false);
    });
  });

  describe("isHealingDoneByPlayer", () => {
    it("returns true for player healing", () => {
      const event = createCombatEvent({
        eventType: "healSpell",
        source: { id: "1", name: "Player", type: "player" },
        target: { id: "2", name: "Player2", type: "player" },
      });
      expect(isHealingDoneByPlayer(event)).toBe(true);
    });

    it("returns false for NPC healing", () => {
      const event = createCombatEvent({
        eventType: "healSpell",
        source: { id: "1", name: "Enemy", type: "npc" },
        target: { id: "2", name: "Player", type: "player" },
      });
      expect(isHealingDoneByPlayer(event)).toBe(false);
    });

    it("returns false for damage event", () => {
      const event = createCombatEvent({
        eventType: "damagePhysical",
        source: { id: "1", name: "Player", type: "player" },
        target: { id: "2", name: "Enemy", type: "npc" },
      });
      expect(isHealingDoneByPlayer(event)).toBe(false);
    });
  });

  describe("isHealingReceivedByPlayer", () => {
    it("returns true for healing received by player", () => {
      const event = createCombatEvent({
        eventType: "healSpell",
        source: { id: "1", name: "Player", type: "player" },
        target: { id: "2", name: "Player2", type: "player" },
      });
      expect(isHealingReceivedByPlayer(event)).toBe(true);
    });

    it("returns false for healing received by NPC", () => {
      const event = createCombatEvent({
        eventType: "healSpell",
        source: { id: "1", name: "Player", type: "player" },
        target: { id: "2", name: "Enemy", type: "npc" },
      });
      expect(isHealingReceivedByPlayer(event)).toBe(false);
    });

    it("returns false for damage event", () => {
      const event = createCombatEvent({
        eventType: "damagePhysical",
        source: { id: "1", name: "Enemy", type: "npc" },
        target: { id: "2", name: "Player", type: "player" },
      });
      expect(isHealingReceivedByPlayer(event)).toBe(false);
    });
  });
});
