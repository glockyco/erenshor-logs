# Raid Combat Tracking Update Design

## Goal

Make Erenshor Logs target the current playtest combat model and capture the
raid and health-affecting combat facts needed for accurate live analysis and
file exports.

The implementation is a clean cutover. The mod no longer needs to support the
main game build. Use compiled playtest references for playtest-only types and
members. Do not keep reflection shims for main compatibility.

## User requirements

- Plan all necessary implementation steps for a full raid-update and combat
  event tracking update.
- Prioritize damage and health-affecting events.
- Do not support the main game version.
- Avoid reflection where compiled playtest references can provide direct types.
- Preserve protocol v2 clean cutover principles.
- Keep Newtonsoft.Json in the mod.
- Keep the web app static and browser-only.

## Sources

This design is based on:

- `docs/PLAYTEST_AUDIT_2026-05-17.md`
- `docs/superpowers/specs/2026-05-17-protocol-redesign.md`
- `docs/LOG_FORMAT.md`
- `shared/protocol/schemas/erenshor-log-v2.schema.json`
- Current mod hook and protocol code under `mod/src`
- Current web parser, importer, exporter, and analyzer code under `web/src`

## Approach considered

### Option A: patch only attribution misses

Patch `AEEvent.TriggerAE`, `DeathTouch.Update`, and `MizukiEvent.SetNewAggro`,
then leave protocol and web analysis mostly unchanged.

Pros:

- Smallest implementation.
- Improves visible damage attribution quickly.

Cons:

- Leaves raid target state, resource drains, death events, healing, and scripted
  health changes invisible.
- Does not satisfy the full combat event tracking update.
- Continues to put pressure on browser-side guesses.

Verdict: reject as insufficient.

### Option B: extend protocol v2 and capture health-affecting families first

Cut the mod over to playtest compiled references. Add missing damage context
patches, raid relevance, healing, death, resource, status, and health-affecting
mechanic events. Update protocol fixtures, mod serialization, web parsing,
import/export, and analyzer handling in one coherent sequence.

Pros:

- Matches the current playtest game model.
- Captures authoritative facts in the mod instead of inferring in the browser.
- Keeps protocol v2 as the single live and file model.
- Produces useful user-facing improvements before lower-value timeline work.

Cons:

- Larger change set.
- Requires schema and fixture updates before some mod and web work can land.

Verdict: recommended.

### Option C: build an encounter scripting framework first

Create a general encounter subsystem with named phases, scripted mechanics,
spawn tracking, raid commands, and timeline rendering before adding specific
hooks.

Pros:

- Elegant long-term model for raid encounters.
- Could support advanced replay and timeline UI later.

Cons:

- Delays core damage and health correctness.
- Adds abstractions before enough real event data exists.
- Risks scope growth beyond the current reliability work.

Verdict: defer. Add narrowly typed mechanic events now and generalize only after
several encounters are captured.

## Recommended architecture

### Playtest-only mod target

The mod should compile against playtest assemblies copied into `mod/lib`. The
CLI setup flow should make this explicit so future builds do not accidentally
compile against main DLLs.

Required changes:

- Add a CLI setup mode or configuration that selects the playtest install as the
  authoritative source for `mod/lib/Assembly-CSharp.dll`.
- Update docs and command help to say the mod targets current playtest.
- Remove main compatibility reflection from mod code touched by this work.
- Use direct references for `NPC.MyRaidSlot`, `RaidManager`, `RaidMemberSlot`,
  raid target fields, and `LooseAdds`.

Do not distribute game DLLs. `mod/lib` remains local developer state.

### Capture pipeline

Keep the current high-level pipeline:

1. Harmony hooks observe game methods.
2. Hook adapters build internal `CombatEvent` records.
3. Session state assigns protocol ordering and registers referenced records.
4. Broadcaster and exporter publish protocol v2 JSON.
5. Web live and file paths normalize into the same session state.
6. Analyzer derives summaries from events and registries.

Extend this pipeline instead of adding side channels. Every captured fact that
matters to logs must become a protocol event or registry update.

### Internal event model

`CombatEvent` is currently damage-oriented. Extend it enough to express all
protocol v2 event families without turning it into an unbounded sparse bag.

Add fields only where they map to protocol event families:

