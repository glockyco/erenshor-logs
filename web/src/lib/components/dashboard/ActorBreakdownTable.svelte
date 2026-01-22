<script lang="ts">
  import type {
    ActorStats,
    SortBy,
    SortDirection,
    ActorType,
    ActorBreakdownTab,
    FactionFilter,
  } from "$lib/types";
  import Badge from "$lib/components/ui/Badge.svelte";
  import TabGroup from "$lib/components/ui/TabGroup.svelte";
  import TabSelect from "$lib/components/ui/TabSelect.svelte";
  import FactionFilterComponent from "$lib/components/ui/FactionFilter.svelte";
  import { formatDps, formatNumber, formatPercent } from "$lib/utils";
  import { filterByFaction } from "$lib/utils/actor-utils";
  import { ChevronUp, ChevronDown } from "@lucide/svelte";

  interface Props {
    actors: ActorStats[];
    activeTab: ActorBreakdownTab;
    factionFilter: FactionFilter;
    sortBy: SortBy;
    sortDirection: SortDirection;
    onTabChange: (tab: ActorBreakdownTab) => void;
    onFactionChange: (filter: FactionFilter) => void;
    onSort: (field: SortBy) => void;
  }

  let {
    actors,
    activeTab,
    factionFilter,
    sortBy,
    sortDirection,
    onTabChange,
    onFactionChange,
    onSort,
  }: Props = $props();

  // Tab configuration
  const tabs = [
    { value: "damageDealt" as const, label: "Damage Dealt" },
    { value: "damageTaken" as const, label: "Damage Taken" },
    { value: "healingDone" as const, label: "Healing Done" },
    { value: "healingReceived" as const, label: "Healing Received" },
  ];

  // Column configuration by tab
  type ColumnConfig = {
    rateField: keyof ActorStats;
    rateLabel: string;
    rateSortKey: SortBy;
    totalField: keyof ActorStats;
    totalLabel: string;
    totalSortKey: SortBy;
    percentField: keyof ActorStats;
    percentLabel: string;
    extraColumns?: Array<{
      field: keyof ActorStats;
      label: string;
      sortKey?: SortBy;
      format: (value: number) => string;
    }>;
  };

  const columnsByTab: Record<ActorBreakdownTab, ColumnConfig> = {
    damageDealt: {
      rateField: "dps",
      rateLabel: "DPS",
      rateSortKey: "dps",
      totalField: "totalDamage",
      totalLabel: "Total",
      totalSortKey: "damage",
      percentField: "percentOfTotalDamage",
      percentLabel: "% of Total",
    },
    damageTaken: {
      rateField: "dtps",
      rateLabel: "DTPS",
      rateSortKey: "dtps",
      totalField: "damageTaken",
      totalLabel: "Total",
      totalSortKey: "damageTaken",
      percentField: "percentOfTotalDamageTaken",
      percentLabel: "% of Total",
      extraColumns: [
        {
          field: "totalMitigated",
          label: "Mitigated",
          format: formatNumber,
        },
        {
          field: "mitigationRate",
          label: "Mit%",
          format: (val) => `${val.toFixed(1)}%`,
        },
        {
          field: "totalMissedAgainst",
          label: "Avoided",
          format: (val) => val.toString(),
        },
        {
          field: "avoidanceRate",
          label: "Avoid%",
          format: (val) => `${val.toFixed(1)}%`,
        },
      ],
    },
    healingDone: {
      rateField: "hps",
      rateLabel: "HPS",
      rateSortKey: "hps",
      totalField: "totalHealing",
      totalLabel: "Total",
      totalSortKey: "healing",
      percentField: "percentOfTotalHealing",
      percentLabel: "% of Total",
    },
    healingReceived: {
      rateField: "hrps",
      rateLabel: "HRPS",
      rateSortKey: "hrps",
      totalField: "healingReceived",
      totalLabel: "Total",
      totalSortKey: "healingReceived",
      percentField: "percentOfTotalHealingReceived",
      percentLabel: "% of Total",
    },
  };

  // Current column configuration
  const columns = $derived(columnsByTab[activeTab]);

  // Filter actors by faction
  const filteredActors = $derived(filterByFaction(actors, factionFilter));

  // Recalculate percentages relative to filtered set
  const actorsWithFilteredPercentages = $derived.by(() => {
    if (filteredActors.length === 0) return [];

    // Calculate total for the current metric within filtered actors
    const totalField = columns.totalField;
    const filteredTotal = filteredActors.reduce((sum, actor) => {
      return sum + ((actor[totalField] as number) || 0);
    }, 0);

    // Recalculate percentage for each actor relative to filtered total
    return filteredActors.map((actor) => {
      const actorValue = (actor[totalField] as number) || 0;
      const filteredPercentage = filteredTotal > 0 ? (actorValue / filteredTotal) * 100 : 0;

      return {
        ...actor,
        _filteredPercentage: filteredPercentage,
      };
    });
  });

  // Actor type to color mapping
  function getActorColor(type: ActorType): string {
    const colors = {
      player: "rgb(34 211 238)", // cyan-400
      simPlayer: "rgb(52 211 153)", // emerald-400
      npc: "rgb(251 113 133)", // rose-400
      pet: "rgb(167 139 250)", // violet-400
    };
    return colors[type] ?? colors.player;
  }

  // Get badge variant from actor type
  function getBadgeVariant(type: ActorType): "player" | "simPlayer" | "npc" | "pet" | "default" {
    const validTypes = ["player", "simPlayer", "npc", "pet"] as const;
    return validTypes.includes(type as (typeof validTypes)[number])
      ? (type as (typeof validTypes)[number])
      : "default";
  }

  // Sort indicator icon
  const SortIcon = $derived.by(() => {
    return sortDirection === "asc" ? ChevronUp : ChevronDown;
  });

  // Column span for empty state
  const emptyColSpan = $derived(5 + (columns.extraColumns?.length ?? 0));
