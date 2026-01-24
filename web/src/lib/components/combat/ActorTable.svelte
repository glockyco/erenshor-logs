<script lang="ts">
  import { ChevronDown, ChevronRight } from "@lucide/svelte";
  import { SvelteSet } from "svelte/reactivity";
  import { Heading } from "$lib/components/ui/typography";
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
  let expandedActors = new SvelteSet<string>();

  // Get actors based on active tab - filter abilities by relevance
  const actors = $derived(
    stats
      ? stats.actorBreakdown.map((a) => {
          // Select base array based on perspective (dealt vs received)
          const baseAbilities =
            activeTab === "damageDealt" || activeTab === "healingDone"
              ? a.abilityBreakdown
              : a.abilitiesReceivedFrom;

          // Filter to only relevant abilities for this tab
          const filteredAbilities =
            activeTab === "damageDealt" || activeTab === "damageTaken"
              ? baseAbilities.filter((ab) => ab.damage > 0)
              : baseAbilities.filter((ab) => ab.healing > 0);

          return {
            actorId: a.actorId,
            actorName: a.actorName,
            actorType: a.actorType,
            actorClass: a.actorClass,
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
                  ? a.dtps
                  : activeTab === "healingDone"
                    ? a.hps
                    : a.hrps,
            percentage:
              activeTab === "damageDealt"
                ? a.percentOfTotalDamage
                : activeTab === "damageTaken"
                  ? a.percentOfTotalDamageTaken
                  : activeTab === "healingDone"
                    ? a.percentOfTotalHealing
                    : a.percentOfTotalHealingReceived,
            abilityBreakdown: filteredAbilities,
          };
        })
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

  // Recalculate percentages based on filtered subset
  const actorsWithRecalculatedPercentages = $derived(
    (() => {
      // Calculate total for the filtered set based on current metric
      const filteredTotal = filteredActors.reduce((sum, a) => sum + a.total, 0);

      // Recalculate percentage for each actor relative to filtered total
      return filteredActors.map((a) => ({
        ...a,
        percentage: filteredTotal > 0 ? (a.total / filteredTotal) * 100 : 0,
      }));
    })()
  );

  // Sort actors
  const sortedActors = $derived(
    [...actorsWithRecalculatedPercentages].sort((a, b) => {
      const aVal = sortBy === "dps" ? a.dps : a.total;
      const bVal = sortBy === "dps" ? b.dps : b.total;
      return sortDirection === "desc" ? bVal - aVal : aVal - bVal;
    })
  );

  const toggleSort = (field: SortField) => {
    if (sortBy === field) {
      sortDirection = sortDirection === "desc" ? "asc" : "desc";
    } else {
      sortBy = field;
      sortDirection = "desc";
    }
  };

  const toggleExpanded = (actorId: string) => {
    if (expandedActors.has(actorId)) {
      expandedActors.delete(actorId);
    } else {
      expandedActors.add(actorId);
    }
  };

  const expandAll = () => {
    sortedActors.forEach((actor) => {
      expandedActors.add(actor.actorId);
    });
  };

  const collapseAll = () => {
    expandedActors.clear();
  };

  // Determine if all actors are expanded
  const allExpanded = $derived(
    sortedActors.length > 0 && sortedActors.every((actor) => expandedActors.has(actor.actorId))
  );

  const toggleExpandAll = () => {
    if (allExpanded) {
      collapseAll();
    } else {
      expandAll();
    }
  };

  // Context-aware rate label for main table header
  const rateLabel = $derived(
    activeTab === "damageDealt"
      ? "DPS"
      : activeTab === "damageTaken"
        ? "DTPS"
        : activeTab === "healingDone"
          ? "HPS"
          : "HRPS"
  );
</script>

<div class="bg-stone-800 border-2 border-stone-700 rounded-lg shadow-lg">
  <div class="border-b border-stone-700 px-4 py-4 md:px-6">
    <div class="mb-4 flex flex-wrap items-center justify-between gap-3">
      <Heading variant="section">Actor Breakdown</Heading>

      <!-- Faction Filter Buttons -->
      <div class="flex flex-wrap gap-2">
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

  <div class="p-4 md:p-6">
    {#if !stats || sortedActors.length === 0}
      <div class="py-12 text-center text-stone-500">
        <p>No actor data available</p>
      </div>
    {:else}
      <div class="overflow-x-auto -mx-4 md:mx-0">
        <table class="w-full text-sm min-w-[700px]">
          <thead class="border-b border-stone-700">
            <tr>
              <th class="w-12 px-3 py-3 text-left">
                <button
                  type="button"
                  class="text-stone-400 hover:text-amber-500 transition-colors cursor-pointer"
                  onclick={toggleExpandAll}
                  aria-label={allExpanded ? "Collapse all" : "Expand all"}
                >
                  {#if allExpanded}
                    <ChevronDown class="h-4 w-4" />
                  {:else}
                    <ChevronRight class="h-4 w-4" />
                  {/if}
                </button>
              </th>
              <th class="px-3 py-3 text-left">
                <Heading level={6} variant="label">#</Heading>
              </th>
              <th class="px-4 py-3 text-left">
                <Heading level={6} variant="label">Actor</Heading>
              </th>
              <th class="px-4 py-3 text-right">
                <button
                  type="button"
                  class="inline-flex items-center gap-1 text-sm font-medium uppercase tracking-wider text-stone-400 hover:text-amber-500 cursor-pointer"
                  onclick={() => toggleSort("dps")}
                >
                  {rateLabel}
                  {#if sortBy === "dps"}
                    <ChevronDown
                      class={`h-3 w-3 transition-transform ${sortDirection === "asc" ? "rotate-180" : ""}`}
                    />
                  {/if}
                </button>
              </th>
              <th class="px-4 py-3 text-right">
                <button
                  type="button"
                  class="inline-flex items-center gap-1 text-sm font-medium uppercase tracking-wider text-stone-400 hover:text-amber-500 cursor-pointer"
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
              <th class="px-4 py-3 text-right">
                <Heading level={6} variant="label">% of Total</Heading>
              </th>
            </tr>
          </thead>
          <tbody>
            {#each sortedActors as actor, i (`${actor.actorId}-${i}`)}
              <ActorRow
                {actor}
                rank={i + 1}
                durationMs={stats?.durationMs || 1}
                perspective={activeTab}
                expanded={expandedActors.has(actor.actorId)}
                onToggleExpand={() => toggleExpanded(actor.actorId)}
              />
            {/each}
          </tbody>
        </table>
      </div>
    {/if}
  </div>
</div>
