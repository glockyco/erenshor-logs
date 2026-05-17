# Erenshor Logs Protocol Redesign

## Goal

Replace the current duplicated event objects and split live/file contracts with a
single long-term combat-log data model that is efficient, replayable,
testable, and explicit about what is captured versus derived.

Backward compatibility is intentionally out of scope. The correct implementation
path is a clean cutover to this contract and deletion of legacy parser shapes.

## Current protocol problems

### Full references are repeated on every event

Current `CombatEvent` embeds `ActorRef` for `source` and `target`, embeds
`AbilityRef`, and optionally embeds `EffectRef`. In a fight with thousands of
events this repeats names, actor classes, levels, owner IDs, ability names, and
stable keys over and over.

This is wasteful, but the deeper problem is ambiguity: actor and ability metadata
can change or be discovered late. Repeating full objects in events makes it
unclear whether old events should preserve old display metadata or reflect the
latest registry state.

### The event model is sparse and under-constrained

`CombatEvent` is one large record with many optional fields. That permits invalid
combinations: a mana event with `damageType`, a buff event without an effect, a
damage event without an amount, or a death event with irrelevant mitigation
fields.

The event type enum also mixes several concerns:

- raw game hook source (`damagePhysical`, `damageMagic`),
- semantic classification (`damageSkill`, `damageProc`, `damagePet`),
- lifecycle (`combatStart`, `combatEnd`).

This makes the schema grow combinatorially and forces consumers to infer too much
from string names.

### Live WebSocket and file export are different contracts

Current live messages are `handshake`, `sessionStart`, `sessionEnd`, and
`combatEvents`. Web export wraps sessions as `{ version, exportedAt, session }`
or `{ version, exportedAt, sessions }`. C# also has a separate `CombatLog` root
with required `session`, `summary`, and top-level `events`.

These should not be separate logical models. A session imported from a file and a
session received live should normalize into the same in-memory state.

### Session lifecycle is mixed into combat data

`combatStart` and `combatEnd` are synthetic combat events. The previous audit
found an ordering risk where the damage event that starts a session can be queued
before the synthetic `combatStart` event.

Session lifecycle is control-plane state; damage/heal/effect records are
combat facts. They should be separate.

### The protocol lacks ordering, completeness, and recovery signals

Current event batches have no sequence range. If a client reconnects or if the
mod drops queued network events while no clients are connected, the browser
cannot know whether its view is complete.

For a combat logger, silent loss is worse than visible partial data.

## Research summary

The redesign follows these researched principles:

- Event streams should be append-only facts with explicit IDs and ordering.
  Confluent’s event-design guidance emphasizes schemas, event IDs for duplicate
  and missing-event detection, and metadata/headers for origin and audit context.
- CloudEvents separates context metadata from event data and requires event
  identity (`source` + `id`), `type`, and `specversion`; it also provides
  `dataschema` for payload schema identification. We should not copy CloudEvents
  wholesale, but the separation of envelope/context from domain payload is the
  right shape.
- Snapshots are an optimization for recovery and late joiners, not the source of
  truth. Events remain authoritative; snapshots are rebuildable from events.
- Game protocols commonly use string/reference tables to avoid transmitting the
  same names repeatedly. The same idea applies here as actor/ability/effect
  registries.
- JSON remains the right canonical/debug format for this project because the
  consumer is a static browser app and the producer already uses Newtonsoft.Json
  under Unity Mono. SignalR documents the tradeoff clearly: MessagePack is
  smaller, but binary messages are unreadable in traces/logs and add client/server
  tooling concerns. Unity/MessagePack also has AOT considerations. Defer binary
  until measurement proves JSON + registries + compression is insufficient.
- RFC 7692 defines per-message WebSocket compression. Use standard
  permessage-deflate if supported rather than inventing a custom compressed live
  payload. Use gzip for exported files.
- NDJSON is useful for line-delimited streaming files, but ordinary sharing is
  better served by one self-contained JSON document. NDJSON can be a later large-
  file option with the same logical frame schema.

Sources used:

- BepInEx/Harmony audit evidence in `docs/MOD_AUDIT_2026-05-17.md`.
- CloudEvents 1.0.2: https://github.com/cloudevents/spec/blob/v1.0.2/cloudevents/spec.md
- Confluent event design best practices: https://developer.confluent.io/courses/event-design/best-practices/
- SignalR overview and JSON/MessagePack tradeoff: https://learn.microsoft.com/en-us/aspnet/core/signalr/introduction
- SignalR MessagePack caveats: https://learn.microsoft.com/en-us/aspnet/core/signalr/messagepackhubprotocol
- WebSocket compression RFC 7692: https://datatracker.ietf.org/doc/html/rfc7692
- NDJSON spec: https://github.com/ndjson/ndjson-spec

