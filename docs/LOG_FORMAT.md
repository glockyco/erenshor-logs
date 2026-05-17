# Combat Log Format Specification

Version: 2.0.0

## Overview

Erenshor Logs uses one canonical JSON model for live WebSocket frames and file
exports:

1. session metadata;
2. session-scoped registries for actors, abilities, and effects;
3. append-only typed combat events that reference registry IDs;
4. lifecycle/end metadata;
5. optional derived summaries.

Live transport uses compact JSON frames. File export uses the same session model
inside an `erenshor.logs.export` document. The canonical compressed extension is
`.erenshorlog.json.gz`; plain `.erenshorlog.json` is useful for development and
support.

## Live Envelope

Every live WebSocket message is an envelope:

```json
{
  "protocol": "erenshor.logs.live",
  "protocolVersion": "2.0.0",
  "schemaVersion": "2.0.0",
  "kind": "events",
  "frameSeq": 4,
  "sessionId": "session-1",
  "sentAtMs": 1800000001500,
  "payload": { }
}
```

Supported `kind` values are:

| Kind | Payload |
| --- | --- |
| `hello` | Producer identity, capabilities, active session ID |
| `sessionSnapshot` | Full session state and full registries |
| `registryDelta` | Registry additions/enrichment for one revision |
| `events` | Contiguous combat event batch |
| `sessionEnded` | End timestamp, reason, duration, diagnostics |
| `error` | Protocol or producer error details |
| `heartbeat` | Empty keepalive payload |
| `serverStats` | Producer-side counters |

Clients validate the semver major version, not the exact literal. Current major
is `2`.

## Reconnect and Ordering Rules

`sessionSnapshot` is a replacement boundary for its `sessionId`: discard any
retained in-memory copy, then apply the snapshot. For active complete sessions,
the producer replays `eventSeq` 1 through `lastEventSeq` after the snapshot
before live tail frames.

Within a connection, `events` frames must be strictly contiguous. Gaps or
overlaps mark the session partial and surface a protocol error. Duplicate event
ranges are not valid in the normal live stream.

## File Export

```json
{
  "format": "erenshor.logs.export",
  "schemaVersion": "2.0.0",
  "exportedAtMs": 1800000022000,
  "producer": {
    "name": "ErenshorLogsWeb",
    "webVersion": "2.0.0"
  },
  "sessions": [
    {
      "snapshot": { },
      "events": [ ],
      "ended": { },
      "derived": { }
    }
  ]
}
```

Importers reject unsupported schema major versions. Summaries are caches only:
consumers recompute them unless the `derived.algorithmVersion` exactly matches
the analyzer version.

## Session Snapshot

```json
{
  "sessionId": "session-1",
  "state": "active",
  "mode": "automatic",
  "startedAtUtcMs": 1800000000000,
  "producer": {
    "name": "ErenshorLogsMod",
    "modVersion": "2026.5.17.14",
    "gameVersion": "playtest-23258843"
  },
  "playerActorId": "player:0",
  "registryRevision": 3,
  "lastEventSeq": 0,
  "eventCount": 0,
  "completeness": "complete",
  "registries": {
    "revision": 3,
    "actors": { },
    "abilities": { },
    "effects": { }
  }
}
```

Ended sessions add `endedAtUtcMs`, `endReason`, and `durationMs`. Partial
sessions include `loss` counters.

## Registries

Registry records are session-scoped. IDs are stable only within a session.
Records are immutable after first use except for safe retroactive enrichment.

### ActorRecord

```json
{
  "id": "npc:1",
  "name": "Raid Boss",
  "kind": "npc",
  "level": 25,
  "faction": "hostile",
  "firstSeenEventSeq": 1
}
```

`kind` is one of `player`, `simPlayer`, `npc`, `pet`, `environment`, or
`unknown`. Player-controlled records may include `class` and
`isPlayerControlled`. Pets use `ownerActorId`.

### AbilityRecord

```json
{
  "id": "skill:101",
  "name": "Backstab",
  "kind": "skill",
  "stableKey": "skill:101",
  "damageType": "physical"
}
```

`kind` is one of `skill`, `spell`, `auto`, `dot`, `hot`, `proc`,
`environmental`, `areaEffect`, or `unknown`.

### EffectRecord

```json
{
  "id": "effect:Poisoned Wound",
  "name": "Poisoned Wound",
  "kind": "debuff",
  "sourceAbilityId": "skill:101",
  "defaultDurationMs": 12000,
  "maxStacks": 1
}
```

## Combat Events

Combat events are typed records in `eventSeq` order. `offsetMs` is relative to
`startedAtUtcMs`.

```json
{
  "eventSeq": 1,
  "offsetMs": 250,
  "kind": "damage",
  "action": "hit",
  "sourceActorId": "player:0",
  "creditActorId": "player:0",
  "targetActorId": "npc:1",
  "abilityId": "skill:101",
  "attribution": "context",
  "data": {
    "amount": 350,
    "rawAmount": 400,
    "mitigatedAmount": 50,
    "damageType": "physical",
    "outcome": {
      "result": "landed",
      "critical": true
    }
  }
}
```

Base fields:

| Field | Description |
| --- | --- |
| `eventSeq` | Monotonic per-session sequence, starting at 1 |
| `offsetMs` | Milliseconds after session start |
| `kind` | Event family discriminator |
| `action` | Family-specific action discriminator |
| `sourceActorId` | Observed source actor, if known |
| `creditActorId` | Actor credited for analytics, if different/known |
| `targetActorId` | Target actor, if known |
| `abilityId` | Registry ability ID, if known |
| `effectId` | Registry effect ID, if known |
| `causeEventSeq` | Prior event that caused this event |
| `attribution` | `verified`, `context`, `effectTracker`, `inferred`, `unknown` |
| `data` | Typed family payload |
| `debug` | Optional attribution diagnostics |

Event families:

| Kind | Actions |
| --- | --- |
| `damage` | `hit`, `tick`, `reflect` |
| `heal` | `direct`, `tick`, `lifesteal`, `regen` |
| `resource` | `spend`, `restore`, `regen` |
| `effect` | `apply`, `refresh`, `fade` |
| `death` | `die` |
| `interrupt` | `interrupt` |

`combatStart` and `combatEnd` are not combat event records in v2. Session
lifecycle is represented by `sessionSnapshot` and `sessionEnded` frames.

## Damage Types

Damage types are lowercase protocol strings: `unknown`, `physical`, `magic`,
`elemental`, `void`, and `poison`.

## Source of Truth

The JSON Schema and golden fixtures under `shared/protocol/` are authoritative.
The TypeScript/Zod and C#/Newtonsoft tests validate the implementation against
those fixtures.
