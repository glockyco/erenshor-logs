---
name: adding-event-types
description: Add new combat event types to the logging system. Use when capturing a new type of combat data like a new damage source or effect type.
---

# Adding Event Types

This guide walks through adding a new event type from mod to web app.

## Overview

Adding an event type touches multiple layers:
1. C# event model (mod)
2. Harmony hooks (mod)
3. TypeScript types (web)
4. Aggregation logic (web)
5. UI display (web)

## Step 1: Update C# Event Model

Add the new event type to the `EventType` enum in `mod/src/Events/EventType.cs`:

```csharp
public enum EventType
{
    // Existing types...
    DamageMelee,
    DamageSpell,

    // New type
    DamageReflect,  // Add here
}
```

If the event needs new fields, add them to `CombatEvent.cs` using record syntax:

```csharp
public sealed record CombatEvent
{
    // Existing fields...

    // New optional field for reflect damage
    [JsonPropertyName("reflectedFrom")]
    public int? ReflectedFrom { get; init; }
}
```

See the `csharp-conventions` skill for record and JSON serialization patterns.

## Step 2: Add Harmony Hooks

Create or update hooks to capture the event. See `reference/game-source/` for
method signatures.

```csharp
[HarmonyPatch(typeof(Character), nameof(Character.DamageShieldTaken))]
class DamageShieldTaken_Patch
{
    [HarmonyPostfix]
    static void Postfix(int _dmg, Stats _attacker, Character __instance)
    {
        EventEmitter.Emit(new CombatEvent
        {
            EventType = EventType.DamageReflect,
            Amount = _dmg,
            Source = ActorRegistry.GetActor(__instance),
            Target = ActorRegistry.GetActor(_attacker.Myself),
            // Set other fields...
        });
    }
}
```

Register the hook in `Plugin.cs` if using manual patching.

## Step 3: Update TypeScript Types

Mirror changes in the TypeScript types:

```typescript
export type EventType =
  | 'damage_melee'
  | 'damage_spell'
  | 'damage_reflect'  // Add here
  // ...

export interface CombatEvent {
  // Existing fields...

  reflectedFrom?: number;  // Add if needed
}
```

TypeScript types use snake_case to match JSON serialization (e.g.,
`damage_reflect` not `DamageReflect`).

## Step 4: Update State and Aggregation

If the event contributes to existing metrics, update the aggregation in
`web/src/lib/state/` or `web/src/lib/data/`:

```typescript
function aggregateDamage(events: CombatEvent[]): DamageBreakdown {
  return events
    .filter(e =>
      e.eventType === 'damage_melee' ||
      e.eventType === 'damage_spell' ||
      e.eventType === 'damage_reflect'  // Include in damage totals
    )
    .reduce((acc, e) => { ... }, {});
}
```

For new metrics, add a new aggregation function.

## Step 5: Update UI

Add display logic in the relevant Svelte components.

For the breakdown view (`web/src/lib/components/Breakdown.svelte`):
```svelte
{#if event.eventType === 'damage_reflect'}
  <span class="text-purple-400">Reflect</span>
{/if}
```

For the event log, add column formatting if needed.

## Step 6: Update Protocol (if streaming)

If the event should be streamed via WebSocket, ensure the serialization works:

1. Test that the event round-trips through JSON correctly
2. Verify the web app's WebSocket client handles it
3. Update `shared/schema.json` if maintaining a formal schema

## Checklist

- [ ] EventType enum updated (C#)
- [ ] CombatEvent fields added if needed (C#)
- [ ] Harmony hook captures the event
- [ ] Unit tests added for serialization
- [ ] EventType added (TypeScript)
- [ ] CombatEvent interface updated (TypeScript)
- [ ] Aggregation includes new event type
- [ ] UI displays the event appropriately
- [ ] Tested end-to-end (mod → export → web import)
- [ ] Tested live mode if applicable
