# Raid Combat Tracking Update Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Cut the mod and web analyzer over to current playtest raid combat and capture health-affecting protocol v2 events end to end.

**Architecture:** The mod compiles against playtest game references and emits authoritative protocol v2 events for damage, healing, resources, effects, deaths, and health-affecting mechanics. The web app validates the same schema for live and file data, then derives summaries from events and registries without guessing missing game facts.

**Tech Stack:** C# netstandard2.1, BepInEx, HarmonyX, Newtonsoft.Json, xUnit, Svelte 5, TypeScript, Zod, Vitest, JSON Schema, Cloudflare static assets.

---

## File structure

### Shared protocol

- Modify: `shared/protocol/schemas/erenshor-log-v2.schema.json`
  - Add `mechanicEvent`.
  - Add `resource` action `drain`.
  - Add `heal` action `scripted`.
  - Add optional `raidGroup` and `raidRole` on actor records.
- Modify: `shared/protocol/fixtures/live/events.json`
  - Include representative damage, heal, resource, effect, death, and mechanic
    events in one contiguous batch.
- Modify: `shared/protocol/fixtures/live/registry-delta.json`
  - Include raid actor metadata and mechanic ability records.
- Modify: `shared/protocol/fixtures/live/session-snapshot.json`
  - Include registries with raid metadata.
- Modify: `shared/protocol/fixtures/export/single-session.json`
  - Include the same event families in exported form.
- Modify: `shared/protocol/fixtures/export/multi-session.json`
  - Keep multi-session coverage aligned with the expanded schema.

### Mod build and references

- Modify: `cli/src/erenshor_dev/commands/setup.py`
  - Make playtest setup explicit.
  - Copy playtest game assemblies into `mod/lib`.
- Modify: `cli/tests/test_setup.py`
  - Cover playtest reference selection.
- Modify: `mod/ErenshorLogs.csproj`
  - Keep direct references to `mod/lib/Assembly-CSharp.dll`.
  - Do not add main compatibility conditions.
- Modify: `mod/lib/README.md`
  - State that local references must come from current playtest.

### Mod event model and protocol

- Modify: `mod/src/Events/EventType.cs`
  - Add or normalize family values used by protocol serialization.
- Modify: `mod/src/Events/CombatEvent.cs`
  - Add fields needed for heal, resource, effect, death, and mechanic events.
- Create: `mod/src/Events/MechanicData.cs`
  - Small strongly typed data for health-affecting encounter facts.
- Modify: `mod/src/Hooks/CombatEventBuilder.cs`
- Modify: `mod/src/Hooks/CombatEventBuilderAdapter.cs`
- Modify: `mod/src/Hooks/ICombatEventBuilder.cs`
  - Add builder methods for non-damage families.
- Modify: `mod/src/Protocol/ProtocolSessionState.cs`
  - Serialize every protocol event family.
- Modify: `mod/src/Protocol/V2Messages.cs`
  - Add DTOs for mechanic events and actor raid metadata.

### Mod playtest raid relevance

- Modify: `mod/src/Hooks/CombatRelevanceCheckerAdapter.cs`
  - Remove reflection for raid fields.
  - Use direct playtest members.
  - Include raid targets and loose adds.
- Modify: `mod/src/Hooks/CombatRelevanceChecker.cs`
  - Add predicates for raid targets and loose adds.
  - Add cache invalidation entry points.
- Create: `mod/src/Hooks/RaidRelevanceInvalidationPatches.cs`
  - Clear relevance cache on roster and target changes.

### Mod capture hooks

- Modify: `mod/src/Hooks/AEEventUpdatePatch.cs`
  - Remove or narrow the old update-only context behavior.
- Create: `mod/src/Hooks/AEEventTriggerPatch.cs`
  - Push area-effect context around `AEEvent.TriggerAE()`.
- Create: `mod/src/Hooks/DeathTouchPatch.cs`
  - Add fixed context and mechanic emission for Death Touch.
- Create: `mod/src/Hooks/MizukiEventPatch.cs`
  - Add fixed context for dagger damage and mechanic events.
- Create: `mod/src/Hooks/HealMePatch.cs`
  - Capture method-based healing.
- Create: `mod/src/Hooks/ScriptedHealthPatch.cs`
  - Capture direct HP resets in named encounter methods.
- Create: `mod/src/Hooks/ResourceChangePatch.cs`
  - Capture mana drains and restores in named encounter methods.
- Modify: `mod/src/Hooks/AddStatusEffectPatch.cs`
  - Emit effect events and preserve owner where known.
- Modify: `mod/src/Hooks/RemoveStatusEffectPatch.cs`
  - Emit fade events.
- Create: `mod/src/Hooks/RaidAuraPatch.cs`
  - Capture aura owner while `RaidManager.UpdateGroupAuras()` applies auras.
- Create: `mod/src/Hooks/EncounterMechanicPatch.cs`
  - Emit mechanic events for Sprinkles, Faith, DPS checks, and AE retuning.

### Mod tests

- Modify: `mod/tests/ErenshorLogs.Tests/Protocol/ProtocolSessionStateTests.cs`
- Modify: `mod/tests/ErenshorLogs.Tests/Protocol/ProtocolFixtureTests.cs`
- Modify: `mod/tests/ErenshorLogs.Tests/Hooks/ContextPatchBalanceTests.cs`
- Modify: `mod/tests/ErenshorLogs.Tests/Hooks/CombatRelevanceCheckerTests.cs`
- Modify: `mod/tests/ErenshorLogs.Tests/Hooks/CombatEventBuilderTests.cs`
- Create: `mod/tests/ErenshorLogs.Tests/Hooks/RaidRelevanceInvalidationTests.cs`
- Create: `mod/tests/ErenshorLogs.Tests/Hooks/HealthEventBuilderTests.cs`
- Create: `mod/tests/ErenshorLogs.Tests/Hooks/ResourceEventBuilderTests.cs`
- Create: `mod/tests/ErenshorLogs.Tests/Hooks/StatusEventBuilderTests.cs`
- Create: `mod/tests/ErenshorLogs.Tests/Hooks/MechanicEventBuilderTests.cs`

### Web types and analysis

- Modify: `web/src/lib/types/schemas.ts`
  - Mirror shared schema additions in Zod.
- Modify: `web/src/lib/services/message-parser.ts`
  - Continue rejecting unsupported major versions and accept expanded v2 events.
- Modify: `web/src/lib/services/session-importer.ts`
- Modify: `web/src/lib/services/session-exporter.ts`
- Modify: `web/src/lib/state/sessions.svelte.ts`
  - Store expanded event families without corrupting event ordering.
- Modify: `web/src/lib/services/combat-analyzer.ts`
  - Include healing totals.
  - Exclude resource, effect, death, and mechanic events from DPS.
  - Expose death, resource, status, and mechanic counts for UI consumers.
- Modify: `web/src/lib/services/protocol-fixtures.test.ts`
- Modify: `web/src/lib/services/combat-analyzer.test.ts`
- Modify: `web/src/lib/services/session-importer.test.ts`
- Modify: `web/src/lib/services/session-exporter.test.ts`

### Demo data and docs

- Modify: `web/static/demo/sessions.json`
  - Regenerate as protocol v2 with new event families only if fixtures require
    demo coverage.
- Modify: `docs/LOG_FORMAT.md`
  - Document mechanic events, resource drain, scripted healing, raid actor
    metadata, and playtest-only support.
- Modify: `docs/COMBAT_EVENTS.md`
  - Document hook sources and event families.
