// Zod schemas - single source of truth for all types
// TypeScript types are inferred from these schemas using z.infer<>

import { z } from "zod";

// =============================================================================
// Enums
// =============================================================================

export const EventTypeSchema = z.enum([
  "damage_physical",
  "damage_magic",
  "damage_melee",
  "damage_skill",
  "damage_spell",
  "damage_dot",
  "damage_proc",
  "damage_pet",
  "damage_reflect",
  "damage_environmental",
  "heal_spell",
  "heal_hot",
  "heal_lifesteal",
  "heal_regen",
  "mana_use",
  "mana_restore",
  "mana_regen",
  "spell_interrupt",
  "buff_apply",
  "buff_refresh",
  "buff_fade",
  "debuff_apply",
  "debuff_refresh",
  "debuff_fade",
  "death",
  "combat_start",
  "combat_end",
]);
export type EventType = z.infer<typeof EventTypeSchema>;

export const ActorTypeSchema = z.enum(["player", "sim_player", "npc", "pet"]);
export type ActorType = z.infer<typeof ActorTypeSchema>;

export const DamageTypeSchema = z.enum([
  "unknown",
  "physical",
  "magic",
  "elemental",
  "void",
  "poison",
]);
export type DamageType = z.infer<typeof DamageTypeSchema>;

export const AbilityTypeSchema = z.enum(["skill", "spell", "auto", "dot", "hot"]);
export type AbilityType = z.infer<typeof AbilityTypeSchema>;

export const ProcSourceSchema = z.enum(["weapon", "wand", "bow", "buff", "skill"]);
export type ProcSource = z.infer<typeof ProcSourceSchema>;

// =============================================================================
// Combat Event Components
// =============================================================================

export const ActorRefSchema = z.object({
  id: z.string(),
  name: z.string(),
  type: ActorTypeSchema,
  class: z.string().optional(),
  level: z.number().optional(),
  masterId: z.string().optional(),
});
export type ActorRef = z.infer<typeof ActorRefSchema>;

export const AbilityRefSchema = z.object({
  name: z.string(),
  type: AbilityTypeSchema,
  stableKey: z.string().optional(),
  procSource: ProcSourceSchema.optional(),
});
export type AbilityRef = z.infer<typeof AbilityRefSchema>;

export const EffectRefSchema = z.object({
  name: z.string(),
  duration: z.number().optional(),
  stacks: z.number().optional(),
});
export type EffectRef = z.infer<typeof EffectRefSchema>;

export const EventFlagsSchema = z.object({
  critical: z.boolean().optional(),
  overkill: z.boolean().optional(),
  fromPlayer: z.boolean().optional(),
  pet: z.boolean().optional(),
  resonating: z.boolean().optional(),
  attributionFailed: z.boolean().optional(),
  missed: z.boolean().optional(),
  resisted: z.boolean().optional(),
  absorbed: z.boolean().optional(),
});
export type EventFlags = z.infer<typeof EventFlagsSchema>;

// =============================================================================
// Combat Event
// =============================================================================

export const CombatEventSchema = z.object({
  id: z.string(),
  timestamp: z.number(),
  eventType: EventTypeSchema,
  source: ActorRefSchema.optional(),
  target: ActorRefSchema.optional(),
  ability: AbilityRefSchema.optional(),
  amount: z.number().optional(),
  rawAmount: z.number().optional(),
  mitigated: z.number().optional(),
  damageType: DamageTypeSchema.optional(),
  effect: EffectRefSchema.optional(),
  flags: EventFlagsSchema.optional(),
});
export type CombatEvent = z.infer<typeof CombatEventSchema>;

// =============================================================================
// Session Types
// =============================================================================

export const SessionInfoSchema = z.object({
  id: z.string(),
  startTime: z.number(),
});
export type SessionInfo = z.infer<typeof SessionInfoSchema>;

export const SessionSchema = z.object({
  id: z.string(),
  startTime: z.number(),
  endTime: z.number().optional(),
  events: z.array(CombatEventSchema),
});
export type Session = z.infer<typeof SessionSchema>;

// =============================================================================
// Statistics Types
// =============================================================================

export const AbilityStatsSchema = z.object({
  abilityName: z.string(),
  abilityType: AbilityTypeSchema,
  damage: z.number(),
  healing: z.number(),
  hits: z.number(),
  crits: z.number(),
  misses: z.number(),
  avgDamage: z.number(),
  avgHealing: z.number(),
  critRate: z.number(),
  missRate: z.number(),
});
export type AbilityStats = z.infer<typeof AbilityStatsSchema>;

