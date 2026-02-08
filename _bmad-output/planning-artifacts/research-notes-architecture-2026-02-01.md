---
status: preliminary
date: 2026-02-01
author: Johann
context: Captured during Product Brief workflow, Step 5 (Scope Definition)
---

# Architecture Research Notes

> **STATUS: PRELIMINARY**
>
> These notes capture early architectural discussions and research conducted during the Product Brief phase. Nothing here is a final decision. All technical choices will be properly evaluated and documented during the Architecture phase of BMAD.
>
> This document exists to preserve valuable research and discussion that occurred naturally during scope exploration, so it doesn't need to be repeated later.

---

## Context

During scope definition for the erenshor-logs Product Brief, we explored technical feasibility of the proposed architecture. The discussion went deeper than typical for a Brief, but surfaced important insights about what's achievable.

---

## Key Architectural Direction (Preliminary)

### Dual-Frontend Architecture

The emerging vision supports two frontends accessing the same data:

1. **In-Game UI** (via mod) - Basic DPS meter, ability breakdowns, session history
2. **Web App** (browser) - Advanced analysis, visualizations, offline capability

This requires the mod to be the **canonical data store**, not just a data streamer.

```
                    ┌─────────────────┐
                    │   Mod (C#)      │
                    │                 │
                    │  ┌───────────┐  │
                    │  │  SQLite   │  │  ← Canonical data store
                    │  │   DB      │  │
                    │  └───────────┘  │
                    │        │        │
                    │   ┌────┴────┐   │
                    │   │         │   │
                    │   ▼         ▼   │
                    │ In-Game  WebSocket
                    │   UI     Server │
                    │                 │
                    └────────┬────────┘
                             │
                             ▼
                    ┌─────────────────┐
                    │  Browser App    │
                    │                 │
                    │  ┌───────────┐  │
                    │  │  SQLite   │  │  ← Local copy for offline
                    │  │  (OPFS)   │  │
                    │  └───────────┘  │
                    └─────────────────┘
```

### Rationale

- **In-game UI is inevitable** - Users will request it (same pattern seen with the interactive maps project)
- **Mod has unrestricted filesystem access** - Can use real SQLite without browser constraints
- **Browser can work offline** - With SQLite WASM + OPFS, full analysis without mod running
- **Same data model** - Both frontends query the same schema, just different instances

---

## Browser Storage Research (2025-2026)

### OPFS (Origin Private File System)

**Status:** Production-ready as of 2025

| Browser | Support |
|---------|---------|
| Chrome/Edge | Full since 2020 |
| Firefox | Full since March 2023 |
| Safari | Partial - 252 file handle limit causes issues |

**Key characteristics:**
- Private sandboxed filesystem per origin
- Near-native performance with synchronous access in Web Workers
- Storage limits: 60%+ of disk space (generous)
- Required for best SQLite WASM performance

### SQLite WASM Options

**1. Official SQLite WASM** (`@sqlite.org/sqlite-wasm`)
- Backed by SQLite team + Google Chrome
- ~1.2MB bundle
- Primary persistence via OPFS
- Best long-term support guarantees
- Requires Web Worker for sync OPFS access

**2. wa-sqlite** (community)
- Pluggable backends: OPFS *and* IndexedDB
- Can fall back gracefully on Safari
- More flexible but more complex setup

**Preliminary recommendation:** Official SQLite WASM for long-term support, with consideration for wa-sqlite if Safari compatibility becomes critical.

### IndexedDB (Alternative)

**Best wrapper:** Dexie.js (~16KB, 13k GitHub stars)

**Limitations for our use case:**
- No native aggregations (SUM, AVG, COUNT) - must load all data to JS
- No JOINs - manual fetch and combine
- Workable with precomputed aggregates, but adds complexity

**Assessment:** SQLite is a better fit for time-series event data with aggregation needs.

### PGlite (Postgres in WASM)

- ~3MB bundle (larger than SQLite)
- Full Postgres SQL support
- Safari support problematic due to OPFS limitations
- Overkill for this use case

**Assessment:** Not recommended - SQLite sufficient, smaller footprint.

---

## Protocol Considerations

### Current State

- One-way push: mod → browser
- ActorRef data embedded in every event (inefficient)
- No way for browser to request missing data

### Desired State

- Bidirectional WebSocket with request/response
- Entity metadata transmitted once, referenced by ID thereafter
- Browser can request: entity details, spell info, session history
- Mod can push: real-time events, session lifecycle

### Open Questions

- Exact message format for request/response (JSON-RPC style? Custom?)
- How to handle browser reconnection (resync protocol?)
- Batching strategy for efficient transmission

---

## Mod-Side Storage

### SQLite in Unity/Mono

**Needs research:** SQLite-net and Microsoft.Data.Sqlite exist, but Unity/Mono compatibility can be tricky.

**Questions to resolve:**
- Which library works reliably in BepInEx/Mono environment?
- Performance implications for real-time event logging?
- File location (game folder? AppData?)

---

## Data to Capture (Beyond Current)

The current mod captures minimal entity data (name, class, level). For full analysis capability, we need:

### Entity Metadata
- Equipment/gear loadout
- Proficiency allocations
- Ascension choices
- Active auras/buffs from group members
- Group composition at session start

### Combat Events (Current + Planned)
- Damage with ability attribution ✓
- Healing with ability attribution ✓
- Buff/debuff application and expiration ✓
- Interrupts
- Resource usage (mana)
- Deaths
- **Future:** Position/movement data (for replay visualization)

### Session Context
- Zone/location
- Target dummy vs field combat
- Encounter type (dungeon, raid, open world)

---

## Constraints (Non-Negotiable)

1. **Static hosting only** - No server-side processing, no hosted databases
2. **User owns their data** - No data stored on our infrastructure
3. **No ongoing costs** - Cloudflare Pages free tier must suffice
4. **Works offline** - Browser should function without mod for analysis of saved data

---

## Next Steps (For Architecture Phase)

1. **Research SQLite in BepInEx/Mono** - Confirm viable library and approach
2. **Design bidirectional protocol** - Message types, request/response patterns
3. **Define data schema** - Tables for events, entities, sessions, summaries
4. **Plan browser persistence** - SQLite WASM setup, import/export format
5. **Consider Safari fallback** - IndexedDB backend if OPFS proves problematic

---

## References

- [SQLite WASM Official](https://sqlite.org/wasm)
- [wa-sqlite GitHub](https://github.com/nicolo-ribaudo/nicolo-nicolo-nicolo/nicolo-nicolo-nicolo/nicolo/nicolo-nicolo/nicolo-nicolo)
- [Dexie.js](https://dexie.org/)
- [OPFS MDN](https://developer.mozilla.org/en-US/docs/Web/API/File_System_API/Origin_private_file_system)