- Modify: `docs/ARCHITECTURE.md`
  - Document playtest-only compiled references and raid capture pipeline.

---

## Task 1: Protocol schema and fixtures

**Files:**

- Modify: `shared/protocol/schemas/erenshor-log-v2.schema.json`
- Modify: `shared/protocol/fixtures/live/events.json`
- Modify: `shared/protocol/fixtures/live/registry-delta.json`
- Modify: `shared/protocol/fixtures/live/session-snapshot.json`
- Modify: `shared/protocol/fixtures/export/single-session.json`
- Modify: `shared/protocol/fixtures/export/multi-session.json`
- Test: `web/src/lib/services/protocol-fixtures.test.ts`
- Test: `mod/tests/ErenshorLogs.Tests/Protocol/ProtocolFixtureTests.cs`

- [ ] **Step 1: Add failing fixture expectations in web tests**

Add assertions to `web/src/lib/services/protocol-fixtures.test.ts` that require
all new event families from the shared fixtures.

```ts
it("fixtures include health-affecting raid event families", async () => {
  const file = await loadFixture("export/single-session.json");
  const events = file.sessions.flatMap((session) => session.events);

  expect(events.some((event) => event.kind === "damage")).toBe(true);
  expect(events.some((event) => event.kind === "heal")).toBe(true);
  expect(events.some((event) => event.kind === "resource")).toBe(true);
  expect(events.some((event) => event.kind === "effect")).toBe(true);
  expect(events.some((event) => event.kind === "death")).toBe(true);
  expect(events.some((event) => event.kind === "mechanic")).toBe(true);
});
```

- [ ] **Step 2: Run the web fixture test and verify it fails**

Run:

```bash
cd web && pnpm test:run -- src/lib/services/protocol-fixtures.test.ts
```

Expected: fail because `mechanic` events and some health-affecting fixture events
are not present or not accepted by the schema.

- [ ] **Step 3: Extend the shared schema**

Update `shared/protocol/schemas/erenshor-log-v2.schema.json`:

```json
"combatEventRecord": {
  "oneOf": [
    { "$ref": "#/$defs/damageEvent" },
    { "$ref": "#/$defs/healEvent" },
    { "$ref": "#/$defs/resourceEvent" },
    { "$ref": "#/$defs/effectEvent" },
    { "$ref": "#/$defs/deathEvent" },
    { "$ref": "#/$defs/interruptEvent" },
    { "$ref": "#/$defs/mechanicEvent" }
  ]
}
```

Add `raidGroup` and `raidRole` to actor records:

```json
"raidGroup": { "type": "integer", "minimum": 1, "maximum": 3 },
"raidRole": { "enum": ["tank", "healer", "dps", "puller", "unknown"] }
```

Change resource actions:

```json
"action": { "enum": ["spend", "restore", "regen", "drain"] }
```

Change heal actions:

```json
"action": { "enum": ["direct", "tick", "lifesteal", "regen", "scripted"] }
```

Add mechanic definitions:

```json
"mechanicEvent": {
  "allOf": [
    { "$ref": "#/$defs/eventBase" },
    {
      "properties": {
        "kind": { "const": "mechanic" },
        "action": {
          "enum": [
            "phase",
            "invulnerability",
            "spawn",
            "despawn",
            "statChange",
            "targetAssignment"
          ]
        },
        "data": { "$ref": "#/$defs/mechanicData" }
      }
    }
  ]
},
"mechanicData": {
  "type": "object",
  "required": ["name"],
  "additionalProperties": false,
  "properties": {
    "name": { "type": "string" },
    "value": { "type": ["string", "number", "boolean"] },
    "previousValue": { "type": ["string", "number", "boolean"] },
    "affectedStat": {
      "enum": ["hp", "mana", "damage", "resist", "armorPen"]
    },
    "amount": { "type": "integer" }
  }
}
```

- [ ] **Step 4: Update shared fixtures**

Update `shared/protocol/fixtures/live/events.json` so the event batch contains a
contiguous sequence like this:

```json
[
  {
    "eventSeq": 1,
    "offsetMs": 250,
    "kind": "damage",
    "action": "hit",
    "sourceActorId": "player:0",
    "creditActorId": "player:0",
    "targetActorId": "npc:mizuki",
    "abilityId": "skill:101",
    "attribution": "context",
    "data": {
      "amount": 350,
      "rawAmount": 400,
      "mitigatedAmount": 50,
      "damageType": "physical",
      "outcome": { "result": "landed" }
    }
  },
  {
    "eventSeq": 2,
    "offsetMs": 500,
    "kind": "heal",
    "action": "scripted",
    "sourceActorId": "npc:grace",
    "targetActorId": "npc:grace",
    "abilityId": "mechanic:grace-echoes",
    "attribution": "verified",
    "data": { "amount": 200000, "rawAmount": 200000 }
  },
  {
    "eventSeq": 3,
    "offsetMs": 750,
    "kind": "resource",
    "action": "drain",
    "sourceActorId": "npc:mana-drain",
    "targetActorId": "player:0",
    "abilityId": "mechanic:mana-drain",
    "attribution": "verified",
    "data": { "resource": "mana", "delta": -300, "current": 1200, "max": 1500 }
  },
  {
    "eventSeq": 4,
    "offsetMs": 900,
    "kind": "effect",
    "action": "apply",
    "sourceActorId": "npc:mizuki",
    "targetActorId": "player:0",
    "effectId": "effect:bleed-ref",
    "attribution": "verified",
    "data": { "durationMs": 12000 }
  },
  {
    "eventSeq": 5,
    "offsetMs": 1200,
    "kind": "death",
    "action": "die",
    "sourceActorId": "npc:death-touch",
    "targetActorId": "sim:cleric",
    "abilityId": "mechanic:death-touch",
    "attribution": "verified",
    "data": { "killingBlowEventSeq": 1 }
  },
  {
    "eventSeq": 6,
    "offsetMs": 1500,
    "kind": "mechanic",
    "action": "invulnerability",
    "sourceActorId": "npc:sprinkles",
    "targetActorId": "npc:sprinkles",
    "abilityId": "mechanic:sprinkles-wards",
    "attribution": "verified",
    "data": { "name": "Sprinkles wards", "value": true }
  }
]
```

Also update registry fixtures with matching actor, ability, and effect IDs.
Include at least one actor with:

```json
"raidGroup": 1,
"raidRole": "healer"
```

- [ ] **Step 5: Run fixture tests and verify they pass**

Run:

```bash
cd web && pnpm test:run -- src/lib/services/protocol-fixtures.test.ts
cd mod && dotnet test tests/ErenshorLogs.Tests --filter ProtocolFixtureTests
```

Expected: both pass.

- [ ] **Step 6: Commit protocol fixtures**

```bash
git add shared/protocol web/src/lib/services/protocol-fixtures.test.ts mod/tests/ErenshorLogs.Tests/Protocol/ProtocolFixtureTests.cs
git commit -m "feat(protocol): add raid event families"
```

---

## Task 2: Playtest-only compiled references

**Files:**

- Modify: `cli/src/erenshor_dev/commands/setup.py`
- Create: `cli/tests/test_setup.py`
- Modify: `mod/lib/README.md`
- Modify: `docs/ARCHITECTURE.md`

- [ ] **Step 1: Write failing CLI setup tests**

Create `cli/tests/test_setup.py`:

