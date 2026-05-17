import type { ActorRecord, ActorStats } from "$lib/types";

export type ActorKind = ActorRecord["kind"];
export type Faction = "friendly" | "hostile";

export function isPlayerFaction(actorKind: ActorKind): boolean {
  return actorKind === "player" || actorKind === "simPlayer" || actorKind === "pet";
}

export function isEnemyFaction(actorKind: ActorKind): boolean {
  return actorKind === "npc";
}

export function getActorFaction(actor?: Pick<ActorRecord, "kind" | "faction">): Faction | null {
  if (!actor) return null;
  if (actor.faction === "friendly" || actor.faction === "hostile") return actor.faction;
  if (actor.faction === "neutral" || actor.faction === "unknown") return null;
  if (isPlayerFaction(actor.kind)) return "friendly";
  if (isEnemyFaction(actor.kind)) return "hostile";
  return null;
}

export function filterByFaction<T extends Pick<ActorStats, "actorType">>(
  actors: T[],
  faction: "all" | Faction
): T[] {
  if (faction === "all") return actors;

  return actors.filter((actor) => {
    if (faction === "friendly") return isPlayerFaction(actor.actorType);
    return isEnemyFaction(actor.actorType);
  });
}
