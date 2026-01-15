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

### Using the CLI (Recommended)

The easiest way to decompile the game source:

```bash
cd cli
uv run erenshor decompile
```

This requires:
1. Game DLLs in `mod/lib/` (run `erenshor setup` first)
2. ilspycmd installed (`dotnet tool install -g ilspycmd`)
3. On macOS/Linux: `DOTNET8_ROOT` set in `cli/.env` (ilspycmd needs .NET 8)

### Manual Decompilation

If you prefer to use a GUI decompiler:

**dnSpy** (recommended for browsing): https://github.com/dnSpy/dnSpy
- Open `mod/lib/Assembly-CSharp.dll`
- Right-click Assembly-CSharp → Export to Project
- Choose this folder as the destination

**ILSpy**: https://github.com/icsharpcode/ILSpy
- Open `mod/lib/Assembly-CSharp.dll`
- File → Save Code
- Choose this folder as the destination

### Resulting Structure

After decompiling, you should have:

```
reference/game-source/
├── README.md           (this file, committed)
├── Assembly-CSharp.csproj (gitignored)
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

When Erenshor updates, run `erenshor decompile` again to get the latest code.
The command cleans existing files before decompiling, so you'll always have
a fresh copy. Method signatures may change between versions.