- `ResourceType`, `ResourceDelta`, `ResourceCurrent`, `ResourceMax`
- `EffectAction`, `EffectReason`, `EffectStacks`, `EffectDurationMs`
- `KillingBlowEventId` or `KillingBlowEventSeq` link support
- `HealRawAmount`, `OverhealAmount`, and heal critical flag
- `MechanicKind` and small structured mechanic data for health-affecting state

If this becomes too sparse during implementation, split the internal model into
small records by family while keeping the public emitter contract simple.

### Protocol shape

Keep protocol major version `2`. Add compatible v2 event-family extensions:

- Add resource action `drain`.
- Add heal action `scripted` for direct health assignments that restore health.
- Add effect action `remove` only if `fade` cannot accurately represent direct
  scripted removal.
- Add a new `mechanic` event family for health-affecting encounter state.

`mechanic` events are for facts that affect interpretation but are not damage,
healing, resource, effect, death, or interrupt facts.

Recommended mechanic actions:

- `phase`
- `invulnerability`
- `spawn`
- `despawn`
- `statChange`
- `targetAssignment`

Recommended mechanic payload:

```ts
interface MechanicData {
  name: string;
  value?: string | number | boolean;
  previousValue?: string | number | boolean;
  affectedStat?: "hp" | "mana" | "damage" | "resist" | "armorPen";
  amount?: number;
}
```

Mechanic events should not be used for damage amounts or healing amounts. If an
amount changes HP or mana directly, emit a `heal` or `resource` event and add a
mechanic event only when phase context is also needed.

### Registry model

Extend records only where current analysis needs stable metadata:

Actor records:

- Add optional `raidGroup`.
- Add optional `raidRole`, using values from playtest raid slots where stable.
- Keep `ownerActorId` for pets and charmed actors.
- Preserve `faction` and `isPlayerControlled`.

Ability records:

- Keep `areaEffect` for encounter AE scripts.
- Add stable keys for fixed mechanics such as `mechanic:death-touch` and
  `mechanic:mizuki-dagger`.
- Use `kind: "areaEffect"` for AE damage and `kind: "unknown"` only as a true
  last resort.

Effect records:

- Store effect owner at application time when the game exposes it through raid
  aura context or caster arguments.
- Preserve `sourceAbilityId` when the effect comes from a skill, spell, proc, or
  encounter mechanic.

### Raid relevance

Use direct playtest members instead of reflection. The relevance checker should
consider these inputs:

- player
- current group members
- raid members with `NPC.MyRaidSlot != null`
- raid target state from `RaidManager.Group1Target`, `Group2Target`,
  `Group3Target`, and `UrgentTarget`
- targets whose current target is the player or a raid member
- `RaidManager.LooseAdds`
- aggro tables involving player, group members, raid members, pets, or loose adds

Clear cached relevance on raid roster and target mutations. At minimum patch:

- `RaidManager.AddToRoster`
- `RaidManager.AssignToSpecificSlot`
- `RaidManager.DismissRaider`
- `RaidManager.DismissAllRaiders`
- `RaidManager.AssignTargetToGroup`
- `RaidManager.AssignUrgentTarget`
- `RaidManager.ClearBurnTarg`
- `RaidMemberSlot.Update` when it restores `MyRaidSlot`

### Damage and attribution priority

P0 damage and health-affecting attribution work:

- Patch `AEEvent.TriggerAE` and avoid double context when `Update` calls it.
- Patch `DeathTouch.Update` for fixed ability context.
- Patch `MizukiEvent.SetNewAggro` for fixed ability context.
- Keep existing damage method hooks as the authoritative amount capture.
- Keep `DoSkillNoChecks` and `ResolveSpell` context coverage.

### Healing and direct health changes

Capture method-based healing:

- `Stats.HealMe(int)` for boss regen, lifetap, and direct heals that call it.
- Any overloaded `HealMe` signatures present in playtest.

Capture scripted direct health writes where they matter:

- `GraceEvent.DoEventScript` full heal.
- `FernallaFightEvent.PhaseHandler` HP resets.
- `LighthouseHealBox.OnTriggerEnter` direct HP restore.
- `NPCFightEvent.FixedUpdate` reset-to-full disengage case if relevant.

For direct health writes:

