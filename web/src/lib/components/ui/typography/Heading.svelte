<script lang="ts">
  import { cn } from "$lib/utils";
  import type { Snippet } from "svelte";

  interface Props {
    level?: 1 | 2 | 3 | 4 | 5 | 6;
    variant?: "app" | "section" | "label";
    class?: string;
    children: Snippet;
  }

  let { level = 3, variant = "section", class: className, children }: Props = $props();

  const variantClasses = {
    app: "font-fantasy text-4xl font-bold text-amber-500",
    section: "font-fantasy text-lg font-semibold text-amber-500",
    label: "text-sm font-medium uppercase tracking-wider text-stone-400",
  };

  const Tag = `h${level}` as const;
</script>

<svelte:element this={Tag} class={cn(variantClasses[variant], className)}>
  {@render children()}
</svelte:element>
