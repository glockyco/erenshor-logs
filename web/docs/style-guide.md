# Cyberpunk Analyst - Style Guide

High-tech data analysis aesthetic with neon accents and monospace typography.

## Color Palette

### Core Colors

- **Background**: `slate-950` (#020617)
- **Surface**: `slate-900` (#0f172a)
- **Surface Elevated**: `slate-800` (#1e293b)
- **Border**: `slate-700` / `cyan-900`

### Accent Colors

- **Primary**: `cyan-400` (#22d3ee) - Player actions, key metrics
- **Success**: `emerald-400` (#34d399) - SimPlayers, positive states
- **Danger**: `rose-400` (#fb7185) - NPCs, errors, warnings
- **Info**: `violet-400` (#a78bfa) - Pets, secondary info

### Actor Type Colors

- **Player**: `cyan-400` (#22d3ee)
- **SimPlayer**: `emerald-400` (#34d399)
- **NPC**: `rose-400` (#fb7185)
- **Pet**: `violet-400` (#a78bfa)

### Damage Type Colors

- **Physical**: `orange-500` (#f97316)
- **Magic**: `violet-500` (#8b5cf6)
- **Elemental**: `yellow-500` (#eab308)
- **Void**: `indigo-500` (#6366f1)
- **Poison**: `lime-500` (#84cc16)

## Typography

### Fonts

- **UI/Headers**: Inter (sans-serif) via Google Fonts
- **Numbers/Data**: JetBrains Mono (monospace) via Google Fonts

### Font Sizes

- **Display**: `text-3xl` (30px) - Page titles
- **Header**: `text-xl` or `text-2xl` - Section headers
- **Body**: `text-base` (16px) - Default
- **Small**: `text-sm` (14px) - Labels, metadata
- **Tiny**: `text-xs` (12px) - Timestamps, session IDs

### Font Weights

- **Bold**: `font-bold` (700) - Headers, emphasis
- **Semibold**: `font-semibold` (600) - Subheaders
- **Regular**: `font-normal` (400) - Body text
- **Mono Bold**: `font-mono font-bold` - Key numbers

### Text Styles

- Headers: Uppercase + `tracking-wider` (letter-spacing)
- Session IDs: `font-mono text-xs text-slate-500`
- Numbers: Always `font-mono` for alignment

## Visual Effects

### Glow Effects

```css
.glow-cyan {
  box-shadow: 0 0 10px rgba(34, 211, 238, 0.4);
}

.glow-cyan-strong {
  box-shadow:
    0 0 20px rgba(34, 211, 238, 0.6),
    0 0 40px rgba(34, 211, 238, 0.3);
}
```

**Apply to:**

- Active/selected states
- Connection status indicators
- Hover states on interactive elements

### Borders

- **Default**: `border border-slate-700`
- **Active**: `border border-cyan-500/30` + glow
- **Hover**: `border-cyan-500/60 transition`

### Backgrounds

- **Card**: `bg-slate-900`
- **Card Hover**: `hover:bg-slate-900/80`
- **Input**: `bg-slate-800`
- **Header/Sticky**: `bg-slate-900/80 backdrop-blur`

### Animations

- **Connection Pulse**: 2s ease-in-out infinite
- **Hover Transitions**: 150ms ease
- **Border Glow**: Subtle, not distracting
- **Scanlines** (optional): Very subtle, `opacity-[0.02]`

## Components

### Buttons

```svelte
<!-- Primary -->
<button
  class="px-4 py-2 bg-cyan-500 hover:bg-cyan-600 text-white font-semibold rounded transition glow-cyan-strong"
>
  Action
</button>

<!-- Secondary/Danger -->
<button class="px-3 py-1.5 text-rose-400 hover:text-rose-300 transition"> Delete </button>
```

### Cards

```svelte
<div
  class="bg-slate-900 border border-cyan-900/50 rounded-lg p-4 hover:border-cyan-500/60 transition"
>
  <!-- Content -->
</div>
```

### Connection Status

- **Dot**: 8px circle with pulse animation
- **Colors**: cyan (connected), yellow (connecting), red (disconnected)
- **Position**: Top-right header

### Session Cards

- Border-left indicator for active session
- Glow effect on active/selected
- Monospace for duration/DPS numbers
- Trash icon: subtle until hover

## Layout

### Spacing

- **Section Padding**: `p-8` (32px)
- **Card Padding**: `p-4` or `p-6` (16/24px)
- **Element Gaps**: `gap-4` or `gap-6`
- **Tight Spacing**: `gap-2` or `gap-3`

### Grid System

- **Sidebar + Main**: `grid-cols-1 lg:grid-cols-3`
  - Sidebar: 1 column
  - Main: 2 columns
- **Stats Grid**: `grid-cols-2` or `grid-cols-4`

### Responsive

- **Mobile**: Single column stack
- **Tablet**: Consider sidebar collapse
- **Desktop**: Full layout

## Accessibility

### Contrast

- All text meets WCAG AA (4.5:1 minimum)
- Cyan on dark backgrounds: Good contrast
- Use `text-slate-400` for secondary text

### Focus States

- Visible focus ring: `focus:ring-2 focus:ring-cyan-400`
- Keyboard navigation support

### ARIA

- Label connection status
- Accessible table markup for damage breakdown
- Screen reader friendly session list

## Do's and Don'ts

### Do

✅ Use monospace for all numeric data  
✅ Uppercase headers with letter-spacing  
✅ Glows on active/interactive elements  
✅ Consistent actor type colors  
✅ Smooth transitions (150-200ms)

### Don't

❌ Mix too many accent colors  
❌ Overuse glow effects (becomes distracting)  
❌ Use light text on light backgrounds  
❌ Forget hover states  
❌ Use arbitrary font sizes (stick to Tailwind scale)

## Examples

See `/web/demos/demo-cyberpunk.html` for full implementation reference.