- Emit `heal/scripted` when HP increases.
- Emit `death/die` or `mechanic/despawn` when a script kills or removes an actor.
- Use the encounter script actor as source when available.

### Death events

Add death emission for real character death and scripted death.

Preferred source:

- Patch the lowest common death method if playtest exposes one stable method.
- If the only reliable path is state mutation in an encounter script, patch the
  script and emit a death event there.

The death event should include:

- target actor
- source actor if known
- ability or mechanic if known
- killing blow link when the killing damage event was captured first

### Resource events

Capture mana events before other resources because current protocol supports
mana only.

Sources:

- `AEManaDrainEvent.Update` as `resource/drain`.
- `FernallaFightEvent.PhaseHandler` mana restore.
- Ability or spell mana spend only after identifying the authoritative playtest
  method that subtracts cost.

Do not classify mana as healing.

### Status lifecycle and ownership

Current status hooks register effects for attribution but do not emit protocol
status events. Extend them to emit:

- `effect/apply`
- `effect/refresh`
- `effect/fade`

Also preserve owner and credit actor:

- Use caster arguments in `Stats.AddStatusEffect` overloads when present.
- Capture raid aura ownership in `RaidManager.UpdateGroupAuras`.
- Keep the effect tracker for later DoT and HoT tick attribution.

Include playtest shapeshift status metadata as effect or mechanic data if the
status changes form.

### Encounter mechanics

Capture only mechanics that affect health, damage, resource, relevance, or
analysis correctness.

Initial mechanic hooks:

- `SprinklesEvent.CleanList` for invulnerability, ward death, and AE tuning.
- `SprinklesEvent.spawnWards` for ward spawn and loose-add registration.
- `MizukiEvent.DoFinal` for final phase AE tuning.
- `MizukiEvent.CheckWinCond` for scripted shadow cleanup.
- `DPSCheckAEEvent.Update` for AE damage and resist ramp.
- `DeathTouch.Update` for post-touch enrage stat changes.
- `FaithEvent.DoEventScript` for heal-object spawn and loose-add registration.

Mechanic events can initially be visible in the event stream and debug views
without being included in DPS/HPS totals.

### Web analyzer

Update the web analyzer in this order:

1. Parse and store new protocol events.
2. Keep damage totals unchanged for existing damage events.
3. Add healing totals for `heal` events.
4. Add death counts and death rows in the event model.
5. Add resource deltas as non-DPS context.
6. Add status and mechanic events to detail or debug views without polluting
   damage totals.

Do not infer missing mechanics in the browser. If a fact is not in the protocol,
the web should show it as absent rather than guess.

### Testing strategy

Use TDD for implementation.

Required test layers:

- Shared protocol JSON Schema and fixture validation.
- Web parser/import/export tests for every new event family.
- Web analyzer tests proving totals and non-total events behave correctly.
- Mod protocol serialization tests for every internal event family.
- Mod hook balance tests for every new context patch.
- Mod relevance tests for raid members, raid targets, and loose adds.
- Mod event-builder tests for healing, resource, effect, death, and mechanic
  events.

Runtime validation remains separate from unit tests and should cover:

- raid member damage attribution
- raid target relevance
- `AEEvent.TriggerAE` damage attribution
- `DeathTouch` lethal hit and death event
- `AEManaDrainEvent` resource drain
- scripted boss heal or HP reset
- status apply and fade event

## Non-goals

- No main game compatibility.
- No protocol v1 support.
- No browser-side translation of missing mod facts.
- No binary protocol.
- No full encounter timeline UI in this update.
- No long-term generic encounter framework before specific captured facts exist.

## Acceptance criteria

- Mod builds against current playtest compiled references with direct playtest
  types and members.
- No reflection remains for raid members, raid targets, or loose adds in the
  touched combat relevance path.
- Protocol schema and fixtures include damage, heal, resource, effect, death,
  interrupt, and mechanic events.
- Mod emits protocol v2 JSON for every implemented event family.
- Web live path, import path, export path, and analyzer handle every implemented
  event family.
- Damage and healing totals remain correct and do not count resource, effect,
  death, or mechanic events as damage.
- Unit tests cover each new hook, event builder path, protocol serializer path,
  and web analyzer branch.
- Runtime validation checklist covers raid, area effect, death touch, mana drain,
  scripted heal, and status lifecycle scenarios.
