<script lang="ts">
  import type { FactionFilter } from "$lib/types";

  interface Props {
    active: FactionFilter;
    onFilterChange: (filter: FactionFilter) => void;
  }

  let { active, onFilterChange }: Props = $props();

  const filters: Array<{ value: FactionFilter; label: string }> = [
    { value: "all", label: "All" },
    { value: "friendly", label: "Friendly" },
    { value: "hostile", label: "Hostile" },
  ];
</script>

<div class="flex gap-2" role="group" aria-label="Faction filter">
  {#each filters as filter (filter.value)}
    <button
      type="button"
      class="px-3 py-1.5 text-xs font-medium rounded transition-colors"
      class:bg-slate-700={active === filter.value}
      class:text-slate-200={active === filter.value}
      class:bg-slate-800={active !== filter.value}
      class:text-slate-400={active !== filter.value}
      class:hover:bg-slate-700={active !== filter.value}
      class:hover:text-slate-200={active !== filter.value}
      aria-pressed={active === filter.value}
      onclick={() => onFilterChange(filter.value)}
    >
      {filter.label}
    </button>
  {/each}
</div>