## Approaches considered

### Approach A: keep embedded records, tighten schemas

Keep `source`, `target`, `ability`, and `effect` embedded in every event, but
replace the sparse event record with typed discriminated unions and add sequence
numbers.

Pros:

- Lowest implementation cost.
- Files remain easy to inspect event-by-event.
- Reconnect snapshots are simpler because each event is self-contained.

Cons:

- Continues repeating actor and ability data in hot-path events.
- Still has stale metadata ambiguity.
- Harder to support late-discovered owner/master relationships cleanly.
- Does not solve the user-identified duplication concern.

Verdict: reject. This is an incremental patch, not the best long-term design.

### Approach B: JSON stream with registries and typed events

Use session-scoped registries for actors, abilities, and effects. Live WebSocket
sends registry deltas before events that reference new IDs. Events reference
registry IDs and carry only event-specific facts. File export stores the final
registries plus the same events.

Pros:

- Removes repeated actor/ability/effect metadata.
- Keeps JSON readable and browser-native.
- Provides one logical model for live and file import/export.
- Makes reconnect/import deterministic.
- Makes ownership, attribution, and metadata updates explicit.
- Easy to validate with Zod/JSON Schema and C# golden fixtures.

Cons:

- Requires clients to maintain registry state.
- Event batches are invalid without prior registry state.
- Requires stronger ordering and revision discipline.

Verdict: recommended. This is the right balance of correctness, efficiency, and
maintainability for a local combat logger.

### Approach C: binary protocol first

Define a compact binary stream using MessagePack or Protocol Buffers from day
one.

Pros:

- Smaller payloads.
- Potentially lower CPU once optimized.
- Strong schema/codegen story if Protocol Buffers are used.

Cons:

- Harder to inspect BepInEx logs and browser network traces.
- Adds browser decoder/runtime dependencies.
- Adds Unity/Mono/AOT and package-shading risk.
- Premature until event volume is measured after registries and gzip.

Verdict: reject for the canonical protocol. Keep binary as a future negotiated
transport encoding only if profiling proves the need.

## Recommended architecture

### Canonical model

The canonical combat log is:

1. session metadata,
2. session-scoped registries,
3. append-only combat events that reference registry IDs,
4. lifecycle/end metadata,
5. optional derived summaries and diagnostics.

Events and registries are the source of truth. Summaries are caches and must be
safe to discard and recompute.

### Producer-side pipeline

The mod should have these conceptual stages:

1. Harmony hooks capture raw game facts.
2. A classifier converts raw facts plus context into typed protocol events.
3. A session event store assigns `eventSeq` once and appends immutable events.
4. Registry services upsert actors, abilities, and effects before any event that
   references them is published.
5. Broadcasters/exporters consume from the session event store. They do not own
   the source event queue.

This is important: the WebSocket broadcaster may have a bounded network queue,
but the session event store should not silently drop source events. If memory or
disk limits are ever added, loss must be explicit in diagnostics and session
completeness.

### Consumer-side pipeline

The web app should normalize both live frames and file import into the same
state:

1. validate envelope/document version,
2. apply session snapshot,
3. apply registry deltas,
4. append events in strict `eventSeq` order,
5. recompute summaries from events and registries.

For live connections, `sessionSnapshot` is a state replacement boundary for its
`sessionId`: the client must discard any retained in-memory copy of that session
before applying the snapshot and catch-up events. The protocol does not support
incremental resume from a client-provided last event sequence yet; adding that
requires an explicit future resume request/response.

An event referencing an unknown actor, ability, or effect ID is invalid unless it
is quarantined with a visible protocol error.

## Versioning

Use separate versions for separate concerns:

- `protocolVersion`: WebSocket frame semantics.
- `schemaVersion`: event/file data shape.
- `producer.modVersion`: build provenance.
- `producer.gameVersion`: observed game build/version.

Use semver strings. Because this is a clean cutover from the current protocol and
`docs/LOG_FORMAT.md` already uses `1.0.0`, use:

- `protocolVersion: "2.0.0"`
- `schemaVersion: "2.0.0"`

Rules:

