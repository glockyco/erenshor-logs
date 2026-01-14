# Game Source Reference

This folder is for your local copy of decompiled Erenshor game code. The
contents are gitignored because we don't have distribution rights for the
game's source code.

## Why This Exists

When developing Harmony hooks, you need to know:
- Exact method signatures (parameter names and types)
- Private field names (for `___fieldName` access)
- How game systems work internally

Having decompiled source locally makes this much easier than repeatedly
opening a decompiler.

## Setup Instructions

### 1. Locate the Game DLL

Find `Assembly-CSharp.dll` in your Erenshor installation:

```
<Steam>/steamapps/common/Erenshor/Erenshor_Data/Managed/Assembly-CSharp.dll
```

### 2. Decompile

Use one of these tools:

**dnSpy** (recommended for browsing): https://github.com/dnSpy/dnSpy
- Open the DLL
- Right-click Assembly-CSharp → Export to Project
- Choose this folder as the destination

**ILSpy**: https://github.com/icsharpcode/ILSpy
- Open the DLL
- File → Save Code
- Choose this folder as the destination

### 3. Resulting Structure

After decompiling, you should have:

```
reference/game-source/
├── README.md           (this file, committed)
├── Character.cs        (gitignored)
├── Stats.cs            (gitignored)
├── UseSkill.cs         (gitignored)
├── CastSpell.cs        (gitignored)
├── SpellVessel.cs      (gitignored)
└── ...                 (gitignored)
```

## Key Files for Combat Logging

When working on hooks, these files are most relevant:

| File | Contains |
|------|----------|
| `Character.cs` | DamageMe, MagicDamageMe, BleedDamageMe, CreditDPS |
| `Stats.cs` | HealMe, AddStatusEffect, TickEffects, regen |
| `UseSkill.cs` | DoSkill (melee/ranged skill execution) |
| `CastSpell.cs` | StartSpell, StartSpellFromProc |
| `SpellVessel.cs` | ResolveSpell (spell damage application) |
| `GameData.cs` | InCombat flag, enums, global state |
| `NPC.cs` | Enemy AI, aggro management |
| `StatusEffect.cs` | Buff/debuff data structure |

## Updating

When Erenshor updates, repeat the decompile process to get the latest code.
Method signatures may change between versions.
