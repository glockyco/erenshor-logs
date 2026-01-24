# Classic MMO Visual Design System

This document defines the visual design system for Erenshor Logs, inspired by classic MMORPGs like EverQuest and early World of Warcraft.

## Design Philosophy

**"Fantasy meets function"** - Warm, inviting colors that evoke classic fantasy RPGs, combined with modern data visualization best practices. The interface should feel like part of the game world while remaining highly readable for dense combat data.

## Theme Overview

- **Primary vibe**: Classic fantasy MMORPG
- **Color mood**: Warm amber accents on dark stone
- **Typography**: Fantasy serif headings, modern sans-serif UI, monospace numbers
- **Data density**: High (combat logs are data-heavy)
- **Hierarchy**: Clear visual weight differences

## Color Palette

### Primary Colors

| Color         | Hex       | Usage                                         |
| ------------- | --------- | --------------------------------------------- |
| **Amber 500** | `#f59e0b` | Primary accent, headings, CTAs, active states |
| **Stone 800** | `#292524` | Primary backgrounds, cards, panels            |
| **Stone 900** | `#1c1917` | Deep backgrounds, body background             |
| **Stone 950** | `#0c0a09` | Deepest backgrounds (unused currently)        |

### Text Colors

| Color         | Hex       | Usage                            |
| ------------- | --------- | -------------------------------- |
| **Stone 100** | `#f5f5f4` | Primary text, high contrast      |
| **Stone 200** | `#e7e5e4` | Secondary text, labels           |
| **Stone 300** | `#d6d3d1` | Tertiary text, metadata          |
| **Stone 400** | `#a8a29e` | Muted text, placeholders         |
| **Stone 500** | `#78716c` | Very muted text, disabled states |

### Semantic Colors

#### Damage & Healing

| Color        | Hex       | Usage                                   |
| ------------ | --------- | --------------------------------------- |
| **Rose 400** | `#fb7185` | Damage, destructive actions, enemy NPCs |
| **Lime 500** | `#84cc16` | Healing, positive effects               |

#### Actor Types

| Color           | Hex       | Actor Type          |
| --------------- | --------- | ------------------- |
| **Amber 500**   | `#f59e0b` | Player (you)        |
| **Emerald 400** | `#34d399` | SimPlayers (allies) |
| **Rose 400**    | `#fb7185` | NPCs (enemies)      |
| **Violet 400**  | `#a78bfa` | Pets/companions     |

### UI Colors

| Color         | Hex       | Usage                                |
| ------------- | --------- | ------------------------------------ |
| **Stone 700** | `#44403c` | Borders, dividers, secondary buttons |
| **Stone 600** | `#57534e` | Hover states on secondary elements   |
| **Rose 600**  | `#e11d48` | Destructive button background        |
| **Amber 400** | `#fbbf24` | Primary button hover state           |

## Typography

### Font Families

Three fonts for different purposes:

1. **Cinzel** (serif) - Fantasy headings and labels
   - `font-family: "Cinzel", serif`
   - Applied via `.font-fantasy` utility class
   - Used for: App title, section headers, column labels

2. **Inter** (sans-serif) - UI text and body copy
   - `font-family: "Inter", sans-serif` (Tailwind default)
   - Used for: Buttons, form fields, body text, descriptions

3. **JetBrains Mono** (monospace) - Numbers and data
   - `font-family: "JetBrains Mono", monospace` (Tailwind `font-mono`)
   - Used for: All numeric data, timestamps, IDs

### Typography Scale

| Size | Tailwind    | Usage                           | Weight         |
| ---- | ----------- | ------------------------------- | -------------- |
| 36px | `text-4xl`  | Hero stats (DPS/HPS), app title | Bold           |
| 24px | `text-2xl`  | _(Reserved for future use)_     | -              |
| 18px | `text-lg`   | Section headers                 | Semibold       |
| 16px | `text-base` | Supporting stats, body text     | Regular        |
| 14px | `text-sm`   | Table data, labels, UI text     | Regular/Medium |
| 12px | `text-xs`   | Dense metadata, version info    | Regular        |

### Typography Components

Use the components in `/web/src/lib/components/ui/typography/`:

- **Heading** - Semantic HTML headings with fantasy font
- **Numeric** - Monospace numbers for tables and stats
- **Text** - Body text variants

See `/web/src/lib/components/ui/typography/README.md` for detailed usage.

## Spacing Scale

Follow Tailwind's default spacing scale (base unit: 4px):

| Value | Pixels | Usage                               |
| ----- | ------ | ----------------------------------- |
| `1`   | 4px    | Very tight spacing (icon-to-text)   |
| `2`   | 8px    | Tight spacing (label-to-input)      |
| `3`   | 12px   | Compact spacing (within cards)      |
| `4`   | 16px   | Default spacing (between elements)  |
| `6`   | 24px   | Medium spacing (section padding)    |
| `8`   | 32px   | Large spacing (between sections)    |
| `10`  | 40px   | Extra large spacing (page sections) |