- Unsupported major versions are rejected.
- Unknown optional fields in the same major version are ignored.
- New required behavior must either bump the major version or be guarded by an
  explicit `requiredCapabilities` field that older clients reject.
- Do not branch parsing behavior on mod version or game version.

## Naming and null semantics

- Use camelCase everywhere.
- Omit unknown or non-applicable optional fields.
- Do not emit JSON `null` in protocol messages or files.
- Do not support snake_case aliases in the main parser.
- Do not use display names as stable IDs.

## Live WebSocket protocol

Every live message uses the same envelope shape:

```ts
interface LiveEnvelope<TPayload> {
  protocol: "erenshor.logs.live";
  protocolVersion: string; // semver, major 2
  schemaVersion: string; // semver, major 2
  kind:
    | "hello"
    | "sessionSnapshot"
    | "registryDelta"
    | "events"
    | "sessionEnded"
    | "error"
    | "heartbeat"
    | "serverStats";
  frameSeq: number;
  sessionId?: string;
  sentAtMs: number;
  payload: TPayload;
}
```

`frameSeq` is connection-local and monotonic. It helps debug missing live frames.
It is not the event ordering key. Combat event ordering uses `eventSeq`.

### `hello`

Sent immediately after a client connects.

```ts
interface HelloPayload {
  producer: ProducerInfo;
  activeSessionId?: string;
  capabilities: Capability[];
  requiredCapabilities?: Capability[];
}

type Capability =
  | "registryDelta"
  | "sessionSnapshot"
  | "gzipFileExport"
  | "perMessageDeflate";

interface ProducerInfo {
  name: "ErenshorLogsMod" | "ErenshorLogsWeb";
  modVersion?: string;
  webVersion?: string;
  gameVersion?: string;
  buildCommit?: string;
}
```

If `activeSessionId` is present, the server must send `sessionSnapshot` before
any `events` frame for that session.

### `sessionSnapshot`

Sent at session start and to late/reconnected clients. In files, this is the
session header.

```ts
interface SessionSnapshotPayload {
  sessionId: string;
  state: "active" | "ended";
  mode: "automatic" | "manual" | "imported";
  startedAtUtcMs: number;
  endedAtUtcMs?: number;
  endReason?: SessionEndReason;
  durationMs?: number;
  producer: ProducerInfo;
  playerActorId?: string;
  registryRevision: number;
  lastEventSeq: number;
  eventCount: number;
  completeness: "complete" | "partial";
  loss?: LossCounters;
  registries: Registries;
  diagnostics?: SessionDiagnostics;
}

type SessionEndReason =
  | "inactivity"
  | "manual"
  | "shutdown"
  | "newSession"
  | "error";

interface LossCounters {
  eventsDropped: number;
  framesDropped: number;
  reason?: string;
}
```

For this project, the preferred behavior is `completeness: "complete"`: keep the
session event log and registries available for late clients and export. Every
`sessionSnapshot` must include a full registry snapshot at `registryRevision` so
late/reconnected clients can render events without relying on earlier
`registryDelta` frames.

A client that joins an active complete session must receive catch-up `events`
frames for `eventSeq` 1 through `lastEventSeq` after `sessionSnapshot` and before
the live tail. The same event records used for file export are used for catch-up,
so live reconnect and file import normalize to the same state. If a future
resource limit prevents full replay, the snapshot must say
`completeness: "partial"`, set `loss`, and the client must mark the session
rendering future events. Silent loss is a bug.

### `registryDelta`

Sends actor/ability/effect updates. A delta must be delivered before any event
that references newly discovered records or registry enrichments.

```ts
interface RegistryDeltaPayload {
  revision: number;
  actors?: Record<ActorId, ActorRecord>;
  abilities?: Record<AbilityId, AbilityRecord>;
  effects?: Record<EffectId, EffectRecord>;
}

type ActorId = string;
type AbilityId = string;
type EffectId = string;

interface Registries {
  revision: number;
  actors: Record<ActorId, ActorRecord>;
  abilities: Record<AbilityId, AbilityRecord>;
  effects: Record<EffectId, EffectRecord>;
}
```

Registry IDs are session-scoped. They may be compact strings (`a1`, `ab12`,
`ef3`) because the registry gives them meaning. Exports must include full
registries so files are self-contained.

Registry records are immutable after first use except for enrichment of fields
that were previously unknown and safe to apply retroactively, such as discovering
a pet owner after the pet was first seen. Time-varying state, such as current HP,
mana, temporary faction changes, or group membership, does not belong in the
registry; it must be represented by events or snapshots. If a field would change
the historical meaning of earlier events, create a new registry ID or emit a
state event instead of mutating the record.

