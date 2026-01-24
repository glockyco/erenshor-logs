<script lang="ts">
  import { cn } from "$lib/utils";
  import type { Snippet } from "svelte";

  interface Props {
    variant?: "body" | "large" | "small" | "muted" | "caption" | "error" | "success" | "warning";
    as?: "p" | "span" | "div";
    class?: string;
    children: Snippet;
  }

  let { variant = "body", as = "p", class: className, children }: Props = $props();

  const variantClasses = {
    body: "text-base text-stone-200",
    large: "text-lg text-stone-100",
    small: "text-sm text-stone-300",
    muted: "text-sm text-stone-400",
    caption: "text-xs text-stone-500",
    error: "text-sm text-rose-400",
    success: "text-sm text-green-400",
    warning: "text-sm text-amber-400",
  };

  const Tag = as;
</script>

<svelte:element this={Tag} class={cn(variantClasses[variant], className)}>
  {@render children()}
</svelte:element>
