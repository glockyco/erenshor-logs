import { describe, it, expect } from "vitest";
import { isPlayerFaction, isEnemyFaction, getActorFaction, filterByFaction } from "./actor-utils";
import type { ActorRef, ActorType } from "$lib/types";

describe("actor-utils", () => {
  describe("isPlayerFaction", () => {
    it("returns true for player type", () => {
      expect(isPlayerFaction("player")).toBe(true);
    });

    it("returns true for simPlayer type", () => {
      expect(isPlayerFaction("simPlayer")).toBe(true);
    });

    it("returns true for pet type", () => {
      expect(isPlayerFaction("pet")).toBe(true);
    });

    it("returns false for npc type", () => {
      expect(isPlayerFaction("npc")).toBe(false);
    });
  });

  describe("isEnemyFaction", () => {
    it("returns true for npc type", () => {
      expect(isEnemyFaction("npc")).toBe(true);
    });

    it("returns false for player type", () => {
      expect(isEnemyFaction("player")).toBe(false);
    });

    it("returns false for simPlayer type", () => {
      expect(isEnemyFaction("simPlayer")).toBe(false);
    });

    it("returns false for pet type", () => {
      expect(isEnemyFaction("pet")).toBe(false);
    });
  });

  describe("getActorFaction", () => {
    it('returns "friendly" for player actor', () => {
      const actor: ActorRef = { id: "1", name: "Player", type: "player" };
      expect(getActorFaction(actor)).toBe("friendly");
    });

    it('returns "friendly" for simPlayer actor', () => {
      const actor: ActorRef = { id: "2", name: "SimPlayer", type: "simPlayer" };
      expect(getActorFaction(actor)).toBe("friendly");
    });

    it('returns "friendly" for pet actor', () => {
      const actor: ActorRef = { id: "3", name: "Pet", type: "pet" };
      expect(getActorFaction(actor)).toBe("friendly");
    });

    it('returns "hostile" for npc actor', () => {
      const actor: ActorRef = { id: "4", name: "Enemy", type: "npc" };
      expect(getActorFaction(actor)).toBe("hostile");
    });

    it("returns null for undefined actor", () => {
      expect(getActorFaction(undefined)).toBeNull();
    });
  });

  describe("filterByFaction", () => {
    const actors = [
      { actorType: "player" as ActorType, name: "Player1" },
      { actorType: "simPlayer" as ActorType, name: "SimPlayer1" },
      { actorType: "pet" as ActorType, name: "Pet1" },
      { actorType: "npc" as ActorType, name: "Enemy1" },
      { actorType: "npc" as ActorType, name: "Enemy2" },
    ];

    it('returns all actors when filter is "all"', () => {
      const result = filterByFaction(actors, "all");
      expect(result).toHaveLength(5);
      expect(result).toEqual(actors);
    });

    it('returns only friendly actors when filter is "friendly"', () => {
      const result = filterByFaction(actors, "friendly");
      expect(result).toHaveLength(3);
      expect(result.map((a) => a.actorType)).toEqual(["player", "simPlayer", "pet"]);
    });

    it('returns only hostile actors when filter is "hostile"', () => {
      const result = filterByFaction(actors, "hostile");
      expect(result).toHaveLength(2);
      expect(result.map((a) => a.actorType)).toEqual(["npc", "npc"]);
    });

    it("returns empty array when no actors match filter", () => {
      const onlyPlayers = [{ actorType: "player" as ActorType, name: "Player1" }];
      const result = filterByFaction(onlyPlayers, "hostile");
      expect(result).toHaveLength(0);
    });
  });
});