```py
import unittest
from pathlib import Path

from erenshor_dev.commands import setup


class SetupReferenceTests(unittest.TestCase):
    def test_required_dlls_include_input_legacy_module(self) -> None:
        self.assertIn("Assembly-CSharp.dll", setup.REQUIRED_DLLS)
        self.assertIn("UnityEngine.CoreModule.dll", setup.REQUIRED_DLLS)
        self.assertIn("UnityEngine.InputLegacyModule.dll", setup.REQUIRED_DLLS)

    def test_playtest_variant_uses_configured_game_install(self) -> None:
        source = setup.resolve_managed_source(
            erenshor_path=Path("/Games/Erenshor Playtest"),
            variant="playtest",
        )

        self.assertEqual(source, Path("/Games/Erenshor Playtest/Erenshor_Data/Managed"))

    def test_main_variant_is_rejected(self) -> None:
        with self.assertRaises(ValueError):
            setup.resolve_managed_source(
                erenshor_path=Path("/Games/Erenshor"),
                variant="main",
            )


if __name__ == "__main__":
    unittest.main()
```

- [ ] **Step 2: Run test and verify it fails**

```bash
cd cli && uv run python -m unittest tests.test_setup
```

Expected: fail because `resolve_managed_source` and the extra DLL entry are not
present.

- [ ] **Step 3: Implement playtest setup helpers**

In `cli/src/erenshor_dev/commands/setup.py`, update `REQUIRED_DLLS`:

```py
REQUIRED_DLLS = [
    "Assembly-CSharp.dll",
    "UnityEngine.dll",
    "UnityEngine.CoreModule.dll",
    "UnityEngine.InputLegacyModule.dll",
]
```

Add:

```py
def resolve_managed_source(erenshor_path: Path, variant: str) -> Path:
    """Resolve the managed assembly source for the supported game variant."""
    if variant != "playtest":
        raise ValueError("Erenshor Logs currently targets the playtest build")
    return erenshor_path / "Erenshor_Data" / "Managed"
```

Update the command decorator and command body:

```py
@click.command()
@click.option(
    "--variant",
    type=click.Choice(["playtest"]),
    default="playtest",
    show_default=True,
    help="Game build to copy references from.",
)
def setup(variant: str) -> None:
    config = load_config()
    managed_path = resolve_managed_source(config.erenshor_path, variant)
```

- [ ] **Step 4: Update docs**

In `mod/lib/README.md`, state:

```md
These local references must be copied from the current Erenshor Playtest
installation with `cd cli && uv run erenshor setup --variant playtest`.

The mod no longer supports compiling against the main game build. Do not commit
DLLs from either game build.
```

In `docs/ARCHITECTURE.md`, add one paragraph to the mod build section:

```md
The mod targets the current Erenshor Playtest assemblies. Playtest-only raid
members and raid target APIs are compiled directly instead of accessed through
reflection. Main game compatibility is intentionally out of scope for the raid
tracking update.
```

- [ ] **Step 5: Run CLI gates**

```bash
cd cli && uv run python -m unittest discover tests
cd cli && uv run ruff check src tests
cd cli && uv run mypy src
```

Expected: all pass.

- [ ] **Step 6: Commit playtest reference setup**

```bash
git add cli/src/erenshor_dev/commands/setup.py cli/tests/test_setup.py mod/lib/README.md docs/ARCHITECTURE.md
git commit -m "build(mod): target playtest references"
```

---

## Task 3: Mod event model for all protocol families

**Files:**

- Modify: `mod/src/Events/EventType.cs`
- Modify: `mod/src/Events/CombatEvent.cs`
- Create: `mod/src/Events/MechanicData.cs`
- Modify: `mod/src/Hooks/ICombatEventBuilder.cs`
- Modify: `mod/src/Hooks/CombatEventBuilder.cs`
- Modify: `mod/src/Hooks/CombatEventBuilderAdapter.cs`
- Test: `mod/tests/ErenshorLogs.Tests/Hooks/CombatEventBuilderTests.cs`
- Create: `mod/tests/ErenshorLogs.Tests/Hooks/HealthEventBuilderTests.cs`
- Create: `mod/tests/ErenshorLogs.Tests/Hooks/ResourceEventBuilderTests.cs`
- Create: `mod/tests/ErenshorLogs.Tests/Hooks/StatusEventBuilderTests.cs`
- Create: `mod/tests/ErenshorLogs.Tests/Hooks/MechanicEventBuilderTests.cs`

- [ ] **Step 1: Write failing event builder tests**

Create `mod/tests/ErenshorLogs.Tests/Hooks/HealthEventBuilderTests.cs`:

```csharp
using System.Collections.Generic;
using ErenshorLogs.Events;
using ErenshorLogs.Hooks;
using Xunit;

namespace ErenshorLogs.Tests.Hooks;

public sealed class HealthEventBuilderTests
{
  [Fact]
  public void CreateHealEvent_BuildsScriptedHeal()
  {
    var source = new object();
    var target = new object();
    var actors = new Dictionary<object, ActorRef>
    {
      [source] = new() { Id = "npc:grace", Name = "Grace", Type = ActorType.Npc },
      [target] = new() { Id = "npc:grace", Name = "Grace", Type = ActorType.Npc },
    };
    var builder = new CombatEventBuilder<object>(
      actor => actor == null ? null : actors[actor],
      () => "evt-1",
      () => 1_800_000_000_000
    );

    var evt = builder.CreateHealEvent(
      source: source,
      target: target,
      ability: new AbilityRef
      {
        Name = "Grace Echoes",
        Type = AbilityType.AreaEffect,
        StableKey = "mechanic:grace-echoes",
      },
      amount: 200000,
      rawAmount: 200000,
      overhealAmount: 0,
      eventType: EventType.HealSpell
    );

    Assert.Equal(EventType.HealSpell, evt.EventType);
    Assert.Equal(200000, evt.Amount);
    Assert.Equal(200000, evt.RawAmount);
    Assert.Equal("Grace Echoes", evt.Ability.Name);
  }
}
```

Create analogous tests for resource, effect, death, and mechanic builders. Use
these expected values:

- resource drain: `EventType.ManaUse`, delta `-300`, current `1200`, max `1500`
- effect apply: `EventType.DebuffApply`, effect name `BleedRef`
- death: `EventType.Death`, target `sim:cleric`, ability `Death Touch`
- mechanic: action `invulnerability`, name `Sprinkles wards`, value `true`

- [ ] **Step 2: Run tests and verify they fail**

```bash
cd mod && dotnet test tests/ErenshorLogs.Tests --filter "HealthEventBuilderTests|ResourceEventBuilderTests|StatusEventBuilderTests|MechanicEventBuilderTests"
```

Expected: fail because builder methods and fields do not exist.

- [ ] **Step 3: Extend the internal event model**

Add `mod/src/Events/MechanicData.cs`:

```csharp
namespace ErenshorLogs.Events;

public sealed record MechanicData
{
  public required string Name { get; init; }
  public string? Action { get; init; }
  public object? Value { get; init; }
  public object? PreviousValue { get; init; }
  public string? AffectedStat { get; init; }
  public int? Amount { get; init; }
}
```

Add fields to `CombatEvent`:

```csharp
public int? OverhealAmount { get; init; }
public string? ResourceType { get; init; }
public int? ResourceDelta { get; init; }
public int? ResourceCurrent { get; init; }
public int? ResourceMax { get; init; }
public string? EffectAction { get; init; }
public string? EffectReason { get; init; }
public int? EffectStacks { get; init; }
public int? EffectDurationMs { get; init; }
public long? KillingBlowEventSeq { get; init; }
public MechanicData? Mechanic { get; init; }
```

