<script lang="ts">
  import { PanelLeftClose, PanelLeftOpen } from "@lucide/svelte";

  interface Props {
    isCollapsed: boolean;
    ontoggle: () => void;
  }

  let { isCollapsed, ontoggle }: Props = $props();
</script>

<button
  type="button"
  class="flex h-8 w-8 cursor-pointer items-center justify-center rounded-md hover:bg-stone-700 transition-colors {isCollapsed
    ? 'mx-auto'
    : ''}"
  onclick={ontoggle}
  title={isCollapsed ? "Expand sidebar (Cmd+B)" : "Collapse sidebar (Cmd+B)"}
  aria-label={isCollapsed ? "Expand sidebar" : "Collapse sidebar"}
  aria-expanded={!isCollapsed}
>
  {#if isCollapsed}
    <PanelLeftOpen class="h-4 w-4" />
  {:else}
    <PanelLeftClose class="h-4 w-4" />
  {/if}
</button>
