# Protocol V2 and Mod Hardening Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace the legacy combat-log protocol with protocol v2 and fix the high-priority mod correctness issues found in the 2026-05-17 audit.

**Architecture:** Use one canonical protocol model for live WebSocket frames and file exports: session metadata, session-scoped registries, append-only typed combat events, lifecycle messages, and derived summaries. The mod captures raw game facts, resolves registry IDs before assigning `eventSeq`, appends to a session event store, and broadcasts snapshots, registry deltas, event batches, and session endings. The web app validates the same model, normalizes live and imported sessions into one state shape, and recomputes summaries from events plus registries.

**Tech Stack:** C# netstandard2.1 mod with Newtonsoft.Json, HarmonyX, BepInEx config, xUnit tests; SvelteKit/Svelte 5 web app with TypeScript, Zod, Vitest; JSON Schema and golden JSON fixtures under `shared/protocol/`.

---

## Scope and sequencing

This plan implements the protocol foundation, clean v2 cutover, and audit hardening items that affect correctness or exposure. It does not add every missing combat capture family. The v2 schema and fixtures include damage, heal, resource, effect, death, and interrupt records so the API is stable, while production hooks continue emitting the currently captured families until each new hook family is implemented in its own focused slice.

Implement in this order:

1. Shared JSON Schema and fixtures.
2. Web v2 validation, normalization, import/export, and analytics.
3. C# protocol records and fixture parity.
4. Mod session registries, event store, and broadcaster cutover.
5. Audit hardening fixes that are not naturally absorbed by the v2 cutover.
6. Docs and final verification.

Commit after each task. Do not keep compatibility adapters for legacy live frames or legacy raw-session imports.

---

## File structure

### Shared protocol contract

- Create `shared/protocol/schemas/erenshor-log-v2.schema.json`
  - JSON Schema source of truth for live envelopes, session snapshots, registries, typed combat events, export files, and diagnostics.
- Create `shared/protocol/fixtures/live/hello.json`
- Create `shared/protocol/fixtures/live/session-snapshot.json`
- Create `shared/protocol/fixtures/live/registry-delta.json`
- Create `shared/protocol/fixtures/live/events.json`
- Create `shared/protocol/fixtures/live/session-ended.json`
- Create `shared/protocol/fixtures/live/error.json`
- Create `shared/protocol/fixtures/export/single-session.json`
- Create `shared/protocol/fixtures/export/multi-session.json`

### Web protocol and state

- Replace protocol sections in `web/src/lib/types/schemas.ts`
  - Keep UI/storage/settings schemas that still apply.
  - Replace legacy event/session/WebSocket schemas with protocol v2 schemas.
- Modify `web/src/lib/types/events.ts`
- Modify `web/src/lib/types/protocol.ts`
- Modify `web/src/lib/types/session.ts`
- Modify `web/src/lib/services/message-parser.ts`
- Modify `web/src/lib/services/websocket.ts`
- Modify `web/src/lib/state/sessions.svelte.ts`
- Modify `web/src/lib/services/combat-analyzer.ts`
- Modify `web/src/lib/utils/event-constants.ts`
- Modify `web/src/lib/utils/event-filters.ts`
- Modify `web/src/lib/utils/actor-utils.ts`
- Modify `web/src/lib/services/session-exporter.ts`
- Modify `web/src/lib/services/session-importer.ts`
- Create `web/src/lib/services/protocol-normalizer.ts`
- Create `web/src/lib/types/protocol-fixtures.test.ts`
- Modify existing web tests under `web/src/lib/**/*.test.ts` that construct legacy events.

### Mod protocol and storage

- Replace `mod/src/Protocol/Messages.cs`
- Modify `mod/src/Protocol/ProtocolVersion.cs`
- Modify `mod/src/Protocol/MessageSerializer.cs` only if the generic serializer needs fixture helpers.
- Replace or split `mod/src/Events/CombatEvent.cs`
- Remove legacy event reference files once unused:
  - `mod/src/Events/ActorRef.cs`
  - `mod/src/Events/AbilityRef.cs`
  - `mod/src/Events/EffectRef.cs`
  - `mod/src/Events/EventFlags.cs`
  - `mod/src/Events/EventType.cs`
  - `mod/src/Events/CombatLog.cs`
- Create `mod/src/Protocol/ProducerInfo.cs`
- Create `mod/src/Protocol/Registries.cs`
- Create `mod/src/Events/CombatEventRecord.cs`
- Create `mod/src/Session/SessionEventStore.cs`
- Create `mod/src/Session/ISessionEventStore.cs`
- Create `mod/src/Registry/ProtocolRegistry.cs`
- Create `mod/src/Registry/IProtocolRegistry.cs`
- Modify `mod/src/Registry/IActorRegistry.cs` and `mod/src/Registry/ActorRegistry.cs` only if the old actor registry remains as an adapter during the cutover.
- Modify `mod/src/Hooks/CombatEventBuilder.cs`
- Modify `mod/src/Hooks/CombatEventBuilderAdapter.cs`
- Modify `mod/src/Hooks/ICombatEventBuilder.cs`
- Modify damage hooks:
  - `mod/src/Hooks/DamageMePatch.cs`
  - `mod/src/Hooks/MagicDamageMePatch.cs`
  - `mod/src/Hooks/BleedDamageMePatch.cs`
  - `mod/src/Hooks/EnvironmentalDamageMePatch.cs`
- Replace `mod/src/Broadcast/CombatEventBroadcaster.cs`
- Modify `mod/src/Broadcast/ICombatEventBroadcaster.cs`
- Modify `mod/src/Session/SessionManager.cs`
- Modify `mod/src/Session/ISessionManager.cs`
- Modify `mod/src/Plugin.cs`

### Mod hardening

- Modify context patches:
  - `mod/src/Hooks/DoSkillPatch.cs`
  - `mod/src/Hooks/DoSkillNoChecksPatch.cs`
  - `mod/src/Hooks/ResolveSpellPatch.cs`
  - `mod/src/Hooks/DeliverDamagePatch.cs`
- Modify `mod/src/Hooks/AddStatusEffectPatch.cs`
- Modify `mod/src/Server/WebSocketServer.cs`
- Modify `mod/src/Config/ModConfig.cs`
- Update tests:
  - `mod/tests/ErenshorLogs.Tests/Protocol/MessageSerializerTests.cs`
  - `mod/tests/ErenshorLogs.Tests/Events/CombatEventTests.cs`
  - `mod/tests/ErenshorLogs.Tests/Hooks/CombatEventBuilderTests.cs`
  - `mod/tests/ErenshorLogs.Tests/Session/SessionManagerTests.cs`
  - `mod/tests/ErenshorLogs.Tests/Registry/ActorRegistryTests.cs`
  - `mod/tests/ErenshorLogs.Tests/Hooks/PatchCoverageTests.cs`
- Create tests:
  - `mod/tests/ErenshorLogs.Tests/Protocol/ProtocolFixtureTests.cs`
  - `mod/tests/ErenshorLogs.Tests/Session/SessionEventStoreTests.cs`
  - `mod/tests/ErenshorLogs.Tests/Registry/ProtocolRegistryTests.cs`
  - `mod/tests/ErenshorLogs.Tests/Hooks/ContextFinalizerTests.cs`
  - `mod/tests/ErenshorLogs.Tests/Server/WebSocketServerConfigTests.cs`

### Documentation

- Modify `docs/ARCHITECTURE.md`
- Replace stale examples in `docs/LOG_FORMAT.md`
- Modify `docs/COMBAT_EVENTS.md`

---

### Task 1: Shared protocol schema and fixtures

**Files:**
- Create: `shared/protocol/schemas/erenshor-log-v2.schema.json`
- Create: `shared/protocol/fixtures/live/hello.json`
- Create: `shared/protocol/fixtures/live/session-snapshot.json`
- Create: `shared/protocol/fixtures/live/registry-delta.json`
- Create: `shared/protocol/fixtures/live/events.json`
- Create: `shared/protocol/fixtures/live/session-ended.json`
- Create: `shared/protocol/fixtures/live/error.json`
- Create: `shared/protocol/fixtures/export/single-session.json`
- Create: `shared/protocol/fixtures/export/multi-session.json`
- Test: `web/src/lib/types/protocol-fixtures.test.ts`
- Test: `mod/tests/ErenshorLogs.Tests/Protocol/ProtocolFixtureTests.cs`

- [ ] **Step 1: Write failing web fixture tests**

Create `web/src/lib/types/protocol-fixtures.test.ts` with tests that load every fixture and validate it through Zod schemas that do not exist yet.

```ts
import { describe, expect, it } from "vitest";
import hello from "../../../../shared/protocol/fixtures/live/hello.json";
import sessionSnapshot from "../../../../shared/protocol/fixtures/live/session-snapshot.json";
import registryDelta from "../../../../shared/protocol/fixtures/live/registry-delta.json";
import events from "../../../../shared/protocol/fixtures/live/events.json";
import sessionEnded from "../../../../shared/protocol/fixtures/live/session-ended.json";
import errorFrame from "../../../../shared/protocol/fixtures/live/error.json";
import singleSessionExport from "../../../../shared/protocol/fixtures/export/single-session.json";
import multiSessionExport from "../../../../shared/protocol/fixtures/export/multi-session.json";
import { CombatLogFileSchema, LiveEnvelopeSchema } from "./schemas";

const liveFixtures = [
  hello,
  sessionSnapshot,
  registryDelta,
  events,
  sessionEnded,
  errorFrame,
];

describe("protocol v2 fixtures", () => {
  it.each(liveFixtures)("validates live fixture %#", (fixture) => {
    expect(() => LiveEnvelopeSchema.parse(fixture)).not.toThrow();
  });

  it("validates single-session export", () => {
    expect(() => CombatLogFileSchema.parse(singleSessionExport)).not.toThrow();
  });

  it("validates multi-session export", () => {
    expect(() => CombatLogFileSchema.parse(multiSessionExport)).not.toThrow();
  });
});
```

- [ ] **Step 2: Run web fixture test and verify it fails**

Run:

```bash
cd web && pnpm test:run -- src/lib/types/protocol-fixtures.test.ts
```

Expected: FAIL because `CombatLogFileSchema` and `LiveEnvelopeSchema` are not exported yet, and fixture files do not exist.

- [ ] **Step 3: Add the JSON Schema source of truth**

Create `shared/protocol/schemas/erenshor-log-v2.schema.json`. Keep the schema strict for required fields and allow unknown optional fields by leaving `additionalProperties` enabled on object definitions unless the object is a discriminated event `data` payload.

The top-level structure must include these `$defs` and anchors:

```json
{
  "$schema": "https://json-schema.org/draft/2020-12/schema",
  "$id": "https://erenshor-logs.local/schemas/erenshor-log-v2.schema.json",
  "title": "Erenshor Logs Protocol V2",
  "oneOf": [
    { "$ref": "#/$defs/liveEnvelope" },
    { "$ref": "#/$defs/combatLogFile" }
  ],
  "$defs": {
    "semverMajor2": {
      "type": "string",
      "pattern": "^2\\.[0-9]+\\.[0-9]+(?:[-+][0-9A-Za-z.-]+)?$"
    },
    "producerInfo": {
      "type": "object",
      "required": ["name"],
      "properties": {
        "name": { "enum": ["ErenshorLogsMod", "ErenshorLogsWeb"] },
        "modVersion": { "type": "string" },
        "webVersion": { "type": "string" },
        "gameVersion": { "type": "string" },
        "buildCommit": { "type": "string" }
      }
    },
    "liveEnvelope": {
      "type": "object",
      "required": [
        "protocol",
        "protocolVersion",
        "schemaVersion",
        "kind",
        "frameSeq",
        "sentAtMs",
        "payload"
      ],
      "properties": {
        "protocol": { "const": "erenshor.logs.live" },
        "protocolVersion": { "$ref": "#/$defs/semverMajor2" },
        "schemaVersion": { "$ref": "#/$defs/semverMajor2" },
        "kind": {
          "enum": [
            "hello",
            "sessionSnapshot",
            "registryDelta",
            "events",
            "sessionEnded",
            "error",
            "heartbeat",
            "serverStats"
          ]
        },
        "frameSeq": { "type": "integer", "minimum": 1 },
        "sessionId": { "type": "string", "minLength": 1 },
        "sentAtMs": { "type": "integer", "minimum": 0 },
        "payload": {}
      },
      "allOf": [
        {
          "if": { "properties": { "kind": { "const": "hello" } } },
          "then": { "properties": { "payload": { "$ref": "#/$defs/helloPayload" } } }
        },
        {
          "if": { "properties": { "kind": { "const": "sessionSnapshot" } } },
          "then": {
            "required": ["sessionId"],
            "properties": { "payload": { "$ref": "#/$defs/sessionSnapshotPayload" } }
          }
        },
        {
          "if": { "properties": { "kind": { "const": "registryDelta" } } },
          "then": {
            "required": ["sessionId"],
            "properties": { "payload": { "$ref": "#/$defs/registryDeltaPayload" } }
          }
        },
        {
          "if": { "properties": { "kind": { "const": "events" } } },
          "then": {
            "required": ["sessionId"],
            "properties": { "payload": { "$ref": "#/$defs/eventsPayload" } }
          }
        },
        {
          "if": { "properties": { "kind": { "const": "sessionEnded" } } },
          "then": {
            "required": ["sessionId"],
            "properties": { "payload": { "$ref": "#/$defs/sessionEndedPayload" } }
          }
        },
        {
          "if": { "properties": { "kind": { "const": "error" } } },
          "then": { "properties": { "payload": { "$ref": "#/$defs/errorPayload" } } }
        }
      ]
    }
  }
}
```

