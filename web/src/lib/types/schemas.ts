// Zod schemas - single source of truth for all types
// TypeScript types are inferred from these schemas using z.infer<>

import { z } from "zod";

// =============================================================================
// Enums
// =============================================================================

export const EventTypeSchema = z.enum([
  "damagePhysical",
  "damageMagic",
  "damageMelee",
  "damageSkill",
  "damageSpell",
  "damageDot",
  "damageProc",
  "damagePet",
  "damageReflect",
  "damageEnvironmental",
  "healSpell",
  "healHot",
  "healLifesteal",
  "healRegen",
  "manaUse",
  "manaRestore",
  "manaRegen",
  "spellInterrupt",
  "buffApply",
  "buffRefresh",
  "buffFade",
  "debuffApply",
  "debuffRefresh",
  "debuffFade",
  "death",
  "combatStart",
  "combatEnd",
]);
export type EventType = z.infer<typeof EventTypeSchema>;

export const ActorTypeSchema = z.enum(["player", "simPlayer", "npc", "pet"]);
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

export const AbilityTypeSchema = z.enum([
  "skill",
  "spell",
  "auto",
  "dot",
  "hot",
  "unknown",
  "environmental",
]);
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

export const ContextSnapshotSchema = z.object({
  stackDepth: z.number(),
  topContextName: z.string().optional(),
  topContextType: AbilityTypeSchema.optional(),
});
export type ContextSnapshot = z.infer<typeof ContextSnapshotSchema>;

export const AttributionDebugInfoSchema = z.object({
  sourceMethod: z.string(),
  parameters: z.record(z.string(), z.string()).optional(),
  stackTrace: z.array(z.string()).optional(),
  context: ContextSnapshotSchema.optional(),
});
export type AttributionDebugInfo = z.infer<typeof AttributionDebugInfoSchema>;

// =============================================================================
// Protocol V2
// =============================================================================

export const ProtocolVersionSchema = z.string().regex(/^2\.[0-9]+\.[0-9]+(?:[-+][0-9A-Za-z.-]+)?$/);
export const SchemaVersionSchema = ProtocolVersionSchema;

export const CapabilitySchema = z.enum([
  "registryDelta",
  "sessionSnapshot",
  "gzipFileExport",
  "perMessageDeflate",
]);
export type Capability = z.infer<typeof CapabilitySchema>;

export const ProducerInfoSchema = z.object({
  name: z.enum(["ErenshorLogsMod", "ErenshorLogsWeb"]),
  modVersion: z.string().optional(),
  webVersion: z.string().optional(),
  gameVersion: z.string().optional(),
  buildCommit: z.string().optional(),
});
export type ProducerInfo = z.infer<typeof ProducerInfoSchema>;

export const ActorRecordSchema = z.object({
  id: z.string(),
  name: z.string(),
  kind: z.enum(["player", "simPlayer", "npc", "pet", "environment", "unknown"]),
  class: z.string().optional(),
  level: z.number().int().optional(),
  ownerActorId: z.string().optional(),
  faction: z.enum(["friendly", "hostile", "neutral", "unknown"]).optional(),
  isPlayerControlled: z.boolean().optional(),
  raidGroup: z.number().int().min(1).max(3).optional(),
  raidRole: z.enum(["tank", "healer", "dps", "puller", "unknown"]).optional(),
  firstSeenEventSeq: z.number().int().positive().optional(),
});
export type ActorRecord = z.infer<typeof ActorRecordSchema>;
export type ActorKind = ActorRecord["kind"];

export const AbilityRecordSchema = z.object({
  id: z.string(),
  name: z.string(),
  kind: z.enum([
    "skill",
    "spell",
    "auto",
    "dot",
    "hot",
    "proc",
    "environmental",
    "areaEffect",
    "unknown",
  ]),
  stableKey: z.string().optional(),
  damageType: DamageTypeSchema.optional(),
  procSource: ProcSourceSchema.optional(),
  parentAbilityId: z.string().optional(),
});
export type AbilityRecord = z.infer<typeof AbilityRecordSchema>;