Actor records:

```ts
interface ActorRecord {
  id: ActorId;
  name: string;
  kind: "player" | "simPlayer" | "npc" | "pet" | "environment" | "unknown";
  class?: string;
  level?: number;
  ownerActorId?: ActorId;
  faction?: "friendly" | "hostile" | "neutral" | "unknown";
  isPlayerControlled?: boolean;
  firstSeenEventSeq?: number;
}
```

Ability records:

```ts
interface AbilityRecord {
  id: AbilityId;
  name: string;
  kind:
    | "skill"
    | "spell"
    | "auto"
    | "dot"
    | "hot"
    | "proc"
    | "environmental"
    | "unknown";
  stableKey?: string;
  damageType?: DamageType;
  procSource?: "weapon" | "wand" | "bow" | "buff" | "skill";
  parentAbilityId?: AbilityId;
}
```

Effect records:

```ts
interface EffectRecord {
  id: EffectId;
  name: string;
  kind: "buff" | "debuff" | "unknown";
  stableKey?: string;
  sourceAbilityId?: AbilityId;
  defaultDurationMs?: number;
  maxStacks?: number;
}
```

### `events`

Carries append-only combat facts.

```ts
interface EventsPayload {
  sessionId: string;
  registryRevision: number;
  eventSeqStart: number;
  eventSeqEnd: number;
  events: CombatEventRecord[];
}
```

Rules:

- `events` must be sorted by `eventSeq`.
- `eventSeqStart` and `eventSeqEnd` must match the first and last event.
- There must be no gaps inside a batch.
- Gaps between batches are protocol errors unless a prior `sessionSnapshot`
  explicitly says the stream is partial.
- The first combat event in a session is `eventSeq: 1`.
- Session lifecycle is not represented as combat events.

### `sessionEnded`

Closes a session.

```ts
interface SessionEndedPayload {
  sessionId: string;
  endedAtUtcMs: number;
  endedAtEventSeq: number;
  reason: SessionEndReason;
  durationMs: number;
  diagnostics?: SessionDiagnostics;
}
```

### `error`

Producer-reported protocol or capture problems.

```ts
interface ErrorPayload {
  code: string;
  severity: "info" | "warning" | "error" | "fatal";
  message: string;
  recoverable: boolean;
  sessionId?: string;
  eventSeq?: number;
  details?: Record<string, unknown>;
}
```

Shared diagnostic and derived-summary shapes:

```ts
interface SessionDiagnostics {
  hookWarnings: string[];
  attributionFailures: number;
  droppedEvents: number;
  droppedFrames: number;
  serializationErrors: number;
}

interface AttributionDebug {
  sourceMethod: string;
  parameters?: Record<string, string>;
  context?: {
    stackDepth: number;
    topContextName?: string;
    topContextType?: string;
  };
}

interface DerivedSummary {
  totalDamage: number;
  totalHealing: number;
  totalDamageTaken: number;
  totalHealingReceived: number;
  durationMs: number;
}
```

Examples: hook compatibility warning, serialization failure, registry invariant
violation, dropped-event threshold exceeded.

### `heartbeat` and `serverStats`

`heartbeat` is optional and only needed if idle connections prove unreliable.
`serverStats` is optional but useful while developing capture quality.

```ts
interface ServerStatsPayload {
  connectedClients: number;
  eventsCaptured: number;
  eventsSent: number;
  registryRevision: number;
  queuedFrames: number;
  droppedEvents: number;
  attributionFailures: number;
  hookWarnings: number;
  bytesSent?: number;
}
```

## Combat events

The event record uses common fields plus typed `data`. This avoids one giant
sparse object while keeping every event easy to index and replay.

```ts
type CombatEventRecord =
  | DamageEvent
  | HealEvent
  | ResourceEvent
  | EffectEvent
  | DeathEvent
  | InterruptEvent;

interface CombatEventBase<TKind extends string, TAction extends string, TData> {
  eventSeq: number;
  offsetMs: number;
  kind: TKind;
  action: TAction;
  sourceActorId?: ActorId;
  creditActorId?: ActorId;
  targetActorId?: ActorId;
  abilityId?: AbilityId;
  effectId?: EffectId;
  causeEventSeq?: number;
  attribution?: AttributionMethod;
  data: TData;
  debug?: AttributionDebug;
}

type AttributionMethod =
  | "verified"
  | "context"
  | "effectTracker"
  | "inferred"
  | "unknown";
```

