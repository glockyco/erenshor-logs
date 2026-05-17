import type { CombatEventRecord } from "$lib/types";

export function isDamageEventKind(kind: CombatEventRecord["kind"]): boolean {
  return kind === "damage";
}

export function isHealEventKind(kind: CombatEventRecord["kind"]): boolean {
  return kind === "heal";
}
