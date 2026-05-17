# Main vs Playtest Combat Logging Audit

Date: 2026-05-17

## Scope

This audit compares the current decompiled main and playtest sources under:

- `reference/game-source/main`
- `reference/game-source/playtest`

The audit focuses on behavior that can affect Erenshor Logs mod capture,
protocol v2 output, and web analyzer correctness. Cosmetic diffs, default field
initializer noise, UI-only changes, and unrelated systems are out of scope.

## Method

The source trees contain 431 main C# files and 459 playtest C# files. Playtest
adds 28 C# files and changes 228 existing C# files. Relevant changed or added
files were reviewed around these concerns:

- combat damage and ability attribution
- area effects and scripted encounter mechanics
- raid, group, SimPlayer, pet, and actor relevance
- status lifecycle, healing, resources, deaths, and direct stat writes
- protocol v2 event coverage and web analyzer implications

The most relevant files inspected were:

- `AEEvent.cs`, `AEEvent2.cs`, `AEManaDrainEvent.cs`, `DeathTouch.cs`
- `DPSCheckAEEvent.cs`, `FaithEvent.cs`, `GraceEvent.cs`, `MizukiEvent.cs`
- `SprinklesEvent.cs`, `NPCFightEvent.cs`, `FernallaFightEvent.cs`
- `SiraetheEvent.cs`, `LighthouseHealBox.cs`, `DmgEventArea.cs`
- `UseSkill.cs`, `CastSpell.cs`, `SpellVessel.cs`, `PlayerCombat.cs`
- `Stats.cs`, `NPC.cs`, `SimPlayer.cs`, `SimPlayerMngr.cs`
- `SimPlayerTracking.cs`, `RaidManager.cs`, `RaidMemberSlot.cs`
- `PlayerRaidCard.cs`, `GroupTasks.cs`, `SimPlayerGrouping.cs`
- `CharmedNPC.cs`, `NPCAggroArea.cs`, `ClearAggroOnDistance.cs`

## Executive summary

Playtest combat changes are real and materially affect the logs mod. The
current mod is in a much better place after the recent fixes, especially for
steady-state raid members and SimPlayer skill attribution, but the playtest
source adds several mechanics that do not fit the current damage-first capture
model.

Highest-risk gaps:

1. `AEEvent.TriggerAE()` can now be called outside `AEEvent.Update()`. The
   current mod patches `AEEvent.Update()`, so direct `TriggerAE()` callers can
   lose area-effect attribution.
2. Playtest adds resource and death mechanics that bypass current event
   families, especially `AEManaDrainEvent` and `DeathTouch`.
3. The raid system is now live combat state, not just UI. Current `MyRaidSlot`
   reflection covers steady-state relevance, but cache invalidation and raid
   target relevance are incomplete.
4. Several encounter scripts directly assign HP, mana, actor alive state,
   invulnerability, and AE tuning fields. These bypass damage, heal, and status
   hooks.
5. Protocol v2 currently serializes damage events only in the mod path. The web
   schema contains broader event shapes, but the analyzer is still mostly
   damage and healing oriented.

Good news:

- Existing `UseSkill.DoSkill` and `UseSkill.DoSkillNoChecks` coverage remains
  aligned with main and playtest skill paths.
- `SpellVessel.ResolveSpell`, `CastSpell.StartSpellFromProc`, damage method
  hooks, and status effect overload hooks remain signature-compatible.
- Current `NPC.MyRaidSlot` reflection is the correct compatibility strategy for
  one binary across main and playtest.
- Pet, charm, and local aggro helper changes were not material in the audited
  files.

## Findings

### 1. `AEEvent.TriggerAE()` can bypass area-effect attribution

Severity: high

Main keeps `AEEvent` damage inside `AEEvent.Update()`. Playtest splits the work
into `AEEvent.TriggerAE()` and adds `TriggerOnly`.

Evidence:

- `reference/game-source/playtest/AEEvent.cs:35` adds `TriggerOnly`.
- `reference/game-source/playtest/AEEvent.cs:63-66` calls `TriggerAE()` from
  `Update()` only when `TriggerOnly` is false.
- `reference/game-source/playtest/AEEvent.cs:71-113` performs physical damage,
  magic damage, lifetap, and proc effect application inside `TriggerAE()`.
- `reference/game-source/playtest/MizukiEvent.cs:151-155` calls
  `MizAE.TriggerAE()` directly.
