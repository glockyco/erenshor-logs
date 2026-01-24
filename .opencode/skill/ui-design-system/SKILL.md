---
name: ui-design-system
description: Classic MMO theme and component design patterns. Use when building UI components or styling screens.
---

# UI Design System

The Classic MMO visual design system. Read `/web/docs/style-guide.md` for
comprehensive theme documentation. This skill covers component patterns and
common implementation details.

## When to Follow the Style Guide

**Always follow:**
- Color palette (stone/amber primary, semantic colors for damage/healing)
- Typography (Cinzel fantasy headings, JetBrains Mono for numbers, Inter for UI)
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

  const baseStyles = "font-semibold rounded-lg transition-colors";

  const variantStyles = {
    primary: "bg-amber-500 text-stone-900 hover:bg-amber-400",
    secondary: "bg-stone-700 text-stone-100 hover:bg-stone-600",
    danger: "bg-rose-600 text-stone-100 hover:bg-rose-500"
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

## Semantic Colors

### Actor Types

Use consistent color classes for actor types:

```typescript
const actorColors = {
  player: 'text-amber-500',       // You (player character)
  sim_player: 'text-emerald-400', // Allies (SimPlayers)
  npc: 'text-rose-400',           // Enemies
  pet: 'text-violet-400'          // Companions/pets
};
```

### Damage & Healing

```typescript
const semanticColors = {
  damage: 'text-rose-400',   // Damage dealt/taken
  healing: 'text-lime-500',  // Healing done/received
  primary: 'text-amber-500', // Primary accent (DPS values, etc.)
  muted: 'text-stone-300'    // Secondary/contextual data
};
```

Never use arbitrary colors - these create a consistent visual language.

## Typography Components

Use the typography components from `$lib/components/ui/typography` instead of
manual classes:

```svelte
import { Heading, Numeric, Text } from '$lib/components/ui/typography';

<!-- Section header -->
<Heading variant="section">Combat Session</Heading>

<!-- Hero stat -->
<Numeric variant="hero" color="primary">{formatDps(900)}</Numeric>

<!-- Table data -->
<Numeric variant="medium" color="damage">{damage}</Numeric>
```

Always use `<Numeric>` for numbers - it applies `font-mono` and proper sizing.

## Fantasy Headings

Use Cinzel font for all headings and uppercase labels:

```svelte
<!-- Via component (recommended) -->
<Heading level={2} variant="section">Combat Session</Heading>

<!-- Via class (when component doesn't fit) -->
<div class="font-fantasy text-lg font-semibold text-amber-500">
  Section Header
</div>
```

## Common Mistakes

**Don't:**
- Use raw `font-mono` classes - use `<Numeric>` component instead
- Use raw `text-amber-500` - use component color props
- Mix semantic colors (lime for damage, rose for healing)
- Use light backgrounds (`bg-white`, `bg-stone-100`)
- Skip hover states on interactive elements

**Do:**
- Use typography components for consistency
- Use semantic color names via component props
- Apply subtle transitions (150-200ms)
- Use `font-fantasy` for uppercase labels and headers
- Keep consistent spacing from the Tailwind scale