export const EffectRecordSchema = z.object({
  id: z.string(),
  name: z.string(),
  kind: z.enum(["buff", "debuff", "unknown"]),
  stableKey: z.string().optional(),
  sourceAbilityId: z.string().optional(),
  defaultDurationMs: z.number().int().nonnegative().optional(),
  maxStacks: z.number().int().positive().optional(),
});
export type EffectRecord = z.infer<typeof EffectRecordSchema>;

export const RegistriesSchema = z.object({
  revision: z.number().int().nonnegative(),
  actors: z.record(z.string(), ActorRecordSchema),
  abilities: z.record(z.string(), AbilityRecordSchema),
  effects: z.record(z.string(), EffectRecordSchema),
});
export type Registries = z.infer<typeof RegistriesSchema>;

export const SessionEndReasonSchema = z.enum([
  "inactivity",
  "manual",
  "shutdown",
  "newSession",
  "error",
]);
export type SessionEndReason = z.infer<typeof SessionEndReasonSchema>;

export const LossCountersSchema = z.object({
  eventsDropped: z.number().int().nonnegative(),
  framesDropped: z.number().int().nonnegative(),
  reason: z.string().optional(),
});
export type LossCounters = z.infer<typeof LossCountersSchema>;

export const SessionDiagnosticsSchema = z.object({
  hookWarnings: z.array(z.string()),
  attributionFailures: z.number().int().nonnegative(),
  droppedEvents: z.number().int().nonnegative(),
  droppedFrames: z.number().int().nonnegative(),
  serializationErrors: z.number().int().nonnegative(),
});
export type SessionDiagnostics = z.infer<typeof SessionDiagnosticsSchema>;

export const AttributionMethodSchema = z.enum([
  "verified",
  "context",
  "effectTracker",
  "inferred",
  "unknown",
]);
export type AttributionMethod = z.infer<typeof AttributionMethodSchema>;

export const AttributionDebugSchema = z.object({
  sourceMethod: z.string(),
  parameters: z.record(z.string(), z.string()).optional(),
  context: z
    .object({
      stackDepth: z.number().int().nonnegative(),
      topContextName: z.string().optional(),
      topContextType: z.string().optional(),
    })
    .optional(),
});
export type AttributionDebug = z.infer<typeof AttributionDebugSchema>;

const CombatEventBaseSchema = z.object({
  eventSeq: z.number().int().positive(),
  offsetMs: z.number().int().nonnegative(),
  action: z.string(),
  sourceActorId: z.string().optional(),
  creditActorId: z.string().optional(),
  targetActorId: z.string().optional(),
  abilityId: z.string().optional(),
  effectId: z.string().optional(),
  causeEventSeq: z.number().int().positive().optional(),
  attribution: AttributionMethodSchema.optional(),
  debug: AttributionDebugSchema.optional(),
});

export const DamageOutcomeSchema = z.object({
  result: z.enum(["landed", "missed", "resisted", "absorbed", "immune"]),
  critical: z.literal(true).optional(),
  blockedAmount: z.number().int().nonnegative().optional(),
  resistedAmount: z.number().int().nonnegative().optional(),
  absorbedAmount: z.number().int().nonnegative().optional(),
});
export type DamageOutcome = z.infer<typeof DamageOutcomeSchema>;

export const DamageDataSchema = z
  .object({
    amount: z.number().int().nonnegative(),
    rawAmount: z.number().int().nonnegative().optional(),
    mitigatedAmount: z.number().int().nonnegative().optional(),
    overkillAmount: z.number().int().nonnegative().optional(),
    damageType: DamageTypeSchema,
    outcome: DamageOutcomeSchema,
  })
  .strict();
export type DamageData = z.infer<typeof DamageDataSchema>;