Add the remaining `$defs` in the same schema file with these exact structural
requirements:

```text
helloPayload: required producer, capabilities; optional activeSessionId,
  requiredCapabilities.
sessionSnapshotPayload: required sessionId, state, mode, startedAtUtcMs,
  producer, registryRevision, lastEventSeq, eventCount, completeness,
  registries; optional endedAtUtcMs, endReason, durationMs, playerActorId,
  loss, diagnostics.
registryDeltaPayload: required revision; optional actors, abilities, effects.
registries: required revision, actors, abilities, effects.
actorRecord: required id, name, kind; optional class, level, ownerActorId,
  faction, isPlayerControlled, firstSeenEventSeq.
abilityRecord: required id, name, kind; optional stableKey, damageType,
  procSource, parentAbilityId.
effectRecord: required id, name, kind; optional stableKey, sourceAbilityId,
  defaultDurationMs, maxStacks.
eventsPayload: required sessionId, registryRevision, eventSeqStart,
  eventSeqEnd, events.
combatEventRecord: oneOf damageEvent, healEvent, resourceEvent, effectEvent,
  deathEvent, interruptEvent.
damageEvent: required eventSeq, offsetMs, kind=damage, action, data.
healEvent: required eventSeq, offsetMs, kind=heal, action, data.
resourceEvent: required eventSeq, offsetMs, kind=resource, action, data.
effectEvent: required eventSeq, offsetMs, kind=effect, action, data.
deathEvent: required eventSeq, offsetMs, kind=death, action=die, data.
interruptEvent: required eventSeq, offsetMs, kind=interrupt,
  action=interrupt, data.
damageData: required amount, damageType, outcome; optional rawAmount,
  mitigatedAmount, overkillAmount.
healData: required amount; optional rawAmount, overhealAmount, critical.
resourceData: required resource=mana, delta; optional current, max.
effectData: optional stacks, durationMs, remainingMs, reason.
deathData: optional killingBlowEventSeq.
interruptData: optional interruptedAbilityId.
sessionEndedPayload: required sessionId, endedAtUtcMs, endedAtEventSeq,
  reason, durationMs; optional diagnostics.
errorPayload: required code, severity, message, recoverable; optional
  sessionId, eventSeq, details.
combatLogFile: required format=erenshor.logs.export, schemaVersion,
  exportedAtMs, producer, sessions.
combatLogSession: required snapshot, events; optional ended, derived.
derivedData: required algorithmVersion, computedAtMs, computedFromEventSeq,
  summary.
derivedSummary: required totalDamage, totalHealing, totalDamageTaken,
  totalHealingReceived, durationMs.
```

For `combatEventRecord`, use `oneOf` with `kind` discriminators. For every event
`data` payload, set `additionalProperties: false` so invalid sparse
combinations fail validation.

- [ ] **Step 4: Add golden live fixtures**

Create `shared/protocol/fixtures/live/hello.json`:

```json
{
  "protocol": "erenshor.logs.live",
  "protocolVersion": "2.0.0",
  "schemaVersion": "2.0.0",
  "kind": "hello",
  "frameSeq": 1,
  "sentAtMs": 1800000000000,
  "payload": {
    "producer": {
      "name": "ErenshorLogsMod",
      "modVersion": "2026.5.17.14",
      "gameVersion": "playtest-23258843",
      "buildCommit": "14d8862"
    },
    "activeSessionId": "session-1",
    "capabilities": ["registryDelta", "sessionSnapshot", "gzipFileExport"]
  }
}
```

Create `shared/protocol/fixtures/live/session-snapshot.json`:

```json
{
  "protocol": "erenshor.logs.live",
  "protocolVersion": "2.0.0",
  "schemaVersion": "2.0.0",
  "kind": "sessionSnapshot",
  "frameSeq": 2,
  "sessionId": "session-1",
  "sentAtMs": 1800000000100,
  "payload": {
    "sessionId": "session-1",
    "state": "active",
    "mode": "automatic",
    "startedAtUtcMs": 1800000000000,
    "producer": {
      "name": "ErenshorLogsMod",
      "modVersion": "2026.5.17.14",
      "gameVersion": "playtest-23258843"
    },
    "playerActorId": "a1",
    "registryRevision": 3,
    "lastEventSeq": 0,
    "eventCount": 0,
    "completeness": "complete",
    "registries": {
      "revision": 3,
      "actors": {
        "a1": {
          "id": "a1",
          "name": "Player",
          "kind": "player",
          "class": "Duelist",
          "level": 20,
          "faction": "friendly",
          "isPlayerControlled": true
        },
        "a2": {
          "id": "a2",
          "name": "Backstabber",
          "kind": "simPlayer",
          "class": "Assassin",
          "level": 20,
          "faction": "friendly"
        },
        "a3": {
          "id": "a3",
          "name": "Raid Boss",
          "kind": "npc",
          "level": 25,
          "faction": "hostile"
        }
      },
      "abilities": {
        "ab1": {
          "id": "ab1",
          "name": "Backstab",
          "kind": "skill",
          "stableKey": "skill:101",
          "damageType": "physical"
        }
      },
      "effects": {}
    },
    "diagnostics": {
      "hookWarnings": [],
      "attributionFailures": 0,
      "droppedEvents": 0,
      "droppedFrames": 0,
      "serializationErrors": 0
    }
  }
}
```

Create `shared/protocol/fixtures/live/registry-delta.json`:

```json
{
  "protocol": "erenshor.logs.live",
  "protocolVersion": "2.0.0",
  "schemaVersion": "2.0.0",
  "kind": "registryDelta",
  "frameSeq": 3,
  "sessionId": "session-1",
  "sentAtMs": 1800000000200,
  "payload": {
    "revision": 5,
    "abilities": {
      "ab2": {
        "id": "ab2",
        "name": "Poisoned Wound",
        "kind": "dot",
        "stableKey": "spell:202",
        "damageType": "poison",
        "parentAbilityId": "ab1"
      },
      "ab3": {
        "id": "ab3",
        "name": "Heal",
        "kind": "spell",
        "stableKey": "spell:303"
      },
      "ab4": {
        "id": "ab4",
        "name": "Meteor",
        "kind": "spell",
        "stableKey": "spell:404",
        "damageType": "magic"
      }
    },
    "effects": {
      "ef1": {
        "id": "ef1",
        "name": "Poisoned Wound",
        "kind": "debuff",
        "stableKey": "spell:202",
        "sourceAbilityId": "ab2",
        "defaultDurationMs": 12000,
        "maxStacks": 1
      }
    }
  }
}
```

Create `shared/protocol/fixtures/live/events.json` with one event of each supported kind:

```json
{
  "protocol": "erenshor.logs.live",
  "protocolVersion": "2.0.0",
  "schemaVersion": "2.0.0",
  "kind": "events",
  "frameSeq": 4,
  "sessionId": "session-1",
  "sentAtMs": 1800000000300,
  "payload": {
    "sessionId": "session-1",
    "registryRevision": 5,
    "eventSeqStart": 1,
    "eventSeqEnd": 6,
    "events": [
      {
        "eventSeq": 1,
        "offsetMs": 120,
        "kind": "damage",
        "action": "hit",
        "sourceActorId": "a2",
        "creditActorId": "a2",
        "targetActorId": "a3",
        "abilityId": "ab1",
        "attribution": "context",
        "data": {
          "amount": 350,
          "rawAmount": 400,
          "mitigatedAmount": 50,
          "damageType": "physical",
          "outcome": { "result": "landed", "critical": true }
        }
      },
      {
        "eventSeq": 2,
        "offsetMs": 1000,
        "kind": "heal",
        "action": "direct",
        "sourceActorId": "a1",
        "creditActorId": "a1",
        "targetActorId": "a2",
        "abilityId": "ab3",
        "attribution": "verified",
        "data": { "amount": 125, "rawAmount": 160, "overhealAmount": 35 }
      },
      {
        "eventSeq": 3,
        "offsetMs": 1250,
        "kind": "resource",
        "action": "spend",
        "sourceActorId": "a1",
        "targetActorId": "a1",
        "abilityId": "ab3",
        "data": { "resource": "mana", "delta": -35, "current": 465, "max": 500 }
      },
      {
        "eventSeq": 4,
        "offsetMs": 1400,
        "kind": "effect",
        "action": "apply",
        "sourceActorId": "a2",
        "creditActorId": "a2",
        "targetActorId": "a3",
        "abilityId": "ab2",
        "effectId": "ef1",
        "data": { "stacks": 1, "durationMs": 12000 }
      },
      {
        "eventSeq": 5,
        "offsetMs": 5200,
        "kind": "interrupt",
        "action": "interrupt",
        "sourceActorId": "a2",
        "targetActorId": "a3",
        "abilityId": "ab1",
        "data": { "interruptedAbilityId": "ab4" }
      },
      {
        "eventSeq": 6,
        "offsetMs": 18000,
        "kind": "death",
        "action": "die",
        "sourceActorId": "a2",
        "creditActorId": "a2",
        "targetActorId": "a3",
        "causeEventSeq": 1,
        "data": { "killingBlowEventSeq": 1 }
      }
    ]
  }
}
```


Create `shared/protocol/fixtures/live/session-ended.json`:

```json
{
  "protocol": "erenshor.logs.live",
  "protocolVersion": "2.0.0",
  "schemaVersion": "2.0.0",
  "kind": "sessionEnded",
  "frameSeq": 5,
  "sessionId": "session-1",
  "sentAtMs": 1800000020000,
  "payload": {
    "sessionId": "session-1",
    "endedAtUtcMs": 1800000019000,
    "endedAtEventSeq": 6,
    "reason": "inactivity",
    "durationMs": 19000,
    "diagnostics": {
      "hookWarnings": [],
      "attributionFailures": 0,
      "droppedEvents": 0,
      "droppedFrames": 0,
      "serializationErrors": 0
    }
  }
}
```

Create `shared/protocol/fixtures/live/error.json`:

```json
{
  "protocol": "erenshor.logs.live",
  "protocolVersion": "2.0.0",
  "schemaVersion": "2.0.0",
  "kind": "error",
  "frameSeq": 6,
  "sentAtMs": 1800000021000,
  "payload": {
    "code": "hookCompatibilityWarning",
    "severity": "warning",
    "message": "Optional status-effect overload was not found in this game build.",
    "recoverable": true,
    "sessionId": "session-1",
    "details": { "hook": "Stats.AddStatusEffect" }
  }
}
```

- [ ] **Step 5: Add export fixtures**

Create `single-session.json` as a self-contained export document. Its
`sessions[0].snapshot` must be the ended form of the `session-snapshot.json`
payload, its `sessions[0].events` must be the exact six-event array from
`events.json`, and its `sessions[0].ended` must be the payload from
`session-ended.json`.

Required top-level and derived fields:

