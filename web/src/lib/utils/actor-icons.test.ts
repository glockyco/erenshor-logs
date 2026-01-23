import { describe, it, expect } from "vitest";
import { getActorIcon } from "./actor-icons";
import {
  WandSparkles,
  Swords,
  Leaf,
  Shield,
  BowArrow,
  Axe,
  User,
  Skull,
  Cat,
} from "@lucide/svelte";

describe("getActorIcon", () => {
  describe("Player class icons", () => {
    it("returns WandSparkles for Arcanist", () => {
      expect(getActorIcon("player", "Arcanist")).toBe(WandSparkles);
    });

    it("returns Swords for Duelist", () => {
      expect(getActorIcon("player", "Duelist")).toBe(Swords);
    });

    it("returns Leaf for Druid", () => {
      expect(getActorIcon("player", "Druid")).toBe(Leaf);
    });

    it("returns Shield for Paladin", () => {
      expect(getActorIcon("player", "Paladin")).toBe(Shield);
    });

    it("returns BowArrow for Stormcaller", () => {
      expect(getActorIcon("player", "Stormcaller")).toBe(BowArrow);
    });

    it("returns Axe for Reaver", () => {
      expect(getActorIcon("player", "Reaver")).toBe(Axe);
    });
  });

  describe("SimPlayer class icons", () => {
    it("returns WandSparkles for simPlayer Arcanist", () => {
      expect(getActorIcon("simPlayer", "Arcanist")).toBe(WandSparkles);
    });

    it("returns Swords for simPlayer Duelist", () => {
      expect(getActorIcon("simPlayer", "Duelist")).toBe(Swords);
    });

    it("returns Leaf for simPlayer Druid", () => {
      expect(getActorIcon("simPlayer", "Druid")).toBe(Leaf);
    });

    it("returns Shield for simPlayer Paladin", () => {
      expect(getActorIcon("simPlayer", "Paladin")).toBe(Shield);
    });

    it("returns BowArrow for simPlayer Stormcaller", () => {
      expect(getActorIcon("simPlayer", "Stormcaller")).toBe(BowArrow);
    });

    it("returns Axe for simPlayer Reaver", () => {
      expect(getActorIcon("simPlayer", "Reaver")).toBe(Axe);
    });
  });

  describe("Fallback icons for players", () => {
    it("returns User for player with no class", () => {
      expect(getActorIcon("player")).toBe(User);
    });

    it("returns User for simPlayer with no class", () => {
      expect(getActorIcon("simPlayer")).toBe(User);
    });

    it("returns User for player with unknown class", () => {
      expect(getActorIcon("player", "UnknownClass")).toBe(User);
    });

    it("returns User for simPlayer with unknown class", () => {
      expect(getActorIcon("simPlayer", "UnknownClass")).toBe(User);
    });

    it("returns User for player with empty string class", () => {
      expect(getActorIcon("player", "")).toBe(User);
    });
  });

  describe("NPC and Pet icons", () => {
    it("returns Skull for npc", () => {
      expect(getActorIcon("npc")).toBe(Skull);
    });

    it("returns Skull for npc even with class specified", () => {
      expect(getActorIcon("npc", "SomeClass")).toBe(Skull);
    });

    it("returns Cat for pet", () => {
      expect(getActorIcon("pet")).toBe(Cat);
    });

    it("returns Cat for pet even with class specified", () => {
      expect(getActorIcon("pet", "SomeClass")).toBe(Cat);
    });
  });

  describe("Case sensitivity", () => {
    it("requires exact case match for class names", () => {
      // Lowercase should not match
      expect(getActorIcon("player", "arcanist")).toBe(User);
      expect(getActorIcon("player", "duelist")).toBe(User);

      // Uppercase should not match
      expect(getActorIcon("player", "ARCANIST")).toBe(User);
      expect(getActorIcon("player", "DUELIST")).toBe(User);

      // Only exact case matches
      expect(getActorIcon("player", "Arcanist")).toBe(WandSparkles);
      expect(getActorIcon("player", "Duelist")).toBe(Swords);
    });
  });

  describe("Edge cases", () => {
    it("handles undefined class gracefully", () => {
      expect(getActorIcon("player", undefined)).toBe(User);
      expect(getActorIcon("simPlayer", undefined)).toBe(User);
    });

    it("handles whitespace in class names", () => {
      expect(getActorIcon("player", " Arcanist ")).toBe(User); // No trim
      expect(getActorIcon("player", "Arcanist ")).toBe(User); // Trailing space
      expect(getActorIcon("player", " Arcanist")).toBe(User); // Leading space
    });

    it("returns icon consistently for same inputs", () => {
      const icon1 = getActorIcon("player", "Arcanist");
      const icon2 = getActorIcon("player", "Arcanist");
      expect(icon1).toBe(icon2);
      expect(icon1).toBe(WandSparkles);
    });
  });
});
