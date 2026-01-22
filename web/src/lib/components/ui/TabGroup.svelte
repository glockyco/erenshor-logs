<script lang="ts" generics="T extends string">
  interface Tab<T> {
    value: T;
    label: string;
  }

  interface Props {
    tabs: Tab<T>[];
    active: T;
    onTabChange: (value: T) => void;
  }

  let { tabs, active, onTabChange }: Props = $props();
</script>

<div class="flex gap-1 border-b border-slate-800" role="tablist">
  {#each tabs as tab (tab.value)}
    <button
      type="button"
      role="tab"
      aria-selected={active === tab.value}
      aria-controls="{tab.value}-panel"
      class="px-4 py-2 text-sm font-medium transition-colors relative"
      class:text-cyan-400={active === tab.value}
      class:text-slate-400={active !== tab.value}
      class:hover:text-slate-200={active !== tab.value}
      onclick={() => onTabChange(tab.value)}
    >
      {tab.label}
      {#if active === tab.value}
        <div class="absolute bottom-0 left-0 right-0 h-0.5 bg-cyan-400"></div>
      {/if}
    </button>
  {/each}
</div>
