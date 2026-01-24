<script lang="ts">
  import { cn } from "$lib/utils";
  import type { Snippet } from "svelte";

  interface Props {
    level?: 1 | 2 | 3 | 4 | 5 | 6;
    variant?: "display" | "section" | "subsection" | "label";
    class?: string;
    children: Snippet;
  }

  let { level = 3, variant = "section", class: className, children }: Props = $props();

  const variantClasses = {
    display: "font-fantasy text-2xl font-bold text-amber-500",
    section: "font-fantasy text-lg font-semibold text-amber-500",
    subsection: "font-fantasy text-base font-semibold text-amber-500",
    label: "text-sm font-semibold uppercase tracking-wider text-amber-500",
  };

  const Tag = `h${level}` as const;
</script>

<svelte:element this={Tag} class={cn(variantClasses[variant], className)}>
  {@render children()}
</svelte:element>