export const HealDataSchema = z
  .object({
    amount: z.number().int().nonnegative(),
    rawAmount: z.number().int().nonnegative().optional(),
    overhealAmount: z.number().int().nonnegative().optional(),
    critical: z.literal(true).optional(),
  })
  .strict();
export type HealData = z.infer<typeof HealDataSchema>;

export const ResourceDataSchema = z
  .object({
    resource: z.literal("mana"),
    delta: z.number().int(),
    current: z.number().int().nonnegative().optional(),
    max: z.number().int().nonnegative().optional(),
  })
  .strict();
export type ResourceData = z.infer<typeof ResourceDataSchema>;

export const EffectDataSchema = z
  .object({
    stacks: z.number().int().positive().optional(),
    durationMs: z.number().int().nonnegative().optional(),
    remainingMs: z.number().int().nonnegative().optional(),
    reason: z.enum(["expired", "dispelled", "consumed", "overwritten", "unknown"]).optional(),
  })
  .strict();
export type EffectData = z.infer<typeof EffectDataSchema>;

export const DeathDataSchema = z
  .object({
    killingBlowEventSeq: z.number().int().positive().optional(),
  })
  .strict();

export const InterruptDataSchema = z
  .object({
    interruptedAbilityId: z.string().optional(),
  })
  .strict();

export const DamageEventSchema = CombatEventBaseSchema.extend({
  kind: z.literal("damage"),
  action: z.enum(["hit", "tick", "reflect"]),
  data: DamageDataSchema,
});
export type DamageEvent = z.infer<typeof DamageEventSchema>;

export const HealEventSchema = CombatEventBaseSchema.extend({
  kind: z.literal("heal"),
  action: z.enum(["direct", "tick", "lifesteal", "regen", "scripted"]),
  data: HealDataSchema,
});
export type HealEvent = z.infer<typeof HealEventSchema>;

export const ResourceEventSchema = CombatEventBaseSchema.extend({
  kind: z.literal("resource"),
  action: z.enum(["spend", "restore", "regen", "drain"]),
  data: ResourceDataSchema,
});
export type ResourceEvent = z.infer<typeof ResourceEventSchema>;

export const EffectEventSchema = CombatEventBaseSchema.extend({
  kind: z.literal("effect"),
  action: z.enum(["apply", "refresh", "fade"]),
  data: EffectDataSchema,
});
export type EffectEvent = z.infer<typeof EffectEventSchema>;

export const DeathEventSchema = CombatEventBaseSchema.extend({
  kind: z.literal("death"),
  action: z.literal("die"),
  data: DeathDataSchema,
});
export type DeathEvent = z.infer<typeof DeathEventSchema>;

export const InterruptEventSchema = CombatEventBaseSchema.extend({
  kind: z.literal("interrupt"),
  action: z.literal("interrupt"),
  data: InterruptDataSchema,
});
export type InterruptEvent = z.infer<typeof InterruptEventSchema>;

export const MechanicDataSchema = z
  .object({
    name: z.string(),
    value: z.union([z.string(), z.number(), z.boolean()]).optional(),
    previousValue: z.union([z.string(), z.number(), z.boolean()]).optional(),
    affectedStat: z.enum(["hp", "mana", "damage", "resist", "armorPen"]).optional(),
    amount: z.number().int().optional(),
  })
  .strict();
export type MechanicData = z.infer<typeof MechanicDataSchema>;

export const MechanicEventSchema = CombatEventBaseSchema.extend({
  kind: z.literal("mechanic"),
  action: z.enum([
    "phase",
    "invulnerability",
    "spawn",
    "despawn",
    "statChange",
    "targetAssignment",
  ]),
  data: MechanicDataSchema,
});
export type MechanicEvent = z.infer<typeof MechanicEventSchema>;

export const CombatEventRecordSchema = z.discriminatedUnion("kind", [
  DamageEventSchema,
  HealEventSchema,
  ResourceEventSchema,
  EffectEventSchema,
  DeathEventSchema,
  InterruptEventSchema,
  MechanicEventSchema,
]);
export type CombatEventRecord = z.infer<typeof CombatEventRecordSchema>;

