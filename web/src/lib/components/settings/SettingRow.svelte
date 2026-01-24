<script lang="ts">
  import type { Snippet } from "svelte";

  interface Props {
    label: string;
    helpText?: string;
    children: Snippet;
  }

  let { label, helpText, children }: Props = $props();
</script>

<div class="space-y-2">
  <div class="flex flex-col gap-1">
    <label class="text-sm font-medium uppercase tracking-wide text-stone-200">
      {label}
    </label>
    {#if helpText}
      <p class="text-xs text-stone-400">{helpText}</p>
    {/if}
  </div>
  <div>
    {@render children()}
  </div>
</div>
