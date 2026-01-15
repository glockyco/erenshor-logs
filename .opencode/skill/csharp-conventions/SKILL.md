---
name: csharp-conventions
description: Modern C# patterns for the mod. Use when writing C# code, defining data types, or working with JSON serialization.
---

# C# Conventions

Modern C# patterns and conventions for the Erenshor Logs mod. The project uses
C# 12+ features while targeting netstandard2.1 for Unity compatibility.

## Runtime Compatibility

The mod targets `netstandard2.1` for Unity/BepInEx compatibility but uses modern
C# features. The `PolySharp` package provides compile-time polyfills for language
features that require runtime types not present in netstandard2.1:

- `record` types (C# 9)
- `init` properties (C# 9)
- `required` modifier (C# 11)

PolySharp is a compile-time only dependency (`PrivateAssets="all"`) that injects
the necessary type stubs. No runtime dependency is added to the mod.

## Data Types

### Records for Data

Use `record` types for immutable data (events, snapshots, references):

```csharp
public sealed record ActorRef
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required ActorType Type { get; init; }
    public string? Class { get; init; }  // Nullable = optional
    public int? Level { get; init; }
}
```

Use `class` for mutable state (trackers, buffers, services).

### Required Properties

Use `required` modifier for properties that must always be set:

```csharp
public sealed record CombatEvent
{
    public required string Id { get; init; }
    public required long Timestamp { get; init; }
    public required EventType EventType { get; init; }
    public string? AbilityName { get; init; }  // Optional
}
```

This ensures consumers can't create incomplete objects.

### Sealed Classes

Mark classes and records as `sealed` unless inheritance is intended:

```csharp
public sealed class Plugin : BaseUnityPlugin { }
public sealed record CombatEvent { }
```

## JSON Serialization

Use `System.Text.Json` with explicit property names:

```csharp
using System.Text.Json.Serialization;

public sealed record ActorRef
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("type")]
    public required ActorType Type { get; init; }
}
```

### Enum Serialization

Enums serialize as snake_case strings. Use `JsonContext.Options` for
serialization to ensure consistent behavior:

```csharp
var json = JsonSerializer.Serialize(event, JsonContext.Options);
var parsed = JsonSerializer.Deserialize<CombatEvent>(json, JsonContext.Options);
```

C# enum values use PascalCase, JSON uses snake_case:

| C# | JSON |
|----|------|
| `EventType.DamageMelee` | `"damage_melee"` |
| `DamageType.Physical` | `"physical"` |

## File Organization

- One public type per file
- File name matches type name: `CombatEvent.cs`
- Organize by feature: `Events/`, `Hooks/`, `Json/`

## Naming Conventions

| Element | Convention | Example |
|---------|------------|---------|
| Types | PascalCase | `CombatEvent`, `ActorRef` |
| Properties | PascalCase | `EventType`, `Timestamp` |
| Private fields | _camelCase | `_harmony`, `_buffer` |
| Local variables | camelCase | `eventCount`, `actor` |
| Constants | PascalCase | `MaxBufferSize` |

## Common Patterns

### Nullable Reference Types

Use nullable annotations consistently:

```csharp
public string? OptionalField { get; init; }   // Can be null
public required string RequiredField { get; init; }  // Never null
```

### Collection Expressions (C# 12)

Use collection expressions for inline collections:

```csharp
int[] numbers = [1, 2, 3];
List<string> names = ["Alice", "Bob"];
```

### File-Scoped Namespaces

Use file-scoped namespaces to reduce nesting:

```csharp
namespace ErenshorLogs.Events;

public sealed record CombatEvent { }
```