- [ ] **Step 4: Add builder methods**

Add these methods to the generic `CombatEventBuilder<TCharacter>` first. The
production `ICombatEventBuilder` should expose the same operations with
`Character` parameters.

Generic builder shape:
```csharp
CombatEvent? CreateHealEvent(
  EventType eventType,
  TCharacter target,
  TCharacter? source,
  AbilityRef ability,
  int amount,
  int? rawAmount,
  int? overhealAmount,
);

CombatEvent? CreateResourceEvent(
  EventType eventType,
  TCharacter target,
  TCharacter? source,
  AbilityRef ability,
  string resourceType,
  int delta,
  int? current,
  int? max,
);

CombatEvent? CreateEffectEvent(
  EventType eventType,
  TCharacter target,
  TCharacter? source,
  AbilityRef ability,
  EffectRef effect,
  string action,
  string? reason
);

CombatEvent? CreateDeathEvent(
  TCharacter target,
  TCharacter? source,
  AbilityRef ability,
  long? killingBlowEventSeq
);

CombatEvent? CreateMechanicEvent(
  TCharacter? target,
  TCharacter? source,
  AbilityRef ability,
  MechanicData mechanic
);
```

Implement these in `CombatEventBuilder` and delegate them from
`CombatEventBuilderAdapter`. Generate IDs with the existing event ID pattern.
Use `DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()` only if the current builder
already uses wall-clock time. Otherwise reuse the existing time source pattern.

- [ ] **Step 5: Run mod tests**

```bash
cd mod && dotnet test tests/ErenshorLogs.Tests --filter "CombatEventBuilderTests|HealthEventBuilderTests|ResourceEventBuilderTests|StatusEventBuilderTests|MechanicEventBuilderTests"
```

Expected: pass.

- [ ] **Step 6: Commit internal event model**

```bash
git add mod/src/Events mod/src/Hooks mod/tests/ErenshorLogs.Tests/Hooks
git commit -m "feat(mod): model raid event families"
```

---

## Task 4: Mod protocol serialization for all event families

**Files:**

- Modify: `mod/src/Protocol/ProtocolSessionState.cs`
- Modify: `mod/src/Protocol/V2Messages.cs`
- Modify: `mod/tests/ErenshorLogs.Tests/Protocol/ProtocolSessionStateTests.cs`

- [ ] **Step 1: Write failing protocol serialization tests**

Add tests to `ProtocolSessionStateTests.cs`:

```csharp
[Fact]
public void Append_HealEvent_SerializesHealRecord()
{
  var session = new CombatSession("playtest-raid", "2026.5.17.1");
  var state = new ProtocolSessionState(session);
  var evt = CreateHealEvent(session.StartTime + 500);

  var record = state.Append(evt)!;

  Assert.Equal("heal", record.Value<string>("kind"));
  Assert.Equal("scripted", record.Value<string>("action"));
  Assert.Equal(200000, record["data"]!.Value<int>("amount"));
}

[Fact]
public void Append_ResourceDrain_SerializesResourceRecord()
{
  var session = new CombatSession("playtest-raid", "2026.5.17.1");
  var state = new ProtocolSessionState(session);
  var evt = CreateResourceDrainEvent(session.StartTime + 750);

  var record = state.Append(evt)!;

  Assert.Equal("resource", record.Value<string>("kind"));
  Assert.Equal("drain", record.Value<string>("action"));
  Assert.Equal(-300, record["data"]!.Value<int>("delta"));
}

[Fact]
public void Append_MechanicEvent_SerializesMechanicRecord()
{
  var session = new CombatSession("playtest-raid", "2026.5.17.1");
  var state = new ProtocolSessionState(session);
  var evt = CreateMechanicEvent(session.StartTime + 1500);

  var record = state.Append(evt)!;

  Assert.Equal("mechanic", record.Value<string>("kind"));
  Assert.Equal("invulnerability", record.Value<string>("action"));
  Assert.Equal("Sprinkles wards", record["data"]!.Value<string>("name"));
}
```

Add helper methods in the same file. Use the event builder methods from Task 3
or construct `CombatEvent` records directly if the tests remain simpler.

- [ ] **Step 2: Run tests and verify they fail**

```bash
cd mod && dotnet test tests/ErenshorLogs.Tests --filter ProtocolSessionStateTests
```

Expected: fail because `ProtocolSessionState.Append` ignores non-damage events.

- [ ] **Step 3: Implement serializer methods**

In `ProtocolSessionState.Append`, add cases:

```csharp
EventType.HealSpell or EventType.HealHot or EventType.HealLifesteal or EventType.HealRegen
  => CreateHealEvent(evt),
EventType.ManaUse or EventType.ManaRestore or EventType.ManaRegen
  => CreateResourceEvent(evt),
EventType.BuffApply or EventType.BuffRefresh or EventType.BuffFade
  or EventType.DebuffApply or EventType.DebuffRefresh or EventType.DebuffFade
  => CreateEffectEvent(evt),
EventType.Death => CreateDeathEvent(evt),
EventType.SpellInterrupt => CreateInterruptEvent(evt),
EventType.Mechanic => CreateMechanicEvent(evt),
```

If `EventType.Mechanic` does not exist yet, add it to `EventType.cs`.

Implement private methods:

```csharp
private JObject CreateHealEvent(CombatEvent evt) { ... }
private JObject CreateResourceEvent(CombatEvent evt) { ... }
private JObject CreateEffectEvent(CombatEvent evt) { ... }
private JObject CreateDeathEvent(CombatEvent evt) { ... }
private JObject CreateInterruptEvent(CombatEvent evt) { ... }
private JObject CreateMechanicEvent(CombatEvent evt) { ... }
```

Use the same common field logic as damage serialization. Extract a helper if the
method starts repeating `eventSeq`, `offsetMs`, actor, ability, effect, and debug
fields more than twice.

- [ ] **Step 4: Add protocol mapping tests for actions**

Add assertions for these mappings:

- `HealSpell` with `Mechanic.Action == "scripted"` maps to `heal/scripted`.
- `HealHot` maps to `heal/tick`.
- `HealLifesteal` maps to `heal/lifesteal`.
- `ManaUse` with a negative delta maps to `resource/drain` when ability kind is
  a mechanic drain.
- `ManaRestore` maps to `resource/restore`.
- `DebuffApply` maps to `effect/apply`.
- `Death` maps to `death/die`.

- [ ] **Step 5: Run mod protocol tests**

```bash
cd mod && dotnet test tests/ErenshorLogs.Tests --filter Protocol
```

Expected: pass.

- [ ] **Step 6: Commit protocol serialization**

```bash
git add mod/src/Protocol mod/src/Events mod/tests/ErenshorLogs.Tests/Protocol
git commit -m "feat(mod): serialize raid event families"
```

---

## Task 5: Web schema, import, export, and analyzer support

**Files:**

- Modify: `web/src/lib/types/schemas.ts`
- Modify: `web/src/lib/services/session-importer.ts`
- Modify: `web/src/lib/services/session-exporter.ts`
- Modify: `web/src/lib/state/sessions.svelte.ts`
- Modify: `web/src/lib/services/combat-analyzer.ts`
- Modify: `web/src/lib/services/combat-analyzer.test.ts`
- Modify: `web/src/lib/services/session-importer.test.ts`
- Modify: `web/src/lib/services/session-exporter.test.ts`
- Modify: `web/src/lib/services/protocol-fixtures.test.ts`

- [ ] **Step 1: Write failing analyzer tests**

