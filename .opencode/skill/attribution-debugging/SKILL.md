---
name: attribution-debugging
description: Debug and fix ability attribution issues. Use when damage or healing is showing as "Unknown" or attributed to the wrong ability.
---

# Attribution Debugging

When combat events show "Unknown" ability or incorrect attribution, use this
guide to diagnose and fix the issue.

## Symptoms

- Damage shows as "Unknown" in breakdown
- Events have `flags.attributionFailed: true`
- Ability name doesn't match what was actually used
- DoT/HoT ticks not linked to source spell

## Understanding Attribution

Attribution works by maintaining context:

1. **Ability activation** (UseSkill.DoSkill, CastSpell.StartSpell) pushes
   context onto a stack
2. **Damage/heal occurs** (DamageMe, HealMe) reads current context
3. **Ability completes** pops context from stack

Attribution fails when damage occurs without context, typically because:
- The code path wasn't hooked
- Context was popped too early
- Delayed effects (DoTs) lost their source reference

## Diagnosis Steps

### 1. Export a Log with the Issue

Capture a combat log that demonstrates the problem. Note:
- What ability you used
- What the log shows instead
- Approximate timestamp

### 2. Find the Code Path

Search `reference/game-source/` for where damage originates. Start from the
ability and trace forward:

```
UseSkill.DoSkill()
  → calls target.DamageMe()

CastSpell.StartSpell()
  → creates SpellVessel
  → SpellVessel.ResolveSpell()
  → calls target.MagicDamageMe()
```

If damage is attributed correctly for similar abilities, compare the code
paths to find the difference.

### 3. Check Hook Coverage

Verify hooks exist for the relevant methods:

| Code Path | Required Hooks |
|-----------|----------------|
| Melee skills | UseSkill.DoSkill (Prefix/Postfix) |
| Spells | CastSpell.StartSpell, SpellVessel.ResolveSpell |
| DoT ticks | Stats.TickEffects, EffectTracker registration |
| Procs | CastSpell.StartSpellFromProc |
| Pet damage | Check _attacker.Master in damage hooks |

### 4. Verify Context Flow

Add debug logging to trace context:

```csharp
[HarmonyPrefix]
static void Prefix(Skill ___MySkill)
{
    Plugin.Log.LogDebug($"DoSkill Prefix: {___MySkill?.SkillName}");
    CombatContext.Push(...);
}

[HarmonyPostfix]
static void Postfix()
{
    Plugin.Log.LogDebug("DoSkill Postfix: popping context");
    CombatContext.Pop();
}
```

Check `BepInEx/LogOutput.log` to see if context is pushed before damage and
popped after.

## Common Issues and Fixes

### DoT Damage Shows Unknown

**Cause**: TickEffects deals damage but context stack is empty (original
spell cast ended long ago).

**Fix**: EffectTracker must store the source spell when AddStatusEffect is
called, then TickEffects hook reads from tracker instead of context stack.

### Proc Damage Shows Unknown

**Cause**: StartSpellFromProc creates a new spell execution without linking
to the triggering item/ability.

**Fix**: Hook StartSpellFromProc to set isProc flag and capture triggering
source if available.

### AE/PBAE Only First Hit Attributed

**Cause**: Context popped after first target, remaining targets have no
context.

**Fix**: Pop context in Postfix after the loop completes, not after each hit.

### Pet Damage Shows as Pet, Not Owner

**Cause**: Source is set to pet instead of checking Master.

**Fix**: In damage hooks, check `_attacker.MyStats.Charmed` and use
`_attacker.Master` as the owner reference.

## Adding New Attribution

When adding attribution for a new ability type:

1. Find all code paths that lead to damage
2. Add Prefix hook at ability start to push context
3. Add Postfix hook at ability end to pop context
4. For delayed effects, register in EffectTracker
5. Test with actual gameplay, check for Unknown events
6. Add debug logging if attribution still fails

## Reporting Issues

If you can't fix an attribution issue, file an issue using the Attribution
Issue template with:
- Steps to reproduce
- Combat log excerpt showing the problem
- Which ability was used vs what it was attributed as
