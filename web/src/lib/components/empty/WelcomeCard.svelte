<script lang="ts">
  import { cn } from "$lib/utils";
  import type { Component } from "svelte";

  interface Props {
    title: string;
    icon: Component;
    class?: string;
    children: import("svelte").Snippet;
  }

  let { title, icon: Icon, class: className, children }: Props = $props();
</script>

<div
  class={cn(
    "rounded-lg border-2 border-stone-700 bg-stone-800 p-6 shadow-lg",
    "transition-all hover:border-amber-600/50",
    className
  )}
>
  <!-- Icon -->
  <div class="mb-4 flex justify-center">
    <Icon class="h-12 w-12 text-amber-500" aria-hidden="true" />
  </div>

  <!-- Title -->
  <h2 class="mb-4 text-center font-fantasy text-xl font-bold text-amber-500">
    {title}
  </h2>

  <!-- Content -->
  <div class="space-y-4">
    {@render children()}
  </div>
</div>
