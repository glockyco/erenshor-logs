# Testing with Erenshor Playtest

The mod can be deployed to both the main Erenshor installation and the Erenshor Playtest (separate Steam install for testing upcoming features).

## Setup (Already Complete)

Both installations are configured and ready:

- ✅ **Main**: BepInEx and mod installed
- ✅ **Playtest**: BepInEx and mod installed

## Switching Between Installations

Edit `cli/.env` and uncomment the installation you want to use:

### Use Main Installation (Default)
```bash
# Main installation (currently active)
ERENSHOR_PATH=~/Library/Application Support/CrossOver/Bottles/Steam/drive_c/Program Files (x86)/Steam/steamapps/common/Erenshor

# Playtest installation (uncomment to switch)
# ERENSHOR_PATH=~/Library/Application Support/CrossOver/Bottles/Steam/drive_c/Program Files (x86)/Steam/steamapps/common/Erenshor Playtest
```

### Use Playtest Installation
```bash
# Main installation
# ERENSHOR_PATH=~/Library/Application Support/CrossOver/Bottles/Steam/drive_c/Program Files (x86)/Steam/steamapps/common/Erenshor

# Playtest installation (currently active)
ERENSHOR_PATH=~/Library/Application Support/CrossOver/Bottles/Steam/drive_c/Program Files (x86)/Steam/steamapps/common/Erenshor Playtest
```

## Deploying to Either Installation

After changing `cli/.env`, just run:

```bash
cd cli
uv run erenshor deploy
```

The mod will be deployed to whichever installation is active in `.env`.

## Testing New Features

The Playtest build may have:
- New classes (like the upcoming Reaver)
- New abilities or damage types
- Balance changes
- Game mechanic updates

Testing with both versions ensures the mod works across game updates.

## Notes

- Both installations have **separate** BepInEx folders and configs
- Mod version is the **same** for both (built from the same codebase)
- Web app works with both (connects via WebSocket on same port)
- Logs are stored separately in each installation's `BepInEx/LogOutput.log`
