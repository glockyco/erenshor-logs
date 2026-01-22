// WebSocket protocol types - re-exported from Zod schemas
// See schemas.ts for the canonical definitions

export type {
  SessionInfo,
  HandshakeMessage,
  SessionStartMessage,
  SessionEndMessage,
  CombatEventsMessage,
  WebSocketMessage,
  ParseError,
  ParseErrorCode,
} from "./schemas";

export {
  SessionInfoSchema,
  HandshakeMessageSchema,
  SessionStartMessageSchema,
  SessionEndMessageSchema,
  CombatEventsMessageSchema,
  WebSocketMessageSchema,
  ParseErrorSchema,
  ParseErrorCodeSchema,
} from "./schemas";