`sourceActorId` is the observed actor that caused the event. `creditActorId` is
who should receive credit in analytics. For normal player damage they are the
same. For pet damage, `sourceActorId` is the pet and `creditActorId` is the
owner. This removes the need for a separate `damagePet` event type and avoids
duplicating pet semantics in both event type and flags.

### Damage

```ts
type DamageAction = "hit" | "tick" | "reflect";

type DamageEvent = CombatEventBase<"damage", DamageAction, DamageData>;

interface DamageData {
  amount: number;
  rawAmount?: number;
  mitigatedAmount?: number;
  overkillAmount?: number;
  damageType: DamageType;
  outcome: DamageOutcome;
}

type DamageType = "physical" | "magic" | "elemental" | "void" | "poison" | "unknown";

interface DamageOutcome {
  result: "landed" | "missed" | "resisted" | "absorbed" | "immune";
  critical?: true;
  blockedAmount?: number;
  resistedAmount?: number;
  absorbedAmount?: number;
}
```

A fully absorbed hit is a `damage` event with `amount: 0` and
`outcome.result: "absorbed"`. A miss/resist does not need a separate event type.

### Healing

```ts
type HealAction = "direct" | "tick" | "lifesteal" | "regen";

type HealEvent = CombatEventBase<"heal", HealAction, HealData>;

interface HealData {
  amount: number;
  rawAmount?: number;
  overhealAmount?: number;
  critical?: true;
}
```

Mana is not healing in the protocol. Mana changes are `resource` events so HPS
and mana economy cannot be accidentally mixed.

### Resource

```ts
type ResourceAction = "spend" | "restore" | "regen";

type ResourceEvent = CombatEventBase<"resource", ResourceAction, ResourceData>;

interface ResourceData {
  resource: "mana";
  delta: number;
  current?: number;
  max?: number;
}
```

`delta` is negative for spend and positive for restore/regen.

### Effect lifecycle

```ts
type EffectAction = "apply" | "refresh" | "fade";

type EffectEvent = CombatEventBase<"effect", EffectAction, EffectData>;

interface EffectData {
  stacks?: number;
  durationMs?: number;
  remainingMs?: number;
  reason?: "expired" | "dispelled" | "consumed" | "overwritten" | "unknown";
}
```

Buff/debuff polarity lives on `EffectRecord.kind`, not in separate event type
names.

### Death

```ts
type DeathEvent = CombatEventBase<"death", "die", DeathData>;

interface DeathData {
  killingBlowEventSeq?: number;
}
```

### Interrupt

```ts
type InterruptEvent = CombatEventBase<"interrupt", "interrupt", InterruptData>;

interface InterruptData {
  interruptedAbilityId?: AbilityId;
}
```

## File format

The file format stores the same logical stream in a self-contained document.

```ts
interface CombatLogFile {
  format: "erenshor.logs.export";
  schemaVersion: string; // semver, major 2
  exportedAtMs: number;
  producer: ProducerInfo;
  sessions: CombatLogSession[];
}

interface CombatLogSession {
  snapshot: SessionSnapshotPayload;
  events: CombatEventRecord[];
  ended?: SessionEndedPayload;
  derived?: DerivedData;
}

interface DerivedData {
  algorithmVersion: string;
  computedAtMs: number;
  computedFromEventSeq: number;
  summary: DerivedSummary;
}
```

Rules:

- The canonical file extension should be `.erenshorlog.json.gz`.
- A pretty `.erenshorlog.json` debug export is acceptable for development and
  support.
- Importers must validate `format` and reject unsupported major
  `schemaVersion` before deep parsing.
- Importers should recompute summaries from events unless the derived algorithm
  version exactly matches the current analyzer.
- Files must include full registries in `snapshot.registries`.
- Summaries may appear only in `derived`; lifecycle snapshots and end messages
   must not carry separate summary copies.

## Compression and payload size strategy

Use this order of optimization:

1. Move actors, abilities, and effects to registries.
2. Batch events by latency, count, and serialized byte thresholds.
3. Use session-relative integer `offsetMs` instead of full wall-clock timestamps
   per event.
4. Omit optional fields instead of emitting `null` or `false`.
5. Gzip file exports.
6. Use standard WebSocket permessage-deflate if Fleck and browser support are
   verified.
7. Consider MessagePack or another binary encoding only after measuring real
   payloads and CPU with registries/compression in place.

