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

Components:
- **mod/**: BepInEx plugin with Harmony patches for combat event capture
- **web/**: Static SvelteKit app for log analysis (Cloudflare Pages)
- **cli/**: Development tools (deploy, launch, setup)

## Essential Commands

```bash
# Web app
cd web && pnpm install && pnpm dev    # Install and run
pnpm build && pnpm check && pnpm lint # Build and validate

# Mod
cd mod && dotnet build                # Build
dotnet test tests/ErenshorLogs.Tests  # Run tests

# Pre-commit
pre-commit install                    # Install git hooks
pre-commit run --all-files            # Run all checks
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

## Project Constraints

1. **Static Hosting**: Web app must work on Cloudflare Pages (no server)
2. **Game Compatibility**: Mod targets Unity/netstandard2.1
3. **No Game Distribution**: Never commit game DLLs or decompiled source
4. **MIT License**: All code MIT licensed under Johann Glock

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
| `svelte-web-development` | Working on web app structure or state |
| `writing-skills` | Creating new agent skills for this project |
