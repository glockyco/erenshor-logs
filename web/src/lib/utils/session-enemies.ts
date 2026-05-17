import type { ActorRecord, Session } from "$lib/types";

export interface SessionEnemyInfo {
  primaryEnemy: string;
  totalEnemies: number;
  hasEnemies: boolean;
}

export function getSessionEnemies(session: Session): SessionEnemyInfo {
  const npcTargets = new Map<string, ActorRecord>();

  for (const event of session.events) {
    const target = event.targetActorId ? session.registries.actors[event.targetActorId] : undefined;
    if (target?.kind !== "npc") continue;

    const existing = npcTargets.get(target.id);
    if (!existing || (target.level ?? 0) > (existing.level ?? 0)) {
      npcTargets.set(target.id, target);
    }
  }

  if (npcTargets.size === 0) {
    return {
      primaryEnemy: "Practice Session",
      totalEnemies: 0,
      hasEnemies: false,
    };
  }

  const sortedNpcs = Array.from(npcTargets.values()).sort(
    (a, b) => (b.level ?? 0) - (a.level ?? 0)
  );

  return {
    primaryEnemy: sortedNpcs[0].name,
    totalEnemies: npcTargets.size,
    hasEnemies: true,
  };
}
