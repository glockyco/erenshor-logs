# Game Libraries

This directory contains game DLLs required to compile the mod. These files are
not included in the repository (copyright) and must be copied from your local
Erenshor installation.

## Automated Setup

From the `cli/` directory:

```bash
uv run erenshor setup
```

This copies the required DLLs from your game installation. You must first
configure `ERENSHOR_PATH` in `cli/.env`.

## Manual Setup

If you prefer to copy files manually, the following DLLs are required:

| File | Source Location |
|------|-----------------|
| `Assembly-CSharp.dll` | `Erenshor_Data/Managed/` |
| `UnityEngine.dll` | `Erenshor_Data/Managed/` |
| `UnityEngine.CoreModule.dll` | `Erenshor_Data/Managed/` |

### Windows Steam Path

```
C:\Program Files (x86)\Steam\steamapps\common\Erenshor\Erenshor_Data\Managed\
```

### macOS CrossOver Path

```
~/Library/Application Support/CrossOver/Bottles/Steam/drive_c/Program Files (x86)/Steam/steamapps/common/Erenshor/Erenshor_Data/Managed/
```

## Notes

- These DLLs are only needed for compilation, not at runtime
- The mod loads against the game's actual DLLs when running
- `Private="false"` in the .csproj prevents copying these to output
