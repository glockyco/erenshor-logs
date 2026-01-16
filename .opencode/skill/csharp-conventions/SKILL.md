---
name: csharp-conventions
description: Project-specific C# patterns. Use when writing mod code, especially services, JSON serialization, DI integration, or testing services with game types.
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

## Testing Services with Game Types

Services that work with Unity types (`Character`, `Stats`, `SpellVessel`) need
special handling because these types require the Unity runtime. Use the
generics + adapter pattern to separate testable logic from game type access.

**Structure:**

1. Generic class contains all logic, parameterized on the game type
2. Public interface uses the concrete game type
3. Adapter implements the interface and wires delegates to the generic

```csharp
// Generic implementation - all logic here, fully testable
public sealed class ActorRegistry<TCharacter> where TCharacter : class
{
    private readonly Func<TCharacter, int> _getInstanceId;

    public ActorRegistry(Func<TCharacter, int> getInstanceId) { /* ... */ }
    public ActorRef? GetOrCreate(TCharacter? character) { /* logic */ }
}

// Public interface - uses concrete game type
public interface IActorRegistry
{
    ActorRef? GetOrCreate(Character character);
}

// Adapter - wires generic to game types, registered in DI
public sealed class ActorRegistryAdapter : IActorRegistry
{
    private readonly ActorRegistry<Character> _inner;

    public ActorRegistryAdapter(/* deps */)
    {
        _inner = new ActorRegistry<Character>(c => c.GetInstanceID());
    }

    public ActorRef? GetOrCreate(Character character) => _inner.GetOrCreate(character);
}
```

**Testing:** Instantiate the generic directly with a mock type:

```csharp
private sealed class MockCharacter { public int InstanceId { get; init; } }

var registry = new ActorRegistry<MockCharacter>(c => c.InstanceId);
var result = registry.GetOrCreate(new MockCharacter { InstanceId = 1 });
```

This ensures tests exercise the same code path as production while avoiding
game type dependencies. See `ActorRegistry` for a complete example.
