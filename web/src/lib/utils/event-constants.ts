// Shared event type classification constants
// Used by event-filters.ts and combat-analyzer.ts

import type { EventType } from "$lib/types";

/**
 * Set of all damage event types.
 * Used to identify damage-dealing events in combat analytics.
 */
export const DAMAGE_EVENTS: ReadonlySet<EventType> = new Set([
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

/**
 * Set of all healing event types.
 * Used to identify healing events in combat analytics.
 */
export const HEAL_EVENTS: ReadonlySet<EventType> = new Set([
  "healSpell",
  "healHot",
  "healLifesteal",
  "healRegen",
]);

/**
 * Check if an event type represents damage.
 */
export function isDamageEventType(eventType: EventType): boolean {
  return DAMAGE_EVENTS.has(eventType);
}

/**
 * Check if an event type represents healing.
 */
export function isHealEventType(eventType: EventType): boolean {
  return HEAL_EVENTS.has(eventType);
}