</script>

<div>
  <!-- Tabs and Filters -->
  <div class="flex flex-col gap-3 mb-4">
    <!-- Mobile: Dropdown + Filters (stacked) -->
    <div class="lg:hidden space-y-3">
      <TabSelect {tabs} active={activeTab} {onTabChange} />
      <FactionFilterComponent active={factionFilter} onFilterChange={onFactionChange} />
    </div>

    <!-- Desktop: Tabs + Filters (horizontal) -->
    <div class="hidden lg:flex lg:items-center lg:justify-between">
      <TabGroup {tabs} active={activeTab} {onTabChange} />
      <FactionFilterComponent active={factionFilter} onFilterChange={onFactionChange} />
    </div>
  </div>

  <!-- Table -->
  <div class="overflow-x-auto">
    <table class="w-full">
      <thead class="sticky top-0 bg-slate-900/95 backdrop-blur">
        <tr class="border-b border-slate-800">
          <!-- Rank -->
          <th
            scope="col"
            class="px-3 py-3 text-right text-xs uppercase tracking-wider text-slate-400"
          >
            #
          </th>

          <!-- Actor Name -->
          <th
            scope="col"
            class="px-4 py-3 text-left text-xs uppercase tracking-wider text-slate-400"
            aria-sort={sortBy === "name"
              ? sortDirection === "asc"
                ? "ascending"
                : "descending"
              : "none"}
          >
            <button
              type="button"
              class="flex items-center gap-1 cursor-pointer hover:text-cyan-400 transition"
              onclick={() => onSort("name")}
            >
              <span>Actor</span>
              {#if sortBy === "name"}
                <SortIcon class="h-3 w-3" />
              {/if}
            </button>
          </th>

          <!-- Rate (DPS/DTPS/HPS/HRPS) -->
          <th
            scope="col"
            class="px-4 py-3 text-right text-xs uppercase tracking-wider text-slate-400"
            aria-sort={sortBy === columns.rateSortKey
              ? sortDirection === "asc"
                ? "ascending"
                : "descending"
              : "none"}
          >
            <button
              type="button"
              class="flex items-center justify-end gap-1 w-full cursor-pointer hover:text-cyan-400 transition"
              onclick={() => onSort(columns.rateSortKey)}
            >
              <span>{columns.rateLabel}</span>
              {#if sortBy === columns.rateSortKey}
                <SortIcon class="h-3 w-3" />
              {/if}
            </button>
          </th>

          <!-- Total -->
          <th
            scope="col"
            class="px-4 py-3 text-right text-xs uppercase tracking-wider text-slate-400"
            aria-sort={sortBy === columns.totalSortKey
              ? sortDirection === "asc"
                ? "ascending"
                : "descending"
              : "none"}
          >
            <button
              type="button"
              class="flex items-center justify-end gap-1 w-full cursor-pointer hover:text-cyan-400 transition"
              onclick={() => onSort(columns.totalSortKey)}
            >
              <span>{columns.totalLabel}</span>
              {#if sortBy === columns.totalSortKey}
                <SortIcon class="h-3 w-3" />
              {/if}
            </button>
          </th>

          <!-- Extra columns (e.g., Mitigation, Avoidance) -->
          {#if columns.extraColumns}
            {#each columns.extraColumns as col (col.field)}
              <th
                scope="col"
                class="px-4 py-3 text-right text-xs uppercase tracking-wider text-slate-400"
              >
                {col.label}
              </th>
            {/each}
          {/if}

          <!-- Percentage -->
          <th
            scope="col"
            class="px-4 py-3 text-right text-xs uppercase tracking-wider text-slate-400"
          >
            {columns.percentLabel}
          </th>
        </tr>
      </thead>

      <tbody>
        {#if actorsWithFilteredPercentages.length === 0}
          <tr>
            <td colspan={emptyColSpan} class="px-4 py-12 text-center text-slate-500">
              No combat data available
            </td>
          </tr>
        {:else}
          {#each actorsWithFilteredPercentages as actor, index (actor.actorId)}
            <tr class="border-b border-slate-800 last:border-b-0 hover:bg-slate-800/30 transition">
              <!-- Rank -->
              <td class="px-3 py-3 text-right text-sm text-slate-500 font-mono">
                {index + 1}
              </td>

              <!-- Actor Name with Badge -->
              <td class="px-4 py-3">
                <div class="flex items-center gap-2">
                  <Badge variant={getBadgeVariant(actor.actorType)}>
                    <span
                      class="inline-block h-1.5 w-1.5 rounded-full"
                      style="background-color: {getActorColor(actor.actorType)}"
                    ></span>
                  </Badge>
                  <span class="text-sm font-medium text-slate-200">{actor.actorName}</span>
                </div>
              </td>

              <!-- Rate (DPS/DTPS/HPS/HRPS) -->
              <td class="px-4 py-3 text-right font-mono text-sm font-semibold text-cyan-400">
                {formatDps(actor[columns.rateField] as number)}
              </td>

              <!-- Total -->
              <td class="px-4 py-3 text-right font-mono text-sm text-slate-200">
                {formatNumber(actor[columns.totalField] as number)}
              </td>

              <!-- Extra columns -->
              {#if columns.extraColumns}
                {#each columns.extraColumns as col (col.field)}
                  <td class="px-4 py-3 text-right font-mono text-sm text-slate-200">
                    {col.format(actor[col.field] as number)}
                  </td>
                {/each}
              {/if}

              <!-- Percentage -->
              <td class="px-4 py-3 relative">
                <!-- Background bar -->
                <div
                  class="absolute inset-y-0 right-0 opacity-20"
                  style="width: {actor._filteredPercentage}%; background-color: {getActorColor(
                    actor.actorType
                  )};"
                ></div>

                <!-- Percentage text -->
                <div class="relative text-right font-mono text-sm text-slate-200">
                  {formatPercent(actor._filteredPercentage / 100)}
                </div>
              </td>
            </tr>
          {/each}
        {/if}
      </tbody>
    </table>
  </div>
</div>
