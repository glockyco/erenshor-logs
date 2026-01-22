// Event filtering utilities for bidirectional combat analytics
// Used to filter events by perspective (dealt vs taken) and faction

import type { CombatEvent, EventType } from "$lib/types";
import { getActorFaction } from "./actor-utils";

// Event type classification
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

/**
 * Check if an event type represents damage.
 */
export function isDamageEvent(eventType: EventType): boolean {
  return DAMAGE_EVENTS.has(eventType);
}

/**
 * Check if an event type represents healing.
 */
export function isHealEvent(eventType: EventType): boolean {
  return HEAL_EVENTS.has(eventType);
}

/**
 * Check if event involves player faction (as source OR target).
 * Used to filter out irrelevant NPC-to-NPC events in aggregate views.
 */
export function isPlayerPerspectiveEvent(event: CombatEvent): boolean {
  const sourceFaction = getActorFaction(event.source);
  const targetFaction = getActorFaction(event.target);
  return sourceFaction === "friendly" || targetFaction === "friendly";
}

/**
 * Check if event represents damage dealt BY player faction TO enemies.
 */
export function isDamageDealtByPlayer(event: CombatEvent): boolean {
  return (
    isDamageEvent(event.eventType) &&
    getActorFaction(event.source) === "friendly" &&
    getActorFaction(event.target) === "hostile"
  );
}

/**
 * Check if event represents damage taken BY player faction FROM enemies.
 */
export function isDamageTakenByPlayer(event: CombatEvent): boolean {
  return (
    isDamageEvent(event.eventType) &&
    getActorFaction(event.source) === "hostile" &&
    getActorFaction(event.target) === "friendly"
  );
}

/**
 * Check if event represents healing done BY player faction.
 */
export function isHealingDoneByPlayer(event: CombatEvent): boolean {
  return isHealEvent(event.eventType) && getActorFaction(event.source) === "friendly";
}

/**
 * Check if event represents healing received BY player faction.
 */
export function isHealingReceivedByPlayer(event: CombatEvent): boolean {
  return isHealEvent(event.eventType) && getActorFaction(event.target) === "friendly";
}
