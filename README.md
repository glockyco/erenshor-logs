# Erenshor Logs

Combat logging and analysis tools for [Erenshor](https://store.steampowered.com/app/2382520/Erenshor/), a single-player simulated MMORPG inspired by classic EverQuest.

## Features

- **Accurate Combat Tracking** - Every hit, heal, and effect with full ability attribution
- **Real-time DPS Meter** - In-game overlay with per-ability breakdown
- **Detailed Analysis** - Web-based tools for deep-diving your combat data
- **Shareable Logs** - Export and share your parses with the community
- **Live Mode** - Stream combat data to the web app in real-time

## Status

This project is currently in development. See our [project board](https://github.com/glockyco/erenshor-logs/projects) for progress.

## Components

### BepInEx Mod (`mod/`)

A BepInEx plugin that hooks into Erenshor's combat system to capture detailed combat events:

- Damage dealt and received (physical, magic, DoTs, etc.)
- Healing done (direct heals, HoTs, lifesteal)
- Buff and debuff tracking with uptime
- Full ability attribution (which skill/spell caused each effect)
- Pet damage attributed to owners
- SimPlayer tracking with the same detail as the player

### Web Application (`web/`)

A static Svelte application for analyzing combat logs:

- **Live Mode** - Connect to the running game via WebSocket
- **Import Mode** - Load exported JSON log files
- **Timeline View** - Damage/healing over time with drill-down
- **Breakdown View** - Ability-by-ability analysis (WoW-style bars)
- **Event Log** - Searchable, filterable event list
- **Comparison** - Side-by-side log comparison for theorycrafting

## Installation

Coming soon!

## Usage

Coming soon!

## Log Format

Combat logs are stored as gzipped JSON files. See [docs/LOG_FORMAT.md](docs/LOG_FORMAT.md) for the complete specification.

## Versioning

This project uses **automatic git-based versioning**. Versions are generated at build time from git commit metadata in the format `YYYY.MM.DD-COMMITHASH` (e.g., `2026.01.24-fdd823c`).

Both the mod and web app always have identical versions since they're built from the same git commit. No manual version bumps are needed - just commit your changes and the version updates automatically.

**Where to find the version:**
- **Mod**: BepInEx log on startup
- **Web App**: Settings drawer (gear icon) with copy button, or browser console on load
- **CLI**: Run `uv run erenshor version`

**Build enforcement:**
- Debug builds allow uncommitted changes and append a timestamp
- Release builds **fail with an error** if any uncommitted changes exist

For detailed documentation, see [docs/VERSIONING.md](docs/VERSIONING.md).

## Development

### Prerequisites

- .NET Framework 4.7.2 SDK (for mod)
- Node.js 18+ and pnpm (for web app)
- Erenshor with BepInEx installed

### Building the Mod

```bash
cd mod
dotnet build
```

### Running the Web App

```bash
cd web
pnpm install
pnpm dev
```

## Contributing

We welcome contributions! See [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

## License

MIT License - See [LICENSE](LICENSE) for details.

## Acknowledgments

- Inspired by [Warcraft Logs](https://www.warcraftlogs.com/) and [Details!](https://www.curseforge.com/wow/addons/details)
- Built for the Erenshor community
