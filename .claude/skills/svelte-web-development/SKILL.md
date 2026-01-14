---
name: svelte-web-development
description: Develop the Svelte web application. Use when working on the web UI, stores, components, or data visualization.
---

# Svelte Web Development

The web app is a SvelteKit static site for analyzing combat logs.

## Project Structure

```
web/
├── src/
│   ├── lib/
│   │   ├── stores/         # Svelte stores for state
│   │   ├── data/           # Parsing, aggregation, types
│   │   ├── websocket/      # WebSocket client
│   │   └── charts/         # Chart components
│   ├── routes/             # SvelteKit pages
│   └── components/         # Reusable UI components
├── static/                 # Static assets
└── package.json
```

## Store Patterns

Use Svelte stores for shared state. Keep stores focused and composable.

### Writable Store

For state that components can modify:

```typescript
// stores/session.ts
import { writable } from 'svelte/store';
import type { SessionMetadata } from '$lib/data/types';

export const session = writable<SessionMetadata | null>(null);
```

### Derived Store

For computed values that depend on other stores:

```typescript
// stores/stats.ts
import { derived } from 'svelte/store';
import { events } from './events';

export const totalDamage = derived(events, ($events) =>
  $events
    .filter(e => e.eventType.startsWith('damage_'))
    .reduce((sum, e) => sum + (e.amount ?? 0), 0)
);
```

### Store with Methods

For stores that need actions:

```typescript
function createEventsStore() {
  const { subscribe, set, update } = writable<CombatEvent[]>([]);
  
  return {
    subscribe,
    add: (event: CombatEvent) => update(events => [...events, event]),
    clear: () => set([]),
    load: (events: CombatEvent[]) => set(events),
  };
}

export const events = createEventsStore();
```

## Component Conventions

### File Structure

```svelte
<script lang="ts">
  // 1. Imports
  import { onMount } from 'svelte';
  import { events } from '$lib/stores/events';
  
  // 2. Props
  export let title: string;
  
  // 3. Local state
  let isExpanded = false;
  
  // 4. Reactive statements
  $: totalEvents = $events.length;
  
  // 5. Functions
  function handleClick() { ... }
</script>

<!-- Template -->
<div class="...">
  ...
</div>

<style>
  /* Scoped styles (prefer Tailwind classes instead) */
</style>
```

### Props and Events

```svelte
<script lang="ts">
  import { createEventDispatcher } from 'svelte';
  
  export let value: number;
  
  const dispatch = createEventDispatcher<{
    change: number;
    select: { id: string };
  }>();
  
  function handleChange(newValue: number) {
    dispatch('change', newValue);
  }
</script>
```

## Tailwind Styling

Use Tailwind utility classes. Follow these conventions:

```svelte
<!-- Layout -->
<div class="flex flex-col gap-4 p-4">

<!-- Card pattern -->
<div class="bg-gray-800 rounded-lg p-4 shadow">

<!-- Text hierarchy -->
<h2 class="text-xl font-semibold text-white">Title</h2>
<p class="text-gray-400 text-sm">Secondary text</p>

<!-- Interactive elements -->
<button class="px-4 py-2 bg-blue-600 hover:bg-blue-700 rounded transition">
```

For dark theme (default), use `bg-gray-900` for page, `bg-gray-800` for cards.

## Chart Integration

Use a charting library (Chart.js or similar). Wrap in Svelte components:

```svelte
<script lang="ts">
  import { onMount, onDestroy } from 'svelte';
  import Chart from 'chart.js/auto';
  
  export let data: number[];
  
  let canvas: HTMLCanvasElement;
  let chart: Chart;
  
  onMount(() => {
    chart = new Chart(canvas, {
      type: 'line',
      data: { datasets: [{ data }] },
      options: { ... }
    });
  });
  
  onDestroy(() => chart?.destroy());
  
  // Update chart when data changes
  $: if (chart) {
    chart.data.datasets[0].data = data;
    chart.update();
  }
</script>

<canvas bind:this={canvas}></canvas>
```

## WebSocket Client

Handle connection state and reconnection:

```typescript
// websocket/client.ts
import { writable } from 'svelte/store';

export const connectionStatus = writable<'disconnected' | 'connecting' | 'connected'>('disconnected');

export function connect(url: string) {
  connectionStatus.set('connecting');
  
  const ws = new WebSocket(url);
  
  ws.onopen = () => connectionStatus.set('connected');
  ws.onclose = () => {
    connectionStatus.set('disconnected');
    // Reconnect after delay
    setTimeout(() => connect(url), 3000);
  };
  ws.onmessage = (event) => {
    const message = JSON.parse(event.data);
    handleMessage(message);
  };
}
```

## Performance Tips

**Large event arrays**: Use virtual scrolling for lists with 1000+ items.
Don't re-render the entire list on every update.

**Derived store efficiency**: Derived stores recalculate on every source
change. For expensive computations, debounce or memoize.

**Chart updates**: Batch data updates and call `chart.update()` once, not
per data point.

## Development Commands

```bash
pnpm dev          # Start dev server
pnpm build        # Production build
pnpm preview      # Preview production build
pnpm check        # Type checking
pnpm lint         # Lint code
```
