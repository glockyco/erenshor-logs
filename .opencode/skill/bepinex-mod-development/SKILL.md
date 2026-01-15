---
name: bepinex-mod-development
description: Project-specific mod patterns. Use when adding hooks, finding game methods, or testing the mod.
---

# BepInEx Mod Development

Project-specific patterns for the combat logging mod. Assumes familiarity with
BepInEx and Harmony basics (Prefix/Postfix, `__instance`, private field access).

## Project Structure

```
mod/
├── src/
│   ├── Plugin.cs           # Entry point, composition root
│   ├── PluginInfo.cs       # GUID, name, version
│   ├── Events/             # Event model and emitter
│   ├── Json/               # Serialization (JsonContext)
│   ├── Hooks/              # Harmony patches by system
│   ├── Context/            # Ability context tracking
│   ├── Session/            # Session management, ring buffer
│   ├── Export/             # JSON file export
│   ├── Server/             # WebSocket server (Fleck)
│   └── UI/                 # IMGUI overlay
└── tests/                  # Unit tests (xUnit)
```

## Game Reference

Decompiled source: `reference/game-source/` (not committed, see README there).

Key classes for combat logging:

| Class | Methods |
|-------|---------|
| `Character` | `DamageMe`, `MagicDamageMe`, `BleedDamageMe`, `CreditDPS` |
| `Stats` | `HealMe`, `AddStatusEffect`, `TickEffects` |
| `UseSkill` | `DoSkill` (skill activation) |
| `CastSpell` | `StartSpell`, `StartSpellFromProc` |
| `SpellVessel` | `ResolveSpell` (spell damage) |

Combat ticks every 3 seconds via `Stats.TickEffects()`.

## Common Pitfalls

**Parameter names must match exactly** - The game uses underscore prefixes like
`_incdmg`, `_attacker`. Harmony requires exact matches.

**Method overloads** - Specify exact signature when needed:
```csharp
[HarmonyPatch(typeof(Stats), "HealMe", new Type[] { typeof(int) })]
```

**Unity lifecycle** - Don't access Unity objects before initialization. Use
null checks: `if (_attacker?.MyStats == null) return;`

**Threading** - Patches run on Unity's main thread. Don't block. WebSocket
operations go on background threads.

## Testing Workflow

```bash
# Unit tests
cd mod && dotnet test tests/ErenshorLogs.Tests

# In-game testing via CLI
cd cli
uv run erenshor deploy   # Build and copy to plugins folder
uv run erenshor launch   # Start the game
```

Check `BepInEx/LogOutput.log` for errors.
