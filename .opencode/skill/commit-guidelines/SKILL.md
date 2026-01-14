---
name: commit-guidelines
description: Write commit messages following project conventions. Use when creating commits, reviewing staged changes, or amending commit messages.
---

# Commit Guidelines

Follow conventional commits format with prose descriptions. Commits should be
atomic (one concept each) and explain why changes were made.

## Format

```
type(scope): short summary in imperative mood

Prose description explaining what changed and why. Focus on reasoning and
context rather than listing files or bullet points. Write in complete
sentences. Wrap lines at 80 characters.
```

## Types

- **feat**: New feature or capability
- **fix**: Bug fix
- **refactor**: Code restructuring without behavior change
- **docs**: Documentation only
- **test**: Adding or updating tests
- **chore**: Maintenance, dependencies, CI configuration

## Scopes

Common scopes for this project:
- **mod**: BepInEx plugin code
- **web**: Svelte web application
- **hooks**: Harmony hook implementations
- **events**: Event model or emitter
- **ui**: In-game IMGUI overlay
- **export**: JSON export functionality
- **ws**: WebSocket server/client
- **docs**: Documentation
- **protocol**: WebSocket protocol

## Rules

- Use imperative mood: "Add feature" not "Added feature"
- No period at end of summary line
- Summary under 72 characters
- Body wrapped at 80 characters
- Prose paragraphs, not bullet lists
- Explain why, not what (the diff shows what)
- One concept per commit

## Good Example

```
feat(hooks): add DoT damage attribution via effect tracker

Track the source spell when damage-over-time effects are applied so that
periodic damage ticks can be correctly attributed to the original ability.
The EffectTracker maintains a mapping from active StatusEffects to their
source spell names, which is consulted when TickEffects deals damage.
```

## Bad Example

```
feat(hooks): add DoT tracking

- Added EffectTracker class
- Modified Stats.AddStatusEffect hook
- Updated DamageHooks to check tracker
- Added unit tests
```

The bad example uses bullet points instead of prose and describes what changed
rather than why. The summary is also too vague to be useful in git log output.

## Atomic Commits

Each commit should represent one logical change:

**Good**: Separate commits for "add event type" and "update UI to display it"

**Bad**: Single commit for "add event type, update UI, fix unrelated bug"

If you find yourself writing "and" in the summary, consider splitting the commit.