```json
{
  "format": "erenshor.logs.export",
  "schemaVersion": "2.0.0",
  "exportedAtMs": 1800000022000,
  "producer": { "name": "ErenshorLogsWeb", "webVersion": "2.0.0" },
  "sessions": [
    {
      "snapshot": {
        "sessionId": "session-1",
        "state": "ended",
        "mode": "automatic",
        "startedAtUtcMs": 1800000000000,
        "endedAtUtcMs": 1800000019000,
        "endReason": "inactivity",
        "durationMs": 19000,
        "producer": { "name": "ErenshorLogsMod", "modVersion": "2026.5.17.14" },
        "playerActorId": "a1",
        "registryRevision": 5,
        "lastEventSeq": 6,
        "eventCount": 6,
        "completeness": "complete",
        "registries": {
          "revision": 5,
          "actors": {
            "a1": { "id": "a1", "name": "Player", "kind": "player" },
            "a2": { "id": "a2", "name": "Backstabber", "kind": "simPlayer" },
            "a3": { "id": "a3", "name": "Raid Boss", "kind": "npc" }
          },
          "abilities": {
            "ab1": { "id": "ab1", "name": "Backstab", "kind": "skill" },
            "ab2": { "id": "ab2", "name": "Poisoned Wound", "kind": "dot" },
            "ab3": { "id": "ab3", "name": "Heal", "kind": "spell" },
            "ab4": { "id": "ab4", "name": "Meteor", "kind": "spell" }
          },
          "effects": {
            "ef1": { "id": "ef1", "name": "Poisoned Wound", "kind": "debuff" }
          }
        }
      },
      "events": [
        {
          "eventSeq": 1,
          "offsetMs": 120,
          "kind": "damage",
          "action": "hit",
          "sourceActorId": "a2",
          "creditActorId": "a2",
          "targetActorId": "a3",
          "abilityId": "ab1",
          "attribution": "context",
          "data": {
            "amount": 350,
            "rawAmount": 400,
            "mitigatedAmount": 50,
            "damageType": "physical",
            "outcome": { "result": "landed", "critical": true }
          }
        },
        {
          "eventSeq": 2,
          "offsetMs": 1000,
          "kind": "heal",
          "action": "direct",
          "sourceActorId": "a1",
          "creditActorId": "a1",
          "targetActorId": "a2",
          "abilityId": "ab3",
          "attribution": "verified",
          "data": { "amount": 125, "rawAmount": 160, "overhealAmount": 35 }
        },
        {
          "eventSeq": 3,
          "offsetMs": 1250,
          "kind": "resource",
          "action": "spend",
          "sourceActorId": "a1",
          "targetActorId": "a1",
          "abilityId": "ab3",
          "data": { "resource": "mana", "delta": -35, "current": 465, "max": 500 }
        },
        {
          "eventSeq": 4,
          "offsetMs": 1400,
          "kind": "effect",
          "action": "apply",
          "sourceActorId": "a2",
          "creditActorId": "a2",
          "targetActorId": "a3",
          "abilityId": "ab2",
          "effectId": "ef1",
          "data": { "stacks": 1, "durationMs": 12000 }
        },
        {
          "eventSeq": 5,
          "offsetMs": 5200,
          "kind": "interrupt",
          "action": "interrupt",
          "sourceActorId": "a2",
          "targetActorId": "a3",
          "abilityId": "ab1",
          "data": { "interruptedAbilityId": "ab4" }
        },
        {
          "eventSeq": 6,
          "offsetMs": 18000,
          "kind": "death",
          "action": "die",
          "sourceActorId": "a2",
          "creditActorId": "a2",
          "targetActorId": "a3",
          "causeEventSeq": 1,
          "data": { "killingBlowEventSeq": 1 }
        }
      ],
      "ended": {
        "sessionId": "session-1",
        "endedAtUtcMs": 1800000019000,
        "endedAtEventSeq": 6,
        "reason": "inactivity",
        "durationMs": 19000
      },
      "derived": {
        "algorithmVersion": "2.0.0",
        "computedAtMs": 1800000022000,
        "computedFromEventSeq": 6,
        "summary": {
          "totalDamage": 350,
          "totalHealing": 125,
          "totalDamageTaken": 0,
          "totalHealingReceived": 125,
          "durationMs": 19000
        }
      }
    }
  ]
}
```


Create `multi-session.json` with the same first session plus this second session:

```json
{
  "snapshot": {
    "sessionId": "session-2",
    "state": "ended",
    "mode": "manual",
    "startedAtUtcMs": 1800000100000,
    "endedAtUtcMs": 1800000105000,
    "endReason": "manual",
    "durationMs": 5000,
    "producer": { "name": "ErenshorLogsMod", "modVersion": "2026.5.17.14" },
    "registryRevision": 0,
    "lastEventSeq": 0,
    "eventCount": 0,
    "completeness": "complete",
    "registries": { "revision": 0, "actors": {}, "abilities": {}, "effects": {} }
  },
  "events": [],
  "ended": {
    "sessionId": "session-2",
    "endedAtUtcMs": 1800000105000,
    "endedAtEventSeq": 0,
    "reason": "manual",
    "durationMs": 5000
  }
}
```

- [ ] **Step 6: Add web schemas needed by the fixture test**

Replace the legacy protocol schemas in `web/src/lib/types/schemas.ts` with Zod v2 schemas. Keep names aligned with the spec and JSON Schema.

Use these core definitions:

```ts
export const ProtocolVersionSchema = z
  .string()
  .regex(/^2\.[0-9]+\.[0-9]+(?:[-+][0-9A-Za-z.-]+)?$/);
export const SchemaVersionSchema = ProtocolVersionSchema;

export const ProducerInfoSchema = z.object({
  name: z.enum(["ErenshorLogsMod", "ErenshorLogsWeb"]),
  modVersion: z.string().optional(),
  webVersion: z.string().optional(),
  gameVersion: z.string().optional(),
  buildCommit: z.string().optional(),
});

export const ActorRecordSchema = z.object({
  id: z.string(),
  name: z.string(),
  kind: z.enum(["player", "simPlayer", "npc", "pet", "environment", "unknown"]),
  class: z.string().optional(),
  level: z.number().optional(),
  ownerActorId: z.string().optional(),
  faction: z.enum(["friendly", "hostile", "neutral", "unknown"]).optional(),
  isPlayerControlled: z.boolean().optional(),
  firstSeenEventSeq: z.number().int().positive().optional(),
});

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
    "unknown",
  ]),
  stableKey: z.string().optional(),
  damageType: z
    .enum(["physical", "magic", "elemental", "void", "poison", "unknown"])
    .optional(),
  procSource: z.enum(["weapon", "wand", "bow", "buff", "skill"]).optional(),
  parentAbilityId: z.string().optional(),
});

export const EffectRecordSchema = z.object({
  id: z.string(),
  name: z.string(),
  kind: z.enum(["buff", "debuff", "unknown"]),
  stableKey: z.string().optional(),
  sourceAbilityId: z.string().optional(),
  defaultDurationMs: z.number().int().nonnegative().optional(),
  maxStacks: z.number().int().positive().optional(),
});

export const RegistriesSchema = z.object({
  revision: z.number().int().nonnegative(),
  actors: z.record(z.string(), ActorRecordSchema),
  abilities: z.record(z.string(), AbilityRecordSchema),
  effects: z.record(z.string(), EffectRecordSchema),
});
```

Define typed combat event schemas using `z.discriminatedUnion("kind", [...])`. Each event shares `eventSeq`, `offsetMs`, optional actor/ability/effect IDs, `attribution`, and typed `data`.

For live envelopes, use `superRefine` to enforce payload shape by `kind`:

```ts
export const LiveEnvelopeSchema = z
  .object({
    protocol: z.literal("erenshor.logs.live"),
    protocolVersion: ProtocolVersionSchema,
    schemaVersion: SchemaVersionSchema,
    kind: z.enum([
      "hello",
      "sessionSnapshot",
      "registryDelta",
      "events",
      "sessionEnded",
      "error",
      "heartbeat",
      "serverStats",
    ]),
    frameSeq: z.number().int().positive(),
    sessionId: z.string().optional(),
    sentAtMs: z.number().int().nonnegative(),
    payload: z.unknown(),
  })
  .superRefine((value, ctx) => {
    const schemaByKind = {
      hello: HelloPayloadSchema,
      sessionSnapshot: SessionSnapshotPayloadSchema,
      registryDelta: RegistryDeltaPayloadSchema,
      events: EventsPayloadSchema,
      sessionEnded: SessionEndedPayloadSchema,
      error: ErrorPayloadSchema,
      heartbeat: z.object({}).passthrough(),
      serverStats: ServerStatsPayloadSchema,
    } satisfies Record<LiveEnvelopeKind, z.ZodType>;

    const payload = schemaByKind[value.kind].safeParse(value.payload);
    if (!payload.success) {
      for (const issue of payload.error.issues) {
        ctx.addIssue({ ...issue, path: ["payload", ...issue.path] });
      }
    }

    if (value.kind !== "hello" && value.kind !== "error" && !value.sessionId) {
      ctx.addIssue({
        code: "custom",
        path: ["sessionId"],
        message: "sessionId is required for session-scoped frames",
      });
    }
  });
```

Export TypeScript types with `z.infer` for every schema used by services.

- [ ] **Step 7: Run web fixture tests and verify they pass**

Run:

```bash
cd web && pnpm test:run -- src/lib/types/protocol-fixtures.test.ts
```

Expected: PASS.

- [ ] **Step 8: Add failing C# fixture tests**

Create `mod/tests/ErenshorLogs.Tests/Protocol/ProtocolFixtureTests.cs`:

```csharp
using ErenshorLogs.Protocol;
using Xunit;

namespace ErenshorLogs.Tests.Protocol;

public class ProtocolFixtureTests
{
  public static TheoryData<string> LiveFixtures =>
    new()
    {
      "hello",
      "session-snapshot",
      "registry-delta",
      "events",
      "session-ended",
      "error",
    };

  [Theory]
  [MemberData(nameof(LiveFixtures))]
  public void Deserialize_LiveFixture_RoundTrips(string fixtureName)
  {
    var json = ReadFixture($"live/{fixtureName}.json");

    var frame = MessageSerializer.Deserialize<LiveEnvelope>(json);
    var serialized = MessageSerializer.Serialize(frame!);
    var reparsed = MessageSerializer.Deserialize<LiveEnvelope>(serialized);

    Assert.NotNull(frame);
    Assert.NotNull(reparsed);
    Assert.Equal("erenshor.logs.live", frame.Protocol);
    Assert.StartsWith("2.", frame.ProtocolVersion);
    Assert.StartsWith("2.", frame.SchemaVersion);
  }

  [Theory]
  [InlineData("export/single-session.json", 1)]
  [InlineData("export/multi-session.json", 2)]
  public void Deserialize_ExportFixture_RoundTrips(string fixturePath, int expectedSessions)
  {
    var json = ReadFixture(fixturePath);

    var file = MessageSerializer.Deserialize<CombatLogFile>(json);
    var serialized = MessageSerializer.Serialize(file!);
    var reparsed = MessageSerializer.Deserialize<CombatLogFile>(serialized);

    Assert.NotNull(file);
    Assert.NotNull(reparsed);
    Assert.Equal("erenshor.logs.export", file.Format);
    Assert.Equal(expectedSessions, file.Sessions.Count);
  }

  private static string ReadFixture(string relativePath)
  {
    var path = Path.Combine(
      AppContext.BaseDirectory,
      "..",
      "..",
      "..",
      "..",
      "..",
      "shared",
      "protocol",
      "fixtures",
      relativePath
    );
    return File.ReadAllText(Path.GetFullPath(path));
  }
}
```

- [ ] **Step 9: Run C# fixture tests and verify they fail**

Run:

```bash
cd mod && dotnet test tests/ErenshorLogs.Tests --filter FullyQualifiedName~ProtocolFixtureTests
```

Expected: FAIL because `LiveEnvelope` and `CombatLogFile` do not exist yet.

- [ ] **Step 10: Commit schema and fixture foundation**

Run:

```bash
git add shared/protocol web/src/lib/types/schemas.ts web/src/lib/types/protocol-fixtures.test.ts mod/tests/ErenshorLogs.Tests/Protocol/ProtocolFixtureTests.cs
git commit -m "test(protocol): add v2 contract fixtures" -m "Add JSON Schema and golden fixtures for the protocol v2 live frames and export document.

Validate the fixtures from the web side first, and add failing C# parity tests so the mod cutover has a concrete serialization contract to satisfy."
```

---

### Task 2: Web v2 parser and normalized session state

**Files:**
- Modify: `web/src/lib/types/protocol.ts`
- Modify: `web/src/lib/types/events.ts`
- Modify: `web/src/lib/types/session.ts`
- Modify: `web/src/lib/services/message-parser.ts`
- Modify: `web/src/lib/services/message-parser.test.ts`
- Create: `web/src/lib/services/protocol-normalizer.ts`
- Modify: `web/src/lib/state/sessions.svelte.ts`
- Modify: `web/src/lib/state/sessions.test.ts`

- [ ] **Step 1: Write failing parser tests for v2 envelopes and old-frame rejection**

Replace legacy parser expectations in `web/src/lib/services/message-parser.test.ts` with v2 behavior:

```ts
import { describe, expect, it } from "vitest";
import hello from "../../../../shared/protocol/fixtures/live/hello.json";
import { isParseError, parseMessage } from "./message-parser";

const asJson = (value: unknown) => JSON.stringify(value);

describe("parseMessage", () => {
  it("parses protocol v2 hello envelopes", () => {
    const result = parseMessage(asJson(hello));

    expect(isParseError(result)).toBe(false);
    if (isParseError(result)) return;
    expect(result.protocol).toBe("erenshor.logs.live");
    expect(result.kind).toBe("hello");
  });

  it("rejects legacy type-based frames", () => {
    const result = parseMessage(
      asJson({ type: "handshake", protocolVersion: "1.0.0", modVersion: "1.0.0" })
    );

    expect(isParseError(result)).toBe(true);
    if (isParseError(result)) {
      expect(result.code).toBe("missing_protocol");
    }
  });

  it("rejects unsupported major protocol versions", () => {
    const result = parseMessage(
      asJson({ ...hello, protocolVersion: "3.0.0" })
    );

    expect(isParseError(result)).toBe(true);
    if (isParseError(result)) {
      expect(result.code).toBe("unsupported_version");
    }
  });
});
```