Add to `combat-analyzer.test.ts`:

```ts
it("counts healing without counting resource or mechanics as damage", () => {
  const stats = analyzeCombat(createSessionWithEvents([
    createDamageEvent({ amount: 1000, sourceActorId: "player:0", targetActorId: "npc:1" }),
    createHealEvent({ amount: 250, sourceActorId: "sim:cleric", targetActorId: "player:0" }),
    createResourceEvent({ delta: -300, targetActorId: "player:0" }),
    createMechanicEvent({ action: "invulnerability", targetActorId: "npc:1" }),
  ]));

  expect(stats.totalDamage).toBe(1000);
  expect(stats.totalHealing).toBe(250);
  expect(stats.eventCounts.resource).toBe(1);
  expect(stats.eventCounts.mechanic).toBe(1);
});
```

Use existing test factories if present. If no factory exists, add typed factories
under `web/src/lib/testing/` rather than inline untyped objects.

- [ ] **Step 2: Run analyzer test and verify it fails**

```bash
cd web && pnpm test:run -- src/lib/services/combat-analyzer.test.ts
```

Expected: fail because expanded event families are not accepted or not counted.

- [ ] **Step 3: Update Zod schemas**

Mirror the shared schema changes in `web/src/lib/types/schemas.ts`:

- add `MechanicEventSchema`
- add `MechanicDataSchema`
- add `"mechanic"` to the combat event union
- add `"drain"` to resource actions
- add `"scripted"` to heal actions
- add optional `raidGroup` and `raidRole` to actor records

- [ ] **Step 4: Update analyzer behavior**

In `combat-analyzer.ts`:

- Keep damage totals based only on `kind === "damage"`.
- Add healing totals for `kind === "heal"`.
- Add event family counts for resource, effect, death, interrupt, and mechanic.
- Do not mutate imported event objects.
- Do not infer missing source actors.

Add a return shape like:

```ts
interface EventFamilyCounts {
  damage: number;
  heal: number;
  resource: number;
  effect: number;
  death: number;
  interrupt: number;
  mechanic: number;
}
```

If the existing return type already has a better place for this, use that place
and update tests accordingly.

- [ ] **Step 5: Update import and export tests**

Add importer and exporter tests that round-trip a session containing:

- `heal/scripted`
- `resource/drain`
- `effect/apply`
- `death/die`
- `mechanic/invulnerability`

The expected result is a deep-equal event array after import and export.

- [ ] **Step 6: Run web unit tests**

```bash
cd web && pnpm test:run -- src/lib/types src/lib/services
```

Expected: pass.

- [ ] **Step 7: Commit web protocol support**

```bash
git add web/src/lib/types web/src/lib/services web/src/lib/state web/src/lib/testing
git commit -m "feat(web): analyze raid event families"
```

---

## Task 6: Direct playtest raid relevance

**Files:**

- Modify: `mod/src/Hooks/CombatRelevanceCheckerAdapter.cs`
- Modify: `mod/src/Hooks/CombatRelevanceChecker.cs`
- Create: `mod/src/Hooks/RaidRelevanceInvalidationPatches.cs`
- Modify: `mod/src/Plugin.cs`
- Modify: `mod/tests/ErenshorLogs.Tests/Hooks/CombatRelevanceCheckerTests.cs`
- Create: `mod/tests/ErenshorLogs.Tests/Hooks/RaidRelevanceInvalidationTests.cs`

- [ ] **Step 1: Write failing relevance tests**

Extend the existing mock types in `CombatRelevanceCheckerTests.cs`:

```csharp
private sealed class MockNpc
{
  public bool SimPlayer { get; init; }
  public bool InGroup { get; init; }
  public object? MyRaidSlot { get; init; }
  public List<AggroSlot> AggroTable { get; } = [];
}
```

Add tests that require raid membership and raid targets through the generic
checker:

```csharp
[Fact]
public void IsRelevantCombat_WhenSimPlayerHasRaidSlot_ReturnsTrue()
{
  var sim = new MockCharacter
  {
    InstanceId = 1,
    Name = "Raid Cleric",
    Npc = new MockNpc { SimPlayer = true, MyRaidSlot = new object() },
  };
  var boss = new MockCharacter
  {
    InstanceId = 2,
    Name = "Raid Boss",
    Npc = new MockNpc(),
  };
  var checker = CreateChecker();

  Assert.True(checker.IsRelevantCombat(sim, boss));
}

[Fact]
public void IsRelevantCombat_WhenTargetIsRaidTarget_ReturnsTrue()
{
  var target = new MockCharacter
  {
    InstanceId = 3,
    Name = "Raid Target",
    Npc = new MockNpc(),
  };
  var other = new MockCharacter
  {
    InstanceId = 4,
    Name = "Other Actor",
    Npc = new MockNpc(),
  };
  var checker = CreateChecker(raidTargets: [target]);

  Assert.True(checker.IsRelevantCombat(target, other));
}
```

Extend the local `CreateChecker` helper with `raidTargets` and `looseAdds`
parameters, then pass them into new `CombatRelevanceChecker` delegates.

- [ ] **Step 2: Run relevance tests and verify they fail**

```bash
cd mod && dotnet test tests/ErenshorLogs.Tests --filter CombatRelevanceCheckerTests
```

Expected: fail because raid targets and loose adds are not relevance inputs and
reflection paths still exist.

- [ ] **Step 3: Remove reflection from adapter**

In `CombatRelevanceCheckerAdapter.cs`:

- Delete `FieldInfo MyRaidSlotField`.
- Replace reflected access with `npc.MyRaidSlot != null`.
- Add direct methods:

```csharp
public IEnumerable<Character> GetRaidTargets()
{
  var raid = GameData.RaidManager;
  if (raid == null)
    yield break;

  if (raid.Group1Target != null) yield return raid.Group1Target;
  if (raid.Group2Target != null) yield return raid.Group2Target;
  if (raid.Group3Target != null) yield return raid.Group3Target;
  if (raid.UrgentTarget != null) yield return raid.UrgentTarget;
}

public IEnumerable<Character> GetLooseAdds()
{
  var raid = GameData.RaidManager;
  if (raid?.LooseAdds == null)
    yield break;

  foreach (var add in raid.LooseAdds)
  {
    if (add != null)
      yield return add;
  }
}
```

- [ ] **Step 4: Extend checker logic**

In `CombatRelevanceChecker.cs`, include raid targets and loose adds as primary
relevance inputs before expensive aggro-table checks.

- [ ] **Step 5: Add invalidation patches**

Create `RaidRelevanceInvalidationPatches.cs` with patches for:

```csharp
[HarmonyPatch(typeof(RaidManager), "AddToRoster")]
[HarmonyPatch(typeof(RaidManager), "AssignToSpecificSlot")]
[HarmonyPatch(typeof(RaidManager), "DismissRaider")]
[HarmonyPatch(typeof(RaidManager), "DismissAllRaiders")]
[HarmonyPatch(typeof(RaidManager), "AssignTargetToGroup")]
[HarmonyPatch(typeof(RaidManager), "AssignUrgentTarget")]
[HarmonyPatch(typeof(RaidManager), "ClearBurnTarg")]
```

Each postfix calls:

```csharp
RaidRelevanceInvalidation.ClearCache?.Invoke();
```

Wire the action in `Plugin.ConfigureDamagePatches`:

```csharp
RaidRelevanceInvalidation.ClearCache = () => _relevanceChecker?.ClearCache();
```

- [ ] **Step 6: Run mod hook tests**

