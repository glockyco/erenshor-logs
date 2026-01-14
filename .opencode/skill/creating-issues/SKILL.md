---
name: creating-issues
description: Create GitHub issues for development tasks. Use when adding new issues or updating existing ones.
---

# Creating Issues

Development issues follow a consistent structure to ensure clarity and
trackability.

## Structure

### Summary
One sentence describing what this issue accomplishes. Be specific and concise.

### Context
Why this work is needed:
- Problem being solved or goal being achieved
- Dependencies on other issues (link them with #number)
- Relevant background information

### Tasks
Checklist of concrete work items:

```markdown
- [ ] First task
- [ ] Second task
- [ ] Third task
```

Use checkboxes for trackability. Break down into small, verifiable steps.
Mark completed items with `[x]` as work progresses.

### Acceptance Criteria
Observable outcomes that indicate completion:
- Specific behavior that should work
- Commands that should succeed
- States that should be true

Focus on "what" not "how". These should be verifiable by someone other than
the implementer.

### Notes (optional)
Additional context that doesn't fit above:
- Constraints or limitations
- Related issues or alternatives considered
- Technical details for implementers

## Labels

Use existing labels consistently:
- **mod**: BepInEx plugin code
- **web**: Svelte web application
- **infrastructure**: Build, CI/CD, tooling
- **attribution**: Damage/heal attribution system
- **docs**: Documentation
- **P0-critical**: Blocking, must do first
- **P1-high**: Important for milestone
- **P2-medium**: Should do
- **P3-low**: Nice to have

## Example

```markdown
## Summary

Add WebSocket server for live combat log streaming.

## Context

The web app needs to receive combat events in real-time for live DPS tracking.
This enables the "live mode" feature where users connect to the mod while
playing.

Depends on #3 (Event emitter system).

## Tasks

- [ ] Add Fleck WebSocket server initialization in Plugin.cs
- [ ] Create message serialization for combat events
- [ ] Handle client connections and disconnections
- [ ] Broadcast events to all connected clients
- [ ] Add configuration option for server port

## Acceptance Criteria

- WebSocket server starts on plugin load
- Clients can connect to ws://localhost:18585
- Combat events are received by connected clients in real-time
- Server handles disconnections gracefully without errors

## Notes

- Default port 18585 (configurable via BepInEx config)
- Consider rate limiting if event volume is high
```

## Updating Existing Issues

When updating an issue:
- Mark completed tasks with `[x]`
- Add new tasks discovered during implementation
- Update acceptance criteria if scope changed
- Add notes explaining any deviations from original plan