- [ ] **Step 2: Run parser tests and verify they fail**

Run:

```bash
cd web && pnpm test:run -- src/lib/services/message-parser.test.ts
```

Expected: FAIL because the parser still expects `type` and legacy error codes.

- [ ] **Step 3: Replace protocol type re-exports**

Update `web/src/lib/types/protocol.ts` to re-export v2 names only:

```ts
export type {
  Capability,
  CombatLogFile,
  CombatLogSession,
  ErrorPayload,
  EventsPayload,
  HelloPayload,
  LiveEnvelope,
  LiveEnvelopeKind,
  ParseError,
  ParseErrorCode,
  ProducerInfo,
  RegistryDeltaPayload,
  ServerStatsPayload,
  SessionEndedPayload,
  SessionSnapshotPayload,
} from "./schemas";

export {
  CombatLogFileSchema,
  CombatLogSessionSchema,
  ErrorPayloadSchema,
  EventsPayloadSchema,
  HelloPayloadSchema,
  LiveEnvelopeSchema,
  ParseErrorCodeSchema,
  ParseErrorSchema,
  ProducerInfoSchema,
  RegistryDeltaPayloadSchema,
  ServerStatsPayloadSchema,
  SessionEndedPayloadSchema,
  SessionSnapshotPayloadSchema,
} from "./schemas";
```

Update `web/src/lib/types/events.ts` to re-export v2 registry and event names:

```ts
export type {
  AbilityRecord,
  ActorRecord,
  AttributionDebug,
  AttributionMethod,
  CombatEventRecord,
  DamageData,
  DamageEvent,
  DamageOutcome,
  DamageType,
  DeathEvent,
  EffectEvent,
  EffectRecord,
  HealEvent,
  InterruptEvent,
  Registries,
  ResourceEvent,
} from "./schemas";

export {
  AbilityRecordSchema,
  ActorRecordSchema,
  CombatEventRecordSchema,
  DamageDataSchema,
  DamageEventSchema,
  DamageOutcomeSchema,
  DamageTypeSchema,
  DeathEventSchema,
  EffectEventSchema,
  EffectRecordSchema,
  HealEventSchema,
  InterruptEventSchema,
  RegistriesSchema,
  ResourceEventSchema,
} from "./schemas";
```

- [ ] **Step 4: Replace parser implementation**

Change `web/src/lib/services/message-parser.ts`:

```ts
import { LiveEnvelopeSchema } from "$lib/types/schemas";
import type { LiveEnvelope, ParseError } from "$lib/types/protocol";

const KNOWN_KINDS = new Set([
  "hello",
  "sessionSnapshot",
  "registryDelta",
  "events",
  "sessionEnded",
  "error",
  "heartbeat",
  "serverStats",
]);

export function parseMessage(json: string): LiveEnvelope | ParseError {
  let parsed: unknown;
  try {
    parsed = JSON.parse(json);
  } catch (error) {
    return {
      code: "invalid_json",
      message: error instanceof Error ? error.message : "Invalid JSON",
      raw: json.slice(0, 200),
    };
  }

  if (typeof parsed !== "object" || parsed === null || !("protocol" in parsed)) {
    return {
      code: "missing_protocol",
      message: "Message missing 'protocol' field",
      raw: json.slice(0, 200),
    };
  }

  const record = parsed as Record<string, unknown>;
  if (record.protocol !== "erenshor.logs.live") {
    return {
      code: "unknown_protocol",
      message: `Unknown protocol: ${String(record.protocol)}`,
      raw: json.slice(0, 200),
    };
  }

  if (typeof record.protocolVersion !== "string" || !record.protocolVersion.startsWith("2.")) {
    return {
      code: "unsupported_version",
      message: `Unsupported protocol version: ${String(record.protocolVersion)}`,
      raw: json.slice(0, 200),
    };
  }

  if (typeof record.kind !== "string" || !KNOWN_KINDS.has(record.kind)) {
    return {
      code: "unknown_kind",
      message: `Unknown message kind: ${String(record.kind)}`,
      raw: json.slice(0, 200),
    };
  }

  const result = LiveEnvelopeSchema.safeParse(parsed);
  if (result.success) return result.data;

  return {
    code: "invalid_structure",
    message: result.error.issues.map((i) => `${i.path.join(".")}: ${i.message}`).join("; "),
    raw: json.slice(0, 200),
  };
}

export function isParseError(result: LiveEnvelope | ParseError): result is ParseError {
  return "code" in result && !("protocol" in result);
}
```

Update `ParseErrorCodeSchema` in `schemas.ts` to include:

```ts
export const ParseErrorCodeSchema = z.enum([
  "invalid_json",
  "missing_protocol",
  "unknown_protocol",
  "unsupported_version",
  "unknown_kind",
  "invalid_structure",
]);
```

- [ ] **Step 5: Add normalized session state tests**

Create tests in `web/src/lib/state/sessions.test.ts` or a new focused file if existing tests are large. Cover snapshot replacement, registry deltas, ordered events, gap errors, and session ending:

```ts
import { beforeEach, describe, expect, it } from "vitest";
import eventsFrame from "../../../../shared/protocol/fixtures/live/events.json";
import registryDeltaFrame from "../../../../shared/protocol/fixtures/live/registry-delta.json";
import sessionEndedFrame from "../../../../shared/protocol/fixtures/live/session-ended.json";
import snapshotFrame from "../../../../shared/protocol/fixtures/live/session-snapshot.json";
import {
  applyLiveEnvelope,
  protocolErrors,
  resetSessionsState,
  sessions,
} from "./sessions.svelte";
import type { LiveEnvelope } from "$lib/types";

describe("protocol v2 session state", () => {
  beforeEach(() => resetSessionsState());

  it("replaces retained session state when a snapshot arrives", () => {
    applyLiveEnvelope(snapshotFrame as LiveEnvelope);
    applyLiveEnvelope(eventsFrame as LiveEnvelope);

    expect(sessions.get("session-1")?.events).toHaveLength(6);

    applyLiveEnvelope(snapshotFrame as LiveEnvelope);

    expect(sessions.get("session-1")?.events).toHaveLength(0);
    expect(sessions.get("session-1")?.registries.actors.a1.name).toBe("Player");
  });

  it("applies registry deltas before event batches", () => {
    applyLiveEnvelope(snapshotFrame as LiveEnvelope);
    applyLiveEnvelope(registryDeltaFrame as LiveEnvelope);
    applyLiveEnvelope(eventsFrame as LiveEnvelope);

    const session = sessions.get("session-1")!;
    expect(session.registries.effects.ef1.name).toBe("Poisoned Wound");
    expect(session.events[0].eventSeq).toBe(1);
  });

  it("marks sequence gaps as visible protocol errors", () => {
    applyLiveEnvelope(snapshotFrame as LiveEnvelope);
    applyLiveEnvelope({
      ...(eventsFrame as LiveEnvelope),
      payload: {
        ...(eventsFrame as LiveEnvelope).payload,
        eventSeqStart: 2,
      },
    } as LiveEnvelope);

    expect(protocolErrors.value).toContainEqual(
      expect.objectContaining({ code: "event_sequence_gap", sessionId: "session-1" })
    );
    expect(sessions.get("session-1")?.completeness).toBe("partial");
  });

  it("records session end metadata", () => {
    applyLiveEnvelope(snapshotFrame as LiveEnvelope);
    applyLiveEnvelope(sessionEndedFrame as LiveEnvelope);

    expect(sessions.get("session-1")?.endedAtUtcMs).toBe(1800000019000);
  });
});
```

- [ ] **Step 6: Run state tests and verify they fail**

Run:

```bash
cd web && pnpm test:run -- src/lib/state/sessions.test.ts
```

Expected: FAIL because `applyLiveEnvelope`, v2 `Session`, and `protocolErrors` do not exist yet.

- [ ] **Step 7: Replace web session shape and live application logic**

Update `SessionSchema` in `schemas.ts` to represent normalized protocol state:

```ts
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
```

Update `web/src/lib/state/sessions.svelte.ts`:

```ts
import type {
  CombatEventRecord,
  LiveEnvelope,
  ProtocolError,
  RegistryDeltaPayload,
  Session,
  SessionEndedPayload,
  SessionSnapshotPayload,
} from "$lib/types";

const errors = $state<{ values: ProtocolError[] }>({ values: [] });

export const protocolErrors = {
  get value() {
    return errors.values;
  },
};

export function applyLiveEnvelope(envelope: LiveEnvelope): void {
  switch (envelope.kind) {
    case "hello":
      return;
    case "sessionSnapshot":
      applySessionSnapshot(envelope.payload as SessionSnapshotPayload);
      return;
    case "registryDelta":
      applyRegistryDelta(envelope.sessionId!, envelope.payload as RegistryDeltaPayload);
      return;
    case "events":
      appendProtocolEvents(envelope.sessionId!, (envelope.payload as { events: CombatEventRecord[] }).events);
      return;
    case "sessionEnded":
      applySessionEnded(envelope.payload as SessionEndedPayload);
      return;
    case "error":
      errors.values = [
        ...errors.values,
        {
          code: envelope.payload.code,
          message: envelope.payload.message,
          sessionId: envelope.payload.sessionId,
          eventSeq: envelope.payload.eventSeq,
        },
      ];
      return;
    case "heartbeat":
    case "serverStats":
      return;
  }
}

export function applySessionSnapshot(snapshot: SessionSnapshotPayload): void {
  const session: Session = {
    id: snapshot.sessionId,
    mode: snapshot.mode,
    state: snapshot.state,
    startedAtUtcMs: snapshot.startedAtUtcMs,
    endedAtUtcMs: snapshot.endedAtUtcMs,
    endReason: snapshot.endReason,
    durationMs: snapshot.durationMs,
    producer: snapshot.producer,
    playerActorId: snapshot.playerActorId,
    registryRevision: snapshot.registryRevision,
    lastEventSeq: snapshot.lastEventSeq,
    eventCount: snapshot.eventCount,
    completeness: snapshot.completeness,
    loss: snapshot.loss,
    registries: snapshot.registries,
    diagnostics: snapshot.diagnostics,
    events: [],
    protocolErrors: [],
  };
  sessions.set(snapshot.sessionId, session);
  state.activeSessionId = snapshot.sessionId;
}
```

Implement `applyRegistryDelta`, `appendProtocolEvents`, `applySessionEnded`, and `recordProtocolError` in the same file. `appendProtocolEvents` must reject batches whose first `eventSeq` is not `session.lastEventSeq + 1`, mark `completeness: "partial"`, and record a `event_sequence_gap` error instead of appending invalid events.

- [ ] **Step 8: Update WebSocket callbacks to v2 frames**

Modify `web/src/lib/services/websocket.ts`:

```ts
import type { LiveEnvelope } from "$lib/types";

export interface WebSocketCallbacks {
  onConnecting: () => void;
  onConnected: (hello: LiveEnvelope) => void;
  onFrame: (message: LiveEnvelope) => void;
  onDisconnected: () => void;
  onError: (
    code: "connection_failed" | "parse_error" | "unexpected_disconnect",
    message: string
  ) => void;
}
```

Replace `handleMessage` with:

```ts
function handleMessage(message: LiveEnvelope): void {
  if (message.kind === "hello") {
    callbacks.onConnected(message);
  }
  callbacks.onFrame(message);
}
```

Remove `onSessionStart`, `onSessionEnd`, and `onCombatEvents` callbacks.

- [ ] **Step 9: Run focused web tests and fix call sites**

Run:

```bash
cd web && pnpm test:run -- src/lib/services/message-parser.test.ts src/lib/state/sessions.test.ts
```

Expected after implementation: PASS. If component/state tests fail because they still use `startTime`, `endTime`, or legacy `eventType`, update those tests and code paths to `startedAtUtcMs`, `endedAtUtcMs`, and typed `kind`/`data`.

- [ ] **Step 10: Commit web parser and state cutover**

Run:

```bash
git add web/src/lib/types web/src/lib/services/message-parser.ts web/src/lib/services/message-parser.test.ts web/src/lib/services/websocket.ts web/src/lib/state/sessions.svelte.ts web/src/lib/state/sessions.test.ts
git commit -m "feat(web): parse protocol v2 frames" -m "Replace legacy type-based WebSocket parsing with protocol v2 envelopes.

Normalize snapshots, registry deltas, ordered event batches, and session endings into one session state shape so live data and imported files can share the same path."
```

---

### Task 3: Web import, export, and analytics on registries

**Files:**
- Modify: `web/src/lib/services/session-importer.ts`
- Modify: `web/src/lib/services/session-exporter.ts`
- Modify: `web/src/lib/services/combat-analyzer.ts`
- Modify: `web/src/lib/utils/event-constants.ts`
- Modify: `web/src/lib/utils/event-filters.ts`
- Modify: `web/src/lib/utils/actor-utils.ts`
- Modify tests for those files.

- [ ] **Step 1: Write failing import/export tests**