```bash
cd mod && dotnet test tests/ErenshorLogs.Tests --filter "CombatRelevanceCheckerTests|RaidRelevanceInvalidationTests"
```

Expected: pass.

- [ ] **Step 7: Commit raid relevance**

```bash
git add mod/src/Hooks mod/src/Plugin.cs mod/tests/ErenshorLogs.Tests/Hooks
git commit -m "feat(mod): track playtest raid relevance"
```

---

## Task 7: Area effect and lethal mechanic attribution

**Files:**

- Modify: `mod/src/Hooks/AEEventUpdatePatch.cs`
- Create: `mod/src/Hooks/AEEventTriggerPatch.cs`
- Create: `mod/src/Hooks/DeathTouchPatch.cs`
- Create: `mod/src/Hooks/MizukiEventPatch.cs`
- Modify: `mod/tests/ErenshorLogs.Tests/Hooks/ContextPatchBalanceTests.cs`
- Modify: `mod/tests/ErenshorLogs.Tests/Hooks/PatchCoverageTests.cs`

- [ ] **Step 1: Write failing patch coverage tests**

Add this helper to `PatchCoverageTests.cs` if the file does not already expose
one:

Add these usings if they are absent:

```csharp
using System;
using System.Linq;
using HarmonyLib;
```


```csharp
private static void AssertHarmonyPatchExists(
  Type targetType,
  string methodName,
  Type patchType
)
{
  var method = AccessTools.Method(targetType, methodName);
  Assert.NotNull(method);
  var patchInfo = Harmony.GetPatchInfo(method);
  Assert.NotNull(patchInfo);

  var patchTypeName = patchType.FullName;
  var owners = patchInfo!.Prefixes
    .Concat(patchInfo.Postfixes)
    .Concat(patchInfo.Finalizers)
    .Select(patch => patch.PatchMethod.DeclaringType?.FullName);
  Assert.Contains(patchTypeName, owners);
}
```

Then add:
```csharp
[Fact]
public void CoversPlaytestRaidMechanicContextMethods()
{
  AssertHarmonyPatchExists(typeof(AEEvent), "TriggerAE", typeof(AEEventTriggerPatch));
  AssertHarmonyPatchExists(typeof(DeathTouch), "Update", typeof(DeathTouchPatch));
  AssertHarmonyPatchExists(typeof(MizukiEvent), "SetNewAggro", typeof(MizukiEventPatch));
}
```

If `SetNewAggro` is compiler-generated because it returns `IEnumerator`, patch
the outer method and document that the context remains active for coroutine
creation only. If the damage occurs after `yield`, patch the generated iterator
`MoveNext` instead and test that method binding explicitly.

- [ ] **Step 2: Run coverage test and verify it fails**

```bash
cd mod && dotnet test tests/ErenshorLogs.Tests --filter PatchCoverageTests
```

Expected: fail because new patches do not exist.

- [ ] **Step 3: Implement `AEEventTriggerPatch`**

Create `mod/src/Hooks/AEEventTriggerPatch.cs`:

```csharp
using ErenshorLogs.Context;
using ErenshorLogs.Events;
using HarmonyLib;

namespace ErenshorLogs.Hooks;

[HarmonyPatch(typeof(AEEvent), "TriggerAE")]
public static class AEEventTriggerPatch
{
  [HarmonyPrefix]
  public static void Prefix(AEEvent __instance, out bool __state)
  {
    __state = false;
    if (__instance == null)
      return;

    CombatContext.PushAbility(new AbilityContext
    {
      Name = __instance.DamageReason ?? "Area Effect",
      Type = AbilityType.AreaEffect,
      StableKey = null,
    });
    __state = true;
  }

  [HarmonyFinalizer]
  public static void Finalizer(bool __state)
  {
    if (__state)
      CombatContext.PopAbility();
  }
}
```

Then remove context push from `AEEventUpdatePatch` or guard it so `Update()` does
not double-push when it calls `TriggerAE()`.

- [ ] **Step 4: Implement `DeathTouchPatch`**

Create `DeathTouchPatch.cs` with fixed context:

```csharp
[HarmonyPatch(typeof(DeathTouch), "Update")]
public static class DeathTouchPatch
{
  [HarmonyPrefix]
  public static void Prefix(out bool __state)
  {
    CombatContext.PushAbility(new AbilityContext
    {
      Name = "Death Touch",
      Type = AbilityType.AreaEffect,
      StableKey = "mechanic:death-touch",
    });
    __state = true;
  }

  [HarmonyFinalizer]
  public static void Finalizer(bool __state)
  {
    if (__state)
      CombatContext.PopAbility();
  }
}
```

- [ ] **Step 5: Implement `MizukiEventPatch`**

Patch the actual damage execution point. If Harmony can patch the generated
iterator, bind the `MoveNext` method for `SetNewAggro`. If tests show that is too
fragile, patch `Character.DamageMe` context by setting a short-lived context in
`MizukiEvent` before starting the coroutine and clear it after the target damage
is observed.

Preferred fixed context:

```csharp
new AbilityContext
{
  Name = "Mizuki Dagger",
  Type = AbilityType.AreaEffect,
  StableKey = "mechanic:mizuki-dagger",
}
```

- [ ] **Step 6: Add balance tests**

Add tests to `ContextPatchBalanceTests.cs` that call each prefix/finalizer pair
and assert stack depth returns to its original value.

- [ ] **Step 7: Run hook tests**

```bash
cd mod && dotnet test tests/ErenshorLogs.Tests --filter "ContextPatchBalanceTests|PatchCoverageTests"
```

Expected: pass.

- [ ] **Step 8: Commit attribution patches**

```bash
git add mod/src/Hooks mod/tests/ErenshorLogs.Tests/Hooks
git commit -m "fix(mod): attribute playtest raid mechanics"
```

---

## Task 8: Healing and scripted health capture

**Files:**

- Create: `mod/src/Hooks/HealMePatch.cs`
- Create: `mod/src/Hooks/ScriptedHealthPatch.cs`
- Modify: `mod/src/Plugin.cs`
- Create: `mod/tests/ErenshorLogs.Tests/Hooks/HealthEventBuilderTests.cs`
- Modify: `mod/tests/ErenshorLogs.Tests/Hooks/PatchCoverageTests.cs`

- [ ] **Step 1: Write failing patch coverage tests**

Add coverage assertions for:

```csharp
AssertHarmonyPatchExists(typeof(Stats), "HealMe", typeof(HealMePatch));
AssertHarmonyPatchExists(typeof(GraceEvent), "DoEventScript", typeof(GraceEventHealthPatch));
AssertHarmonyPatchExists(typeof(FernallaFightEvent), "PhaseHandler", typeof(FernallaPhaseHealthPatch));
AssertHarmonyPatchExists(typeof(LighthouseHealBox), "OnTriggerEnter", typeof(LighthouseHealPatch));
```

- [ ] **Step 2: Run tests and verify they fail**

```bash
cd mod && dotnet test tests/ErenshorLogs.Tests --filter PatchCoverageTests
```

Expected: fail because patches do not exist.

- [ ] **Step 3: Implement `HealMePatch`**

Patch `Stats.HealMe(int)` with a prefix that records the target HP before heal
and a postfix that emits the effective change.

Required behavior:

```csharp
var before = __instance.CurrentHP;
var max = __instance.CurrentMaxHP;
var raw = healAmount;
var after = __instance.CurrentHP;
var effective = Math.Max(0, after - before);
var overheal = Math.Max(0, raw - effective);
```

