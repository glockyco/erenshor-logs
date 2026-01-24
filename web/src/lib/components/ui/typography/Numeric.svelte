<script lang="ts">
  import { cn } from "$lib/utils";
  import type { Snippet } from "svelte";

  interface Props {
    variant?: "stat" | "large" | "medium" | "small";
    color?: "primary" | "damage" | "healing" | "muted" | "crit" | "hit" | "miss";
    as?: "span" | "div";
    class?: string;
    children: Snippet;
  }

  let {
    variant = "medium",
    color = "muted",
    as = "span",
    class: className,
    children,
  }: Props = $props();

  const sizeClasses = {
    stat: "text-4xl font-mono font-bold",
    large: "text-2xl font-mono font-semibold",
    medium: "text-base font-mono font-semibold",
    small: "text-sm font-mono",
  };

  const colorClasses = {
    primary: "text-amber-500",
    damage: "text-rose-400",
    healing: "text-lime-500",
    muted: "text-stone-300",
    crit: "text-amber-400",
    hit: "text-emerald-400",
    miss: "text-rose-400",
  };

  const Tag = as;
</script>

<svelte:element this={Tag} class={cn(sizeClasses[variant], colorClasses[color], className)}>
  {@render children()}
</svelte:element>
