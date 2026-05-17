import { describe, expect, it } from "vitest";
import { filterByFaction, getActorFaction, isEnemyFaction, isPlayerFaction } from "./actor-utils";
import type { ActorRecord, ActorStats } from "$lib/types";

describe("actor-utils", () => {
  it("classifies player-side actor kinds as friendly", () => {
    expect(isPlayerFaction("player")).toBe(true);
    expect(isPlayerFaction("simPlayer")).toBe(true);
    expect(isPlayerFaction("pet")).toBe(true);
    expect(isPlayerFaction("npc")).toBe(false);
  });

  it("classifies npc actors as enemies", () => {
    expect(isEnemyFaction("npc")).toBe(true);
    expect(isEnemyFaction("player")).toBe(false);
  });

  it("prefers explicit registry faction when present", () => {
    const actor: ActorRecord = {
      id: "friendly-npc",
      name: "Guard",
      kind: "npc",
      faction: "friendly",
    };

    expect(getActorFaction(actor)).toBe("friendly");
  });

  it("returns null for missing, neutral, or unknown actors", () => {
    expect(getActorFaction(undefined)).toBeNull();
    expect(getActorFaction({ kind: "unknown" })).toBeNull();
    expect(getActorFaction({ kind: "npc", faction: "neutral" })).toBeNull();
  });

  it("filters actor stats by faction", () => {
    const actors = [
      { actorType: "player", actorName: "Player" },
      { actorType: "simPlayer", actorName: "Sim" },
      { actorType: "pet", actorName: "Pet" },
      { actorType: "npc", actorName: "Enemy" },
    ] as ActorStats[];

    expect(filterByFaction(actors, "all")).toEqual(actors);
    expect(filterByFaction(actors, "friendly").map((actor) => actor.actorType)).toEqual([
      "player",
      "simPlayer",
      "pet",
    ]);
    expect(filterByFaction(actors, "hostile").map((actor) => actor.actorType)).toEqual(["npc"]);
  });
});