Emit `EventType.HealSpell`, `HealLifesteal`, or `HealRegen` based on current
`AbilityContext`. Use `HealSpell` when unknown.

- [ ] **Step 4: Implement scripted health patches**

For `GraceEvent.DoEventScript`, record Grace HP before prefix and after postfix.
When HP increases, emit `HealSpell` with mechanic ability:

```csharp
new AbilityRef
{
  Name = "Grace Echoes",
  Type = AbilityType.AreaEffect,
  StableKey = "mechanic:grace-echoes",
}
```

For `FernallaFightEvent.PhaseHandler`, emit one `heal/scripted` event for each
HP reset and resource restore events for mana refills in Task 9.

For `LighthouseHealBox.OnTriggerEnter`, emit one scripted heal for Kio when HP
increases.

- [ ] **Step 5: Run health tests**

```bash
cd mod && dotnet test tests/ErenshorLogs.Tests --filter "HealthEventBuilderTests|PatchCoverageTests"
```

Expected: pass.

- [ ] **Step 6: Commit healing capture**

```bash
git add mod/src/Hooks mod/src/Plugin.cs mod/tests/ErenshorLogs.Tests/Hooks
git commit -m "feat(mod): capture scripted healing"
```

---

## Task 9: Resource drain and restore capture

**Files:**

- Create: `mod/src/Hooks/ResourceChangePatch.cs`
- Modify: `mod/src/Plugin.cs`
- Create: `mod/tests/ErenshorLogs.Tests/Hooks/ResourceEventBuilderTests.cs`
- Modify: `mod/tests/ErenshorLogs.Tests/Hooks/PatchCoverageTests.cs`

- [ ] **Step 1: Write failing resource patch tests**

Add patch coverage for:

```csharp
AssertHarmonyPatchExists(typeof(AEManaDrainEvent), "Update", typeof(AEManaDrainEventPatch));
AssertHarmonyPatchExists(typeof(FernallaFightEvent), "PhaseHandler", typeof(FernallaManaRestorePatch));
```

Add builder assertions that the emitted event has:

```csharp
Assert.Equal("mana", evt.ResourceType);
Assert.Equal(-300, evt.ResourceDelta);
Assert.Equal(EventType.ManaUse, evt.EventType);
```

- [ ] **Step 2: Run tests and verify they fail**

```bash
cd mod && dotnet test tests/ErenshorLogs.Tests --filter ResourceEventBuilderTests
```

Expected: fail because resource capture is missing.

- [ ] **Step 3: Implement `AEManaDrainEventPatch`**

Patch `AEManaDrainEvent.Update` with prefix snapshots of each affected target's
mana and postfix comparison after the game method runs.

Emit one `ManaUse` event per target whose mana decreased. Use ability:

```csharp
new AbilityRef
{
  Name = __instance.DamageReason ?? "Mana Drain",
  Type = AbilityType.AreaEffect,
  StableKey = "mechanic:mana-drain",
}
```

- [ ] **Step 4: Implement Fernalla mana restore capture**

In the Fernalla phase patch from Task 8, capture player and group member mana
before and after `PhaseHandler`. Emit `ManaRestore` for each actor whose mana
increased.

- [ ] **Step 5: Run resource tests**

```bash
cd mod && dotnet test tests/ErenshorLogs.Tests --filter "ResourceEventBuilderTests|PatchCoverageTests"
```

Expected: pass.

- [ ] **Step 6: Commit resource capture**

```bash
git add mod/src/Hooks mod/src/Plugin.cs mod/tests/ErenshorLogs.Tests/Hooks
git commit -m "feat(mod): capture raid resource events"
```

---

## Task 10: Status lifecycle and raid aura ownership

**Files:**

- Modify: `mod/src/Hooks/AddStatusEffectPatch.cs`
- Modify: `mod/src/Hooks/RemoveStatusEffectPatch.cs`
- Create: `mod/src/Hooks/RaidAuraPatch.cs`
- Modify: `mod/src/Context/EffectTracker.cs`
- Modify: `mod/src/Plugin.cs`
- Create: `mod/tests/ErenshorLogs.Tests/Hooks/StatusEventBuilderTests.cs`

- [ ] **Step 1: Write failing status tests**

Add tests that require:

- `AddStatusEffect` emits `DebuffApply`.
- `RemoveStatusEffect` emits `DebuffFade` or `BuffFade`.
- Raid aura application records source actor credit.

Example assertion:

```csharp
Assert.Equal(EventType.DebuffApply, evt.EventType);
Assert.Equal("BleedRef", evt.Effect!.Name);
Assert.Equal("npc:mizuki", evt.Source!.Id);
Assert.Equal("player:0", evt.Target!.Id);
```

- [ ] **Step 2: Run tests and verify they fail**

```bash
cd mod && dotnet test tests/ErenshorLogs.Tests --filter StatusEventBuilderTests
```

Expected: fail because status hooks only register effect tracker state.

- [ ] **Step 3: Extend `EffectTracker` ownership**

Store source actor and credit actor at application time:

```csharp
public sealed record TrackedEffect(
  Character Target,
  int Slot,
  Spell Spell,
  Character? Source,
  Character? Credit
);
```

Update registrations from `AddStatusEffectPatch` to pass caster when the overload
contains `Character`.

- [ ] **Step 4: Emit apply and fade events**

In `AddStatusEffectPatch` postfix, emit `BuffApply` or `DebuffApply` based on
spell polarity when available. If polarity is not exposed, use `DebuffApply` for
hostile `_fromPlayer: false` target effects and `BuffApply` for friendly aura
applications.

In `RemoveStatusEffectPatch`, look up the tracked effect slot before removal and
emit fade after the game removes it.

- [ ] **Step 5: Capture raid aura owner**

Patch `RaidManager.UpdateGroupAuras` and wrap each application block with an aura
owner context. If direct per-application patching is too invasive, maintain a
short-lived map from `Spell` to current raid slot source while the method runs.

The emitted event must use:

- `sourceActorId`: aura owner
- `creditActorId`: aura owner
- `targetActorId`: actor receiving aura
- `effectId`: applied aura spell

- [ ] **Step 6: Run status tests**

```bash
cd mod && dotnet test tests/ErenshorLogs.Tests --filter StatusEventBuilderTests
```

Expected: pass.

- [ ] **Step 7: Commit status lifecycle capture**

```bash
git add mod/src/Hooks mod/src/Context mod/src/Plugin.cs mod/tests/ErenshorLogs.Tests/Hooks
git commit -m "feat(mod): emit status lifecycle events"
```

---

## Task 11: Death events and killing blow links

**Files:**

- Create: `mod/src/Hooks/DeathEventPatch.cs`
- Modify: `mod/src/Hooks/DeathTouchPatch.cs`
- Modify: `mod/src/Events/CombatEventDispatcher.cs`
- Create: `mod/tests/ErenshorLogs.Tests/Hooks/DeathEventBuilderTests.cs`

- [ ] **Step 1: Write failing death tests**

Create tests requiring:

- a death event is emitted when a character transitions from alive to dead
- `DeathTouch` emits or causes a death event
- death event includes killing blow sequence when known

```csharp
Assert.Equal(EventType.Death, evt.EventType);
Assert.Equal("sim:cleric", evt.Target!.Id);
Assert.Equal("mechanic:death-touch", evt.Ability.StableKey);
```

- [ ] **Step 2: Run tests and verify they fail**

```bash
cd mod && dotnet test tests/ErenshorLogs.Tests --filter DeathEventBuilderTests
```

