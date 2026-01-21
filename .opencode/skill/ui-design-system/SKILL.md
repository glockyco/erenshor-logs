---
name: ui-design-system
description: Cyberpunk theme and component design patterns. Use when building UI components or styling screens.
---

# UI Design System

The Cyberpunk Analyst visual design system. Read `/web/docs/style-guide.md`
for comprehensive documentation. This skill covers common patterns and gotchas.

## When to Follow the Style Guide

**Always follow:**
- Color palette (actor types, damage types, backgrounds)
- Typography (Inter for UI, JetBrains Mono for numbers)
- Component structure (shadcn/ui compatible)
- Spacing scale (Tailwind default: 4, 6, 8, 12, 16, 24, 32)

**Discuss before deviating:**
- New accent colors
- Custom CSS (always prefer Tailwind utilities)
- New font families or sizes
- Breaking component conventions

## Component Variant Pattern

All UI components should support variants via props:

```svelte
<script lang="ts">
  import { cn } from '$lib/utils';

  interface Props {
    variant?: 'primary' | 'secondary' | 'danger';
    size?: 'sm' | 'md' | 'lg';
    class?: string;
  }

  let { variant = 'primary', size = 'md', class: className }: Props = $props();

  const baseStyles = "font-semibold rounded transition";

  const variantStyles = {
    primary: "bg-cyan-500 hover:bg-cyan-600 text-white glow-cyan-strong",
    secondary: "bg-slate-700 hover:bg-slate-600 text-white",
    danger: "bg-rose-500 hover:bg-rose-600 text-white"
  };

  const sizeStyles = {
    sm: "px-3 py-1.5 text-sm",
    md: "px-4 py-2 text-base",
    lg: "px-6 py-3 text-lg"
  };
</script>

<button class={cn(baseStyles, variantStyles[variant], sizeStyles[size], className)}>
  <slot />
</button>
```

## Actor Type Colors

Use consistent semantic classes:

```typescript
const actorColors = {
  player: 'text-cyan-400',      // You
  sim_player: 'text-emerald-400', // Allies
  npc: 'text-rose-400',          // Enemies
  pet: 'text-violet-400'         // Companions
};
```

Never use arbitrary colors for actor types - these are part of the visual
language users will learn.

## Glow Effects

Apply glows to active/interactive states:

```html
<!-- Connection status -->
<div class="w-2 h-2 bg-cyan-400 rounded-full glow-cyan animate-pulse" />

<!-- Active card -->
<div class="border border-cyan-500/30 glow-cyan hover:border-cyan-500/60" />
```

Custom glow utilities in `app.css`:

```css
.glow-cyan {
  box-shadow: 0 0 10px rgba(34, 211, 238, 0.4);
}

.glow-cyan-strong {
  box-shadow: 0 0 20px rgba(34, 211, 238, 0.6),
              0 0 40px rgba(34, 211, 238, 0.3);
}
```

## Monospace Numbers

Always use `font-mono` for numeric data:

```svelte
<!-- DPS display -->
<div class="text-2xl font-mono font-bold text-cyan-400">156</div>

<!-- Duration -->
<div class="font-mono">21.6s</div>

<!-- Damage amounts -->
<div class="font-mono text-lg">3,372</div>
```

This creates visual consistency and makes scanning data easier.

## Common Mistakes

**Don't:**
- Mix actor type colors (`text-blue-400` for player instead of `text-cyan-400`)
- Use light backgrounds (`bg-white`, `bg-gray-100`)
- Skip hover states on interactive elements
- Use arbitrary font sizes (use Tailwind scale)
- Overuse glow effects (only on key interactive elements)

**Do:**
- Use semantic color classes from the style guide
- Apply transitions to interactive elements (150-200ms)
- Use uppercase + `tracking-wider` for section headers
- Keep consistent spacing (gap-4, gap-6, p-4, p-6)
