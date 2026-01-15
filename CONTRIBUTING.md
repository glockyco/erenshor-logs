# Contributing to Erenshor Logs

Thank you for your interest in contributing! This document provides guidelines
for contributing to the project.

## Getting Started

1. Fork the repository
2. Clone your fork locally
3. Set up development environment (see below)
4. Create a branch for your changes

## Development Setup

### Prerequisites (All Platforms)

- .NET SDK 8.0+
- Node.js 18+ and pnpm
- Python 3.11+ and uv (`pip install uv`)
- pre-commit (`pip install pre-commit && pre-commit install`)

### Development CLI

The `cli/` directory contains development tools for the mod workflow. All
commands require configuration via a `.env` file.

#### Initial Setup

```bash
cd cli
cp .env.example .env
# Edit .env and set ERENSHOR_PATH to your game installation
uv sync
```

#### Available Commands

```bash
cd cli

# Copy game DLLs for mod compilation
uv run erenshor setup

# Install BepInEx to game folder
uv run erenshor bepinex

# Build the mod
uv run erenshor build

# Build and deploy mod to BepInEx plugins
uv run erenshor deploy

# Launch Erenshor (via CrossOver on macOS)
uv run erenshor launch

# Decompile game source for reference (requires ilspycmd)
uv run erenshor decompile
```

### Web Development

```bash
cd web
pnpm install
pnpm dev
```

### macOS Development (CrossOver)

Erenshor is Windows-only, so macOS developers need a way to run the game.
We recommend CrossOver, which runs Windows apps via Wine translation with
good performance.

#### CrossOver Setup

1. **Install CrossOver** (~$75, 14-day free trial available):
   - Download from [codeweavers.com](https://www.codeweavers.com/crossover)
   - Or: `brew install --cask crossover`

2. **Install Steam in CrossOver**:
   - Open CrossOver
   - Click "Install a Windows Application"
   - Search for "Steam" and install it (creates a bottle named "Steam")

3. **Install Erenshor**:
   - Launch Steam from CrossOver
   - Log in and install Erenshor

4. **Configure CLI**:

   ```bash
   cd cli
   cp .env.example .env
   ```

   Edit `.env` with:

   ```bash
   ERENSHOR_PATH=~/Library/Application Support/CrossOver/Bottles/Steam/drive_c/Program Files (x86)/Steam/steamapps/common/Erenshor
   CROSSOVER_BOTTLE=Steam
   ```

5. **Install BepInEx and copy game DLLs**:

   ```bash
   uv run erenshor bepinex
   uv run erenshor setup
   ```

6. **Decompile game source** (optional, for hook development):

   ```bash
   # Install ilspycmd if not already installed
   dotnet tool install -g ilspycmd

   # Add to .env (ilspycmd requires .NET 8)
   # DOTNET8_ROOT=/opt/homebrew/opt/dotnet@8/libexec

   uv run erenshor decompile
   ```

   This creates searchable C# files in `reference/game-source/`.

#### Development Workflow

```bash
cd cli

# Build and deploy mod
uv run erenshor deploy

# Launch game to test
uv run erenshor launch
```

#### When the Game Updates

After an Erenshor update, refresh your development environment:

```bash
cd cli
uv run erenshor setup      # Copy updated game DLLs
uv run erenshor decompile  # Refresh decompiled source
uv run erenshor deploy     # Rebuild against new DLLs
```

Method signatures may change between versions, which could break Harmony
patches. Check the decompiled source if hooks stop working.

## Testing

### Mod Tests

Run unit tests locally (requires game DLLs from `erenshor setup`):

```bash
cd mod && dotnet test tests/ErenshorLogs.Tests
```

Tests target `net9.0` while the mod targets `netstandard2.1` for Unity
compatibility.

### Web Tests

```bash
cd web
pnpm check  # Type checking
pnpm lint   # ESLint
```

## Code Style

### C# (Mod)

- Use modern C# features (records, init, required) via PolySharp polyfills
- Follow standard .NET naming conventions
- Add XML documentation for public APIs
- Keep methods focused and small
- Use `sealed` on classes/records unless inheritance is intended

### TypeScript (Web)

- Use TypeScript strict mode
- Prefer functional patterns
- Use Svelte stores for state management
- Follow existing code formatting (Prettier)

## Commit Messages

We use conventional commits with prose descriptions. Each commit should be
atomic (one concept per commit) and the message should explain *why* the
change was made, not just *what* changed.

### Format

```
type(scope): short summary

Prose description explaining the change. Focus on the reasoning and context
rather than listing what files changed. Keep lines to 80 characters max.
```

### Types

- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `refactor`: Code refactoring
- `test`: Adding or updating tests
- `chore`: Maintenance tasks

### Examples

Good:
```
feat(mod): add DoT damage attribution

Track the source spell when damage-over-time effects are applied so that
periodic damage ticks can be correctly attributed to the original ability
rather than showing as "Unknown" in the breakdown.
```

```
fix(web): correct DPS calculation for partial seconds

The final second of combat was being excluded from DPS calculations when
the session duration wasn't an exact multiple of the bucket size. This
caused artificially inflated DPS numbers for short encounters.
```

Avoid bullet-point style messages like:
```
feat: add new feature

- Added file X
- Modified file Y
- Updated file Z
```

## Pull Requests

1. Create a feature branch from `main`
2. Make your changes with clear commits
3. Test your changes thoroughly
4. Update documentation if needed
5. Submit a PR with a clear description

### PR Checklist

- [ ] Code compiles without warnings
- [ ] Changes are tested
- [ ] Documentation is updated
- [ ] Commit messages follow conventions

## Reporting Issues

### Bug Reports

Please include:
- Erenshor version
- Mod version
- Steps to reproduce
- Expected vs actual behavior
- Combat log excerpt if relevant

### Feature Requests

Please include:
- Clear description of the feature
- Use case / why it's needed
- Any implementation ideas

### Attribution Issues

If ability attribution is incorrect:
- Export a combat log showing the issue
- Describe what ability was used
- Note what it was attributed as vs what it should be

## Questions?

Open a discussion on GitHub or reach out on the Erenshor Discord.