Expected: fail because death events are not emitted.

- [ ] **Step 3: Identify and patch the stable death method**

Search playtest source for the method that finalizes death. Prefer a method on
`Character` or `Stats` over script-specific state checks. Patch it with direct
playtest references.

If no stable method exists, add targeted death emission in:

- `DeathTouch.Update`
- `MizukiEvent.CheckWinCond`
- `SprinklesEvent.CleanList`

- [ ] **Step 4: Link killing blows where possible**

Record the latest damage event for each target actor in session state or a small
`KillingBlowTracker`. When a death event fires, set `KillingBlowEventSeq` if the
latest target damage reduced HP to zero or below.

- [ ] **Step 5: Run death tests**

```bash
cd mod && dotnet test tests/ErenshorLogs.Tests --filter DeathEventBuilderTests
```

Expected: pass.

- [ ] **Step 6: Commit death events**

```bash
git add mod/src/Hooks mod/src/Events mod/tests/ErenshorLogs.Tests/Hooks
git commit -m "feat(mod): emit death events"
```

---

## Task 12: Health-affecting encounter mechanic events

**Files:**

- Create: `mod/src/Hooks/EncounterMechanicPatch.cs`
- Modify: `mod/src/Plugin.cs`
- Create: `mod/tests/ErenshorLogs.Tests/Hooks/MechanicEventBuilderTests.cs`
- Modify: `mod/tests/ErenshorLogs.Tests/Hooks/PatchCoverageTests.cs`

- [ ] **Step 1: Write failing mechanic patch tests**

Add coverage for:

```csharp
AssertHarmonyPatchExists(typeof(SprinklesEvent), "CleanList", typeof(SprinklesMechanicPatch));
AssertHarmonyPatchExists(typeof(SprinklesEvent), "spawnWards", typeof(SprinklesWardSpawnPatch));
AssertHarmonyPatchExists(typeof(DPSCheckAEEvent), "Update", typeof(DpsCheckAeMechanicPatch));
AssertHarmonyPatchExists(typeof(FaithEvent), "DoEventScript", typeof(FaithEventMechanicPatch));
AssertHarmonyPatchExists(typeof(MizukiEvent), "DoFinal", typeof(MizukiFinalPhasePatch));
```

- [ ] **Step 2: Run tests and verify they fail**

```bash
cd mod && dotnet test tests/ErenshorLogs.Tests --filter "MechanicEventBuilderTests|PatchCoverageTests"
```

Expected: fail because mechanic patches do not exist.

- [ ] **Step 3: Implement Sprinkles mechanics**

Emit:

- `mechanic/invulnerability` when `Sprinkles.Invulnerable` changes.
- `mechanic/despawn` or `death/die` when wards are forced dead.
- `mechanic/statChange` when AE `tickDmg` or `ResistMod` changes.
- `mechanic/spawn` for each ward added to `LooseAdds`.

- [ ] **Step 4: Implement DPS check and Mizuki mechanics**

Emit `mechanic/statChange` when `DPSCheckAEEvent` increases AE damage or resist.
Emit `mechanic/phase` and `mechanic/statChange` from `MizukiEvent.DoFinal`.

- [ ] **Step 5: Implement Faith spawn mechanics**

Emit `mechanic/spawn` when `FaithEvent.DoEventScript` registers the heal object
as a loose add.

- [ ] **Step 6: Run mechanic tests**

```bash
cd mod && dotnet test tests/ErenshorLogs.Tests --filter "MechanicEventBuilderTests|PatchCoverageTests"
```

Expected: pass.

- [ ] **Step 7: Commit mechanic events**

```bash
git add mod/src/Hooks mod/src/Plugin.cs mod/tests/ErenshorLogs.Tests/Hooks
git commit -m "feat(mod): emit raid mechanic events"
```

---

## Task 13: Demo data, docs, and final protocol consistency

**Files:**

- Modify: `web/static/demo/sessions.json`
- Modify: `docs/LOG_FORMAT.md`
- Modify: `docs/COMBAT_EVENTS.md`
- Modify: `docs/ARCHITECTURE.md`
- Modify: `web/src/lib/services/protocol-fixtures.test.ts`

- [ ] **Step 1: Update docs from implemented schema**

Update `docs/LOG_FORMAT.md` to include:

- `mechanic` event family
- `resource/drain`
- `heal/scripted`
- `ActorRecord.raidGroup`
- `ActorRecord.raidRole`

Update `docs/COMBAT_EVENTS.md` with hook sources:

- `AEEvent.TriggerAE`
- `DeathTouch.Update`
- `MizukiEvent.SetNewAggro`
- `Stats.HealMe`
- `AEManaDrainEvent.Update`
- `RaidManager.UpdateGroupAuras`
- scripted health patches
- mechanic patches

- [ ] **Step 2: Regenerate demo data if needed**

If demo data is used for visible analyzer examples, update
`web/static/demo/sessions.json` with one representative session that includes:

- raid member actor metadata
- damage event
- scripted heal event
- resource drain event
- effect apply event
- death event
- mechanic event

Keep existing attribution debug examples if they are still useful. Do not keep a
legacy v1 demo file.

- [ ] **Step 3: Run docs and fixture tests**

```bash
cd web && pnpm test:run -- src/lib/services/protocol-fixtures.test.ts
```

Expected: pass.

- [ ] **Step 4: Commit docs and demo**

```bash
git add docs web/static/demo/sessions.json web/src/lib/services/protocol-fixtures.test.ts
git commit -m "docs: document raid event tracking"
```

---

## Task 14: Full verification and deployment readiness

**Files:**

- No source files unless verification exposes a bug.

- [ ] **Step 1: Run mod tests**

```bash
cd mod && dotnet test tests/ErenshorLogs.Tests
```

Expected: all tests pass.

- [ ] **Step 2: Run web tests and static checks**

```bash
cd web && pnpm test:run
cd web && pnpm check
cd web && pnpm lint
```

Expected: all pass with no Svelte warnings.

- [ ] **Step 3: Run CLI tests and static checks**

```bash
cd cli && uv run python -m unittest discover tests
cd cli && uv run ruff check src tests
cd cli && uv run mypy src
```

Expected: all pass.

- [ ] **Step 4: Build and deploy locally to CrossOver playtest**

```bash
cd cli && uv run erenshor setup --variant playtest
cd cli && uv run erenshor deploy
```

Expected: Release build succeeds and copies `ErenshorLogs.dll` to the playtest
BepInEx plugins directory.

- [ ] **Step 5: Runtime validation checklist**

Validate in playtest and capture notes in the final status:

- raid member damage appears with correct actor names
- raid target damage appears before and after target assignment changes
- `AEEvent.TriggerAE` damage has an area-effect ability
- `DeathTouch` damage shows `Death Touch` and emits a death event
- `AEManaDrainEvent` emits `resource/drain`
- scripted boss heal emits `heal/scripted`
- status application and fade emit effect events
- mechanic events do not change DPS or HPS totals

- [ ] **Step 6: Create preview build**

```bash
cd cli && uv run erenshor cf-deploy --preview
```

Expected: preview URL loads and `/mods/ErenshorLogs.dll` serves the rebuilt DLL.

- [ ] **Step 7: Commit final fixes if verification found any**

Only commit if verification required code changes:

```bash
git add <changed-files>
git commit -m "fix: address raid tracking verification"
```

- [ ] **Step 8: Final status**

Report:

- commit list
- exact verification commands and observed results
- preview URL if deployed
- runtime validation notes
- any known limitations that remain