export const DerivedSummarySchema = z.object({
  totalDamage: z.number().nonnegative(),
  totalHealing: z.number().nonnegative(),
  totalDamageTaken: z.number().nonnegative(),
  totalHealingReceived: z.number().nonnegative(),
  durationMs: z.number().int().nonnegative(),
});
export type DerivedSummary = z.infer<typeof DerivedSummarySchema>;

export const HelloPayloadSchema = z.object({
  producer: ProducerInfoSchema,
  activeSessionId: z.string().optional(),
  capabilities: z.array(CapabilitySchema),
  requiredCapabilities: z.array(CapabilitySchema).optional(),
});
export type HelloPayload = z.infer<typeof HelloPayloadSchema>;

export const SessionSnapshotPayloadSchema = z
  .object({
    sessionId: z.string(),
    state: z.enum(["active", "ended"]),
    mode: z.enum(["automatic", "manual", "imported"]),
    startedAtUtcMs: z.number().int().nonnegative(),
    endedAtUtcMs: z.number().int().nonnegative().optional(),
    endReason: SessionEndReasonSchema.optional(),
    durationMs: z.number().int().nonnegative().optional(),
    producer: ProducerInfoSchema,
    playerActorId: z.string().optional(),
    registryRevision: z.number().int().nonnegative(),
    lastEventSeq: z.number().int().nonnegative(),
    eventCount: z.number().int().nonnegative(),
    completeness: z.enum(["complete", "partial"]),
    loss: LossCountersSchema.optional(),
    registries: RegistriesSchema,
    diagnostics: SessionDiagnosticsSchema.optional(),
  })
  .superRefine((value, ctx) => {
    if (value.completeness === "partial" && !value.loss) {
      ctx.addIssue({
        code: "custom",
        path: ["loss"],
        message: "loss is required for partial snapshots",
      });
    }
  });
export type SessionSnapshotPayload = z.infer<typeof SessionSnapshotPayloadSchema>;

export const RegistryDeltaPayloadSchema = z.object({
  revision: z.number().int().nonnegative(),
  actors: z.record(z.string(), ActorRecordSchema).optional(),
  abilities: z.record(z.string(), AbilityRecordSchema).optional(),
  effects: z.record(z.string(), EffectRecordSchema).optional(),
});
export type RegistryDeltaPayload = z.infer<typeof RegistryDeltaPayloadSchema>;

export const EventsPayloadSchema = z
  .object({
    sessionId: z.string(),
    registryRevision: z.number().int().nonnegative(),
    eventSeqStart: z.number().int().positive(),
    eventSeqEnd: z.number().int().positive(),
    events: z.array(CombatEventRecordSchema),
  })
  .superRefine((value, ctx) => {
    if (value.events.length === 0) return;

    if (value.events[0].eventSeq !== value.eventSeqStart) {
      ctx.addIssue({
        code: "custom",
        path: ["eventSeqStart"],
        message: "eventSeqStart must match first event",
      });
    }

    for (let index = 0; index < value.events.length; index += 1) {
      const expectedSeq = value.eventSeqStart + index;
      if (value.events[index].eventSeq !== expectedSeq) {
        ctx.addIssue({
          code: "custom",
          path: ["events", index, "eventSeq"],
          message: "eventSeq values must be contiguous",
        });
      }
    }
    if (value.events[value.events.length - 1].eventSeq !== value.eventSeqEnd) {
      ctx.addIssue({
        code: "custom",
        path: ["eventSeqEnd"],
        message: "eventSeqEnd must match last event",
      });
    }
  });
export type EventsPayload = z.infer<typeof EventsPayloadSchema>;

