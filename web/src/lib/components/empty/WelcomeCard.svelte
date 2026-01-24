<script lang="ts">
  import { cn } from "$lib/utils";
  import { Heading } from "$lib/components/ui/typography";
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
  <Heading level={2} variant="section" class="mb-4 text-center text-xl">
    {title}
  </Heading>

  <!-- Content -->
  <div class="space-y-4">
    {@render children()}
  </div>
</div>
