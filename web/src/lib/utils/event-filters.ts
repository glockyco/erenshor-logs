import type { CombatEventRecord, DamageEvent, HealEvent, Registries } from "$lib/types";
import { getActorFaction } from "./actor-utils";
import { isDamageEventKind, isHealEventKind } from "./event-constants";

export function isDamageEvent(event: CombatEventRecord): event is DamageEvent {
  return isDamageEventKind(event.kind);
}

export function isHealEvent(event: CombatEventRecord): event is HealEvent {
  return isHealEventKind(event.kind);
}

export function getEventSource(event: CombatEventRecord, registries: Registries) {
  return event.sourceActorId ? registries.actors[event.sourceActorId] : undefined;
}

export function getEventTarget(event: CombatEventRecord, registries: Registries) {
  return event.targetActorId ? registries.actors[event.targetActorId] : undefined;
}

export function isPlayerPerspectiveEvent(
  event: CombatEventRecord,
  registries: Registries
): boolean {
  const sourceFaction = getActorFaction(getEventSource(event, registries));
  const targetFaction = getActorFaction(getEventTarget(event, registries));
  return sourceFaction === "friendly" || targetFaction === "friendly";
}

export function isDamageDealtByPlayer(event: CombatEventRecord, registries: Registries): boolean {
  return (
    isDamageEvent(event) &&
    getActorFaction(getEventSource(event, registries)) === "friendly" &&
    getActorFaction(getEventTarget(event, registries)) === "hostile"
  );
}

export function isDamageTakenByPlayer(event: CombatEventRecord, registries: Registries): boolean {
  return (
    isDamageEvent(event) &&
    getActorFaction(getEventSource(event, registries)) === "hostile" &&
    getActorFaction(getEventTarget(event, registries)) === "friendly"
  );
}

export function isHealingDoneByPlayer(event: CombatEventRecord, registries: Registries): boolean {
  return isHealEvent(event) && getActorFaction(getEventSource(event, registries)) === "friendly";
}

export function isHealingReceivedByPlayer(
  event: CombatEventRecord,
  registries: Registries
): boolean {
  return isHealEvent(event) && getActorFaction(getEventTarget(event, registries)) === "friendly";
}