- `mod/src/Hooks/AEEventUpdatePatch.cs:12` patches only `AEEvent.Update()`.

Impact:

Damage still reaches `DamageMe()` or `MagicDamageMe()`, so raw damage capture is
mostly preserved. Attribution is the problem. Direct `TriggerAE()` calls outside
`Update()` have no area-effect `AbilityContext`, which can produce unknown or
generic ability records in protocol v2 and the web UI.

Recommendation:

Patch `AEEvent.TriggerAE()` directly. Keep or remove the `Update()` patch based
on the final balance check, but ensure exactly one context is active for both
main-style timed ticks and playtest direct triggers. Add tests for:

- playtest `TriggerAE()` physical damage
- playtest `TriggerAE()` magic damage
- direct caller coverage from `MizukiEvent`
- no double-push when `Update()` invokes `TriggerAE()`

### 2. `AEManaDrainEvent` is an uncaptured resource mechanic

Severity: high

Playtest adds `AEManaDrainEvent`. Main has no equivalent file.

Evidence:

- `reference/game-source/playtest/AEManaDrainEvent.cs:63-74` iterates the NPC
  aggro table and directly subtracts `tickDmg` from each target's
  `CurrentMana`, then clamps to zero.
- No mod hook currently emits resource events for direct mana changes.
- `mod/src/Protocol/ProtocolSessionState.cs:38-50` serializes only damage
  event types.

Impact:

This mechanic is invisible to live streaming and JSON exports. The web analyzer
cannot explain failed casts or resource collapse during the encounter.

Recommendation:

Add resource event capture. Prefer a generic direct resource-change model if a
safe low-level hook exists. If not, patch `AEManaDrainEvent.Update()` as a
playtest encounter-specific first step. Protocol output should include:

- target actor
- source actor or encounter source
- resource type, starting with mana
- delta amount
- action, such as `drain`
- offset and event sequence

### 3. `DeathTouch` needs mechanic attribution and death events

Severity: critical

Playtest adds `DeathTouch`. Main has no equivalent file.

Evidence:

- `reference/game-source/playtest/DeathTouch.cs:24-47` picks a non-current
  aggro-table target and calls `DamageMe(999999, ...)` with void damage.
- `reference/game-source/playtest/DeathTouch.cs:49-53` mutates boss damage,
  armor penetration, and damage range after the hit.
- `mod/src/Hooks/DamageMePatch.cs` will capture only the underlying damage.
- `mod/src/Protocol/ProtocolSessionState.cs:38-50` serializes only damage.

Impact:

The lethal hit is probably present as damage, but the explicit mechanic is not.
The victim death is also not represented as a death event. The web analyzer can
show a large hit without explaining the wipe cause or the encounter's follow-up
enrage state.

Recommendation:

Add a `DeathTouch.Update` context patch with a fixed ability kind/name, then add
generic death event emission. If encounter-state reconstruction matters, also
emit a mechanic event when the script mutates the boss damage and armor values.

### 4. Raid membership is live combat state in playtest

Severity: high

Main raid structures are mostly UI and roster level. Playtest turns raid slots
into combat state through `NPC.MyRaidSlot`, `NPC.indexInRaid`, `SimPlayer.InRaid`,
and persisted `SimPlayerTracking.ATeam`.

Evidence:

- `reference/game-source/playtest/RaidManager.cs:1061-1091` clears `InRaid`,
  `indexInRaid`, and `MyRaidSlot` when dismissing raiders.
- `reference/game-source/playtest/RaidManager.cs:1219-1225` assigns
  `MyRaidSlot` to a specific slot.
- `reference/game-source/playtest/RaidManager.cs:1242-1266` adds a SimPlayer to
  the roster, removes it from the normal group, sets `InRaid`, and assigns
  `MyRaidSlot`.
- Current mod relevance includes `MyRaidSlot` via reflection in
  `CombatRelevanceCheckerAdapter`, but positive relevance cache is cleared only
  at session end.

Impact:

Steady-state raid members are mostly covered by the current reflection-based
adapter. The remaining risk is lifecycle. Live roster changes, dismisses,
swaps, and startup relinks can leave stale relevance decisions or miss a small
window before slot linkage is rebuilt.

Recommendation:

Treat raid roster mutation as a relevance invalidation boundary. Clear or
recompute relevance on these playtest paths:

