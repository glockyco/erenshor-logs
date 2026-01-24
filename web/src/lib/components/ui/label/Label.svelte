<script lang="ts">
  import { cn } from "$lib/utils";
  import type { Snippet } from "svelte";

  interface Props {
    for?: string;
    required?: boolean;
    class?: string;
    children?: Snippet;
  }

  let { for: htmlFor, required = false, class: className, children }: Props = $props();
</script>

<label for={htmlFor} class={cn("block text-sm font-medium text-stone-300 mb-1.5", className)}>
  {@render children?.()}
  {#if required}
    <span class="text-rose-500 ml-0.5" aria-label="required">*</span>
  {/if}
</label>