Do not introduce cryptic one-letter canonical field names. `sourceActorId` is
larger than `s`, but gzip and registries handle repetition, and maintainability
matters more. If a binary/compact transport is later added, it should be a
negotiated encoding of the same logical schema.

## Batching policy

The broadcaster should flush an `events` frame when any threshold is reached:

- latency target: 100 ms by default,
- event count: 256 events by default,
- serialized size: 64 KiB by default.

These values should be implementation constants or bounded config values, not
part of the schema. Registry deltas must be sent before event batches that
reference them.

## Contract source of truth

The current contract is split across C# records, TypeScript/Zod schemas, and
Markdown. The new protocol needs one source of truth.

Recommended setup:

- Store JSON Schema files under `shared/protocol/schemas/`.
- Generate or validate TypeScript/Zod types from those schemas.
- Keep C# records hand-written only if generation is too heavy for the Unity
  project, but require golden fixture parity tests both ways.
- Keep Markdown examples generated from or validated against fixtures.

Required fixtures:

- `hello`
- `sessionSnapshot` with full registries
- `registryDelta`
- `events` batch with damage, heal, resource, effect, death, interrupt
- `sessionEnded`
- `error`
- single-session file export
- multi-session file export

## Error handling rules

- Unknown live `kind`: client reports parse error and ignores the frame.
- Unsupported major protocol/schema version: client fails closed and shows a
  clear error.
- Unknown optional fields: ignore.
- Missing registry reference: protocol error; event must not be rendered as
  `Unknown` unless the producer explicitly registered an `unknown` record.
- Event sequence gap or overlap on a single connection: mark the session
  `partial` and surface a visible warning. Duplicates are not valid in the
  normal live stream; retry/resume flows must be explicit protocol features
  before they may replay overlapping event ranges.
- Producer capture warning: send `error` or include in `sessionSnapshot.diagnostics`.

## Architectural concerns beyond the wire format

### Keep the broadcaster separate from capture storage

The broadcaster is a transport adapter. It should not decide whether source facts
exist. Capture should append to a session event store first; WebSocket and file
export should read from that store.

This resolves the current silent-drop behavior when no client is connected and
makes mod-side export possible without a parallel pipeline.

### Make registries session-scoped and lifecycle-owned

Actor/ability/effect registries must reset at session start. This fixes stale
actor metadata and keeps IDs small. Mid-session registry deltas may only enrich
previously unknown stable metadata. Mutable gameplay state belongs in events or
snapshots, not registry mutation.

### Separate observed source from credited source

Do not encode pet/proc ownership as special event types. Store observed source,
credited actor, owner relationships, and cause event sequence explicitly. This is
more robust for pets, charmed NPCs, reflection, procs, and future raid mechanics.

### Treat summaries as derived caches

Summaries are useful for fast display, but they are not authoritative. Every
summary should say which event sequence and algorithm version produced it. The
web app should be able to discard and recompute it.

### Do not overfit to current incomplete capture

The protocol should support healing/resources/status/death/interrupts now, even
if implementation lands incrementally. However, UI and docs must distinguish
"schema supports this" from "current mod emits this" until each hook exists.

## Implementation cutover outline

This is not the implementation plan, but the clean cutover should happen in this
order:

1. Add shared protocol schemas and golden fixtures.
2. Replace web Zod protocol schemas with the new envelope/document model.
3. Replace C# protocol records with envelope, session snapshot, registries, and
   typed events.
4. Introduce session-scoped registry services for actors, abilities, and effects.
5. Introduce a session event store assigning `eventSeq` before broadcasting.
6. Rewrite broadcaster to send `hello`, `sessionSnapshot`, `registryDelta`,
   `events`, and `sessionEnded`.
7. Rewrite web live ingestion and file import/export to use the same normalized
   model.
8. Update docs and delete legacy snake_case/raw-session parser paths.
9. Add capture families incrementally against the new event model.

## Acceptance criteria for the redesign implementation

- A live session and an exported/imported file produce identical in-memory web
  session state and identical computed summaries.
- No combat event embeds full actor, ability, or effect display records.
- Every event has a monotonic `eventSeq` and session-relative `offsetMs`.
- A client connecting mid-session receives enough snapshot/registry state to
  render subsequent events correctly, or the session is explicitly marked
  `partial`.
- File exports are self-contained and validate before import.
- Unsupported major versions fail closed.
- Unknown registry references and sequence gaps are visible protocol errors.
- Documentation examples validate against the same schema used by tests.