- `RaidManager.AddToRoster`
- `RaidManager.AssignToSpecificSlot`
- `RaidManager.DismissRaider`
- `RaidManager.DismissAllRaiders`
- `RaidMemberSlot.Update` relink when it restores `MyRaidSlot`

Keep reflection for playtest-only members. Do not introduce compile-time
references to `RaidMemberSlot` or `NPC.MyRaidSlot`.

### 5. Raid target state is not represented in relevance inputs

Severity: high

Playtest adds raid target state outside `SimPlayerGrouping.GroupTargets`.

Evidence:

- `reference/game-source/playtest/RaidManager.cs:1514-1565` checks whether a
  raid group is fully aggroed on a target.
- `reference/game-source/playtest/RaidManager.cs:1567-1585` checks whether a
  target is targeting the raid.
- `reference/game-source/playtest/RaidManager.cs:1587-1595` exposes per-group
  raid pull targets.
- Current mod relevance reads `GroupTargets` but not `RaidManager.Group1Target`,
  `Group2Target`, `Group3Target`, `UrgentTarget`, or target-targeting-raid
  state.

Impact:

Raid-assigned enemies can be strategically relevant before they appear in normal
aggro or group-target structures. Combat initiated from a known raid member will
usually be captured, but target-first relevance and pre-pull encounter context
can be missed.

Recommendation:

Extend playtest relevance inputs to include raid target state through reflection.
If the web analyzer should explain raid strategy, add protocol events for raid
target assignment instead of inferring it from damage.

### 6. Raid aura ownership is underspecified

Severity: medium

Playtest `RaidManager.UpdateGroupAuras` applies raid-wide auras with
`Stats.AddStatusEffect(...)`. Current status hooks register the target slot and
spell, but they do not preserve the aura owner.

Impact:

Future status tick, proc, heal, or damage attribution can know the effect but
not the credited source actor. This can skew actor totals and make
`creditActorId` ambiguous.

Recommendation:

When implementing status lifecycle events, capture aura owner at application
time. A targeted hook around `RaidManager.UpdateGroupAuras` is likely cleaner
than trying to infer ownership later from an effect slot.

### 7. Direct HP, mana, and alive-state writes bypass hooks

Severity: high

Several scripts write combat state directly instead of calling methods the mod
currently patches.

Evidence:

- `reference/game-source/playtest/GraceEvent.cs:46-57` full-heals Grace by
  assigning `CurrentHP = CurrentMaxHP` and spawns adds.
- `reference/game-source/playtest/MizukiEvent.cs:110-115` kills shadows by
  assigning `CurrentHP = -1`.
- `reference/game-source/playtest/SprinklesEvent.cs:116-123` sets ward
  `Alive = false` and then increases AE damage and resist modifiers.
- `reference/game-source/main/FernallaFightEvent.cs` and playtest equivalent
  directly reset boss HP and refill player/group mana during phase transitions.
- `reference/game-source/main/LighthouseHealBox.cs` and playtest equivalent
  directly heal Kio and mutate attack delay.

Impact:

These actions never enter normal damage, healing, status, or resource hooks.
The web analyzer can show strange health jumps, stale actors, missing deaths,
unexplained phase transitions, and missing mana restores.

Recommendation:

Add an encounter/mechanic event family before trying to force every direct write
into damage or healing. For user-facing analysis, emit at least:

- direct health reset or scripted heal
- resource restore or drain
- scripted actor death or despawn
- invulnerability on/off
- phase transition text or code
- AE parameter changes when they explain damage spikes

### 8. `MizukiEvent` has mixed covered and uncovered paths

Severity: high

Evidence:

- `reference/game-source/playtest/MizukiEvent.cs:151-155` spawns a shadow and
  calls `MizAE.TriggerAE()`.
- `reference/game-source/playtest/MizukiEvent.cs:180-184` retunes AE fields for
  the final phase.
- `reference/game-source/playtest/MizukiEvent.cs:240-242` applies a quarter-HP
  dagger hit and a bleed status.

Impact:

The dagger damage should be captured by `DamageMePatch`, but it lacks
Mizuki-specific ability context. The bleed application is tracked only for
future effect attribution, not emitted as a status event. Shadow cleanup and
final phase AE retuning are invisible.

Recommendation:

Add a Mizuki-specific context patch for the dagger mechanic, and rely on the new
`AEEvent.TriggerAE()` patch for the direct AE. Emit status lifecycle and
encounter phase events for the rest.

### 9. Playtest status effects now carry shapeshift semantics

Severity: medium

