<script lang="ts">
  import type { Snippet } from "svelte";

  interface Props {
    title?: string;
    actions?: Snippet;
    children: Snippet;
  }

  let { title, actions, children }: Props = $props();
</script>

<div class="rounded-lg border border-slate-700 bg-slate-900">
  {#if title || actions}
    <div class="flex items-center justify-between border-b border-slate-800 px-6 py-4">
      {#if title}
        <h3 class="text-sm font-semibold uppercase tracking-wider text-slate-300">
          {title}
        </h3>
      {/if}
      {#if actions}
        <div class="flex items-center gap-2">
          {@render actions()}
        </div>
      {/if}
    </div>
  {/if}
  <div class="p-6">
    {@render children()}
  </div>
</div>
