// Event filtering utilities for bidirectional combat analytics
// Used to filter events by perspective (dealt vs taken) and faction

import type { CombatEvent } from "$lib/types";
import { getActorFaction } from "./actor-utils";
import { isDamageEventType, isHealEventType } from "./event-constants";

/**
 * Check if an event type represents damage.
 */
export function isDamageEvent(event: CombatEvent): boolean {
  return isDamageEventType(event.eventType);
}

/**
 * Check if an event type represents healing.
 */
export function isHealEvent(event: CombatEvent): boolean {
  return isHealEventType(event.eventType);
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
    isDamageEvent(event) &&
    getActorFaction(event.source) === "friendly" &&
    getActorFaction(event.target) === "hostile"
  );
}

/**
 * Check if event represents damage taken BY player faction FROM enemies.
 */
export function isDamageTakenByPlayer(event: CombatEvent): boolean {
  return (
    isDamageEvent(event) &&
    getActorFaction(event.source) === "hostile" &&
    getActorFaction(event.target) === "friendly"
  );
}

/**
 * Check if event represents healing done BY player faction.
 */
export function isHealingDoneByPlayer(event: CombatEvent): boolean {
  return isHealEvent(event) && getActorFaction(event.source) === "friendly";
}

/**
 * Check if event represents healing received BY player faction.
 */
export function isHealingReceivedByPlayer(event: CombatEvent): boolean {
  return isHealEvent(event) && getActorFaction(event.target) === "friendly";
}
