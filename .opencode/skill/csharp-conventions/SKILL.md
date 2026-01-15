---
name: csharp-conventions
description: Project-specific C# patterns. Use when writing mod code, especially services, JSON serialization, or DI integration.
---

# C# Conventions

Project-specific patterns for the Erenshor Logs mod. Assumes familiarity with
modern C# (records, nullable references, collection expressions).

## Unity Compatibility

Target `netstandard2.1` for Unity/BepInEx. The `PolySharp` package provides
compile-time polyfills for `record`, `init`, and `required` - no runtime
dependency added.

## JSON Serialization

Always use `JsonContext.Options` for serialization:

```csharp
var json = JsonSerializer.Serialize(event, JsonContext.Options);
var parsed = JsonSerializer.Deserialize<CombatEvent>(json, JsonContext.Options);
```

Enums serialize as snake_case strings:

| C# | JSON |
|----|------|
| `EventType.DamageMelee` | `"damage_melee"` |
| `DamageType.Physical` | `"physical"` |

## Naming Conventions

| Element | Convention | Example |
|---------|------------|---------|
| Types | PascalCase | `CombatEvent`, `ActorRef` |
| Properties | PascalCase | `EventType`, `Timestamp` |
| Private fields | _camelCase | `_harmony`, `_buffer` |
| Local variables | camelCase | `eventCount`, `actor` |
| Constants | PascalCase | `MaxBufferSize` |

## Dependency Injection

The mod uses `Microsoft.Extensions.DependencyInjection`. Plugin.cs is the
composition root:

```csharp
public sealed class Plugin : BaseUnityPlugin
{
    internal static ServiceProvider ServiceProvider { get; private set; } = null!;

    private void Awake()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IEventEmitter>(new EventEmitter(Log));
        ServiceProvider = services.BuildServiceProvider();
    }
}
```

### Service Design

- Public constructors (no `internal` visibility tricks)
- Optional logging via `Action<string>?` to avoid BepInEx coupling in tests
- Interface-based: define `IFoo`, implement in `Foo`

```csharp
public sealed class EventEmitter : IEventEmitter
{
    private readonly Action<string>? _log;

    public EventEmitter(Action<string>? log = null)
    {
        _log = log;
    }
}
```

### Harmony Patch Integration

Patches are static and can't use constructor injection. Use static properties
set during plugin startup:

```csharp
[HarmonyPatch(typeof(Stats), nameof(Stats.TakeDamage))]
public static class TakeDamagePatch
{
    internal static IEventEmitter? Emitter { get; set; }

    [HarmonyPostfix]
    public static void Postfix(Stats __instance, int damage)
    {
        Emitter?.Emit(new DamageEvent { /* ... */ });
    }
}
```

In Plugin.Awake():

```csharp
TakeDamagePatch.Emitter = ServiceProvider.GetRequiredService<IEventEmitter>();
_harmony.PatchAll();
```

### Testing

Create services directly without DI container:

```csharp
var emitter = new EventEmitter(msg => logged.Add(msg));
```
