# Combat Log Format Specification

Version: 1.0.0

## Overview

Combat logs are stored as JSON files (`.json`). This document specifies the complete format.

## Export Formats

### Web App Export (Current Implementation)

The web app exports sessions in a simplified format for debugging and data sharing:

**Single Session:**
```json
{
  "version": "1.0.0",
  "exportedAt": 1704067200000,
  "session": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "startTime": 1704067200000,
    "endTime": 1704067260000,
    "events": [ ... ]
  }
}
```

**Multiple Sessions:**
```json
{
  "version": "1.0.0",
  "exportedAt": 1704067200000,
  "sessions": [
    { "id": "...", "startTime": ..., "events": [...] },
    { "id": "...", "startTime": ..., "events": [...] }
  ]
}
```

This format contains the raw session data without computed summaries. Statistics can be computed on import.

### Full Format (Future: Mod Export)

When the mod implements JSON export, it will use the full format with pre-computed statistics:

```json
{
  "version": "1.0.0",
  "session": { ... },
  "summary": { ... },
  "events": [ ... ]
}
```

## Session Metadata

```json
{
  "session": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "startTime": 1704067200000,
    "endTime": 1704067260000,
    "duration": 60000,
    "gameVersion": "1.2.3",
    "modVersion": "1.0.0"
  }
}
```

| Field | Type | Description |
|-------|------|-------------|
| `id` | string | UUID v4 |
| `startTime` | number | Unix timestamp (ms) of first event |
| `endTime` | number | Unix timestamp (ms) of last event |
| `duration` | number | Session duration in milliseconds |
| `gameVersion` | string | Erenshor version |
| `modVersion` | string | Combat Logger mod version |

**Note**: Player identity is not stored in session metadata. The player is identified through combat events via `ActorRef` where `type: "player"`.

## Summary Statistics

Pre-computed for quick display without parsing all events:

```json
{
  "summary": {
    "totalDamageDealt": 150000,
    "totalDamageReceived": 25000,
    "totalHealing": 30000,
    "dps": 2500.0,
    "hps": 500.0,
    "deaths": 0,
    "kills": 5,
    "critRate": 0.15,
    "highestHit": 5000,
    "damageByType": {
      "Physical": 100000,
      "Magic": 30000,
      "Elemental": 20000
    },
    "topAbilities": [
      { "name": "Backstab", "damage": 45000, "hits": 30 },
      { "name": "Auto Attack", "damage": 40000, "hits": 150 }
    ]
  }
}
```

## Events

Array of combat events in chronological order:

```json
{
  "events": [
    {
      "id": "event-uuid",
      "timestamp": 1704067200000,
      "eventType": "damage_skill",
      "source": { ... },
      "target": { ... },
      "ability": { ... },
      "amount": 1500,
      "rawAmount": 2000,
      "mitigated": 500,
      "damageType": "Physical",
      "flags": {
        "critical": true,
        "fromPlayer": true
      }
    }
  ]
}
```

### Event Types

| Type | Description |
|------|-------------|
| `damage_physical` | Physical damage (pre-attribution) |
| `damage_magic` | Magic damage (pre-attribution) |
| `damage_melee` | Auto-attack damage |
| `damage_skill` | Melee/ranged skill damage |
| `damage_spell` | Direct damage spell |
| `damage_dot` | Damage over time tick |
| `damage_proc` | Weapon/ability proc damage |
| `damage_pet` | Pet damage (attributed to owner) |
| `damage_reflect` | Damage shield reflection |
| `damage_environmental` | Environmental damage |
| `heal_spell` | Direct healing spell |
| `heal_hot` | Heal over time tick |
| `heal_lifesteal` | Lifesteal healing |
| `heal_regen` | Natural HP regeneration tick |
| `mana_use` | Mana consumed by ability |
| `mana_restore` | Mana restored by ability or effect |
| `mana_regen` | Natural mana regeneration tick |
| `spell_interrupt` | Spell cast was interrupted |
| `buff_apply` | Buff applied |
| `buff_refresh` | Buff duration refreshed |
| `buff_fade` | Buff removed/expired |
| `debuff_apply` | Debuff applied |
| `debuff_refresh` | Debuff duration refreshed |
| `debuff_fade` | Debuff removed/expired |
| `death` | Entity died |
| `combat_start` | Combat began |
| `combat_end` | Combat ended |

### Actor Reference

```json
{
  "id": "player:0",
  "name": "Valdris",
  "type": "player",
  "class": "Duelist",
  "level": 35,
  "masterId": null
}
```

| Field | Type | Description |
|-------|------|-------------|
| `id` | string | Stable identifier (`type:instanceId`) |
| `name` | string | Display name |
| `type` | string | `player`, `simPlayer`, `npc`, `pet` |
| `class` | string? | Character class: Arcanist, Paladin, Duelist, Druid, Stormcaller |
| `level` | number? | Character level (1-35) |
| `masterId` | string? | Owner's ID (for pets only) |

### Ability Reference

```json
{
  "name": "Fireball",
  "type": "spell",
  "stableKey": "spell:Fireball",
  "procSource": "weapon"
}
```

| Field | Type | Description |
|-------|------|-------------|
| `name` | string | Display name |
| `type` | string | `skill`, `spell`, `auto`, `dot`, `hot` |
| `stableKey` | string? | Game's stable key for linking |
| `procSource` | string? | What triggered this ability: `weapon`, `wand`, `bow`, `buff`, `skill` |

Null for auto-attacks without a named ability. `procSource` is only present when the ability was triggered by a proc mechanism.

### Damage Types

| Type | Description |
|------|-------------|
| `Unknown` | Unrecognized game damage type (indicates mapper needs updating) |
| `Physical` | Melee/physical damage |
| `Magic` | Arcane/magic damage |
| `Elemental` | Fire/ice/lightning |
| `Void` | Shadow/void damage |
| `Poison` | Poison/nature damage |

### Event Flags

| Flag | Type | Description |
|------|------|-------------|
| `critical` | boolean | Was a critical hit |
| `overkill` | boolean | Damage exceeded target's HP |
| `fromPlayer` | boolean | Originated from player (not NPC) |
| `pet` | boolean | Source was a pet |
| `resonating` | boolean | Spell was triggered by resonance mechanic |
| `attributionFailed` | boolean | Ability attribution failed (debug) |
| `missed` | boolean | Attack missed (failed hit roll) |
| `resisted` | boolean | Spell was fully resisted |
| `absorbed` | boolean | Damage fully absorbed by shield |

### Status Effect Events

For `buff_apply`, `buff_refresh`, `buff_fade`, `debuff_apply`, `debuff_refresh`, `debuff_fade`:

```json
{
  "effect": {
    "name": "Battle Shout",
    "duration": 300,
    "stacks": 1
  }
}
```

## File Size Estimates

| Session Type | Duration | File Size |
|--------------|----------|-----------|
| Training Dummy | 1 min | ~70 KB |
| Solo Farming | 10 min | ~1 MB |
| Group Content | 10 min | ~3 MB |
| Extended Session | 1 hour | ~20 MB |

**Note**: Files are exported as uncompressed JSON. Users can manually compress files if needed for sharing or archival.

## Versioning

The `version` field uses semantic versioning:
- Major: Breaking changes to format
- Minor: New fields added (backward compatible)
- Patch: Documentation/clarification only

Parsers should check version compatibility before processing.
