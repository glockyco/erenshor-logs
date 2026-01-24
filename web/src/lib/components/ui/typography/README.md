# Typography System

Erenshor Logs uses a 6-level typography scale based on Tailwind CSS defaults, optimized for data-dense combat log analysis.

## Design Principles

1. **Size provides hierarchy** - Use smaller/larger sizes to show importance, not just decoration
2. **Weight is minimal** - Bold reserved for hero elements only, prevents visual fatigue
3. **Consistency over flexibility** - Use components instead of manual classes
4. **Semantic naming** - Component variants describe purpose, not just appearance

## Typography Scale

| Level | Size               | Weight         | Usage                       | Examples                            |
| ----- | ------------------ | -------------- | --------------------------- | ----------------------------------- |
| 6     | 36px (`text-4xl`)  | Bold           | App branding, hero metrics  | "ERENSHOR LOGS", "900 DPS"          |
| 5     | 24px (`text-2xl`)  | -              | _(Reserved for future use)_ | -                                   |
| 4     | 18px (`text-lg`)   | Semibold       | Section headers             | "Combat Session", "Actor Breakdown" |
| 3     | 16px (`text-base`) | Regular        | Supporting data             | Total damage (9,239), Duration      |
| 2     | 14px (`text-sm`)   | Regular/Medium | Primary data, labels        | Table data, column headers          |
| 1     | 12px (`text-xs`)   | Regular        | _(Reserved for future use)_ | Very dense metadata                 |

## Components

### Heading

Semantic HTML headings (`<h1>` through `<h6>`) with three variants.

**Variants:**

- `app` - Application title (36px bold, fantasy font, amber-500)
- `section` - Section headers (18px semibold, fantasy font, amber-500)
- `label` - Column headers, field labels (14px medium, uppercase, stone-400)

**Props:**

- `level` - HTML heading level (1-6), default: 3
- `variant` - Visual style, default: "section"
- `class` - Additional CSS classes (optional)

**Usage:**

```svelte
<Heading level={1} variant="app">Erenshor Logs</Heading>
<Heading level={2} variant="section">Combat Session</Heading>
<Heading level={6} variant="label">Total Damage</Heading>
```

**Accessibility notes:**

