---
name: svelte-web-development
description: Develop the Svelte 5 web application. Use when working on the web UI, state management, components, or data visualization.
---

# Svelte Web Development

The web app is a SvelteKit static site for analyzing combat logs. Uses Svelte 5
with runes for state management.

## Project Structure

```
web/
├── src/
│   ├── lib/
│   │   ├── state/          # Svelte 5 state (runes)
│   │   ├── data/           # Parsing, aggregation, types
│   │   ├── websocket/      # WebSocket client
│   │   └── components/     # Reusable UI components
│   ├── routes/             # SvelteKit pages
│   └── app.html            # HTML template
├── static/                 # Static assets
└── package.json
```

## Svelte 5 Runes

Use runes for all reactive state. Never use legacy `$:` syntax or stores.

### Component State with `$state`

```svelte
<script lang="ts">
  let count = $state(0);
  let items = $state<string[]>([]);

  function increment() {
    count++;  // Direct mutation works
  }

  function addItem(item: string) {
    items.push(item);  // Array mutations work
  }
</script>
```

### Derived Values with `$derived`

```svelte
<script lang="ts">
  let events = $state<CombatEvent[]>([]);

  // Simple derived
  let totalDamage = $derived(
    events
      .filter(e => e.eventType.startsWith('damage_'))
      .reduce((sum, e) => sum + (e.amount ?? 0), 0)
  );

  // Derived with complex logic
  let dps = $derived.by(() => {
    if (events.length === 0) return 0;
    const duration = events.at(-1)!.timestamp - events[0].timestamp;
    return duration > 0 ? totalDamage / (duration / 1000) : 0;
  });
</script>
```

### Side Effects with `$effect`

```svelte
<script lang="ts">
  let query = $state('');

  // Runs when query changes
  $effect(() => {
    console.log('Query changed:', query);
  });

  // Cleanup pattern
  $effect(() => {
    const ws = new WebSocket(url);
    return () => ws.close();  // Cleanup function
  });
</script>
```

### Props with `$props`

```svelte
<script lang="ts">
  interface Props {
    title: string;
    count?: number;
    onSelect?: (id: string) => void;
  }

  let { title, count = 0, onSelect }: Props = $props();
</script>
```

### Bindable Props with `$bindable`

```svelte
<script lang="ts">
  interface Props {
    value: string;
  }

  let { value = $bindable() }: Props = $props();
</script>

<!-- Parent can use bind:value -->
```

## Shared State

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

export function addEvent(event: CombatEvent) {
  events.push(event);
}
```

Use in components:

```svelte
<script lang="ts">
  import { events, addEvent } from '$lib/state/session.svelte';
</script>

<p>Total events: {events.length}</p>
```

## Component Conventions

### File Structure

```svelte
<script lang="ts">
  // 1. Imports
  import { events } from '$lib/state/session.svelte';
  import DamageChart from '$lib/components/DamageChart.svelte';

  // 2. Props
  interface Props {
    title: string;
  }
  let { title }: Props = $props();

  // 3. Local state
  let isExpanded = $state(false);

  // 4. Derived values
  let totalEvents = $derived(events.length);

  // 5. Effects
  $effect(() => {
    document.title = `${title} (${totalEvents})`;
  });

  // 6. Functions
  function toggle() {
    isExpanded = !isExpanded;
  }
</script>

<!-- Template -->
<div class="...">
  <h1>{title}</h1>
  {#if isExpanded}
    <DamageChart data={events} />
  {/if}
</div>
```

### Event Handlers

Use callback props instead of custom events:

```svelte
<!-- Child.svelte -->
<script lang="ts">
  interface Props {
    onSelect: (id: string) => void;
  }
  let { onSelect }: Props = $props();
</script>

<button onclick={() => onSelect('123')}>Select</button>

<!-- Parent.svelte -->
<Child onSelect={(id) => console.log('Selected:', id)} />
```

## Tailwind Styling

Use Tailwind utility classes. Dark theme is default.

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

Page background: `bg-gray-900`. Card background: `bg-gray-800`.

## Chart Integration

Wrap Chart.js in Svelte components using `$effect` for lifecycle:

```svelte
<script lang="ts">
  import Chart from 'chart.js/auto';

  interface Props {
    data: number[];
  }
  let { data }: Props = $props();

  let canvas: HTMLCanvasElement;
  let chart: Chart | undefined;

  $effect(() => {
    chart = new Chart(canvas, {
      type: 'line',
      data: { datasets: [{ data }] },
      options: { responsive: true }
    });

    return () => chart?.destroy();
  });

  // Update chart when data changes
  $effect(() => {
    if (chart) {
      chart.data.datasets[0].data = data;
      chart.update();
    }
  });
</script>

<canvas bind:this={canvas}></canvas>
```

## WebSocket Client

```typescript
// websocket/client.svelte.ts
export let connectionStatus = $state<'disconnected' | 'connecting' | 'connected'>('disconnected');

let ws: WebSocket | null = null;

export function connect(url: string) {
  connectionStatus = 'connecting';

  ws = new WebSocket(url);

  ws.onopen = () => {
    connectionStatus = 'connected';
  };

  ws.onclose = () => {
    connectionStatus = 'disconnected';
    setTimeout(() => connect(url), 3000);
  };

  ws.onmessage = (event) => {
    const message = JSON.parse(event.data);
    handleMessage(message);
  };
}

export function disconnect() {
  ws?.close();
  ws = null;
}
```

## Development Commands

```bash
pnpm dev          # Start dev server
pnpm build        # Production build
pnpm preview      # Preview production build
pnpm check        # Type checking
pnpm lint         # Lint code
pnpm format       # Format code
```
