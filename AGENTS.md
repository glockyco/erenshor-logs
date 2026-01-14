# AGENTS.md

Guidance for AI coding agents working with this repository.

## Project Overview

Combat logging mod and web analyzer for Erenshor, a single-player simulated
MMORPG. Captures combat events via BepInEx/Harmony hooks, exports to JSON,
analyzes via static web app.

## Architecture

```
BepInEx Mod (C#) → JSON logs → Web App (Svelte) → DPS/HPS analysis
                → WebSocket → Live streaming
```

Two components:
- **mod/**: BepInEx plugin with Harmony patches for combat event capture
- **web/**: Static SvelteKit app for log analysis (Cloudflare Pages)

## Essential Commands

```bash
# Web app
cd web && pnpm install        # Install dependencies
pnpm dev                      # Development server
pnpm build                    # Production build
pnpm check                    # Type check
pnpm lint                     # ESLint
pnpm format                   # Prettier

# Mod
cd mod && dotnet restore      # Restore dependencies
dotnet build                  # Build mod
dotnet tool restore           # Restore CSharpier
dotnet csharpier .            # Format C#

# Pre-commit
pre-commit install            # Install git hooks
pre-commit run --all-files    # Run all checks
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
   Search the codebase, read files, verify implementations.

2. **Fail Fast**: No fallback functionality that hides errors. Fail immediately
   with clear messages.

3. **No Backward Compatibility**: Clean breaks when changing behavior. No
   legacy code paths "just in case".

4. **Keep It Simple**: No extra config options or features. Suggest improvements
   but only implement after discussion.

5. **Clean Cuts Only**: Remove old code entirely when refactoring. Less code
   means less maintenance.

6. **Minimal Comments**: Don't comment obvious code. Comments explain why,
   not what.

7. **Atomic Commits**: One concept per commit. Conventional commits format.
   Prose descriptions, not bullet lists. 80 char line limit.

8. **Fix All Errors**: Don't ignore errors. Fix bugs discovered during testing.

9. **Modern Dependencies**: Use latest stable versions. No pinning to old
   versions without explicit reason.

## Development Guidelines

### Web App (Svelte 5 + SvelteKit)
- TypeScript strict mode required
- Use Svelte 5 runes (`$state`, `$derived`, `$effect`) not legacy syntax
- Static site only - all routes must be prerenderable
- No server-side code or dynamic routes
- Tailwind CSS for styling

### Mod (C# / BepInEx)
- Target .NET Standard 2.1 for Unity compatibility
- Use Harmony for runtime patching
- Attribute-based event context tracking for damage attribution
- Fleck for WebSocket server (live streaming)

### Game Reference
- Decompiled game source in `reference/game-source/` (not committed)
- Key classes: `Character`, `Stats`, `UseSkill`, `CastSpell`, `SimPlayer`
- Combat ticks every 3 seconds via `Stats.TickEffects()`

## Testing

```bash
# Web
cd web && pnpm check          # Svelte + TypeScript validation
cd web && pnpm lint           # ESLint

# Mod
cd mod && dotnet build --warnaserror  # Build with warnings as errors
```

CI runs: formatting checks, linting, type checking, builds, secret scanning.

## Project Constraints

1. **Static Hosting**: Web app must work on Cloudflare Pages (no server)
2. **Game Compatibility**: Mod targets whatever Unity version Erenshor uses
3. **No Game Distribution**: Never commit game DLLs or decompiled source
4. **MIT License**: All code MIT licensed under Johann Glock