Update or create tests that import `single-session.json`, reject old raw sessions, and export the v2 file shape:

```ts
import { describe, expect, it, vi } from "vitest";
import singleSessionExport from "../../../../shared/protocol/fixtures/export/single-session.json";
import { importSessions } from "./session-importer";
import { buildCombatLogFile } from "./session-exporter";

vi.mock("$lib/utils/download", () => ({ downloadJSON: vi.fn() }));

describe("session import/export protocol v2", () => {
  it("imports v2 export files", () => {
    const result = importSessions(JSON.stringify(singleSessionExport));

    expect(result.success).toBe(true);
    if (!result.success) return;
    expect(result.sessions).toHaveLength(1);
    expect(result.sessions[0].registries.actors.a1.name).toBe("Player");
  });

  it("rejects legacy raw session files", () => {
    const result = importSessions(JSON.stringify({ id: "old", startTime: 1, events: [] }));

    expect(result.success).toBe(false);
    if (!result.success) {
      expect(result.error).toContain("erenshor.logs.export");
    }
  });

  it("builds self-contained v2 exports", () => {
    const imported = importSessions(JSON.stringify(singleSessionExport));
    if (!imported.success) throw new Error(imported.error);

    const exported = buildCombatLogFile(imported.sessions, 1800000023000);

    expect(exported.format).toBe("erenshor.logs.export");
    expect(exported.sessions[0].snapshot.registries.actors.a1.name).toBe("Player");
    expect(exported.sessions[0].derived?.summary.totalDamage).toBe(350);
  });
});
```

- [ ] **Step 2: Run importer/exporter tests and verify they fail**

Run:

```bash
cd web && pnpm test:run -- src/lib/services/session-importer.test.ts src/lib/services/session-exporter.test.ts
```

Expected: FAIL until the services use `CombatLogFileSchema` and v2 sessions.

- [ ] **Step 3: Replace importer with v2-only validation**

Update `web/src/lib/services/session-importer.ts`:

```ts
import { CombatLogFileSchema } from "$lib/types/schemas";
import type { Session } from "$lib/types";

export type ImportResult =
  | { success: true; sessions: Session[] }
  | { success: false; error: string };

export function importSessions(jsonText: string): ImportResult {
  let parsed: unknown;
  try {
    parsed = JSON.parse(jsonText);
  } catch (err) {
    return {
      success: false,
      error: `Invalid JSON: ${err instanceof Error ? err.message : "Parse error"}`,
    };
  }

  if (
    typeof parsed !== "object" ||
    parsed === null ||
    (parsed as Record<string, unknown>).format !== "erenshor.logs.export"
  ) {
    return {
      success: false,
      error: "File is not an erenshor.logs.export combat log.",
    };
  }

  const result = CombatLogFileSchema.safeParse(parsed);
  if (!result.success) {
    return {
      success: false,
      error: result.error.issues.map((i) => `${i.path.join(".")}: ${i.message}`).join("; "),
    };
  }

  return {
    success: true,
    sessions: result.data.sessions.map((session) => ({
      id: session.snapshot.sessionId,
      mode: "imported",
      state: session.snapshot.state,
      startedAtUtcMs: session.snapshot.startedAtUtcMs,
      endedAtUtcMs: session.snapshot.endedAtUtcMs,
      endReason: session.snapshot.endReason,
      durationMs: session.snapshot.durationMs,
      producer: session.snapshot.producer,
      playerActorId: session.snapshot.playerActorId,
      registryRevision: session.snapshot.registryRevision,
      lastEventSeq: session.snapshot.lastEventSeq,
      eventCount: session.snapshot.eventCount,
      completeness: session.snapshot.completeness,
      loss: session.snapshot.loss,
      registries: session.snapshot.registries,
      diagnostics: session.snapshot.diagnostics,
      events: session.events,
      protocolErrors: [],
    })),
  };
}
```

- [ ] **Step 4: Replace exporter with v2 document builder**

Update `web/src/lib/services/session-exporter.ts`:

```ts
import type { CombatLogFile, DerivedSummary, Session } from "$lib/types";
import { calculateSessionStats } from "$lib/services";
import { downloadJSON } from "$lib/utils/download";

const SCHEMA_VERSION = "2.0.0";
const DERIVED_ALGORITHM_VERSION = "2.0.0";

export function buildCombatLogFile(sessions: Session[], exportedAtMs = Date.now()): CombatLogFile {
  return {
    format: "erenshor.logs.export",
    schemaVersion: SCHEMA_VERSION,
    exportedAtMs,
    producer: { name: "ErenshorLogsWeb", webVersion: SCHEMA_VERSION },
    sessions: sessions.map((session) => {
      const durationMs = session.durationMs ?? effectiveDurationMs(session, exportedAtMs);
      const stats = calculateSessionStats(session.events, session.registries, durationMs);
      const summary: DerivedSummary = {
        totalDamage: stats.totalDamage,
        totalHealing: stats.totalHealing,
        totalDamageTaken: stats.totalDamageTaken,
        totalHealingReceived: stats.totalHealingReceived,
        durationMs,
      };

      return {
        snapshot: {
          sessionId: session.id,
          state: session.state,
          mode: session.mode,
          startedAtUtcMs: session.startedAtUtcMs,
          endedAtUtcMs: session.endedAtUtcMs,
          endReason: session.endReason,
          durationMs: session.durationMs,
          producer: session.producer,
          playerActorId: session.playerActorId,
          registryRevision: session.registryRevision,
          lastEventSeq: session.lastEventSeq,
          eventCount: session.eventCount,
          completeness: session.completeness,
          loss: session.loss,
          registries: session.registries,
          diagnostics: session.diagnostics,
        },
        events: session.events,
        derived: {
          algorithmVersion: DERIVED_ALGORITHM_VERSION,
          computedAtMs: exportedAtMs,
          computedFromEventSeq: session.lastEventSeq,
          summary,
        },
      };
    }),
  };
}

export function exportSession(session: Session): void {
  const file = buildCombatLogFile([session]);
  const filename = `erenshor-session-${session.id.slice(0, 8)}-${timestampForFilename()}`;
  downloadJSON(file, filename);
}

export function exportSessions(sessions: Session[]): void {
  if (sessions.length === 0) {
    console.warn("exportSessions called with empty array");
    return;
  }
  const file = buildCombatLogFile(sessions);
  downloadJSON(file, `erenshor-sessions-${sessions.length}-${timestampForFilename()}`);
}

function effectiveDurationMs(session: Session, nowMs: number): number {
  return (session.endedAtUtcMs ?? nowMs) - session.startedAtUtcMs;
}

function timestampForFilename(): string {
  return new Date().toISOString().replace(/[:.]/g, "-").slice(0, -5);
}
```

- [ ] **Step 5: Update analyzer to resolve IDs through registries**

Change the analyzer signature:

```ts
export function calculateSessionStats(
  events: CombatEventRecord[],
  registries: Registries,
  durationMs: number
): SessionStats
```

Replace legacy checks:

```ts
if (event.kind === "damage") {
  total += event.data.amount;
}
```

Resolve actors and abilities from registries instead of event-embedded records:

```ts
const sourceId = event.creditActorId ?? event.sourceActorId;
const source = sourceId ? registries.actors[sourceId] : undefined;
const ability = event.abilityId ? registries.abilities[event.abilityId] : undefined;
```

For ability breakdowns, group by `abilityId ?? "unknown"`, and display `ability?.name ?? "Unknown"` only after verifying the producer registered an unknown ability record or the event truly has no ability.

- [ ] **Step 6: Update event filters and actor utilities**

Replace event-type helpers in `web/src/lib/utils/event-constants.ts`:

```ts
import type { CombatEventRecord } from "$lib/types";

export function isDamageEvent(event: CombatEventRecord): boolean {
  return event.kind === "damage";
}

export function isHealEvent(event: CombatEventRecord): boolean {
  return event.kind === "heal";
}
```

Update filters in `event-filters.ts` to use `registries` where faction or player-controlled information is required. Avoid fallback logic that treats missing registry records as player/friendly.

- [ ] **Step 7: Run focused web analytics/import tests**

Run:

```bash
cd web && pnpm test:run -- src/lib/services/session-importer.test.ts src/lib/services/session-exporter.test.ts src/lib/services/combat-analyzer.test.ts src/lib/utils/event-filters.test.ts src/lib/utils/actor-utils.test.ts
```

Expected: PASS.

- [ ] **Step 8: Commit web import/export and analytics cutover**

Run:

```bash
git add web/src/lib/services web/src/lib/utils web/src/lib/types web/src/lib/state/sessions.svelte.ts web/src/lib/**/*.test.ts
git commit -m "feat(web): normalize protocol v2 sessions" -m "Use registries and typed events for import, export, and analytics.

Reject legacy raw session files so live WebSocket data and exported combat logs flow through the same validated session model."
```

---

### Task 4: C# protocol records and fixture parity

**Files:**
- Replace: `mod/src/Protocol/Messages.cs`
- Modify: `mod/src/Protocol/ProtocolVersion.cs`
- Create: `mod/src/Protocol/ProducerInfo.cs`
- Create: `mod/src/Protocol/Registries.cs`
- Replace: `mod/src/Events/CombatEvent.cs`
- Create: `mod/src/Events/CombatEventRecord.cs`
- Modify: `mod/tests/ErenshorLogs.Tests/Protocol/MessageSerializerTests.cs`
- Modify: `mod/tests/ErenshorLogs.Tests/Protocol/ProtocolFixtureTests.cs`

- [ ] **Step 1: Run C# fixture tests and confirm current failure**

Run:

```bash
cd mod && dotnet test tests/ErenshorLogs.Tests --filter FullyQualifiedName~ProtocolFixtureTests
```

Expected: FAIL because protocol v2 records are absent.

- [ ] **Step 2: Replace protocol version constants**

Update `mod/src/Protocol/ProtocolVersion.cs`:

```csharp
namespace ErenshorLogs.Protocol;

public static class ProtocolVersion
{
  public const string LiveProtocol = "erenshor.logs.live";
  public const string ExportFormat = "erenshor.logs.export";
  public const string Version = "2.0.0";
  public const string SchemaVersion = "2.0.0";
}
```

- [ ] **Step 3: Add producer and registry records**

Create `mod/src/Protocol/ProducerInfo.cs`:

```csharp
namespace ErenshorLogs.Protocol;

public sealed record ProducerInfo
{
  public required string Name { get; init; }
  public string? ModVersion { get; init; }
  public string? WebVersion { get; init; }
  public string? GameVersion { get; init; }
  public string? BuildCommit { get; init; }

  public static ProducerInfo Mod(string modVersion, string? gameVersion = null) =>
    new()
    {
      Name = "ErenshorLogsMod",
      ModVersion = modVersion,
      GameVersion = gameVersion,
    };
}
```

Create `mod/src/Protocol/Registries.cs`:

```csharp
using ErenshorLogs.Events;

namespace ErenshorLogs.Protocol;

public sealed record Registries
{
  public required int Revision { get; init; }
  public required IReadOnlyDictionary<string, ActorRecord> Actors { get; init; }
  public required IReadOnlyDictionary<string, AbilityRecord> Abilities { get; init; }
  public required IReadOnlyDictionary<string, EffectRecord> Effects { get; init; }
}

public sealed record RegistryDeltaPayload
{
  public required int Revision { get; init; }
  public IReadOnlyDictionary<string, ActorRecord>? Actors { get; init; }
  public IReadOnlyDictionary<string, AbilityRecord>? Abilities { get; init; }
  public IReadOnlyDictionary<string, EffectRecord>? Effects { get; init; }
}

public sealed record ActorRecord
{
  public required string Id { get; init; }
  public required string Name { get; init; }
  public required ActorKind Kind { get; init; }
  public string? Class { get; init; }
  public int? Level { get; init; }
  public string? OwnerActorId { get; init; }
  public ActorFaction? Faction { get; init; }
  public bool? IsPlayerControlled { get; init; }
  public long? FirstSeenEventSeq { get; init; }
}

public enum ActorKind { Player, SimPlayer, Npc, Pet, Environment, Unknown }
public enum ActorFaction { Friendly, Hostile, Neutral, Unknown }

public sealed record AbilityRecord
{
  public required string Id { get; init; }
  public required string Name { get; init; }
  public required AbilityKind Kind { get; init; }
  public string? StableKey { get; init; }
  public DamageType? DamageType { get; init; }
  public ProcSource? ProcSource { get; init; }
  public string? ParentAbilityId { get; init; }
}

public enum AbilityKind { Skill, Spell, Auto, Dot, Hot, Proc, Environmental, Unknown }
public enum ProcSource { Weapon, Wand, Bow, Buff, Skill }

public sealed record EffectRecord
{
  public required string Id { get; init; }
  public required string Name { get; init; }
  public required EffectKind Kind { get; init; }
  public string? StableKey { get; init; }
  public string? SourceAbilityId { get; init; }
  public int? DefaultDurationMs { get; init; }
  public int? MaxStacks { get; init; }
}

public enum EffectKind { Buff, Debuff, Unknown }
```

