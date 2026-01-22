<script lang="ts">
  import type { ActorStats, SortBy, SortDirection, ActorType } from "$lib/types";
  import Badge from "$lib/components/ui/Badge.svelte";
  import { formatDps, formatNumber, formatPercent } from "$lib/utils";
  import { ChevronUp, ChevronDown } from "@lucide/svelte";

  interface Props {
    actors: ActorStats[];
    sortBy: SortBy;
    sortDirection: SortDirection;
    onSort: (field: SortBy) => void;
  }

  let { actors, sortBy, sortDirection, onSort }: Props = $props();

  // Actor type to color mapping (matches Badge variants)
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
</script>

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

        <!-- DPS -->
        <th
          scope="col"
          class="px-4 py-3 text-right text-xs uppercase tracking-wider text-slate-400"
          aria-sort={sortBy === "dps"
            ? sortDirection === "asc"
              ? "ascending"
              : "descending"
            : "none"}
        >
          <button
            type="button"
            class="flex items-center justify-end gap-1 w-full cursor-pointer hover:text-cyan-400 transition"
            onclick={() => onSort("dps")}
          >
            <span>DPS</span>
            {#if sortBy === "dps"}
              <SortIcon class="h-3 w-3" />
            {/if}
          </button>
        </th>

        <!-- Total -->
        <th
          scope="col"
          class="px-4 py-3 text-right text-xs uppercase tracking-wider text-slate-400"
          aria-sort={sortBy === "damage"
            ? sortDirection === "asc"
              ? "ascending"
              : "descending"
            : "none"}
        >
          <button
            type="button"
            class="flex items-center justify-end gap-1 w-full cursor-pointer hover:text-cyan-400 transition"
            onclick={() => onSort("damage")}
          >
            <span>Total</span>
            {#if sortBy === "damage"}
              <SortIcon class="h-3 w-3" />
            {/if}
          </button>
        </th>

        <!-- Percentage -->
        <th
          scope="col"
          class="px-4 py-3 text-right text-xs uppercase tracking-wider text-slate-400"
        >
          % of Total
        </th>
      </tr>
    </thead>

    <tbody>
      {#if actors.length === 0}
        <tr>
          <td colspan="5" class="px-4 py-12 text-center text-slate-500">
            No combat data available
          </td>
        </tr>
      {:else}
        {#each actors as actor, index (actor.actorId)}
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

            <!-- DPS -->
            <td class="px-4 py-3 text-right font-mono text-sm font-semibold text-cyan-400">
              {formatDps(actor.dps)}
            </td>

            <!-- Total -->
            <td class="px-4 py-3 text-right font-mono text-sm text-slate-200">
              {formatNumber(actor.totalDamage)}
            </td>

            <!-- Percentage -->
            <td class="px-4 py-3 relative">
              <!-- Background bar -->
              <div
                class="absolute inset-y-0 right-0 opacity-20"
                style="width: {actor.percentOfTotalDamage}%; background-color: {getActorColor(
                  actor.actorType
                )};"
              ></div>

              <!-- Percentage text -->
              <div class="relative text-right font-mono text-sm text-slate-200">
                {formatPercent(actor.percentOfTotalDamage / 100)}
              </div>
            </td>
          </tr>
        {/each}
      {/if}
    </tbody>
  </table>
</div>