export const SessionEndedPayloadSchema = z.object({
  sessionId: z.string(),
  endedAtUtcMs: z.number().int().nonnegative(),
  endedAtEventSeq: z.number().int().nonnegative(),
  reason: SessionEndReasonSchema,
  durationMs: z.number().int().nonnegative(),
  diagnostics: SessionDiagnosticsSchema.optional(),
});
export type SessionEndedPayload = z.infer<typeof SessionEndedPayloadSchema>;

export const ErrorPayloadSchema = z.object({
  code: z.string(),
  severity: z.enum(["info", "warning", "error", "fatal"]),
  message: z.string(),
  recoverable: z.boolean(),
  sessionId: z.string().optional(),
  eventSeq: z.number().int().positive().optional(),
  details: z.record(z.string(), z.unknown()).optional(),
});
export type ErrorPayload = z.infer<typeof ErrorPayloadSchema>;

export const ServerStatsPayloadSchema = z.object({
  connectedClients: z.number().int().nonnegative(),
  eventsCaptured: z.number().int().nonnegative(),
  eventsSent: z.number().int().nonnegative(),
  registryRevision: z.number().int().nonnegative(),
  queuedFrames: z.number().int().nonnegative(),
  droppedEvents: z.number().int().nonnegative(),
  attributionFailures: z.number().int().nonnegative(),
  hookWarnings: z.number().int().nonnegative(),
  bytesSent: z.number().int().nonnegative().optional(),
});
export type ServerStatsPayload = z.infer<typeof ServerStatsPayloadSchema>;

export const LiveEnvelopeKindSchema = z.enum([
  "hello",
  "sessionSnapshot",
  "registryDelta",
  "events",
  "sessionEnded",
  "error",
  "heartbeat",
  "serverStats",
]);
export type LiveEnvelopeKind = z.infer<typeof LiveEnvelopeKindSchema>;

const sessionScopedLiveKinds = new Set<LiveEnvelopeKind>([
  "sessionSnapshot",
  "registryDelta",
  "events",
  "sessionEnded",
]);

const livePayloadSchemas = {
  hello: HelloPayloadSchema,
  sessionSnapshot: SessionSnapshotPayloadSchema,
  registryDelta: RegistryDeltaPayloadSchema,
  events: EventsPayloadSchema,
  sessionEnded: SessionEndedPayloadSchema,
  error: ErrorPayloadSchema,
  heartbeat: z.object({}).passthrough(),
  serverStats: ServerStatsPayloadSchema,
};

export const LiveEnvelopeSchema = z
  .object({
    protocol: z.literal("erenshor.logs.live"),
    protocolVersion: ProtocolVersionSchema,
    schemaVersion: SchemaVersionSchema,
    kind: LiveEnvelopeKindSchema,
    frameSeq: z.number().int().positive(),
    sessionId: z.string().optional(),
    sentAtMs: z.number().int().nonnegative(),
    payload: z.unknown(),
  })
  .superRefine((value, ctx) => {
    const payload = livePayloadSchemas[value.kind].safeParse(value.payload);
    if (!payload.success) {
      for (const issue of payload.error.issues) {
        ctx.addIssue({ ...issue, path: ["payload", ...issue.path] });
      }
    }

    if (sessionScopedLiveKinds.has(value.kind) && !value.sessionId) {
      ctx.addIssue({
        code: "custom",
        path: ["sessionId"],
        message: "sessionId is required for session-scoped frames",
      });
    }
  });
export type LiveEnvelope = z.infer<typeof LiveEnvelopeSchema>;

export const DerivedDataSchema = z.object({
  algorithmVersion: z.string(),
  computedAtMs: z.number().int().nonnegative(),
  computedFromEventSeq: z.number().int().nonnegative(),
  summary: DerivedSummarySchema,
});
export type DerivedData = z.infer<typeof DerivedDataSchema>;

export const CombatLogSessionSchema = z.object({
  snapshot: SessionSnapshotPayloadSchema,
  events: z.array(CombatEventRecordSchema),
  ended: SessionEndedPayloadSchema.optional(),
  derived: DerivedDataSchema.optional(),
});
export type CombatLogSession = z.infer<typeof CombatLogSessionSchema>;

