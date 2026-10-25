# Erenshor Logs

Erenshor Logs records combat in [Erenshor](https://store.steampowered.com/app/2382520/Erenshor/) and shows damage, healing and ability breakdowns in a browser. It combines a BepInEx mod with a web analyzer for live combat and saved sessions. This unofficial project is in development and currently targets Erenshor Playtest.

[Open the web analyzer](https://erenshor-logs.wowmuch1.workers.dev/).

## Analyze combat

You can try the analyzer's demo or import saved sessions without installing the mod. Use **Import Sessions** on the welcome screen, or **Import** in the session list, to open a log. Use **Export** to save sessions for later analysis or sharing.

For live analysis, install the mod, launch the game and open the analyzer on the same computer. Combat sessions appear automatically. Select a session and an actor to inspect damage, healing and abilities. The settings drawer lets you change the connection address if it differs from the mod's configuration.

## Install the mod from source

You need Erenshor Playtest, Git, a .NET SDK, Python available as `python3`, uv, and Node.js with pnpm for the web app. The CLI's Python requirement is in [cli/pyproject.toml](cli/pyproject.toml). [CONTRIBUTING.md](CONTRIBUTING.md) covers platform setup, including CrossOver on macOS.

From the repository root:

```sh
cd cli
cp .env.example .env
uv sync
```

Set `ERENSHOR_PATH` in `cli/.env` to your Playtest installation. Then, from `cli/`:

```sh
uv run erenshor bepinex
uv run erenshor setup
```

Launch the game once and close it so BepInEx creates its configuration. Build and install the mod from `cli/` with a clean working tree:

```sh
uv run erenshor deploy
```

Launch the game again through Steam or your usual launcher. The deployment command copies the mod into the game's BepInEx plugins folder and updates BepInEx configuration. It builds a Release DLL with its dependencies included. Release builds reject uncommitted changes.

## Run the analyzer locally

From the repository root:

```sh
cd web
pnpm install
pnpm dev
```

Open the address printed by Vite. You can use the local analyzer with demos, imported sessions or the running mod.

## Data and settings

The mod streams combat data over a WebSocket connection. It listens only on loopback unless you enable LAN connections. See [ModConfig.cs](mod/src/Config/ModConfig.cs) for connection and session settings.

The analyzer saves sessions and settings in your browser's local storage. Export sessions before clearing browser data if you want to keep them. Exported logs include actor names and combat events, so review them before sharing.

## Development and reference

[CONTRIBUTING.md](CONTRIBUTING.md) covers setup and checks. [AGENTS.md](AGENTS.md) holds coding-agent guidance. Web commands are defined in [web/package.json](web/package.json), and [.github/workflows/ci.yml](.github/workflows/ci.yml) defines CI checks.

- [Architecture](docs/ARCHITECTURE.md) describes the mod and analyzer.
- [Combat events](docs/COMBAT_EVENTS.md) describes capture and attribution.
- [Log format](docs/LOG_FORMAT.md) defines live frames and exported files.
- [Versioning](docs/VERSIONING.md) explains build-time versions and release requirements.

## License

[MIT](LICENSE).