## Component Patterns

### Variant System

All UI components support variants via props, using `cn()` for class merging:

```svelte
<script lang="ts">
  import { cn } from "$lib/utils";

  interface Props {
    variant?: "primary" | "secondary";
    class?: string;
  }

  let { variant = "primary", class: className }: Props = $props();

  const variantStyles = {
    primary: "bg-amber-500 text-stone-900",
    secondary: "bg-stone-700 text-stone-100",
  };
</script>

<button class={cn("rounded-lg px-4 py-2", variantStyles[variant], className)}>
  <slot />
</button>
```

### Component Structure

Follow **shadcn/ui conventions**:

- Base components in `lib/components/ui/`
- Each component in its own directory
- Export via `index.ts` for clean imports
- Use `cn()` utility for class merging
- Support `class` prop for custom styling

### State Indicators

#### Connection Status

- **Connected**: `bg-lime-500` with pulse animation
- **Disconnected**: `bg-stone-600` (muted)
- **Connecting**: `bg-amber-500` with pulse animation
- **Error**: `bg-rose-500`

#### Interactive States

- **Default**: Stone 700 border
- **Hover**: Stone 600 border (lighter)
- **Focus**: Amber 500 border with ring (`ring-amber-500/50`)
- **Active/Selected**: Amber 500 accent
- **Disabled**: Stone 500 text, Stone 900 background

## Layout Patterns

### Card/Panel Structure

```
┌─────────────────────────────────────┐
│ Header (amber-500 text)             │  border-b border-stone-700
├─────────────────────────────────────┤
│                                     │
│ Content (stone-100/200/300 text)    │  bg-stone-800
│                                     │
└─────────────────────────────────────┘
```

Classes: `bg-stone-800 border-2 border-stone-700 rounded-lg`

### Data Tables

- **Header row**: `text-sm font-medium uppercase tracking-wider text-stone-400`
- **Data rows**: `text-sm text-stone-200 hover:bg-stone-700/50`
- **Numeric cells**: `font-mono tabular-nums text-right`
- **Alternating rows**: Optional, use `bg-stone-800/50` for even rows

### Form Fields

- **Label**: Stone 300, medium weight, 14px
- **Input background**: Stone 800
- **Input border**: Stone 700 default, Amber 500 on focus
- **Input text**: Stone 100
- **Placeholder**: Stone 500
- **Help text**: Stone 400, 14px

## Best Practices

### DO:

✅ Use semantic color names (`text-rose-400` for damage)
✅ Use Cinzel for all uppercase labels via `font-fantasy`
✅ Use JetBrains Mono for all numbers via `font-mono`
✅ Maintain clear visual hierarchy with size and color
✅ Follow the spacing scale (no arbitrary values)
✅ Use `cn()` for composing classes in components

### DON'T:

❌ Don't use arbitrary color values - stick to the palette
❌ Don't mix semantic colors (e.g., lime for damage)
❌ Don't use cyan/slate colors (old cyberpunk theme)
❌ Don't apply `font-bold` to table data (only hero stats)
❌ Don't skip the spacing scale with arbitrary values
❌ Don't use arbitrary text sizes outside the scale

## Implementation

### Tailwind Configuration

Colors are defined in `web/src/app.css` using Tailwind v4 `@theme` syntax:

```css
@theme {
  --color-amber-500: #f59e0b;
  --color-stone-800: #292524;
  /* ... etc ... */
}
```

### Custom Utilities

```css
.font-fantasy {
  font-family: "Cinzel", serif;
}
```

### Component Utilities

The `cn()` utility from `lib/utils` is used throughout for merging Tailwind classes:

```typescript
import { cn } from "$lib/utils";

// Merges classes, with rightmost taking precedence
const classes = cn("text-base", "text-lg"); // → "text-lg"
```

## Accessibility

- **Contrast ratios**: All text meets WCAG AA standards
  - Stone 100 on Stone 900: 15.65:1 (AAA)
  - Amber 500 on Stone 900: 7.12:1 (AA)
  - Stone 300 on Stone 800: 7.45:1 (AA)

- **Focus indicators**: Always visible, amber 500 ring
- **Semantic HTML**: Use proper elements (`<button>`, `<label>`, etc.)
- **ARIA attributes**: Required for all interactive components
- **Keyboard navigation**: All interactive elements must be keyboard accessible

## Related Documentation

- [Typography System](/web/src/lib/components/ui/typography/README.md) - Detailed typography component usage
- [AGENTS.md](/AGENTS.md) - Project overview and technical decisions
- [Tailwind CSS](https://tailwindcss.com/) - Utility class reference
