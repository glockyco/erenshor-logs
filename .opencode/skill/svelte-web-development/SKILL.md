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
├── docs/
│   └── style-guide.md      # Cyberpunk theme documentation
├── src/
│   ├── lib/
│   │   ├── components/
│   │   │   ├── ui/         # Base components (shadcn-compatible)
│   │   │   ├── layout/     # Layout components
│   │   │   └── session/    # Feature-specific components
│   │   ├── state/          # Svelte 5 runes (.svelte.ts files)
│   │   ├── types/          # TypeScript types
│   │   ├── websocket/      # WebSocket client
│   │   └── utils/          # Utilities (cn() for class merging)
│   ├── routes/             # SvelteKit pages
│   ├── app.css             # Tailwind directives
│   └── app.html            # HTML template
└── package.json
```

## Shared State with Svelte 5 Runes

Use module-level `$state` in `.svelte.ts` files (NOT `.ts`):

```typescript
// state/connection.svelte.ts
export type ConnectionStatus = 'disconnected' | 'connecting' | 'connected';

export const connectionStatus = $state<ConnectionStatus>('disconnected');
export const protocolVersion = $state<string | null>(null);

export function setConnected(version: string) {
  connectionStatus = 'connected';
  protocolVersion = version;
}
```

Import and use directly in components:

```svelte
<script lang="ts">
  import { connectionStatus } from '$lib/state/connection.svelte';
</script>

<div>Status: {connectionStatus}</div>
```

**Important:** For complex objects, mutate in place instead of reassigning:

```typescript
// ❌ Wrong - breaks reactivity
export const sessions = $state<Map<string, Session>>(new Map());
sessions = new Map([...sessions, [id, session]]); // Don't reassign!

// ✅ Correct - mutate in place
sessions.set(id, session); // Svelte tracks this
sessions.delete(id);       // Also works
```

## Component Props with Svelte 5

Use `$props()` rune with TypeScript interface:

```svelte
<script lang="ts">
  interface Props {
    variant?: 'primary' | 'secondary';
    class?: string;
    children: Snippet; // For slot content
  }

  let { variant = 'primary', class: className, children }: Props = $props();
</script>

<button class={cn('base-styles', variantStyles[variant], className)}>
  {@render children()}
</button>
```

## Class Name Merging

Use `cn()` utility for conditional classes and variant support:

```typescript
// lib/utils/cn.ts
import { clsx, type ClassValue } from "clsx";
import { twMerge } from "tailwind-merge";

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs));
}
```

Usage in components:

```svelte
<script lang="ts">
  import { cn } from '$lib/utils';

  const baseStyles = "px-4 py-2 rounded transition";
  const variantStyles = {
    primary: "bg-cyan-500 hover:bg-cyan-600",
    secondary: "bg-slate-700 hover:bg-slate-600"
  };
</script>

<button class={cn(baseStyles, variantStyles[variant], className)}>
  <!-- Content -->
</button>
```

## Styling and Theme

Follow the **Cyberpunk Analyst** theme documented in `/web/docs/style-guide.md`.

Key points:
- Use Tailwind utility classes (never write custom CSS without discussion)
- Dark theme: `slate-950` backgrounds, `cyan-400` accents
- Monospace numbers: `font-mono` class (JetBrains Mono font)
- Actor type colors: cyan (player), emerald (ally), rose (enemy), violet (pet)
- Reference demos: `/web/demos/demo-cyberpunk.html`

## localStorage Integration

Persist state to localStorage with debouncing:

```typescript
// state/sessions.svelte.ts
import { debounce } from '$lib/utils';

export const sessions = $state<Map<string, Session>>(new Map());

const saveToStorage = debounce(() => {
  localStorage.setItem('sessions', JSON.stringify([...sessions]));
}, 500);

export function addSession(session: Session) {
  sessions.set(session.id, session);
  saveToStorage();
}

export function loadFromStorage() {
  const data = localStorage.getItem('sessions');
  if (data) {
    const parsed = JSON.parse(data);
    sessions.clear();
    parsed.forEach(([id, session]) => sessions.set(id, session));
  }
}
```

## shadcn/ui Compatibility

Structure components to match shadcn/ui conventions:
- Base components go in `lib/components/ui/`
- Use `cn()` for class merging
- Support `variant`, `size`, and `class` props
- Export from `lib/components/ui/index.ts`

This makes future shadcn/ui migration seamless.
