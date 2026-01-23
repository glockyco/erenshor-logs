import type { Session, ActorRef } from "$lib/types";

export interface SessionEnemyInfo {
  primaryEnemy: string; // Name of representative enemy or "Practice Session"
  totalEnemies: number; // Total unique NPC count
  hasEnemies: boolean; // Whether any NPCs exist
}

/**
 * Analyzes a combat session to determine enemy information for display.
 * Returns the name of the highest-level enemy and total enemy count.
 */
export function getSessionEnemies(session: Session): SessionEnemyInfo {
  // Track unique NPC targets by ID
  const npcTargets = new Map<string, ActorRef>();

  // Scan all events for NPC targets
  for (const event of session.events) {
    const target = event.target;

    // Only consider NPCs (not pets, players, simPlayers)
    if (target?.type === "npc") {
      const existing = npcTargets.get(target.id);

      // Keep highest level NPC, or first encountered if levels are equal/unknown
      if (!existing || (target.level || 0) > (existing.level || 0)) {
        npcTargets.set(target.id, target);
      }
    }
  }

  // No enemies found - practice/training session
  if (npcTargets.size === 0) {
    return {
      primaryEnemy: "Practice Session",
      totalEnemies: 0,
      hasEnemies: false,
    };
  }

  // Find highest-level NPC as representative
  const sortedNpcs = Array.from(npcTargets.values()).sort(
    (a, b) => (b.level || 0) - (a.level || 0)
  );

  return {
    primaryEnemy: sortedNpcs[0].name,
    totalEnemies: npcTargets.size,
    hasEnemies: true,
  };
}
