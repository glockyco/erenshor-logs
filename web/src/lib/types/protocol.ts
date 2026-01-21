// WebSocket protocol message types mirroring mod/src/Protocol/Messages.cs
// Discriminated union for type-safe message handling

import type { CombatEvent, PlayerInfo } from "./events";

export interface SessionInfo {
  id: string; // UUID
  startTime: number; // Unix timestamp in milliseconds
  player: PlayerInfo;
}

export interface HandshakeMessage {
  type: "handshake";
  protocolVersion: string; // e.g., "0.1.0"
  modVersion: string;
  session: SessionInfo | null; // Current session if one is active
}

export interface SessionStartMessage {
  type: "sessionStart";
  session: SessionInfo;
}

export interface SessionEndMessage {
  type: "sessionEnd";
  sessionId: string;
  duration: number; // Duration in milliseconds
}

export interface CombatEventsMessage {
  type: "combatEvents";
  sessionId: string;
  events: CombatEvent[];
}

// Discriminated union for type-safe message handling
export type WebSocketMessage =
  | HandshakeMessage
  | SessionStartMessage
  | SessionEndMessage
  | CombatEventsMessage;

// Error type for message parsing failures
export interface ParseError {
  code: "invalid_json" | "missing_type" | "unknown_type" | "invalid_structure";
  message: string;
  raw?: string;
}