- Always use semantic heading levels (don't skip levels)
- Only one `<h1>` per page
- Heading levels create document structure for screen readers

---

### Numeric

Monospace numbers for tabular data and statistics. All variants use `font-mono` for proper alignment.

**Variants:**

- `hero` - Hero stats only (36px bold) - The 3 main DPS/DTPS/HPS numbers
- `large` - Supporting stats (16px regular) - Totals, duration, summary data
- `medium` - Main table data (14px regular) - Actor table rows
- `small` - Detail tables (14px regular) - Ability breakdown (same as medium)

**Colors:**

- `primary` (amber-500) - Key metrics (DPS values)
- `damage` (rose-400) - Damage-related metrics
- `healing` (lime-500) - Healing-related metrics
- `muted` (stone-300) - Secondary/contextual data
- `crit` (amber-400) - Critical hit counts
- `hit` (emerald-400) - Regular hit counts
- `miss` (rose-400) - Miss counts

**Props:**

- `variant` - Size variant, default: "medium"
- `color` - Semantic color, default: "muted"
- `as` - HTML element ("span" | "div"), default: "span"
- `class` - Additional CSS classes (optional)

**Usage:**

```svelte
<!-- Hero stats (DPS/DTPS/HPS) -->
<Numeric variant="hero" color="primary">{formatDps(900)}</Numeric>

<!-- Supporting stats -->
<Numeric variant="large" color="muted">{formatNumber(9239)}</Numeric>

<!-- Main table data -->
<Numeric variant="medium" color="primary">{formatDps(451)}</Numeric>

<!-- Detail tables -->
<Numeric variant="small" color="crit">{ability.crits}</Numeric>
```

---

### Text

Body text variants for prose and UI labels. Currently minimal usage in this project.

**Variants:**

- `body` - Standard paragraphs (16px, stone-200)
- `small` - Secondary text (14px, stone-300)
- `muted` - Tertiary text (14px, stone-400)

**Props:**

- `variant` - Text variant, default: "body"
- `as` - HTML element ("p" | "span" | "div"), default: "p"
- `class` - Additional CSS classes (optional)

**Usage:**

```svelte
<Text variant="body">Main body text</Text>
<Text variant="small">Secondary information</Text>
<Text variant="muted" as="span">Muted helper text</Text>
```

## Visual Hierarchy

The typography system enforces a clear visual hierarchy:

1. **App Title** (Level 6) - Largest, establishes brand identity
2. **Hero Stats** (Level 6) - Equal prominence, the star of the show
3. **Section Headers** (Level 4) - Organize content into logical sections
4. **Supporting Data** (Level 3) - Contextual metrics below hero stats
5. **Primary Data** (Level 2) - Main table content, scannable at a glance
6. **Labels** (Level 2) - Descriptive text, subordinate to data
7. **Detail Data** (Level 1) - Dense information, compact but readable

## Migration Guide

### From raw classes to components

```diff
<!-- Headings -->
- <h1 class="text-4xl font-fantasy font-bold text-amber-500">Title</h1>
+ <Heading level={1} variant="app">Title</Heading>

- <h2 class="text-lg font-fantasy font-semibold text-amber-500">Section</h2>
+ <Heading level={2} variant="section">Section</Heading>

- <div class="text-xs uppercase tracking-wider text-stone-400">Label</div>
+ <Heading level={6} variant="label">Label</Heading>

<!-- Numeric data -->
- <span class="text-4xl font-mono font-bold text-amber-500">900</span>
+ <Numeric variant="hero" color="primary">900</Numeric>

- <span class="text-base font-mono text-stone-300">9,239</span>
+ <Numeric variant="large" color="muted">9,239</Numeric>

- <span class="text-sm font-mono text-amber-500">451</span>
+ <Numeric variant="medium" color="primary">451</Numeric>
```

### From old variants (Breaking Changes)

```diff
<!-- Heading variants renamed -->
- <Heading variant="display">Title</Heading>
+ <Heading variant="app">Title</Heading>

<!-- Numeric variants renamed and resized -->
- <Numeric variant="stat" color="primary">900</Numeric>
+ <Numeric variant="hero" color="primary">900</Numeric>

<!-- Note: large and medium variants now render smaller -->
- <Numeric variant="large">...</Numeric>  <!-- Was text-2xl (24px) -->
+ <Numeric variant="large">...</Numeric>  <!-- Now text-base (16px) -->

- <Numeric variant="medium">...</Numeric>  <!-- Was text-base (16px) -->
+ <Numeric variant="medium">...</Numeric>  <!-- Now text-sm (14px) -->
```

## Best Practices

### DO:

✅ Use `<Heading>` for all uppercase labels (column headers, field labels)  
✅ Use `<Numeric>` for all numbers in tables and stats  
✅ Use semantic colors (`primary`, `damage`, `healing`) instead of raw Tailwind colors  
✅ Let the component handle sizing - avoid manual `text-*` classes  
✅ Use proper heading levels for document structure

### DON'T:

❌ Don't use raw `font-mono` classes - use `<Numeric>` instead  
❌ Don't use raw `text-amber-500` - use component colors  
❌ Don't use `font-bold` on table data - only hero stats are bold  
❌ Don't skip heading levels (e.g., h1 → h4)  
❌ Don't manually add `uppercase tracking-wider` - use `label` variant

## Line Height Considerations

- **Hero stats (36px)**: Default tight line-height for visual impact
- **Section headers (18px)**: Default for single-line headers
- **Table data (14px)**: Default for dense data presentation

All table variants (`medium` and `small`) use 14px for consistency and readability.

## Future Enhancements

Potential additions as the project evolves:

- **Metric variant** for emphasized numbers (e.g., large statistics on dashboards)
- **Code variant** for inline code or technical identifiers
- **Link styles** for hyperlinks within text blocks
- **Error/Warning/Success text variants** for semantic messaging

## Related Documentation

- [Tailwind Typography Docs](https://tailwindcss.com/docs/font-size)
- [WCAG Typography Guidelines](https://www.w3.org/WAI/WCAG21/Understanding/text-spacing.html)
- [Classic MMO Theme](/web/docs/style-guide.md) (if created)
