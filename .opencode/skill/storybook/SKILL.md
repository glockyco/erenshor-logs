---
name: storybook
description: Component story conventions. Use when creating or updating Storybook stories.
---

# Storybook

Conventions for writing component stories with Storybook 10 and Svelte 5.

## File Location

Stories live alongside components:

```
ComponentName.svelte
ComponentName.stories.svelte
```

## Story Structure

Use `@storybook/addon-svelte-csf` with Svelte 5 snippet syntax:

```svelte
<script module>
  import { defineMeta } from "@storybook/addon-svelte-csf";
  import MyComponent from "./MyComponent.svelte";

  const { Story } = defineMeta({
    title: "Category/MyComponent",
    component: MyComponent,
    tags: ["autodocs"],
  });
</script>

<Story name="Default">
  {#snippet template(_args)}
    <div class="bg-slate-950 p-6 rounded-lg">
      <MyComponent />
    </div>
  {/snippet}
</Story>

<Story name="With Props">
  {#snippet template(_args)}
    <div class="bg-slate-950 p-6 rounded-lg">
      <MyComponent variant="secondary" disabled />
    </div>
  {/snippet}
</Story>
```

## Category Naming

Use consistent categories matching component directories:

- `UI/Button`, `UI/Card`, `UI/Badge`, `UI/StatBlock`
- `Status/ConnectionStatus`
- `Layout/Header`, `Layout/ResponsiveLayout`

## Testing Component States

For components that read from state modules, pass props to override:

```svelte
<!-- ConnectionStatus accepts optional status prop -->
<Story name="Connected">
  {#snippet template(_args)}
    <div class="bg-slate-950 p-6 rounded-lg">
      <ConnectionStatus status="connected" />
    </div>
  {/snippet}
</Story>
```

Design components to accept optional props that override global state for
testability.

## Dark Background Wrapper

Always wrap stories in dark background to match the app theme:

```svelte
<div class="bg-slate-950 p-6 rounded-lg">
  <!-- Component -->
</div>
```

Without this, components designed for dark theme will be invisible or look wrong
on Storybook's default light canvas.

## Commands

```bash
pnpm --dir web storybook       # Dev server (port 6006)
pnpm --dir web build-storybook # Build static site
```

## Common Pitfalls

**`effect_orphan` error**: State modules with bare `$effect()` at module level
break in Storybook. Use `$effect.root()` pattern (see `svelte-web-development`
skill).

**Missing dark background**: Components designed for dark theme look wrong on
Storybook's default light canvas. Always wrap in `bg-slate-950`.

**State not resetting between stories**: Each story shares module state. Pass
props to override for isolated testing, or design components to accept optional
props that bypass state.
