<script lang="ts">
  import { ChevronDown } from "@lucide/svelte";
  import FactionTabs from "./FactionTabs.svelte";
  import ActorRow from "./ActorRow.svelte";
  import type { SessionStats } from "$lib/types";

  type TabValue = "damageDealt" | "damageTaken" | "healingDone" | "healingReceived";
  type FactionFilter = "all" | "friendly" | "hostile";
  type SortField = "dps" | "total";

  interface Props {
    stats: SessionStats | null;
  }

  let { stats }: Props = $props();

  let activeTab = $state<TabValue>("damageDealt");
  let factionFilter = $state<FactionFilter>("all");
  let sortBy = $state<SortField>("dps");
  let sortDirection = $state<"asc" | "desc">("desc");

  // Get actors based on active tab - map to simplified structure
  const actors = $derived(
    stats
      ? stats.actorBreakdown.map((a) => ({
          actorName: a.actorName,
          actorType: a.actorType,
          total:
            activeTab === "damageDealt"
              ? a.totalDamage
              : activeTab === "damageTaken"
                ? a.damageTaken
                : activeTab === "healingDone"
                  ? a.totalHealing
                  : a.healingReceived,
          dps:
            activeTab === "damageDealt"
              ? a.dps
              : activeTab === "damageTaken"
                ? (a.damageTaken / (stats?.durationMs || 1)) * 1000
                : activeTab === "healingDone"
                  ? a.hps
                  : (a.healingReceived / (stats?.durationMs || 1)) * 1000,
        }))
      : []
  );

  // Filter by faction
  const filteredActors = $derived(
    factionFilter === "all"
      ? actors
      : actors.filter((a) =>
          factionFilter === "friendly"
            ? a.actorType === "player" || a.actorType === "simPlayer" || a.actorType === "pet"
            : a.actorType === "npc"
        )
  );

  // Sort actors
  const sortedActors = $derived(
    [...filteredActors].sort((a, b) => {
      const aVal = sortBy === "dps" ? a.dps : a.total;
      const bVal = sortBy === "dps" ? b.dps : b.total;
      return sortDirection === "desc" ? bVal - aVal : aVal - bVal;
    })
  );

  const maxValue = $derived(
    sortedActors.length > 0 ? Math.max(...sortedActors.map((a) => a.total)) : 0
  );

  const toggleSort = (field: SortField) => {
    if (sortBy === field) {
      sortDirection = sortDirection === "desc" ? "asc" : "desc";
    } else {
      sortBy = field;
      sortDirection = "desc";
    }
  };
</script>

<div class="bg-stone-800 border-2 border-stone-700 rounded-lg shadow-lg">
  <div class="border-b border-stone-700 px-6 py-4">
    <div class="flex items-center justify-between mb-4">
      <h3 class="font-fantasy text-lg font-semibold text-amber-500">Actor Breakdown</h3>

      <!-- Faction Filter Buttons -->
      <div class="flex gap-2">
        <button
          type="button"
          class={`rounded px-3 py-1.5 text-xs font-medium uppercase tracking-wider transition-colors cursor-pointer ${
            factionFilter === "all"
              ? "bg-amber-600 text-white"
              : "bg-stone-700 text-stone-300 hover:bg-stone-600 hover:text-white"
          }`}
          onclick={() => (factionFilter = "all")}
        >
          All
        </button>
        <button
          type="button"
          class={`rounded px-3 py-1.5 text-xs font-medium uppercase tracking-wider transition-colors cursor-pointer ${
            factionFilter === "friendly"
              ? "bg-amber-600 text-white"
              : "bg-stone-700 text-stone-300 hover:bg-stone-600 hover:text-white"
          }`}
          onclick={() => (factionFilter = "friendly")}
        >
          Friendly
        </button>
        <button
          type="button"
          class={`rounded px-3 py-1.5 text-xs font-medium uppercase tracking-wider transition-colors cursor-pointer ${
            factionFilter === "hostile"
              ? "bg-amber-600 text-white"
              : "bg-stone-700 text-stone-300 hover:bg-stone-600 hover:text-white"
          }`}
          onclick={() => (factionFilter = "hostile")}
        >
          Hostile
        </button>
      </div>
    </div>

    <!-- Tabs -->
    <FactionTabs {activeTab} onTabChange={(tab) => (activeTab = tab)} />
  </div>

  <div class="p-6">
    {#if !stats || sortedActors.length === 0}
      <div class="py-12 text-center text-stone-500">
        <p>No actor data available</p>
      </div>
    {:else}
      <table class="w-full text-sm">
        <thead class="border-b border-stone-700">
          <tr>
            <th class="px-3 py-3 text-left text-xs uppercase tracking-wider text-stone-400"> # </th>
            <th class="px-4 py-3 text-left text-xs uppercase tracking-wider text-stone-400">
              Actor
            </th>
            <th class="px-4 py-3 text-right text-xs uppercase tracking-wider text-stone-400">
              <button
                type="button"
                class="inline-flex items-center gap-1 hover:text-amber-500 cursor-pointer"
                onclick={() => toggleSort("dps")}
              >
                DPS
                {#if sortBy === "dps"}
                  <ChevronDown
                    class={`h-3 w-3 transition-transform ${sortDirection === "asc" ? "rotate-180" : ""}`}
                  />
                {/if}
              </button>
            </th>
            <th class="px-4 py-3 text-right text-xs uppercase tracking-wider text-stone-400">
              <button
                type="button"
                class="inline-flex items-center gap-1 hover:text-amber-500 cursor-pointer"
                onclick={() => toggleSort("total")}
              >
                Total
                {#if sortBy === "total"}
                  <ChevronDown
                    class={`h-3 w-3 transition-transform ${sortDirection === "asc" ? "rotate-180" : ""}`}
                  />
                {/if}
              </button>
            </th>
            <th class="px-4 py-3 text-right text-xs uppercase tracking-wider text-stone-400">
              % of Total
            </th>
          </tr>
        </thead>
        <tbody>
          {#each sortedActors as actor, i (`${actor.actorName}-${actor.actorType}-${i}`)}
            <ActorRow {actor} rank={i + 1} {maxValue} />
          {/each}
        </tbody>
      </table>
    {/if}
  </div>
</div>