Playtest `Stats.cs` adds shapeshift behavior around status application and
removal. Main does not have equivalent `Stats.cs` shapeshift logic.

Impact:

A status application can now also imply a form/state transition. Treating this
as an ordinary buff/debuff loses relevant playtest state.

Recommendation:

When status lifecycle events are implemented, include enough metadata to
represent form changes. Cover checked status application paths and no-check
paths.

### 10. Shared boss healing remains uncaptured

Severity: medium

Some important gaps are not playtest regressions. They exist in both branches.

Examples:

- `NPCFightEvent.FixedUpdate` boss regen through `Stats.HealMe(...)`.
- `SiraetheEvent.Update` ward-based boss healing through `Stats.HealMe(...)`.
- `LighthouseHealBox.OnTriggerEnter` direct HP restore and attack-delay changes.

Impact:

The analyzer underreports sustain and can misread DPS checks or phase pacing.

Recommendation:

Track these as shared encounter capture gaps under the broader protocol v2 event
coverage work. Issue #96 is the right umbrella.

## Files with no material logging impact found

These files were inspected and did not show material main/playtest deltas for
mod capture or web analysis:

- `CharmedNPC.cs`
- `NPCAggroArea.cs`
- `ClearAggroOnDistance.cs`
- `PlayerRaidCard.cs`, UI-only lifebar changes
- `GroupTasks.cs`
- `SimPlayerGrouping.cs`
- `GroupBuilder.cs`
- `GroupBuilderSlot.cs`
- `StatusEffect.cs`
- `StatusEffectIcon.cs`
- `TargetStatusIcon.cs`
- `SpellEffectDB.cs`
- `CombatLogHandler.cs`
- `RaidHealSlider.cs`, UI-only
- `NPCAliveListener.cs`, playtest-only environment toggling

Existing dedicated area-effect patches still line up with shared scripts such
as `AstraBreathScriot`, `NPCFightEvent.BreathAttack`, `SableheartEvent`,
`ShiverEvent`, `SpiderEvent`, `WaveEvent`, `BellEvent`, and `PhantomFightEvent`.
The important playtest additions are separate scripts rather than signature
breaks in those existing scripts.

## Web and protocol implications

Protocol v2 has the right direction, but the current mod implementation only
serializes damage events in `ProtocolSessionState`. The web schema has shapes
for broader event kinds, but the analyzer and UI are still centered on damage
and healing totals.

Recommended protocol order:

1. Patch missing damage attribution contexts first. This preserves current UI
   value with low schema churn.
2. Add death and status lifecycle next. These explain lethal mechanics and DoT
   ownership.
3. Add resource events for mana drains and restores.
4. Add encounter/mechanic state events for phase changes, invulnerability,
   direct HP resets, and AE parameter changes.
5. Extend the analyzer only after the mod emits authoritative events. Avoid
   browser-side guesses for mechanics the mod can observe.

## Recommended implementation plan

### P0: Fix playtest attribution misses

- Patch `AEEvent.TriggerAE()`.
- Patch `DeathTouch.Update()` with fixed ability context.
- Patch `MizukiEvent.SetNewAggro()` with fixed ability context.
- Add regression tests for balanced Harmony finalizers.

### P1: Make raid relevance robust

- Clear relevance cache on raid roster mutation.
- Include raid target state in relevance inputs via reflection.
- Consider spawn-time seeding for `RaidManager.LooseAdds` if encounter timeline
  accuracy requires it.

### P1: Add missing event families already tracked by issue #96

- Death events.
- Status lifecycle events with effect owner support.
- Resource drain and restore events.
- Healing events and scripted health reset events.

### P2: Add encounter/mechanic events

- Phase transitions.
- Invulnerability on/off.
- Scripted actor spawn/despawn/death.
- AE damage or resistance retuning.
- Boss enrage state mutations.

## Compatibility notes

- Keep one mod binary compatible with main and playtest.
- Continue using reflection for playtest-only members such as `NPC.MyRaidSlot`,
  raid targets, and loose-add lists.
- Do not reference playtest-only types directly from the mod.
- Keep Newtonsoft.Json for Unity runtime serialization.
- Do not add protocol v1 compatibility or web-side inference shims.

## Related tracking

Issue #96 tracks the broader event-family work:

- https://github.com/glockyco/erenshor-logs/issues/96

This audit adds the concrete playtest source evidence needed to break that work
into focused implementation tasks.
