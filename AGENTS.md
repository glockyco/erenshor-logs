# AGENTS.md

Guidance for AI coding agents working with this repository.

## Project Overview

Combat logging mod and web analyzer for Erenshor, a single-player simulated
MMORPG. Captures combat events via BepInEx/Harmony hooks, exports to JSON,
analyzes via static web app.

## Architecture

```
BepInEx Mod (C#) → WebSocket → Web App (Svelte) → Live DPS/HPS analysis
                → JSON export → File import (later)
```

Components:
- **mod/**: BepInEx plugin with Harmony patches for combat event capture
- **web/**: Static SvelteKit app for log analysis (Cloudflare Pages)
- **cli/**: Development tools (deploy, launch, setup)

Live streaming via WebSocket (port 38729) is the primary mode. JSON file
export exists for offline analysis but is secondary.

## Essential Commands

```bash
# Web app
cd web && pnpm install && pnpm dev    # Install and run
pnpm build && pnpm check && pnpm lint # Build and validate

# Mod
cd mod && dotnet build                # Build only
cd cli && uv run erenshor deploy      # Build and deploy to game

# Development workflow (most common)
cd cli && uv run erenshor deploy      # Build mod and copy to BepInEx plugins
cd cli && uv run erenshor launch      # Launch Erenshor to test

# Other CLI commands
uv run erenshor setup                 # Copy game DLLs (needed once after install)
uv run erenshor version               # Check current version
uv run erenshor build                 # Build mod only (no deploy)

# Tests
cd mod && dotnet test tests/ErenshorLogs.Tests  # Run mod tests

# Pre-commit
pre-commit install                    # Install git hooks
pre-commit run --all-files            # Run all checks

# Storybook (component development)
cd web && pnpm storybook              # Dev server (port 6006)
```

## Collaboration Expectations

Prioritize accuracy over agreement. Avoid sycophantic behavior.

- **Challenge when appropriate**: If a request seems wrong, say so directly.
  Propose alternatives instead of just complying.
- **Flag concerns proactively**: Outdated patterns, inconsistencies, potential
  bugs, architectural issues - raise them without being asked.
- **Verify before stating**: Don't write docs or make claims without checking
  actual code. Grep, read files, confirm.
- **Ask instead of assuming**: When details are unclear, ask. Don't fill gaps
  with guesses that might be wrong.
- **Maintain positions when correct**: If pushback is based on misunderstanding,
  explain clearly rather than immediately yielding.

## Code Quality Principles

1. **Validate Every Claim**: Never make claims without checking actual code.
2. **Fail Fast**: No fallback functionality that hides errors.
3. **No Backward Compatibility**: Clean breaks when changing behavior.
4. **Keep It Simple**: No extra config options or features without discussion.
5. **Clean Cuts Only**: Remove old code entirely when refactoring.
6. **Minimal Comments**: Comments explain why, not what.
7. **Atomic Commits**: One concept per commit. Conventional commits format.
8. **Fix All Errors**: Don't ignore errors discovered during testing.
9. **Modern Dependencies**: Use latest stable versions.
10. **Clean, Robust, Maintainable**: Strive for the highest quality standards.

## Quality Standards

All code must be **clean**, **robust**, and **maintainable**. No shortcuts.

### Component Design (Web)
- **Testable in isolation**: Components should work independently when possible
- **Props for flexibility**: Accept props to override behavior for testing
- **Callback props**: Actions (delete, select, etc.) should be callbacks

### Accessibility (a11y)
- **Semantic HTML**: Use proper elements (`<button>`, `<ul>/<li>`, `<dl>/<dt>/<dd>`)
- **ARIA attributes**: All interactive elements must have proper roles, labels, and states
- **Keyboard navigation**: All interactive elements must be keyboard accessible
- **Focus management**: Visible focus indicators, logical tab order
- **Screen readers**: Decorative elements must have `aria-hidden="true"`

### Type Safety
- **No `any` types**: All data must be properly typed
- **Mock data factories**: Use typed factory functions in `src/lib/testing/`, not inline objects with JSDoc
- **Zod validation**: Leverage existing schemas for type safety

### Testing
- **Unit tests**: Core logic should have test coverage
- **Type safety**: Leverage TypeScript and Zod schemas
- **Mock data**: Use `$lib/testing` factories for test data

## Project Constraints

1. **Static Hosting**: Web app must work on Cloudflare Pages (no server)
2. **Game Compatibility**: Mod targets Unity/netstandard2.1
3. **No Game Distribution**: Never commit game DLLs or decompiled source
4. **MIT License**: All code MIT licensed under Johann Glock

## Key Technical Decisions

### JSON Serialization (Mod)
Use **Newtonsoft.Json** for all JSON serialization in the mod.
System.Text.Json fails at runtime in Unity's Mono environment with VTable
errors. This is non-negotiable.

### State Management (Web)
Use **Svelte 5 runes** in `.svelte.ts` files. Traditional stores are deprecated.

**Module-level constraints:**
- Export state/derived via getter objects (`.value` pattern)
- Use `$effect.root()` for persistence, init from components
- See `svelte-web-development` skill for patterns

### Visual Design
**Classic MMO** theme - dark stone backgrounds (`stone-800/900`) with amber
accents (`amber-500`), Cinzel fantasy font for headings, and JetBrains Mono for
numbers. See `/web/docs/style-guide.md`.

### Component Structure
Follow **shadcn/ui conventions** even before using shadcn components:
- Base components in `lib/components/ui/`
- Use `cn()` utility for class merging
- Support variants via props

### WebSocket Configuration
Default port **38729** (configurable via BepInEx config). This port was chosen
as a random high port unlikely to conflict with other services.

### Session Lifecycle (Mod)
Sessions start **lazily** when the first combat event occurs rather than
waiting for combat state confirmation. This ensures all events are captured,
including the first attack which was historically lost.

**Timeout Behavior**: Sessions awaiting confirmation timeout after 1 second.
This prevents false positives from non-combat events while trusting the game's
combat state determination. Confirmed sessions proceed normally with no timeout.

**Environmental Damage**: Does not trigger session creation as it is not combat.
Fall damage and other environmental sources are logged but do not start sessions.

The `CheckForTrueCombat` hook confirms combat state rather than initiating it.

### Versioning
**Automatic git-based versioning** in System.Version compatible CalVer format: `YYYY.M.D.REVISION`.

**How it works:**
- Version is generated from git commit metadata at build time
- Both mod and web app always have identical versions (same git commit)
- No manual version bumps needed - just commit and version updates automatically
- REVISION field is commit hash converted from hex to decimal for BepInEx compatibility

**Version format:**
- **Production**: `YYYY.M.D.REVISION` (e.g., `2026.1.24.72108205` where `72108205` = `0x44c48ad`)
- **Dirty Debug**: `YYYY.M.D.REVISION-dirty-YYYYMMDD-HHMMSS`
- **Fallback (no git)**: `0.0.0-YYYYMMDD-HHMMSS`

**Build enforcement:**
- **Debug builds**: Allow dirty trees, append timestamp
- **Release builds**: **Fail with error** if any uncommitted changes exist

**Where versions appear:**
- Mod: BepInEx log on startup
- Web: Settings drawer (with copy button), browser console on init
- CLI: `uv run erenshor version`

**Implementation:**
- Mod: `GetGitVersion` MSBuild target generates `PluginInfo.g.cs`
- Web: `scripts/generate-version.js` runs in `prebuild`/`predev` hooks
- See `docs/VERSIONING.md` for detailed documentation

## Available Skills

Load these for specialized guidance:

| Skill | Use when... |
|-------|-------------|
| `adding-event-types` | Adding a new combat event type end-to-end |
| `attribution-debugging` | Damage shows as "Unknown" or wrong ability |
| `bepinex-mod-development` | Working on mod structure, finding game methods |
| `commit-guidelines` | Writing commit messages |
| `creating-issues` | Creating GitHub issues |
| `csharp-conventions` | Writing C# code, especially DI or JSON |
| `debugging` | Troubleshooting mod or web app issues |
| `storybook` | Creating or updating component stories |
| `svelte-web-development` | Working on web app structure or state |
| `ui-design-system` | Building UI components or styling screens |
| `writing-skills` | Creating new agent skills for this project |