export const ActorStatsSchema = z.object({
  actorId: z.string(),
  actorName: z.string(),
  actorType: ActorTypeSchema,
  totalDamage: z.number(),
  totalHealing: z.number(),
  dps: z.number(),
  hps: z.number(),
  percentOfTotalDamage: z.number(),
  percentOfTotalHealing: z.number(),
  abilityBreakdown: z.array(AbilityStatsSchema),
});
export type ActorStats = z.infer<typeof ActorStatsSchema>;

export const SessionStatsSchema = z.object({
  totalDamage: z.number(),
  totalHealing: z.number(),
  durationMs: z.number(),
  dps: z.number(),
  hps: z.number(),
  actorBreakdown: z.array(ActorStatsSchema),
});
export type SessionStats = z.infer<typeof SessionStatsSchema>;

// =============================================================================
// WebSocket Protocol Messages
// =============================================================================

export const HandshakeMessageSchema = z.object({
  type: z.literal("handshake"),
  protocolVersion: z.string(),
  modVersion: z.string(),
  session: SessionInfoSchema.nullable(),
});
export type HandshakeMessage = z.infer<typeof HandshakeMessageSchema>;

export const SessionStartMessageSchema = z.object({
  type: z.literal("sessionStart"),
  session: SessionInfoSchema,
});
export type SessionStartMessage = z.infer<typeof SessionStartMessageSchema>;

export const SessionEndMessageSchema = z.object({
  type: z.literal("sessionEnd"),
  sessionId: z.string(),
  duration: z.number(),
});
export type SessionEndMessage = z.infer<typeof SessionEndMessageSchema>;

export const CombatEventsMessageSchema = z.object({
  type: z.literal("combatEvents"),
  sessionId: z.string(),
  events: z.array(CombatEventSchema),
});
export type CombatEventsMessage = z.infer<typeof CombatEventsMessageSchema>;

export const WebSocketMessageSchema = z.discriminatedUnion("type", [
  HandshakeMessageSchema,
  SessionStartMessageSchema,
  SessionEndMessageSchema,
  CombatEventsMessageSchema,
]);
export type WebSocketMessage = z.infer<typeof WebSocketMessageSchema>;

// =============================================================================
// Error Types
// =============================================================================

export const ParseErrorCodeSchema = z.enum([
  "invalid_json",
  "missing_type",
  "unknown_type",
  "invalid_structure",
]);
export type ParseErrorCode = z.infer<typeof ParseErrorCodeSchema>;

export const ParseErrorSchema = z.object({
  code: ParseErrorCodeSchema,
  message: z.string(),
  raw: z.string().optional(),
});
export type ParseError = z.infer<typeof ParseErrorSchema>;

export const ConnectionErrorCodeSchema = z.enum([
  "connection_failed",
  "parse_error",
  "unexpected_disconnect",
]);
export type ConnectionErrorCode = z.infer<typeof ConnectionErrorCodeSchema>;

export const ConnectionErrorSchema = z.object({
  code: ConnectionErrorCodeSchema,
  message: z.string(),
  timestamp: z.number(),
});
export type ConnectionError = z.infer<typeof ConnectionErrorSchema>;

// =============================================================================
// UI State Types
// =============================================================================

export const ConnectionStatusSchema = z.enum(["disconnected", "connecting", "connected"]);
export type ConnectionStatus = z.infer<typeof ConnectionStatusSchema>;

export const SortBySchema = z.enum(["damage", "dps", "name"]);
export type SortBy = z.infer<typeof SortBySchema>;

export const SortDirectionSchema = z.enum(["asc", "desc"]);
export type SortDirection = z.infer<typeof SortDirectionSchema>;

export const UIPreferencesSchema = z.object({
  collapsedActors: z.array(z.string()),
  sortBy: SortBySchema,
  sortDirection: SortDirectionSchema,
});
export type UIPreferences = z.infer<typeof UIPreferencesSchema>;

// =============================================================================
// Storage Schemas (for localStorage validation)
// =============================================================================

export const StoredSessionsSchema = z.array(z.tuple([z.string(), SessionSchema]));
export type StoredSessions = z.infer<typeof StoredSessionsSchema>;