- [ ] **Step 4: Replace combat event records**

Create `mod/src/Events/CombatEventRecord.cs`:

```csharp
namespace ErenshorLogs.Events;

public abstract record CombatEventRecord
{
  public required long EventSeq { get; init; }
  public required long OffsetMs { get; init; }
  public abstract string Kind { get; }
  public required string Action { get; init; }
  public string? SourceActorId { get; init; }
  public string? CreditActorId { get; init; }
  public string? TargetActorId { get; init; }
  public string? AbilityId { get; init; }
  public string? EffectId { get; init; }
  public long? CauseEventSeq { get; init; }
  public AttributionMethod? Attribution { get; init; }
  public AttributionDebugInfo? Debug { get; init; }
}

public enum AttributionMethod { Verified, Context, EffectTracker, Inferred, Unknown }
public enum DamageType { Physical, Magic, Elemental, Void, Poison, Unknown }

public sealed record DamageEventRecord : CombatEventRecord
{
  public override string Kind => "damage";
  public required DamageData Data { get; init; }
}

public sealed record DamageData
{
  public required int Amount { get; init; }
  public int? RawAmount { get; init; }
  public int? MitigatedAmount { get; init; }
  public int? OverkillAmount { get; init; }
  public required DamageType DamageType { get; init; }
  public required DamageOutcome Outcome { get; init; }
}

public sealed record DamageOutcome
{
  public required DamageResult Result { get; init; }
  public bool? Critical { get; init; }
  public int? BlockedAmount { get; init; }
  public int? ResistedAmount { get; init; }
  public int? AbsorbedAmount { get; init; }
}

public enum DamageResult { Landed, Missed, Resisted, Absorbed, Immune }
```

Add `HealEventRecord`, `ResourceEventRecord`, `EffectEventRecord`, `DeathEventRecord`, and `InterruptEventRecord` in the same file with the exact fields from the spec. The mod will not emit all of these immediately, but it must deserialize fixtures containing them.

- [ ] **Step 5: Replace live/export message records**

Replace `mod/src/Protocol/Messages.cs`:

```csharp
using ErenshorLogs.Events;

namespace ErenshorLogs.Protocol;

public sealed record LiveEnvelope
{
  public string Protocol { get; init; } = ProtocolVersion.LiveProtocol;
  public string ProtocolVersion { get; init; } = Protocol.ProtocolVersion.Version;
  public string SchemaVersion { get; init; } = Protocol.ProtocolVersion.SchemaVersion;
  public required string Kind { get; init; }
  public required long FrameSeq { get; init; }
  public string? SessionId { get; init; }
  public required long SentAtMs { get; init; }
  public required object Payload { get; init; }
}

public sealed record HelloPayload
{
  public required ProducerInfo Producer { get; init; }
  public string? ActiveSessionId { get; init; }
  public required IReadOnlyList<string> Capabilities { get; init; }
  public IReadOnlyList<string>? RequiredCapabilities { get; init; }
}

public sealed record SessionSnapshotPayload
{
  public required string SessionId { get; init; }
  public required string State { get; init; }
  public required string Mode { get; init; }
  public required long StartedAtUtcMs { get; init; }
  public long? EndedAtUtcMs { get; init; }
  public string? EndReason { get; init; }
  public long? DurationMs { get; init; }
  public required ProducerInfo Producer { get; init; }
  public string? PlayerActorId { get; init; }
  public required int RegistryRevision { get; init; }
  public required long LastEventSeq { get; init; }
  public required int EventCount { get; init; }
  public required string Completeness { get; init; }
  public LossCounters? Loss { get; init; }
  public required Registries Registries { get; init; }
  public SessionDiagnostics? Diagnostics { get; init; }
}
```

Continue with `EventsPayload`, `SessionEndedPayload`, `ErrorPayload`, `ServerStatsPayload`, `CombatLogFile`, `CombatLogSession`, `DerivedData`, and `DerivedSummary` using the spec field names.

Because Newtonsoft.Json deserializes `object Payload` into `JObject`, add typed factory methods for producer-side sending:

```csharp
public static LiveEnvelope CreateHello(long frameSeq, ProducerInfo producer, string? activeSessionId) =>
  new()
  {
    Kind = "hello",
    FrameSeq = frameSeq,
    SentAtMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
    Payload = new HelloPayload
    {
      Producer = producer,
      ActiveSessionId = activeSessionId,
      Capabilities = ["registryDelta", "sessionSnapshot", "gzipFileExport"],
    },
  };
```

- [ ] **Step 6: Configure polymorphic deserialization where needed**

If `ProtocolFixtureTests` require deserializing event arrays into concrete records, add a custom Newtonsoft converter for `CombatEventRecord` in `mod/src/Json/JsonSettings.cs` or a new `mod/src/Json/CombatEventRecordConverter.cs`.

Converter rule:

```csharp
var kind = (string?)obj["kind"];
return kind switch
{
  "damage" => obj.ToObject<DamageEventRecord>(serializer),
  "heal" => obj.ToObject<HealEventRecord>(serializer),
  "resource" => obj.ToObject<ResourceEventRecord>(serializer),
  "effect" => obj.ToObject<EffectEventRecord>(serializer),
  "death" => obj.ToObject<DeathEventRecord>(serializer),
  "interrupt" => obj.ToObject<InterruptEventRecord>(serializer),
  _ => throw new JsonSerializationException($"Unknown combat event kind '{kind}'"),
};
```

Do not allocate this converter on every serialization call; register a single converter instance in `JsonSettings.Settings`.

- [ ] **Step 7: Run fixture parity tests and existing protocol tests**

Run:

```bash
cd mod && dotnet test tests/ErenshorLogs.Tests --filter "FullyQualifiedName~ProtocolFixtureTests|FullyQualifiedName~MessageSerializerTests"
```

Expected after implementation: PASS.

- [ ] **Step 8: Commit C# protocol records**

Run:

```bash
git add mod/src/Protocol mod/src/Events mod/src/Json mod/tests/ErenshorLogs.Tests/Protocol
git commit -m "feat(mod): define protocol v2 records" -m "Replace legacy protocol messages with v2 envelopes, registries, typed events, and export documents.

Use golden fixtures to keep Newtonsoft serialization aligned with the browser contract before wiring production capture into the new model."
```

---

### Task 5: Mod session registry and event store

**Files:**
- Create: `mod/src/Registry/IProtocolRegistry.cs`
- Create: `mod/src/Registry/ProtocolRegistry.cs`
- Create: `mod/src/Session/ISessionEventStore.cs`
- Create: `mod/src/Session/SessionEventStore.cs`
- Modify: `mod/src/Plugin.cs`
- Test: `mod/tests/ErenshorLogs.Tests/Registry/ProtocolRegistryTests.cs`
- Test: `mod/tests/ErenshorLogs.Tests/Session/SessionEventStoreTests.cs`

- [ ] **Step 1: Write failing registry tests**

Create `ProtocolRegistryTests.cs`:

```csharp
using ErenshorLogs.Protocol;
using ErenshorLogs.Registry;
using Xunit;

namespace ErenshorLogs.Tests.Registry;

public class ProtocolRegistryTests
{
  [Fact]
  public void Reset_ClearsRecordsAndRestartsIds()
  {
    var registry = new ProtocolRegistry();
    var first = registry.UpsertActor(new ActorRecord { Id = "", Name = "Goblin", Kind = ActorKind.Npc });

    registry.Reset();
    var second = registry.UpsertActor(new ActorRecord { Id = "", Name = "Goblin", Kind = ActorKind.Npc });

    Assert.Equal("a1", first.Id);
    Assert.Equal("a1", second.Id);
    Assert.Equal(1, registry.Snapshot().Revision);
  }

  [Fact]
  public void DrainDelta_ReturnsOnlyChangedRecordsThenClearsPendingChanges()
  {
    var registry = new ProtocolRegistry();
    registry.UpsertAbility(new AbilityRecord { Id = "", Name = "Backstab", Kind = AbilityKind.Skill });

    var first = registry.DrainDelta();
    var second = registry.DrainDelta();

    Assert.NotNull(first.Abilities);
    Assert.Null(second.Abilities);
  }
}
```

- [ ] **Step 2: Write failing event store tests**

Create `SessionEventStoreTests.cs`:

```csharp
using ErenshorLogs.Events;
using ErenshorLogs.Session;
using Xunit;

namespace ErenshorLogs.Tests.Session;

public class SessionEventStoreTests
{
  [Fact]
  public void Append_AssignsMonotonicEventSeqAndOffset()
  {
    var store = new SessionEventStore(() => 1_800_000_000_250);
    store.StartSession("session-1", 1_800_000_000_000);

    var appended = store.AppendDamage(
      new DamageEventRecord
      {
        EventSeq = 0,
        OffsetMs = 0,
        Action = "hit",
        SourceActorId = "a1",
        TargetActorId = "a2",
        Data = new DamageData
        {
          Amount = 10,
          DamageType = DamageType.Physical,
          Outcome = new DamageOutcome { Result = DamageResult.Landed },
        },
      }
    );

    Assert.Equal(1, appended.EventSeq);
    Assert.Equal(250, appended.OffsetMs);
    Assert.Equal(1, store.LastEventSeq);
  }

  [Fact]
  public void Reset_StartsNewSessionWithoutOldEvents()
  {
    var store = new SessionEventStore(() => 10);
    store.StartSession("one", 0);
    store.AppendDamage(MakeDamage());

    store.StartSession("two", 10);

    Assert.Empty(store.Events);
    Assert.Equal(0, store.LastEventSeq);
  }

  private static DamageEventRecord MakeDamage() =>
    new()
    {
      EventSeq = 0,
      OffsetMs = 0,
      Action = "hit",
      Data = new DamageData
      {
        Amount = 1,
        DamageType = DamageType.Physical,
        Outcome = new DamageOutcome { Result = DamageResult.Landed },
      },
    };
}
```

- [ ] **Step 3: Run registry/store tests and verify they fail**

Run:

```bash
cd mod && dotnet test tests/ErenshorLogs.Tests --filter "FullyQualifiedName~ProtocolRegistryTests|FullyQualifiedName~SessionEventStoreTests"
```

Expected: FAIL because the registry and store do not exist.

- [ ] **Step 4: Implement `IProtocolRegistry`**

Create `mod/src/Registry/IProtocolRegistry.cs`:

```csharp
using ErenshorLogs.Protocol;

namespace ErenshorLogs.Registry;

public interface IProtocolRegistry
{
  int Revision { get; }
  ActorRecord UpsertActor(ActorRecord record);
  AbilityRecord UpsertAbility(AbilityRecord record);
  EffectRecord UpsertEffect(EffectRecord record);
  Registries Snapshot();
  RegistryDeltaPayload DrainDelta();
  void Reset();
}
```

- [ ] **Step 5: Implement `ProtocolRegistry`**

Create `mod/src/Registry/ProtocolRegistry.cs` with dictionaries keyed by stable signature:

```csharp
using ErenshorLogs.Protocol;

namespace ErenshorLogs.Registry;

public sealed class ProtocolRegistry : IProtocolRegistry
{
  private readonly Dictionary<string, ActorRecord> _actorsByKey = [];
  private readonly Dictionary<string, AbilityRecord> _abilitiesByKey = [];
  private readonly Dictionary<string, EffectRecord> _effectsByKey = [];
  private readonly Dictionary<string, ActorRecord> _pendingActors = [];
  private readonly Dictionary<string, AbilityRecord> _pendingAbilities = [];
  private readonly Dictionary<string, EffectRecord> _pendingEffects = [];
  private int _nextActorId = 1;
  private int _nextAbilityId = 1;
  private int _nextEffectId = 1;

  public int Revision { get; private set; }

  public ActorRecord UpsertActor(ActorRecord record)
  {
    var key = ActorKey(record);
    if (_actorsByKey.TryGetValue(key, out var existing)) return existing;

    var stored = record with { Id = string.IsNullOrEmpty(record.Id) ? $"a{_nextActorId++}" : record.Id };
    _actorsByKey.Add(key, stored);
    _pendingActors[stored.Id] = stored;
    Revision++;
    return stored;
  }

  public AbilityRecord UpsertAbility(AbilityRecord record)
  {
    var key = record.StableKey ?? $"name:{record.Kind}:{record.Name}";
    if (_abilitiesByKey.TryGetValue(key, out var existing)) return existing;

    var stored = record with { Id = string.IsNullOrEmpty(record.Id) ? $"ab{_nextAbilityId++}" : record.Id };
    _abilitiesByKey.Add(key, stored);
    _pendingAbilities[stored.Id] = stored;
    Revision++;
    return stored;
  }

  public EffectRecord UpsertEffect(EffectRecord record)
  {
    var key = record.StableKey ?? $"name:{record.Kind}:{record.Name}";
    if (_effectsByKey.TryGetValue(key, out var existing)) return existing;

    var stored = record with { Id = string.IsNullOrEmpty(record.Id) ? $"ef{_nextEffectId++}" : record.Id };
    _effectsByKey.Add(key, stored);
    _pendingEffects[stored.Id] = stored;
    Revision++;
    return stored;
  }

  public Registries Snapshot() =>
    new()
    {
      Revision = Revision,
      Actors = _actorsByKey.Values.ToDictionary(a => a.Id),
      Abilities = _abilitiesByKey.Values.ToDictionary(a => a.Id),
      Effects = _effectsByKey.Values.ToDictionary(e => e.Id),
    };

  public RegistryDeltaPayload DrainDelta()
  {
    var delta = new RegistryDeltaPayload
    {
      Revision = Revision,
      Actors = _pendingActors.Count == 0 ? null : new Dictionary<string, ActorRecord>(_pendingActors),
      Abilities = _pendingAbilities.Count == 0 ? null : new Dictionary<string, AbilityRecord>(_pendingAbilities),
      Effects = _pendingEffects.Count == 0 ? null : new Dictionary<string, EffectRecord>(_pendingEffects),
    };
    _pendingActors.Clear();
    _pendingAbilities.Clear();
    _pendingEffects.Clear();
    return delta;
  }

  public void Reset()
  {
    _actorsByKey.Clear();
    _abilitiesByKey.Clear();
    _effectsByKey.Clear();
    _pendingActors.Clear();
    _pendingAbilities.Clear();
    _pendingEffects.Clear();
    _nextActorId = 1;
    _nextAbilityId = 1;
    _nextEffectId = 1;
    Revision = 0;
  }

  private static string ActorKey(ActorRecord record) =>
    record.Kind == ActorKind.Pet && record.OwnerActorId != null
      ? $"{record.Kind}:{record.OwnerActorId}:{record.Name}"
      : $"{record.Kind}:{record.Name}:{record.Class}:{record.Level}";
}
```

