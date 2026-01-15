---
name: bepinex-mod-development
description: Develop the BepInEx mod with Harmony hooks. Use when adding hooks, working with game code, or modifying the plugin.
---

# BepInEx Mod Development

The mod uses BepInEx 5.x with Harmony for runtime patching of Erenshor's
Unity game code.

## Project Structure

```
mod/
├── src/
│   ├── Plugin.cs           # Entry point, BepInPlugin attribute
│   ├── PluginInfo.cs       # GUID, name (version is generated)
│   ├── Events/             # Event model and emitter
│   ├── Json/               # Serialization utilities
│   ├── Hooks/              # Harmony patches organized by system
│   ├── Context/            # Ability context, effect tracking
│   ├── Session/            # Session management, ring buffer
│   ├── Export/             # JSON file export
│   ├── Server/             # WebSocket server
│   └── UI/                 # IMGUI overlay
├── tests/                  # Unit tests (xUnit)
└── ErenshorLogs.csproj
```

For C# coding conventions (records, JSON serialization, etc.), see the
`csharp-conventions` skill.

## Finding Methods to Hook

Decompiled game source should be in `reference/game-source/`. If not set up,
see `reference/game-source/README.md` for instructions.

Key classes for combat logging:
- `Character`: DamageMe, MagicDamageMe, BleedDamageMe, CreditDPS
- `Stats`: HealMe, AddStatusEffect, TickEffects
- `UseSkill`: DoSkill (skill activation)
- `CastSpell`: StartSpell, StartSpellFromProc
- `SpellVessel`: ResolveSpell (spell damage application)

## Adding Harmony Hooks

### Basic Postfix Hook

Use Postfix to run code after the original method:

```csharp
[HarmonyPatch(typeof(Character), nameof(Character.DamageMe))]
class DamageMe_Patch
{
    [HarmonyPostfix]
    static void Postfix(
        int __result,           // Return value
        Character __instance,   // The object (this)
        int _incdmg,           // Original parameter
        Character _attacker)   // Original parameter
    {
        // __result contains the actual damage dealt
        EventEmitter.Emit(new DamageEvent { ... });
    }
}
```

### Prefix Hook

Use Prefix to run code before the original, or to skip it entirely:

```csharp
[HarmonyPrefix]
static bool Prefix(Character __instance)
{
    // Return false to skip original method
    // Return true to continue to original
    CombatContext.Push(new AbilityContext { ... });
    return true;
}
```

### Accessing Private Fields

Use triple-underscore prefix to access private fields:

```csharp
[HarmonyPostfix]
static void Postfix(
    List<StatusEffect> ___StatusEffects,  // Private field
    float ___RollingDPS)                   // Another private field
{
    // Fields are passed by reference
}
```

## BepInEx Configuration

Define config entries in Plugin.cs:

```csharp
private ConfigEntry<int> _bufferSize;
private ConfigEntry<int> _wsPort;

void Awake()
{
    _bufferSize = Config.Bind("General", "BufferSize", 50000,
        "Maximum events to keep in memory");
    _wsPort = Config.Bind("WebSocket", "Port", 8765,
        "WebSocket server port");
}
```

Config is saved to `BepInEx/config/com.github.glockyco.erenshorlogs.cfg`.

## Common Pitfalls

**Method overloads**: Harmony matches by parameter types. If a method has
overloads, specify the exact signature:

```csharp
[HarmonyPatch(typeof(Stats), "HealMe", new Type[] { typeof(int) })]
```

**Instance vs static**: Check if the target method is static. Static methods
don't have `__instance`.

**Unity lifecycle**: Don't access Unity objects in Awake before the game
initializes them. Use null checks liberally.

**Threading**: Harmony patches run on Unity's main thread. Don't block.
WebSocket operations should happen on a background thread.

**Parameter names**: Original parameter names must match exactly (including
underscore prefix convention the game uses like `_incdmg`).

## Testing Changes

### Unit Tests

Run unit tests from the mod directory:

```bash
cd mod
dotnet test
```

Tests require game DLLs to be present (run `erenshor setup` first).

### In-Game Testing

Use the development CLI for the full workflow:

```bash
cd cli
uv run erenshor deploy   # Build and copy to plugins folder
uv run erenshor launch   # Start the game
```

Check `BepInEx/LogOutput.log` for errors and verify events are captured.
