import { describe, expect, it } from "vitest";
import {
  isDamageDealtByPlayer,
  isDamageEvent,
  isDamageTakenByPlayer,
  isHealEvent,
  isHealingDoneByPlayer,
  isHealingReceivedByPlayer,
  isPlayerPerspectiveEvent,
} from "./event-filters";
import { createDamageEvent, createHealEvent, createRegistries } from "$lib/testing";

describe("event-filters", () => {
  const registries = createRegistries();

  it("classifies damage and heal records by kind", () => {
    expect(isDamageEvent(createDamageEvent())).toBe(true);
    expect(isDamageEvent(createHealEvent())).toBe(false);
    expect(isHealEvent(createHealEvent())).toBe(true);
    expect(isHealEvent(createDamageEvent())).toBe(false);
  });

  it("detects events involving the player faction through registries", () => {
    expect(isPlayerPerspectiveEvent(createDamageEvent(), registries)).toBe(true);
    expect(
      isPlayerPerspectiveEvent(
        createDamageEvent({ sourceActorId: "npc-1", targetActorId: "npc-1" }),
        registries
      )
    ).toBe(false);
  });

  it("detects outgoing friendly damage to hostile targets", () => {
    expect(isDamageDealtByPlayer(createDamageEvent(), registries)).toBe(true);
    expect(
      isDamageDealtByPlayer(
        createDamageEvent({ sourceActorId: "npc-1", targetActorId: "player-1" }),
        registries
      )
    ).toBe(false);
  });

  it("detects incoming hostile damage to friendly targets", () => {
    expect(
      isDamageTakenByPlayer(
        createDamageEvent({ sourceActorId: "npc-1", targetActorId: "player-1" }),
        registries
      )
    ).toBe(true);
    expect(isDamageTakenByPlayer(createDamageEvent(), registries)).toBe(false);
  });

  it("detects friendly healing and friendly healing received", () => {
    const heal = createHealEvent();

    expect(isHealingDoneByPlayer(heal, registries)).toBe(true);
    expect(isHealingReceivedByPlayer(heal, registries)).toBe(true);
    expect(isHealingDoneByPlayer(createHealEvent({ sourceActorId: "npc-1" }), registries)).toBe(
      false
    );
  });
});