- [ ] **Step 6: Implement event store**

Create `mod/src/Session/ISessionEventStore.cs`:

```csharp
using ErenshorLogs.Events;

namespace ErenshorLogs.Session;

public interface ISessionEventStore
{
  string? SessionId { get; }
  long LastEventSeq { get; }
  IReadOnlyList<CombatEventRecord> Events { get; }
  void StartSession(string sessionId, long startedAtUtcMs);
  CombatEventRecord Append(CombatEventRecord evt);
  IReadOnlyList<CombatEventRecord> GetEventsFrom(long firstEventSeq);
  void EndSession();
}
```

Create `mod/src/Session/SessionEventStore.cs`:

```csharp
using ErenshorLogs.Events;

namespace ErenshorLogs.Session;

public sealed class SessionEventStore(Func<long> getUtcNowMs) : ISessionEventStore
{
  private readonly List<CombatEventRecord> _events = [];
  private long _startedAtUtcMs;

  public string? SessionId { get; private set; }
  public long LastEventSeq { get; private set; }
  public IReadOnlyList<CombatEventRecord> Events => _events;

  public void StartSession(string sessionId, long startedAtUtcMs)
  {
    SessionId = sessionId;
    _startedAtUtcMs = startedAtUtcMs;
    LastEventSeq = 0;
    _events.Clear();
  }

  public CombatEventRecord Append(CombatEventRecord evt)
  {
    if (SessionId == null)
      throw new InvalidOperationException("Cannot append combat events without an active session.");

    var eventSeq = LastEventSeq + 1;
    var offsetMs = Math.Max(0, getUtcNowMs() - _startedAtUtcMs);
    var stored = evt with { EventSeq = eventSeq, OffsetMs = offsetMs };
    _events.Add(stored);
    LastEventSeq = eventSeq;
    return stored;
  }

  public IReadOnlyList<CombatEventRecord> GetEventsFrom(long firstEventSeq) =>
    _events.Where(e => e.EventSeq >= firstEventSeq).ToArray();

  public void EndSession()
  {
    SessionId = null;
  }
}
```

If C# does not allow `with` on the abstract base in this shape, replace `Append` with a type switch that copies each concrete event record with new `EventSeq` and `OffsetMs`.

- [ ] **Step 7: Register services**

Modify `Plugin.ConfigureServices()`:

```csharp
services.AddSingleton<IProtocolRegistry, ProtocolRegistry>();
services.AddSingleton<ISessionEventStore>(_ => new SessionEventStore(
  () => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
));
```

- [ ] **Step 8: Run registry/store tests**

Run:

```bash
cd mod && dotnet test tests/ErenshorLogs.Tests --filter "FullyQualifiedName~ProtocolRegistryTests|FullyQualifiedName~SessionEventStoreTests"
```

Expected: PASS.

- [ ] **Step 9: Commit registry and event store**

Run:

```bash
git add mod/src/Registry mod/src/Session mod/src/Plugin.cs mod/tests/ErenshorLogs.Tests/Registry mod/tests/ErenshorLogs.Tests/Session
git commit -m "feat(mod): add session event storage" -m "Add session-scoped protocol registries and an append-only event store that assigns eventSeq and offsetMs.

This gives the broadcaster and exporter a durable source of combat facts instead of treating the network queue as capture storage."
```

---

### Task 6: Mod capture and broadcaster protocol v2 cutover

**Files:**
- Modify: `mod/src/Hooks/ICombatEventBuilder.cs`
- Modify: `mod/src/Hooks/CombatEventBuilder.cs`
- Modify: `mod/src/Hooks/CombatEventBuilderAdapter.cs`
- Modify damage hooks listed in file structure.
- Modify: `mod/src/Session/SessionManager.cs`
- Modify: `mod/src/Session/ISessionManager.cs`
- Modify: `mod/src/Broadcast/CombatEventBroadcaster.cs`
- Modify: `mod/src/Broadcast/ICombatEventBroadcaster.cs`
- Modify: `mod/src/Plugin.cs`
- Update tests in `mod/tests/ErenshorLogs.Tests/Hooks/CombatEventBuilderTests.cs`, `SessionManagerTests.cs`, and `Protocol/MessageSerializerTests.cs`.

- [ ] **Step 1: Write failing builder test for v2 damage events**

Update `CombatEventBuilderTests.cs` to expect ID references, typed data, and no embedded actor/ability records:

```csharp
[Fact]
public void CreateDamageEvent_ReturnsTypedDamageRecordWithRegistryIds()
{
  var registry = new ProtocolRegistry();
  var builder = new CombatEventBuilder<object>(
    resolveActor: character => character == null
      ? null
      : new ActorRecord { Id = "", Name = character.ToString()!, Kind = ActorKind.Npc },
    registry: registry
  );

  var evt = builder.CreateDamageEvent(
    target: "target",
    source: "source",
    amount: 42,
    damageType: DamageType.Physical,
    ability: new AbilityRecord { Id = "", Name = "Backstab", Kind = AbilityKind.Skill },
    attribution: AttributionMethod.Context,
    critical: true
  );

  Assert.NotNull(evt);
  Assert.Equal("damage", evt.Kind);
  Assert.Equal("hit", evt.Action);
  Assert.Equal("a1", evt.SourceActorId);
  Assert.Equal("a2", evt.TargetActorId);
  Assert.Equal("ab1", evt.AbilityId);
  Assert.Equal(42, evt.Data.Amount);
  Assert.True(evt.Data.Outcome.Critical);
}
```

- [ ] **Step 2: Run builder test and verify it fails**

Run:

```bash
cd mod && dotnet test tests/ErenshorLogs.Tests --filter FullyQualifiedName~CombatEventBuilderTests
```

Expected: FAIL because the builder still creates legacy `CombatEvent`.

- [ ] **Step 3: Change builder interfaces to protocol records**

Update `ICombatEventBuilder` to return `DamageEventRecord?` and accept enough facts to fill typed damage data:

```csharp
public interface ICombatEventBuilder
{
  DamageEventRecord? CreateDamageEvent(
    Character target,
    Character? source,
    int amount,
    DamageType damageType,
    AbilityRecord ability,
    AttributionMethod attribution,
    bool critical = false,
    int? rawAmount = null,
    int? mitigatedAmount = null,
    AttributionDebugInfo? debugInfo = null
  );
}
```

The generic builder should resolve actors through `IProtocolRegistry` before returning the event. Do not assign `eventSeq` or `offsetMs` in the builder; set both to `0` and let `SessionEventStore.Append` assign them exactly once.

- [ ] **Step 4: Update damage hooks ordering**

In every damage hook, notify the session manager before appending/emitting the triggering event. The new order must be:

```csharp
SessionManager?.OnCombatEvent(evt.Kind, evt.Action, timestamp);
var stored = SessionEventStore?.Append(evt);
Emitter?.Emit(stored);
```

If the session manager rejects the event because no session is active and auto detection is disabled, do not append. Add an `IsSessionActive` or `EnsureSessionForEvent` method to `ISessionManager` if needed:

```csharp
bool EnsureSessionForEvent(string kind, string action, long eventTimestamp);
```

Expected behavior:

- Automatic session starts before the first damage event is appended.
- No synthetic `combatStart` event is emitted.
- Manual sessions accept events immediately.
- Environmental damage does not start sessions unless explicitly configured.

- [ ] **Step 5: Change session manager from event enum to kind/action**

Replace `EventType` configuration parsing with v2 event keys such as `damage:hit`, `damage:tick`, and `damage:reflect`.

Default start/keepalive config:

```text
damage:hit,damage:tick,damage:reflect
```

Update `SessionManager`:

```csharp
public bool EnsureSessionForEvent(string kind, string action, long eventTimestamp)
{
  var key = $"{kind}:{action}";

  if (_sessionKeepAliveEvents.Contains(key))
  {
    _lastEventTime = _timeProvider.CurrentTime;
    _lastEventTimestamp = eventTimestamp;
  }

  if (_currentSession == null)
  {
    if (!_autoDetectionEnabled || !_sessionStartEvents.Contains(key))
      return false;

    StartSession(isManual: false);
  }

  return true;
}
```

Remove synthetic `CombatStart` and `CombatEnd` emission. Session lifecycle is represented by `SessionStarted` and `SessionEnded` events plus protocol frames.

- [ ] **Step 6: Write failing broadcaster tests**

Update `MessageSerializerTests` or add broadcaster tests that verify:

- first client receives `hello`.
- active session client receives `sessionSnapshot` with full registries.
- catch-up sends `events` from `1..lastEventSeq` before live tail.
- no queued events are cleared just because there are zero clients.

Example assertion:

```csharp
Assert.Contains("\"kind\":\"sessionSnapshot\"", json);
Assert.Contains("\"registries\"", json);
Assert.Contains("\"eventSeqStart\":1", json);
```

- [ ] **Step 7: Rewrite broadcaster**

`CombatEventBroadcaster` should keep only transport batching state. Source facts live in `ISessionEventStore`.

Required fields:

```csharp
private long _frameSeq;
private long _lastBroadcastEventSeq;
private readonly List<CombatEventRecord> _pendingEvents = [];
```

On client connect:

```csharp
public void SendHandshakeToNewClient()
{
  var activeSession = _sessionManager.CurrentSession;
  Broadcast(LiveEnvelope.CreateHello(NextFrameSeq(), _producer, activeSession?.Id));

  if (activeSession != null)
  {
    Broadcast(CreateSnapshot(activeSession));
    BroadcastCatchUpEvents();
  }
}
```

On session start:

- reset `_lastBroadcastEventSeq` to `0`.
- broadcast `sessionSnapshot` if clients exist.

On event append:

- queue the stored event for the next flush.
- do not clear source storage when no clients are connected.

On flush:

- send `registryDelta` from `IProtocolRegistry.DrainDelta()` before events if any delta contains records.
- batch events by `BroadcastInterval`, `256` events, or `64 KiB` serialized size.
- create `EventsPayload` with matching `eventSeqStart`/`eventSeqEnd`.

- [ ] **Step 8: Update plugin wiring**

Pass the new services to patches and broadcaster:

```csharp
var protocolRegistry = services.GetRequiredService<IProtocolRegistry>();
var sessionEventStore = services.GetRequiredService<ISessionEventStore>();

DamageMePatch.SessionEventStore = sessionEventStore;
DamageMePatch.ProtocolRegistry = protocolRegistry;
```

On session start, reset registries and start store before any events append:

```csharp
sessionManager.SessionStarted += session =>
{
  protocolRegistry.Reset();
  sessionEventStore.StartSession(session.Id, session.StartTime);
};
```

If this event ordering is too late because broadcaster also observes `SessionStarted`, move registry/store reset into `SessionManager.StartSession` through injected lifecycle collaborators. The test must prove the first event of an automatic session receives `eventSeq: 1` and no stale registry records.

- [ ] **Step 9: Run focused mod cutover tests**

Run:

```bash
cd mod && dotnet test tests/ErenshorLogs.Tests --filter "FullyQualifiedName~CombatEventBuilderTests|FullyQualifiedName~SessionManagerTests|FullyQualifiedName~MessageSerializerTests|FullyQualifiedName~SessionEventStoreTests|FullyQualifiedName~ProtocolRegistryTests"
```

