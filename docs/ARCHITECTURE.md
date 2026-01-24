# Architecture Overview

This document describes the technical architecture of Erenshor Logs.

## System Overview

```
+------------------------------------------------------------------+
|                        GAME (Erenshor)                           |
|  +------------------------------------------------------------+  |
|  |                    BepInEx Mod                              |  |
|  |                                                              |  |
|  |  +----------------+    +------------------+                  |  |
|  |  |  Harmony Hooks |    |  Context Manager |                  |  |
|  |  |  - DamageMe    |--->|  - Ability Stack |                  |  |
|  |  |  - HealMe      |    |  - Effect Tracker|                  |  |
|  |  |  - DoSkill     |    |  - Actor Registry|                  |  |
|  |  |  - StartSpell  |    +--------+---------+                  |  |
|  |  +----------------+             |                            |  |
|  |                                 v                            |  |
|  |                      +------------------+                    |  |
|  |                      |  Event Emitter   |                    |  |
|  |                      +--------+---------+                    |  |
|  |                               |                              |  |
|  |         +---------------------+---------------------+        |  |
|  |         |                     |                     |        |  |
|  |         v                     v                     v        |  |
|  |  +------------------+    +------------------+    +----------+|  |
|  |  |  JSON Exporter   |    |  WebSocket Server|    | (Future) ||  |
|  |  +------------------+    +--------+---------+    +----------+|  |
|  |                                   |                          |  |
|  +------------------------------------------------------------+  |
+------------------------------------------------------------------+
                                                        |
                                                        | WebSocket
                                                        v
+------------------------------------------------------------------+
|                      WEB APP (Svelte)                            |
|                                                                   |
|  +--------------------+    +--------------------+                |
|  |  WebSocket Client  |    |   File Importer   |                |
|  +----------+---------+    +---------+----------+                |
|             |                        |                           |
|             +------------+-----------+                           |
|                          |                                       |
|                          v                                       |
|              +------------------------+                          |
|              |     Event Store        |                          |
|              |   (Svelte 5 Runes)     |                          |
|              +-----------+------------+                          |
|                          |                                       |
|                          v                                       |
|              +------------------------+                          |
|              |   Analysis Engine      |                          |
|              +-----------+------------+                          |
|                          |                                       |
|         +----------------+----------------+                      |
|         |                |                |                      |
|         v                v                v                      |
|  +-----------+    +------------+   +------------+                |
|  | Timeline  |    | Breakdown  |   | Event Log  |                |
|  +-----------+    +------------+   +------------+                |
|                                                                   |
+------------------------------------------------------------------+
```

## Mod Components

### Hook Layer

Harmony patches that intercept game methods:

| Hook Target | Purpose |
|-------------|---------|
| `Character.DamageMe` | Capture physical damage |
| `Character.MagicDamageMe` | Capture magic/spell damage |
| `Character.BleedDamageMe` | Capture DoT ticks |
| `Stats.HealMe` | Capture all healing |
| `UseSkill.DoSkill` | Track skill activation (context) |
| `CastSpell.StartSpell` | Track spell casting (context) |
| `Stats.AddStatusEffect` | Track buff/debuff application |
| `Stats.TickEffects` | Attribute DoT damage to source |

### Context Manager

Maintains state needed for attribution:

- **Ability Stack**: Thread-local stack tracking current ability being executed
- **Effect Tracker**: Maps active StatusEffects to their source abilities
- **Actor Registry**: Generates stable IDs for game entities

### Event Emitter

Central event bus that:
- Creates typed `CombatEvent` objects from hook data + context
- Validates and normalizes events
- Dispatches to all registered listeners (buffer, exporter, WebSocket)

### Session Manager

Tracks combat sessions:
- Hooks into `GameData.InCombat` for combat state
- Creates session boundaries with metadata
- Handles encounter segmentation

### JSON Exporter

Saves sessions to files:
- Gzipped JSON format
- Includes header with session metadata
- Pre-computed summary statistics

### WebSocket Server

Real-time streaming via Fleck:
- Broadcasts events to connected clients
- Supports commands from clients (reset, export)
- Batches events for efficiency

## Web App Components

### Data Sources

- **WebSocket Client**: Connects to game for live data
- **File Importer**: Loads exported JSON files

### Session State

Svelte 5 runes managing:
- Current session metadata
- Event stream
- Computed aggregations (reactive DPS/HPS)

### Analysis Services

Computes:
- DPS/HPS calculations per session
- Per-actor and per-ability damage/healing breakdowns
- Session statistics and summaries

### Current Views

- **Session Management**: Live session tracking with sidebar
- **Stats Panel**: Real-time DPS/HPS/DTPS metrics
- **Actor Table**: Per-actor breakdown with expandable ability details

### Planned Views

- **Timeline**: Interactive damage/healing over time chart
- **Event Log**: Searchable, filterable event list
- **Comparison**: Side-by-side session analysis

## Event Data Model

```typescript
interface CombatEvent {
  id: string;                    // UUID
  timestamp: number;             // Unix ms
  eventType: EventType;

  source: ActorRef;
  target: ActorRef;

  ability: {
    name: string;
    type: "skill" | "spell" | "auto" | "proc" | "dot" | "hot";
    stableKey?: string;
  } | null;

  amount?: number;
  rawAmount?: number;
  mitigated?: number;
  damageType?: DamageType;

  flags: {
    critical?: boolean;
    overkill?: boolean;
    fromPlayer?: boolean;
    isPet?: boolean;
    isProc?: boolean;
    attributionFailed?: boolean;
  };

  effect?: {
    name: string;
    duration?: number;
    stacks?: number;
  };
}

interface ActorRef {
  id: string;
  name: string;
  type: "player" | "simPlayer" | "npc" | "pet";
  class?: CharacterClass;  // Arcanist, Paladin, Duelist, Druid, Stormcaller
  level?: number;          // 1-35
  masterId?: string;
}
```

## Two-Tier Accuracy

### Tier 1: Verified Accurate

Always correct because data comes directly from hook parameters:
- Total damage dealt/received per source/target
- Damage breakdown by type (Physical, Magic, etc.)
- Critical hit tracking
- Death events
- Combat timing

### Tier 2: Attributed (Best Effort)

Requires context correlation, may occasionally miss:
- Per-ability breakdown
- Proc attribution
- DoT/HoT source tracking
- Buff uptime correlation

Events with failed attribution are flagged with `attributionFailed: true` for debugging.
