# Combat Event Source Reference

This document catalogs all combat-related events in Erenshor based on game source code analysis. It serves as the authoritative reference for implementing combat logging hooks.

**Last Updated**: 2026-01-21  
**Game Version**: Latest (as of decompilation)

---

## Table of Contents

- [Damage Sources](#damage-sources)
- [Healing Sources](#healing-sources)
- [Proc Sources](#proc-sources)
- [Status Effects (Buffs/Debuffs)](#status-effects-buffsdebuffs)
- [Death Events](#death-events)
- [Combat State](#combat-state)
- [Resource Events](#resource-events)
- [Encounter Mechanics](#encounter-mechanics)
- [Spell Interrupts](#spell-interrupts)
- [Combat Avoidance](#combat-avoidance)
- [Pet/Charmed Mechanics](#petcharmed-mechanics)

---

## Damage Sources

All damage events should include: source actor, target actor, damage type, amount, flags (critical, missed, resisted, absorbed).

### Physical Damage

**Method**: `Character.DamageMe(int _incdmg, bool _fromPlayer, GameData.DamageType _dmgType, Character _attacker, bool _animEffect, bool _criticalHit)`  
**File**: `reference/game-source/Character.cs:1049`  
**EventType**: `damage/hit physical` or `damage/hit physical` (depending on context)

**Called by**:
- Melee auto-attacks (PlayerCombat, NPC combat loops)
- Skill damage (`UseSkill.DoSkill()` at UseSkill.cs:252)
- Bow/arrow damage (`WandBolt.DeliverDamage()` at WandBolt.cs:129)
- Environmental hazards

**Returns**:
- `> 0`: Actual damage dealt (after mitigation)
- `0`: Attack missed
- `-1`: Target invulnerable
- `-2`: Fully absorbed by shield
- `-3`: No damage dealt (other reason)
- `-5`: Hit mining node (special case)
- `-6`: Hit treasure chest (special case)

**Hook strategy**: Postfix on `DamageMe` to capture final damage and return codes.

**Lifesteal handling**: Character.cs:1181-1183 calls `_attacker.MyStats.HealMe()` when lifesteal procs. Emit separate `heal/lifesteal` event here.

---

### Magic Damage

**Method**: `Character.MagicDamageMe(int _dmg, bool _fromPlayer, GameData.DamageType _dmgType, Character _attacker, float resistMod, int _baseDmg)`  
**File**: `reference/game-source/Character.cs:1215`  
**EventType**: `damage/hit magic` or `damage/hit magic` (depending on context)

**Called by**:
- Direct damage spells (via SpellVessel.ResolveSpell())
- Wand bolts (`WandBolt.DeliverDamage()` at WandBolt.cs:109 for magic damage type)
- Magic-typed proc effects

**Returns**: Same return codes as `DamageMe`

**Hook strategy**: Postfix on `MagicDamageMe` to capture magic damage events.

---

### Bleed/DoT Damage

**Method**: `Character.BleedDamageMe(int _incdmg, bool _fromPlayer, Character _attacker)`  
**File**: `reference/game-source/Character.cs:952`  
**EventType**: `damage/tick`

**Called by**:
- Bleed effects specifically
- Some percentage-based DoT effects

**Hook strategy**: Postfix on `BleedDamageMe` for bleed-specific damage.

---

### Status Effect DoT Ticks

**Method**: `Stats.TickEffects()` - private tick handler  
**File**: `reference/game-source/Stats.cs:1233`  
**EventType**: `damage/tick`

**Process**:
1. Iterates through all active StatusEffects every 3 seconds
2. For effects with `TargetDamage > 0`, calculates damage with resist check (lines 1240-1258)
3. Calls `Myself.DamageMe(num3, ...)` at line 1275 to apply damage
4. For effects with `BleedDamagePercent > 0`, calls `Myself.BleedDamageMe()` at line 1327

**Attribution**:
- `StatusEffects[i].Effect.SpellName` - spell that applied the DoT
- `StatusEffects[i].Owner` - character who cast the spell
- `StatusEffects[i].CreditDPS` - character to credit for DPS

**Hook strategy**: Hook `Stats.TickEffects()` (Prefix) to capture effect before damage, then correlate with DamageMe hook.

**Special case - ReapAndRenew**: Some DoT effects have `Spell.ReapAndRenew` flag (line 1303-1321). When these effects deal damage to an NPC, they also heal the attacker's current target for 50% of the damage dealt. This creates a simultaneous `damage/tick` and `heal/direct` event pair.

---

### Environmental Damage

**Method**: `Character.DamageMe()` called with environmental damage type  
**EventType**: `damage/hit environmental`

**Hook for environmental**: `EnvironmentalDamageMePatch.cs` (already implemented)

---

### Damage Shield (Reflect)

**Method**: `Character.DamageShieldTaken(int _dmg, Stats _giver)`  
**File**: `reference/game-source/Character.cs:897`  
**EventType**: `damage/reflect`

**Process**:
1. Called when attacker hits a target with active damage shield
2. `_giver` is the character with the damage shield
3. Calls `MyStats.ReduceHP(_dmg, GameData.DamageType.Magic, ...)` at line 912

**Hook strategy**: Postfix on `DamageShieldTaken` to emit reflect damage events.

---

### Wand and Bow Projectiles

**Method**: `WandBolt.DeliverDamage()`  
**File**: `reference/game-source/WandBolt.cs:87`  
**EventType**:
- `damage/hit magic` (for wand magic damage)
- `damage/hit physical` or `damage/hit physical` (for bow physical damage)

**Process**:
1. Wand bolts (magic): Call `MagicDamageMe()` at line 109
2. Arrows (physical): Call `DamageMe()` at line 129
3. Can trigger procs via `Proc` field (lines 211-227)

**Attribution**:
- For wand: Check `MyInv.MH.MyItem.IsWand` and proc from `WandEffect`
- For bow: Check `MyInv.MH.MyItem.IsBow` and proc from `BowEffect`

**Hook strategy**: Hooks on DamageMe/MagicDamageMe will capture these. Add context tracking for wand/bow attribution.

---

### Skill Damage

**Method**: `UseSkill.DoSkill(Skill _skill, Character _target)`  
**File**: `reference/game-source/UseSkill.cs:48`  
**Protocol event**: `damage` with `action: "hit"` and skill ability attribution

**Process**:
1. Calculates skill damage (lines 200-240)
2. Calls `_target.DamageMe()` at line 252
3. May trigger procs via `TryProc()` at line 274

**Hook strategy**: Prefix on `UseSkill.DoSkill()` to capture skill context before damage.

---

## Healing Sources

All healing events should include: source actor, target actor, heal amount, ability reference.

### Direct Healing (Simple)

**Method**: `Stats.HealMe(int _amt)`
**File**: `reference/game-source/playtest/Stats.cs:1620`
**Protocol event**: `heal` with `action: "direct"` or `action: "scripted"`

**Process**:
1. Adds HP directly: `CurrentHP += _amt`
2. Caps at `CurrentMaxHP`
3. Carries no source parameter, so attribution comes from the active combat
   context or from the scripted call-site patch.

**Hook strategy**: `HealMePatch` captures before and after HP snapshots and
emits effective healing plus overheal. Scripted raid heals add context in
`GraceEvent.DoEventScript`, `FernallaFightEvent.PhaseHandler`, and
`LighthouseHealBox.OnTriggerEnter`.

---

### Spell Healing (Full Context)

**Method**: `Stats.HealMe(Spell _spell, int _amt, bool _isCrit, bool _isMana, Character _source)`  
**File**: `reference/game-source/Stats.cs:1968`  
**EventType**: `heal/direct`

**Process**:
1. Receives full spell context including source character
2. Applies heal amount (lines 1983-1988)
3. Can also heal mana if `_isMana` is true (lines 2014-2020)
4. Returns actual amount healed (accounting for overheal)

**Hook strategy**: Postfix on this overload to emit `heal/direct` events with full attribution.

---

### Heal over Time (HoT) Ticks

**Method**: `Stats.TickEffects()` - HoT portion  
**File**: `reference/game-source/Stats.cs:1347+`  
**EventType**: `heal/tick`

**Process**:
1. For effects with `TargetHealing > 0` and `MyDamageType == Physical` (line 1347)
2. Calculates heal with intelligence scaling (lines 1349-1356)
3. Directly modifies `CurrentHP` (line 1358)
4. Shows combat log if from player (lines 1363-1375)

**Attribution**:
- `StatusEffects[i].Effect` - the HoT spell
- `StatusEffects[i].Owner` - caster who applied the HoT

**Hook strategy**: Hook `Stats.TickEffects()` to capture HoT context before healing.

---

### Lifesteal Healing

**Location**: Inside `Character.DamageMe()` and `MagicDamageMe()`  
**Files**:
- Character.cs:1181-1183 (physical damage lifesteal)
- Character.cs:1305 (via HealMe call in PlayerCombat/NPC combat)
**EventType**: `heal/lifesteal`

**Process**:
1. After damage is dealt: `if (_attacker != null && _attacker.MyStats.PercentLifesteal > 0f)`
2. Calculates: `Mathf.RoundToInt((float)damage * (_attacker.MyStats.PercentLifesteal / 100f))`
3. Calls `_attacker.MyStats.HealMe(healAmount)`

**Hook strategy**: Emit `heal/lifesteal` event in damage hooks when lifesteal conditions are met.

---

### Lifetap (Spell-based)

**Location**: `SpellVessel.ResolveSpell()` - spell resolution  
**File**: `reference/game-source/SpellVessel.cs:707, 775, 1296, 1338`  
**Protocol event**: `heal` with `action: "lifesteal"`

**Process**:
1. After spell damage: `if (spell.Lifetap)`
2. Calls `SpellSource.MyChar.MyStats.HealMe(damageAmount)`
3. Heals caster for the damage dealt

**Hook strategy**: Check spell properties in spell damage hooks to identify lifetap healing.

---

### Natural HP Regeneration

**Method**: `Stats.RegenEffects(float _mod)` - HP portion  
**File**: `reference/game-source/Stats.cs:1443-1449`  
**EventType**: `heal/regen`

**Process**:
- Restores HP based on level and Endurance
- Formula: `Level + RoundToInt((2 * EndScaleMod / 100) * CurrentEnd)`
- Caps at `CurrentMaxHP`
- Called every tick for passive regeneration

**Hook strategy**: Postfix on `RegenEffects()` or hook the HP modification to emit regen events with:
- Actor being healed
- Amount restored
- Remaining HP

**Note**: For 1:1 combat replay, this is required to show passive HP recovery between damage events.

---

## Proc Sources

Procs are triggered spells/abilities. The trigger source should be tracked separately from the ability type.

### Weapon Procs (Melee)

**Method**: `Stats.CheckProc(ItemIcon slot, Character _tar)` and `Stats.CheckProc(Item slot, Character _tar)`  
**File**: `reference/game-source/Stats.cs:1585, 1606`  
**ProcSource**: `weapon`

**Process**:
1. Checks if weapon has `WeaponProcOnHit` (lines 1587, 1615)
2. Rolls against `WeaponProcChance` (modified by Dex for Item overload at line 1614)
3. Calls `MySpells.StartSpellFromProc(slot.WeaponProcOnHit, _tar.MyStats, ...)` (lines 1589, 1617)

**Called by**:
- PlayerCombat.HandleDamageResult() at PlayerCombat.cs:804 (Coup de Grace)
- PlayerCombat after melee hits at PlayerCombat.cs:487
- NPC combat at NPC.cs:2796, 2931, 5613

**Hook strategy**: Postfix on `CheckProc` or Prefix on `StartSpellFromProc` to identify weapon procs.

---

### Shield Procs

**Method**: `UseSkill.TryProc()` - shield variant  
**File**: `reference/game-source/UseSkill.cs:1283, 1291, 1303`  
**ProcSource**: `weapon` (shields use WeaponProcOnHit)

**Process**:
1. If `_skill.ProcShield` and shield equipped (line 1291 for SimPlayer, 1303 for player)
2. Gets `MyInv.OH.MyItem.WeaponProcOnHit` (off-hand weapon proc)
3. Calls `StartSpellFromProc()`

---

### Buff/Status Effect Procs

**Method**: `Stats.CheckProc()` - buff portion  
**File**: `reference/game-source/Stats.cs:1595-1603, 1635-1643`  
**ProcSource**: `buff`

**Process**:
1. Iterates through active `StatusEffects`
2. Checks if effect has `AddProc` (lines 1598, 1638)
3. Rolls against `AddProcChance`
4. Calls `MySpells.StartSpellFromProc(statusEffect.Effect.AddProc, ...)`

---

### Skill-Triggered Procs

**Method**: `UseSkill.TryProc()` - skill variant  
**File**: `reference/game-source/UseSkill.cs:1287, 1307`  
**ProcSource**: `skill`

**Process**:
1. If `_skill.CastOnTarget` exists (lines 1287, 1307)
2. Calls `StartSpellFromProc(_skill.CastOnTarget, _target.MyStats, 0.1f)`

**Example**: Skill that casts a spell on target when it hits.

---

### Wand Procs

**Method**: `WandBolt.DeliverDamage()`  
**File**: `reference/game-source/WandBolt.cs:211-227`  
**ProcSource**: `wand`

**Process**:
1. Wand bolt has `Proc` field (line 12)
2. After damage, if `Proc != null` (line 211)
3. Calls `SourceChar.MySpells.StartSpellFromProc(Proc, TargetChar.MyStats, num3)` (line 225)

**Set via**: Item.WandEffect property at Item.cs:180

---

### Bow Procs

**Method**: `WandBolt.DeliverDamage()` (same as wand)  
**File**: `reference/game-source/WandBolt.cs:211-227`  
**ProcSource**: `bow`

**Process**: Same mechanism as wand, but uses `Item.BowEffect` at Item.cs:194

---

### Proc Execution Entry Point

**Method**: `CastSpell.StartSpellFromProc(Spell _spell, Stats _target, float modifiedCastTime, bool _resonating = false, float _scaleDmg = 1f)`  
**File**: `reference/game-source/CastSpell.cs:251`

**Process**:
1. Creates `SpellVessel` via `CreateSpellProc()` at line 271
2. SpellVessel has `isProc = true` flag (SpellVessel.cs:107)
3. Spell resolves normally but with proc flag set

**Hook strategy**: Prefix on `StartSpellFromProc` to capture all proc triggers with context from call stack.

---

### Resonating Spells (Self-Proc)

**Method**: `SpellVessel.ResolveSpell()` - resonance check  
**Files**: `reference/game-source/SpellVessel.cs:794-806, 1183-1211, 1360-1388, 1639-1667`  
**ProcSource**: N/A (resonance is self-triggered, not from external source)  
**EventFlag**: `resonating: true`

**Process**:
1. After spell resolves, rolls against resonance chance (`Spell.ResonateChance`)
2. Resonance chance can be modified by buffs (`StatusEffect.ResonateChance`)
3. If successful, calls `StartSpell()` or `StartSpellFromProc()` with `_resonate: true` or `_resonating: true`
4. Resonated spell may have reduced damage (`_scaleDmg` parameter)
5. Shows combat log: "Your spell resonates and casts again!"

**Key mechanic**: Resonance allows spells to re-cast themselves instantly for no mana cost. This is a critical DPS multiplier for spell-based classes and occurs on both direct casts and proc'd spells.

**Locations where resonance triggers**:
- SpellVessel.cs:794-806 - Direct damage spells (Type.Damage)
- SpellVessel.cs:1183-1211 - AE/PBAE spells
- SpellVessel.cs:1360-1388 - Status effect application spells
- SpellVessel.cs:1639-1667 - Beneficial/healing spells

**Hook strategy**: Check `_resonating` parameter in `StartSpellFromProc()` or `CreateSpellProc()` to set `flags.resonating = true`. This distinguishes resonance procs from weapon/buff/skill procs.

**Analysis value**:
- Track resonance proc rate vs character's resonance stat
- Calculate effective DPS multiplier from resonance
- Identify which spells benefit most from resonance

---

## Status Effects (Buffs/Debuffs)

Status effects modify character stats, apply DoTs/HoTs, and can trigger procs.

### Buff/Debuff Application

**Method**: `Stats.AddStatusEffect(Spell spell, bool _fromPlayer, int _dmgBonus, Character _specificCaster)`  
**File**: `reference/game-source/Stats.cs:2450` (`AddStatusEffectNoChecks` variant)  
**Protocol event**: `effect` with `action: "apply"` and buff/debuff effect kind

**Process**:
1. Finds empty slot in `StatusEffects[30]` array (line 2453-2457)
2. Applies root/stun if spell has those flags (lines 2459-2485)
3. Sets effect properties including:
   - `Effect` - the spell
   - `Duration` - tick count
   - `Owner` - caster (line 2499)
   - `CreditDPS` - who gets credit (line 2500)
   - `bonusDmg` - bonus damage for DoT (line 2495)
4. Calls `CalcStats()` to recalculate character stats (line 2514)

**Classification**: Determine buff vs debuff based on spell properties:
- `Spell.Type == SpellType.Beneficial` → buff
- `Spell.Type == SpellType.StatusEffect` with damage → debuff
- Check target faction relationship

**Hook strategy**: Postfix on `AddStatusEffect` variants to emit buff/debuff apply events.

---

### Buff/Debuff Removal

**Method**: `Stats.RemoveStatusEffect(int index)`  
**File**: `reference/game-source/Stats.cs:1172`  
**Protocol event**: `effect` with `action: "fade"` and buff/debuff effect kind

**Process**:
1. Reads effect before clearing: `StatusEffects[index]`
2. Handles special cases (root, stun, shield) (lines 1174-1207)
3. Clears the effect: `StatusEffects[index] = new StatusEffect()` (line 1208)
4. Recalculates stats: `CalcStats()` (line 1209)

**Removal reasons**:
- Duration expired (in `TickEffects`)
- Manually removed/dispelled
- Death (calls `RemoveAllStatusEffects`)

**Hook strategy**: Prefix on `RemoveStatusEffect` to capture effect before removal, then emit fade event.

---

### Buff/Debuff Refresh

**Method**: Multiple paths - depends on buff type  
**Files**:
- `Stats.AddStatusEffect()` - checks for existing effect and refreshes
- `Stats.RefreshWornSE(Spell spell)` - dedicated refresh method for worn effects

**Protocol event**: `effect` with `action: "refresh"` and buff/debuff effect kind

**Process**:
1. When buff is reapplied while already active, duration is reset
2. Some buffs check `CheckForHigherLevelSE()` to prevent lower-rank refreshes
3. `RefreshWornSE()` specifically handles worn item effects (auras)

**Refresh vs Apply distinction**:
- **Apply**: New buff instance, triggers application effects
- **Refresh**: Existing buff extended, does NOT trigger application effects again
- For stacking buffs, refresh may increment stacks

**Hook strategy**: In `AddStatusEffect()`, check if effect already exists in `StatusEffects[]` array before emitting event:
- If slot empty → emit `effect/apply buff`
- If slot occupied by same spell → emit `effect/refresh buff`
- If slot occupied by different spell → emit `effect/fade buff` then `effect/apply buff`

**Required for 1:1 replay**: Distinguishing refresh from apply matters because:
- Combat logs show different messages ("refreshed" vs "applied")
- Some mechanics depend on application events (on-apply damage/healing)
- Buff timeline visualization needs to show refresh points

---

## Death Events

Character death triggers when HP reaches zero.

### Death Execution

**Method**: `Character.DoDeath()`  
**File**: `reference/game-source/Character.cs:617`  
**EventType**: `death`

**Trigger**: Called from `Update()` when `CurrentHP <= 0` at Character.cs:301-302

**Process**:
1. Sets `Alive = false` (line 649)
2. Clears combat state - removes from aggro, nearby enemies lists
3. Sets animation to dead state (line 655)
4. Removes all status effects (line 656)
5. For NPCs:
   - Calls `MyNPC.Die()` (line 668)
   - Updates corpse name plate (line 685-686)
   - Handles loot/XP if player participated (lines 691-894)

**Hook strategy**: `DeathEventPatch` patches `Character.DoDeath()` and emits
`death/die` when an actor transitions from alive to dead. `KillingBlowTracker`
links the event to the latest damage event against the dead actor when known.

---

## Combat State

Combat state boundaries define session start/end. Protocol v2 does not emit
synthetic combat events for lifecycle transitions.

### Combat Detection

**Source**: `SessionManager` lazy session lifecycle
**Protocol**: `sessionSnapshot` starts/replaces session state; `sessionEnded`
ends a session.
**Implementation**: Damage hooks notify `SessionManager` before emitting combat
events, so the first event is captured under the newly-created session. The
older combat-state hook approach is not part of the current mod.

**Process**:
- Sessions start lazily from configured combat event families.
- Environmental damage is logged but does not start sessions.
- Automatic sessions end after inactivity; manual sessions end via hotkey.
---

## Resource Events

Track mana consumption and regeneration for efficiency analysis.

### Mana Consumption

**Method**: `Stats.ReduceMana(int _amt)`  
**File**: `reference/game-source/Stats.cs:1494`  
**EventType**: `resource/spend`

**Process**:
1. `CurrentMana -= _amt`
2. Clamps to minimum 0 (lines 1497-1499)

**Called by**:
- Spell casting: `SpellVessel.ResolveSpell()` via `SpellSource.MyChar.MyStats.ReduceMana(spell.ManaCost)` at SpellVessel.cs:544
- Skill usage (if skills cost mana)

**Hook strategy**: Postfix on `ReduceMana` to emit mana consumption events with:
- Actor who spent mana
- Amount consumed
- Ability that consumed it (requires context tracking)
- Remaining mana


### Raid Mana Drain and Restore Scripts

**Methods**:
- `AEManaDrainEvent.Update`
- `FernallaFightEvent.PhaseHandler`

**Protocol events**:
- `resource/drain` for raid AE mana drain
- `resource/restore` for scripted Fernalla mana restoration

**Hook strategy**: `ResourceChangePatch` snapshots mana before and after the
scripted method and emits one event per affected raid member.
---

### Mana Regeneration (Natural)

**Method**: `Stats.RegenEffects(float _mod)` - mana portion  
**File**: `reference/game-source/Stats.cs:1451-1458`  
**EventType**: `resource/regen`

**Process**:
- Restores mana based on Wisdom scaling
- Called every tick for passive regen

---

### Mana Regeneration (Spell/Ability)

**Method**: `Stats.TickEffects()` - mana restore portion  
**File**: `reference/game-source/Stats.cs:1377-1394, 1396-1406`  
**EventType**: `resource/restore`

**Process**:
1. Effects with `Mana > 0` (lines 1377-1394)
2. Effects with `PercentManaRestoration > 0` (lines 1396-1406)
3. Both directly modify `CurrentMana`

**Hook strategy**: Hook `TickEffects` to emit mana restore events from effects.

---

## Encounter Mechanics

Raid mechanic hooks capture health-affecting scripted encounter changes that do
not always flow through normal damage, healing, resource, or status methods.

### Area Effect Damage Context

**Method**: `AEEvent.TriggerAE`

**Hook strategy**: `AEEventTriggerPatch` pushes area-effect context before the
AE applies damage so downstream damage hooks can attribute the hit.

### Death Touch

**Method**: `DeathTouch.Update`

**Hook strategy**: `DeathTouchPatch` pushes `mechanic:death-touch` context for
the death-touch damage and resulting death event.

### Mizuki Aggro and Final Phase

**Methods**:
- `MizukiEvent.SetNewAggro`
- `MizukiEvent.DoFinal`

**Hook strategy**: `MizukiEventPatch` attributes dagger target-swap damage.
`MizukiFinalPhasePatch` emits `mechanic/phase` and `mechanic/statChange` when
the final phase changes AE behavior.

### Sprinkles Wards and AE Growth

**Methods**:
- `SprinklesEvent.Update`
- `SprinklesEvent.CleanList`
- `SprinklesEvent.spawnWards`

**Hook strategy**: Sprinkles mechanic patches emit invulnerability changes,
ward spawns, forced ward despawns, and AE damage or resist stat changes.

### DPS Check AE Growth

**Method**: `DPSCheckAEEvent.Update`

**Hook strategy**: `DpsCheckAeMechanicPatch` emits `mechanic/statChange` when
the scripted DPS check increases AE damage or resist modifiers.

### Faith Heal Adds

**Method**: `FaithEvent.DoEventScript`

**Hook strategy**: `FaithEventMechanicPatch` emits `mechanic/spawn` when the
script registers a heal object as a raid loose add.

---

## Spell Interrupts

Track when spells are interrupted, preventing their completion.

### Interrupt Execution

**Method**: `CastSpell.InterruptCast()`  
**File**: `reference/game-source/CastSpell.cs:338`  
**EventType**: `interrupt/interrupt`

**Process**:
1. Checks if currently casting via `CurrentVessel != null`
2. Shows combat log: "casting has been interrupted"
3. Calls `CurrentVessel.EndSpellNoCD()` (line 343)
4. Cancels the spell without triggering cooldown

**Called by**:
- Movement during casting (SpellVessel.Update() at line 499-504)
- Knockback/interrupt skills (`UseSkill` at line 272, 1314-1336)
- Jolt spells (SpellVessel.ResolveSpell() at line 627)

**Hook strategy**: Postfix on `InterruptCast()` to emit interrupt events with:
- Interrupted caster
- Spell being interrupted
- Interrupt source (if available from context)

---

## Combat Avoidance

Dodge and block checks occur BEFORE damage methods, so normal damage hooks won't see them.

### Dodge/Block Check (Player Combat)

**Method**: `PlayerCombat.CheckTargetInnateAvoidance(Character _target)`  
**File**: `reference/game-source/PlayerCombat.cs:850+` (exact line not in excerpt)  
**Protocol event**: future `damage` outcome/avoidance representation

**Returns**: String describing what happened:
- Empty string: No avoidance
- "DODGED!": Target dodged
- "BLOCKED!": Target blocked
- Other innate avoidance messages

**Process**:
1. Checks target's dodge stat
2. Checks target's block stat
3. Returns message if avoided, prevents damage call

**Hook strategy**: Postfix on `CheckTargetInnateAvoidance` to emit avoidance events when non-empty string returned.

---

### Dodge/Block Check (NPC Combat)

**Method**: `NPC.CheckTargetInnateAvoidance(Character _target)`  
**File**: `reference/game-source/NPC.cs` (exact line not in excerpt)

Similar to player variant, called before NPC damage application.

---

## Pet/Charmed Mechanics

Pets and charmed NPCs deal damage that should credit their owner.

### Pet Identification

**Fields on `Character`**:
- `Master` - Pet's owner (Character.cs:112)
- `MyStats.Charmed` - Whether NPC is charmed (Stats.cs)
- `MyCharmedNPC` - Player's charmed pet (Character.cs:110)

### Pet Damage Credit

**Location**: Inside `Character.MagicDamageMe()` and `Character.DamageMe()`  
**File**: Character.cs:1194, 1343

**Process**:
1. After damage dealt, checks if attacker has `Master`
2. Calls `_attacker.Master?.CreditDPS(damage)` to credit owner

**Hook strategy**: In damage hooks, check `_attacker.Master` and `_attacker.MyStats.Charmed` to:
- Set `flags.pet = true`
- Set `actor.masterId` to owner's ID
- Optionally emit duplicate event crediting owner

---

## Summary: Event Type Coverage

| Category | Events | Implementation Status |
|----------|--------|---------------------|
| **Damage** | `damage` with `hit`, `tick`, `reflect` actions | Partial: physical, magic, DoT, environmental implemented |
| **Healing** | `heal` with `direct`, `tick`, `lifesteal`, `regen` actions | Not implemented |
| **Status Effects** | `effect` with `apply`, `refresh`, `fade` actions | Tracking hooks partial; event emission not implemented |
| **Combat State** | `sessionSnapshot`, `sessionEnded` | Implemented |
| **Death** | `death` / `die` | Not implemented |
| **Resources** | `resource` with `spend`, `restore`, `regen` actions | Not implemented |
| **Interrupts** | `interrupt` / `interrupt` | Not implemented |
| **Avoidance** | Damage outcome or future avoidance event | Not implemented |

**Key Mechanics**:
- **Resonance**: Tracked via `flags.resonating` - spells that re-cast themselves
- **Procs**: Tracked via `ability.procSource` (weapon/wand/bow/buff/skill) - NOT as AbilityType
- **Pets**: Tracked via `flags.pet` and `actor.masterId` - damage credited to owner

---

## Implementation Priority

Based on "replay combat 1:1" goal:

**P0 - Critical** (blocks basic combat replay):
1. Complete damage attribution (proc, pet, skill sources)
2. Healing events (#49)
3. Death events
4. Status effect application/removal (#14)

**P1 - High** (important for accurate replay):
5. Mana consumption/regen (NEW)
6. Spell interrupts (NEW)
7. DoT/HoT attribution (#11)

**P2 - Medium** (nice to have):
8. Combat avoidance tracking (#47)
9. Pre-absorption damage amounts (#48)

**P3 - Low** (optional enhancements):
10. Position tracking (#52) - for visual replay
11. Threat tracking (#51) - for tank analysis

---

*This document is a living reference. Update it as game code is analyzed further or when new event types are discovered.*