Expected: PASS.

- [ ] **Step 10: Commit mod protocol cutover**

Run:

```bash
git add mod/src mod/tests/ErenshorLogs.Tests
git commit -m "feat(mod): broadcast protocol v2 sessions" -m "Wire capture into session-scoped registries, an append-only event store, and v2 WebSocket frames.

Session lifecycle now lives in snapshots and sessionEnded frames, while combat facts carry monotonic eventSeq and offsetMs values."
```

---

### Task 7: Mod audit hardening fixes

**Files:**
- Modify context patches listed in file structure.
- Modify `mod/src/Hooks/AddStatusEffectPatch.cs`
- Modify `mod/src/Server/WebSocketServer.cs`
- Modify `mod/src/Config/ModConfig.cs`
- Create/modify tests listed in file structure.

- [ ] **Step 1: Write failing context finalizer tests**

Create `mod/tests/ErenshorLogs.Tests/Hooks/ContextFinalizerTests.cs`. Use direct patch method calls to simulate prefix/finalizer pairs:

```csharp
using ErenshorLogs.Context;
using ErenshorLogs.Events;
using ErenshorLogs.Hooks;
using Xunit;

namespace ErenshorLogs.Tests.Hooks;

public class ContextFinalizerTests
{
  [Fact]
  public void DoSkill_FinalizerDoesNotPopWhenPrefixDidNotPush()
  {
    CombatContext.Clear();
    CombatContext.PushAbility(new AbilityContext { Name = "Outer", Type = AbilityType.Skill });

    DoSkillPatch.Prefix(null!, out var pushed);
    DoSkillPatch.Finalizer(pushed);

    Assert.Equal("Outer", CombatContext.CurrentAbility?.Name);
    CombatContext.Clear();
  }
}
```

Add equivalent tests for `DoSkillNoChecksPatch`, `ResolveSpellPatch`, and `DeliverDamagePatch` using null/invalid inputs that currently return before pushing.

- [ ] **Step 2: Run context tests and verify they fail**

Run:

```bash
cd mod && dotnet test tests/ErenshorLogs.Tests --filter FullyQualifiedName~ContextFinalizerTests
```

Expected: FAIL because finalizers pop unconditionally or signatures lack `out bool __state`.

- [ ] **Step 3: Add Harmony `__state` push/pop balancing**

Update every affected patch to this shape:

```csharp
[HarmonyPrefix]
public static void Prefix(Skill _skill, out bool __state)
{
  __state = false;
  if (_skill == null)
    return;

  CombatContext.PushAbility(new AbilityContext
  {
    Name = _skill.SkillName,
    Type = AbilityType.Skill,
    StableKey = $"skill:{_skill.Id}",
  });
  __state = true;
}

[HarmonyFinalizer]
public static void Finalizer(bool __state)
{
  if (__state)
    CombatContext.PopAbility();
}
```

Apply the same pattern to spell and wand/bow context patches.

- [ ] **Step 4: Write failing WebSocket bind config tests**

Create `WebSocketServerConfigTests.cs` around a pure helper. Add a helper first if needed:

```csharp
[Theory]
[InlineData(false, "ws://127.0.0.1:38729")]
[InlineData(true, "ws://0.0.0.0:38729")]
public void BuildLocation_DefaultsToLoopbackUnlessLanIsEnabled(bool allowLan, string expected)
{
  Assert.Equal(expected, WebSocketServer.BuildLocation(38729, allowLan));
}
```

- [ ] **Step 5: Implement loopback default and bounded config values**

Update `ModConfig`:

```csharp
public ConfigEntry<bool> AllowLanConnections { get; }

Port = config.Bind(
  "Server",
  "Port",
  38729,
  new ConfigDescription(
    "WebSocket server port. Clients connect to ws://127.0.0.1:{port} by default.",
    new AcceptableValueRange<int>(1024, 65535)
  )
);

BroadcastInterval = config.Bind(
  "Server",
  "BroadcastInterval",
  100,
  new ConfigDescription(
    "Interval in milliseconds between event broadcasts to clients.",
    new AcceptableValueRange<int>(16, 5000)
  )
);

AllowLanConnections = config.Bind(
  "Server",
  "AllowLanConnections",
  false,
  "Bind the WebSocket server to all network interfaces. This exposes live combat data on the LAN."
);
```

Update `WebSocketServer.Start()`:

```csharp
var location = BuildLocation(_config.Port.Value, _config.AllowLanConnections.Value);
_server = new Fleck.WebSocketServer(location);
```

Add helper:

```csharp
internal static string BuildLocation(int port, bool allowLanConnections) =>
  $"ws://{(allowLanConnections ? "0.0.0.0" : "127.0.0.1")}:{port}";
```

Only log LAN URLs when `AllowLanConnections` is true.

- [ ] **Step 6: Add status-effect overload coverage tests**

Update `PatchCoverageTests.cs` to require patch classes or target methods for the 3-, 4-, and 5-parameter overloads:

```csharp
[Fact]
public void HooksIncludeAllKnownAddStatusEffectOverloads()
{
  var methods = typeof(AddStatusEffectPatch).GetMethods(
    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static
  );

  Assert.Contains(methods, m => m.Name.Contains("TargetMethods", StringComparison.Ordinal));
}
```

A better test is to invoke `AddStatusEffectPatch.TargetMethods()` and assert that returned method signatures include parameter counts 3, 4, and 5 when available in the referenced game DLL.

- [ ] **Step 7: Implement status-effect overload patching**

Replace the fixed overload attribute with `TargetMethods()`:

```csharp
[HarmonyPatch]
public static class AddStatusEffectPatch
{
  internal static EffectTracker? Tracker { get; set; }

  [HarmonyTargetMethods]
  public static IEnumerable<MethodBase> TargetMethods()
  {
    return typeof(Stats)
      .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
      .Where(m =>
        m.Name == nameof(Stats.AddStatusEffect) &&
        m.GetParameters().Length is 3 or 4 or 5 &&
        m.GetParameters()[0].ParameterType == typeof(Spell));
  }

  [HarmonyPostfix]
  public static void Postfix(Stats __instance, Spell spell, int __result)
  {
    if (Tracker == null || __instance == null || spell == null)
      return;

    if (__result >= 0 && __result < 30)
      Tracker.RegisterEffect(__instance.Myself, __result, spell);
  }
}
```

If any overload does not return the slot index, split postfix methods by signature so the no-slot overload scans `__instance.StatusEffects` for the applied spell. Do not record attribution if the slot cannot be determined reliably.

- [ ] **Step 8: Run hardening tests**

Run:

```bash
cd mod && dotnet test tests/ErenshorLogs.Tests --filter "FullyQualifiedName~ContextFinalizerTests|FullyQualifiedName~WebSocketServerConfigTests|FullyQualifiedName~PatchCoverageTests"
```

Expected: PASS.

- [ ] **Step 9: Commit hardening fixes**

Run:

```bash
git add mod/src/Hooks mod/src/Server mod/src/Config mod/tests/ErenshorLogs.Tests
git commit -m "fix(mod): harden capture lifecycle" -m "Balance Harmony context cleanup with __state, bind WebSocket to loopback by default, and patch known AddStatusEffect overloads.

These fixes address audit findings that could misattribute nested damage, expose live data unexpectedly, or miss effect attribution on current game builds."
```

---

### Task 8: Documentation and stale legacy removal

**Files:**
- Modify: `docs/ARCHITECTURE.md`
- Modify: `docs/LOG_FORMAT.md`
- Modify: `docs/COMBAT_EVENTS.md`
- Modify any tests/docs that still mention legacy live message names.

- [ ] **Step 1: Search for legacy protocol names**

Use the repository search tool, not shell grep, for these strings:

```text
handshake
sessionStart
sessionEnd
combatEvents
combatStart
combatEnd
damage_physical
damage_skill
heal_spell
buff_apply
CheckForTrueCombatPatch
```

Expected: production code should not use legacy live frame names or snake_case event examples after this task. Historical mentions in committed spec/audit docs may remain if they describe old behavior explicitly.

- [ ] **Step 2: Update architecture docs**

Change `docs/ARCHITECTURE.md` to state:

- live protocol uses v2 envelopes.
- session lifecycle uses `sessionSnapshot` and `sessionEnded`, not combat events.
- mod currently emits damage records only from existing damage hooks.
- healing/resources/status/death/interrupts are schema-supported but capture work is separate.
- WebSocket default binding is loopback, with LAN opt-in.

- [ ] **Step 3: Replace log format docs**

Update `docs/LOG_FORMAT.md` around the v2 model. Include a compact live frame example:

```json
{
  "protocol": "erenshor.logs.live",
  "protocolVersion": "2.0.0",
  "schemaVersion": "2.0.0",
  "kind": "events",
  "frameSeq": 4,
  "sessionId": "session-1",
  "sentAtMs": 1800000000300,
  "payload": {
    "sessionId": "session-1",
    "registryRevision": 5,
    "eventSeqStart": 1,
    "eventSeqEnd": 1,
    "events": [
      {
        "eventSeq": 1,
        "offsetMs": 120,
        "kind": "damage",
        "action": "hit",
        "sourceActorId": "a2",
        "targetActorId": "a3",
        "abilityId": "ab1",
        "data": {
          "amount": 350,
          "damageType": "physical",
          "outcome": { "result": "landed" }
        }
      }
    ]
  }
}
```

State that examples are validated against `shared/protocol/fixtures/`.

- [ ] **Step 4: Update combat events docs**

Remove `CheckForTrueCombatPatch` as a current implementation claim. Document event-driven sessions and the fact that `CheckForTrueCombat` is not currently used to start sessions.

- [ ] **Step 5: Commit docs update**

Run:

```bash
git add docs/ARCHITECTURE.md docs/LOG_FORMAT.md docs/COMBAT_EVENTS.md
git commit -m "docs: document protocol v2 cutover" -m "Update architecture, log format, and combat event docs to match the implemented v2 model.

Remove stale legacy frame names, snake_case examples, and combat-state hook claims that no longer describe the mod."
```

---

### Task 9: Final verification and review

**Files:**
- No planned source edits unless verification finds real defects.

- [ ] **Step 1: Run mod tests**

Run:

```bash
cd mod && dotnet test tests/ErenshorLogs.Tests
```

Expected: `Failed: 0`.

- [ ] **Step 2: Run web tests, typecheck, build, and lint**

Run:

```bash
cd web && pnpm test:run && pnpm check && pnpm build && pnpm lint
```

Expected: all commands exit `0`.

- [ ] **Step 3: Run fixture parity checks explicitly**

Run:

```bash
cd web && pnpm test:run -- src/lib/types/protocol-fixtures.test.ts
cd mod && dotnet test tests/ErenshorLogs.Tests --filter FullyQualifiedName~ProtocolFixtureTests
```

Expected: both pass.

- [ ] **Step 4: Request code review**

Use the requesting-code-review skill. Ask reviewers to focus on:

- protocol/schema mismatch between shared fixtures, C# records, and Zod schemas.
- event ordering, catch-up, and session replacement semantics.
- registry ID stability and reset behavior.
- accidental backward-compatibility paths left in parser/importer.
- WebSocket exposure defaults.

- [ ] **Step 5: Address review findings**

For each finding, verify the claim against code or tests. Apply only changes that are correct for the v2 clean cutover. Re-run the focused tests covering each change.

- [ ] **Step 6: Final status check**

Run:

```bash
git status --short
git log --oneline -n 10
```

Expected: clean worktree and recent commits matching the task slices above.

---

## Acceptance checklist

- [ ] Live protocol uses `protocol: "erenshor.logs.live"` and `kind`, not legacy `type` frames.
- [ ] `protocolVersion` and `schemaVersion` accept major `2` and reject unsupported majors.
- [ ] No live parser or importer accepts raw legacy sessions or snake_case aliases.
- [ ] File exports use `format: "erenshor.logs.export"` and include full registries in each session snapshot.
- [ ] Every combat event contains `eventSeq`, `offsetMs`, `kind`, `action`, and typed `data`.
- [ ] No combat event embeds actor, ability, or effect display records.
- [ ] Web analytics resolve actor and ability display data through registries.
- [ ] Session snapshots replace retained in-memory state.
- [ ] Gaps or overlaps in live event sequences produce visible protocol errors and mark the session partial.
- [ ] Late clients receive `hello`, `sessionSnapshot`, registry state, and catch-up events before live tail.
- [ ] Actor/ability/effect registries reset at session start.
- [ ] The first event in an automatic session is appended after session state exists and receives `eventSeq: 1`.
- [ ] Harmony finalizers pop only when their prefix pushed.
- [ ] WebSocket binds to `127.0.0.1` by default and requires explicit LAN opt-in for `0.0.0.0`.
- [ ] `Stats.AddStatusEffect` coverage includes known 3-, 4-, and 5-parameter overloads when present.
- [ ] Docs describe implemented behavior and clearly separate schema-supported events from emitted hooks.
