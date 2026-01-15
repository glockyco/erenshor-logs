---
name: svelte-web-development
description: Project-specific Svelte patterns. Use when working on the web app structure, shared state, or styling.
---

# Svelte Web Development

Project-specific patterns for the combat log analyzer. Assumes familiarity with
Svelte 5 runes (`$state`, `$derived`, `$effect`, `$props`).

## Project Structure

```
web/
├── src/
│   ├── lib/
│   │   ├── state/          # Shared state (.svelte.ts files)
│   │   ├── data/           # Parsing, aggregation, types
│   │   ├── websocket/      # WebSocket client
│   │   └── components/     # Reusable UI components
│   ├── routes/             # SvelteKit pages
│   └── app.html            # HTML template
├── static/                 # Static assets
└── package.json
```

## Shared State Pattern

For state shared across components, use module-level `$state` in `.svelte.ts`
files:

```typescript
// state/session.svelte.ts
import type { CombatEvent, SessionMetadata } from '$lib/data/types';

export const session = $state<SessionMetadata | null>(null);
export const events = $state<CombatEvent[]>([]);

export function clearSession() {
  session.value = null;
  events.length = 0;
}
```

Import and use directly in components:

```svelte
<script lang="ts">
  import { events } from '$lib/state/session.svelte';
</script>

<p>Total events: {events.length}</p>
```

## Tailwind Color Scheme

Dark theme by default:

| Element | Class |
|---------|-------|
| Page background | `bg-gray-900` |
| Card background | `bg-gray-800` |
| Primary text | `text-white` |
| Secondary text | `text-gray-400` |
| Interactive | `bg-blue-600 hover:bg-blue-700` |

## Commands

```bash
pnpm dev          # Development server
pnpm build        # Production build
pnpm check        # Type checking
pnpm lint         # Lint code
pnpm format       # Format code
```
