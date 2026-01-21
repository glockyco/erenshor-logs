// Session types for storing combat sessions in the web app

import type { CombatEvent, PlayerInfo } from "./events";

export interface Session {
  id: string; // UUID matching SessionInfo.id
  startTime: number; // Unix timestamp in milliseconds
  endTime?: number; // Unix timestamp in milliseconds (undefined if session is still active)
  player: PlayerInfo;
  events: CombatEvent[];
}

// Aggregated statistics calculated from combat events
export interface SessionStats {
  totalDamage: number;
  totalHealing: number;
  durationMs: number;
  dps: number;
  hps: number;
  actorBreakdown: ActorStats[];
}

export interface ActorStats {
  actorId: string;
  actorName: string;
  actorType: string;
  totalDamage: number;
  totalHealing: number;
  dps: number;
  hps: number;
  percentOfTotalDamage: number;
  percentOfTotalHealing: number;
  abilityBreakdown: AbilityStats[];
}

export interface AbilityStats {
  abilityName: string;
  abilityType: string;
  damage: number;
  healing: number;
  hits: number;
  crits: number;
  misses: number;
  avgDamage: number;
  avgHealing: number;
  critRate: number;
  missRate: number;
}