export const CombatLogFileSchema = z.object({
  format: z.literal("erenshor.logs.export"),
  schemaVersion: SchemaVersionSchema,
  exportedAtMs: z.number().int().nonnegative(),
  producer: ProducerInfoSchema,
  sessions: z.array(CombatLogSessionSchema),
});
export type CombatLogFile = z.infer<typeof CombatLogFileSchema>;

// =============================================================================
// Combat Event
// =============================================================================

export const CombatEventSchema = z.object({
  id: z.string(),
  timestamp: z.number(),
  eventType: EventTypeSchema,
  source: ActorRefSchema.optional(),
  target: ActorRefSchema.optional(),
  ability: AbilityRefSchema, // Always present; uses "Unknown" when attribution fails
  amount: z.number().optional(),
  rawAmount: z.number().optional(),
  mitigated: z.number().optional(),
  damageType: DamageTypeSchema.optional(),
  effect: EffectRefSchema.optional(),
  flags: EventFlagsSchema.optional(),
  debugInfo: AttributionDebugInfoSchema.optional(),
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

export const ProtocolErrorSchema = z.object({
  code: z.string(),
  message: z.string(),
  sessionId: z.string().optional(),
  eventSeq: z.number().int().positive().optional(),
});
export type ProtocolError = z.infer<typeof ProtocolErrorSchema>;

export const SessionSchema = z.object({
  id: z.string(),
  mode: z.enum(["automatic", "manual", "imported"]),
  state: z.enum(["active", "ended"]),
  startedAtUtcMs: z.number().int().nonnegative(),
  endedAtUtcMs: z.number().int().nonnegative().optional(),
  endReason: SessionEndReasonSchema.optional(),
  durationMs: z.number().int().nonnegative().optional(),
  producer: ProducerInfoSchema,
  playerActorId: z.string().optional(),
  registryRevision: z.number().int().nonnegative(),
  lastEventSeq: z.number().int().nonnegative(),
  eventCount: z.number().int().nonnegative(),
  completeness: z.enum(["complete", "partial"]),
  loss: LossCountersSchema.optional(),
  registries: RegistriesSchema,
  diagnostics: SessionDiagnosticsSchema.optional(),
  events: z.array(CombatEventRecordSchema),
  protocolErrors: z.array(ProtocolErrorSchema).default([]),
});
export type Session = z.infer<typeof SessionSchema>;

// =============================================================================
// Statistics Types
// =============================================================================

export const AbilityStatsSchema = z.object({
  abilityName: z.string(),
  abilityType: z.enum([
    "skill",
    "spell",
    "auto",
    "dot",
    "hot",
    "proc",
    "environmental",
    "areaEffect",
    "unknown",
  ]),
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
  actorType: z.enum(["player", "simPlayer", "npc", "pet", "environment", "unknown"]),
  actorClass: z.string().optional(), // Class name (e.g., "Arcanist", "Duelist") for players
  // Outgoing metrics (damage/healing dealt by this actor)
  totalDamage: z.number(),
  totalHealing: z.number(),
  dps: z.number(),
  hps: z.number(),
  percentOfTotalDamage: z.number(),
  percentOfTotalHealing: z.number(),
  // Incoming metrics (damage/healing received by this actor)
  damageTaken: z.number(),
  healingReceived: z.number(),
  dtps: z.number(), // Damage Taken Per Second
  hrps: z.number(), // Healing Received Per Second
  percentOfTotalDamageTaken: z.number(),
  percentOfTotalHealingReceived: z.number(),
  // Defensive stats
  totalMitigated: z.number(), // Total damage reduced by armor/resists
  mitigationRate: z.number(), // Percentage of raw damage mitigated
  totalMissedAgainst: z.number(), // Number of attacks that missed this actor
  avoidanceRate: z.number(), // Percentage of attacks avoided
  // Ability breakdowns
  abilityBreakdown: z.array(AbilityStatsSchema), // Abilities this actor USED
  abilitiesReceivedFrom: z.array(AbilityStatsSchema), // Abilities that HIT this actor
});
export type ActorStats = z.infer<typeof ActorStatsSchema>;

export const EventFamilyCountsSchema = z.object({
  damage: z.number().int().nonnegative(),
  heal: z.number().int().nonnegative(),
  resource: z.number().int().nonnegative(),
  effect: z.number().int().nonnegative(),
  death: z.number().int().nonnegative(),
  interrupt: z.number().int().nonnegative(),
  mechanic: z.number().int().nonnegative(),
});
export type EventFamilyCounts = z.infer<typeof EventFamilyCountsSchema>;

export const SessionStatsSchema = z.object({
  // Outgoing metrics (dealt by player faction)
  totalDamage: z.number(),
  totalHealing: z.number(),
  dps: z.number(),
  hps: z.number(),
  // Incoming metrics (received by player faction)
  totalDamageTaken: z.number(),
  totalHealingReceived: z.number(),
  dtps: z.number(), // Damage Taken Per Second
  hrps: z.number(), // Healing Received Per Second
  // Defense stats
  totalMitigated: z.number(),
  mitigationRate: z.number(),
  // Duration
  durationMs: z.number(),
  eventCounts: EventFamilyCountsSchema,
  // Actor breakdown (complete bidirectional tracking)
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
  session: SessionInfoSchema.optional(),
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
  endTime: z.number(),
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
  "missing_protocol",
  "legacy_mod",
  "unknown_protocol",
  "unsupported_version",
  "unknown_kind",
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
  "legacy_mod",
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

export const SortBySchema = z.enum([
  "name",
  "damage",
  "dps",
  "damageTaken",
  "dtps",
  "healing",
  "hps",
  "healingReceived",
  "hrps",
]);
export type SortBy = z.infer<typeof SortBySchema>;

export const SortDirectionSchema = z.enum(["asc", "desc"]);
export type SortDirection = z.infer<typeof SortDirectionSchema>;

export const ActorBreakdownTabSchema = z.enum([
  "damageDealt",
  "damageTaken",
  "healingDone",
  "healingReceived",
]);
export type ActorBreakdownTab = z.infer<typeof ActorBreakdownTabSchema>;

export const FactionFilterSchema = z.enum(["all", "friendly", "hostile"]);
export type FactionFilter = z.infer<typeof FactionFilterSchema>;

export const UIPreferencesSchema = z.object({
  collapsedActors: z.array(z.string()),
  sortBy: SortBySchema,
  sortDirection: SortDirectionSchema,
  actorBreakdownTab: ActorBreakdownTabSchema,
  factionFilter: FactionFilterSchema,
  sidebarCollapsed: z.boolean().optional(),
});

export type UIPreferences = z.infer<typeof UIPreferencesSchema>;

// =============================================================================
// App Settings
// =============================================================================

export const AppSettingsSchema = z.object({
  websocket: z.object({
    url: z
      .string()
      .min(1, "WebSocket URL is required")
      .refine(
        (url) => url.startsWith("ws://") || url.startsWith("wss://"),
        "URL must start with ws:// or wss://"
      )
      .refine((url) => {
        try {
          new URL(url);
          return true;
        } catch {
          return false;
        }
      }, "Invalid URL format"),
    autoReconnect: z.boolean().default(true),
    reconnectInterval: z.number().min(1000).max(30000).default(2000),
  }),
});

export type AppSettings = z.infer<typeof AppSettingsSchema>;

// =============================================================================
// Storage Schemas (for localStorage validation)
// =============================================================================

export const StoredSessionsSchema = z.array(z.tuple([z.string(), SessionSchema]));
export type StoredSessions = z.infer<typeof StoredSessionsSchema>;

export const DismissedUpdateSchema = z.string();
export type DismissedUpdate = z.infer<typeof DismissedUpdateSchema>;
