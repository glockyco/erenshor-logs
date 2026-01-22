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
├── .storybook/             # Storybook configuration
├── docs/
│   └── style-guide.md      # Cyberpunk theme documentation
├── src/
│   ├── lib/
│   │   ├── components/
│   │   │   ├── ui/         # Base components (Button, Card, Badge, StatBlock)
│   │   │   ├── layout/     # Layout components (Header, ResponsiveLayout)
│   │   │   └── status/     # Status indicators (ConnectionStatus)
│   │   ├── services/       # Business logic (WebSocket, combat analyzer)
│   │   ├── state/          # Svelte 5 runes (.svelte.ts files)
│   │   ├── types/          # TypeScript types and Zod schemas
│   │   └── utils/          # Utilities (cn, format, storage, constants)
│   └── routes/             # SvelteKit pages
└── package.json
```

## Module-Level State

Svelte 5 prohibits exporting reactive values that get reassigned. Use the
**internal state + getter pattern**:

```typescript
// state/connection.svelte.ts
import type { ConnectionStatus } from "$lib/types";

// Internal state object - never exported directly
const state = $state({
  connectionStatus: "disconnected" as ConnectionStatus,
  protocolVersion: null as string | null,
});

// Export getters for reactive values (accessed via .value)
export const connectionStatus = {
  get value() {
    return state.connectionStatus;
  },
};

export const protocolVersion = {
  get value() {
    return state.protocolVersion;
  },
};

// Setter functions mutate the internal state
export function setConnected(version: string): void {
  state.connectionStatus = "connected";
  state.protocolVersion = version;
}
```

Usage in components:

```svelte
<script lang="ts">
  import { connectionStatus } from "$lib/state";
</script>

<div>Status: {connectionStatus.value}</div>
```

**Why?** Svelte 5 throws `state_invalid_export` if you `export let x = $state()`
and reassign `x`. The getter pattern satisfies the compiler while maintaining
reactivity.

**For collections:** `SvelteMap` and `SvelteSet` can be exported directly since
mutations don't reassign the collection itself:

```typescript
import { SvelteMap } from "svelte/reactivity";

export const sessions = new SvelteMap<string, Session>();

// These mutations are tracked automatically
sessions.set(id, session);  // ✅ Works
sessions.delete(id);        // ✅ Works
```

## Derived State

Same pattern applies to `$derived`:

```typescript
// Internal derived - not exported directly
const _activeSession = $derived.by(() => {
  if (!state.activeSessionId) return null;
  return sessions.get(state.activeSessionId) ?? null;
});

// Export via getter
export const activeSession = {
  get value() {
    return _activeSession;
  },
};
```

**Why?** Svelte 5 throws `derived_invalid_export` for `export const x = $derived()`.

## Persistence with $effect.root

Module-level `$effect` causes `effect_orphan` errors in Storybook. Wrap
persistence logic in `$effect.root()` and initialize from a component:

```typescript
// state/sessions.svelte.ts
import { loadFromStorage, saveToStorage } from "$lib/utils/storage";
import { STORAGE_KEYS } from "$lib/utils/constants";
import { StoredSessionsSchema } from "$lib/types/schemas";

// Load on module init (safe - no effects)
const stored = loadFromStorage(STORAGE_KEYS.SESSIONS, StoredSessionsSchema);
if (stored) {
  stored.forEach(([id, session]) => sessions.set(id, session));
}

// Initialize persistence from component context
export function initSessionsPersistence(): () => void {
  return $effect.root(() => {
    $effect(() => {
      saveToStorage(STORAGE_KEYS.SESSIONS, Array.from(sessions.entries()));
    });
  });
}
```

Call from root layout:

```svelte
<!-- +layout.svelte -->
<script lang="ts">
  import { initSessionsPersistence } from "$lib/state";

  $effect(() => {
    const cleanup = initSessionsPersistence();
    return cleanup;
  });
</script>
```

**Utilities:** Use `loadFromStorage()` / `saveToStorage()` from `$lib/utils/storage`
for SSR-safe, Zod-validated localStorage access.

## Component Props

Use `$props()` rune with TypeScript interface:

```svelte
<script lang="ts">
  import type { Snippet } from "svelte";
  import { cn } from "$lib/utils";

  interface Props {
    variant?: "primary" | "secondary";
    class?: string;
    children: Snippet;
  }

  let { variant = "primary", class: className, children }: Props = $props();

  const variantStyles = {
    primary: "bg-cyan-500 hover:bg-cyan-600",
    secondary: "bg-slate-700 hover:bg-slate-600",
  };
</script>

<button class={cn("px-4 py-2 rounded transition", variantStyles[variant], className)}>
  {@render children()}
</button>
```

## Styling

Follow the **Cyberpunk Analyst** theme documented in `/web/docs/style-guide.md`.

Key conventions:
- Use Tailwind utility classes (never write custom CSS without discussion)
- Dark backgrounds: `slate-950`, `slate-900`
- Primary accent: `cyan-400`, `cyan-500`
- Monospace for numbers: `font-mono` class (JetBrains Mono font)
- Actor type colors: `cyan` (player), `emerald` (ally), `rose` (enemy), `violet` (pet)

The `cn()` utility (from `$lib/utils`) merges Tailwind classes with proper
precedence using `clsx` and `tailwind-merge`.
