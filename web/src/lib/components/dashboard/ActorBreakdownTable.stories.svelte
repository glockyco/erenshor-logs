<script module lang="ts">
  import { defineMeta } from "@storybook/addon-svelte-csf";
  import ActorBreakdownTable from "./ActorBreakdownTable.svelte";
  import { createActorStats } from "$lib/testing";
  import type { ActorStats } from "$lib/types";

  // Create realistic mock actors with actual game numbers (sorted by DPS descending by default)
  // Typical DPS range: 10-15k per character
  const mockActors: ActorStats[] = [
    createActorStats({
      actorId: "player-1",
      actorName: "Adventurer",
      actorType: "player",
      totalDamage: 4125000, // ~5 min fight at 13.75k DPS
      dps: 13750.0,
      percentOfTotalDamage: 45.2,
    }),
    createActorStats({
      actorId: "pet-1",
      actorName: "Wolf",
      actorType: "pet",
      totalDamage: 2350000,
      dps: 7833.3,
      percentOfTotalDamage: 25.8,
    }),
    createActorStats({
      actorId: "sim-1",
      actorName: "SimPlayer1",
      actorType: "simPlayer",
      totalDamage: 1775000,
      dps: 5916.7,
      percentOfTotalDamage: 19.5,
    }),
    createActorStats({
      actorId: "sim-2",
      actorName: "SimPlayer2",
      actorType: "simPlayer",
      totalDamage: 865000,
      dps: 2883.3,
      percentOfTotalDamage: 9.5,
    }),
  ];

  // Pre-sorted variants
  const actorsSortedByName = [...mockActors].sort((a, b) => a.actorName.localeCompare(b.actorName));

  const singleActor = [mockActors[0]];

  const { Story } = defineMeta({
    title: "Data Display/ActorBreakdownTable",
    component: ActorBreakdownTable,
    tags: ["autodocs"],
  });
</script>

<Story name="Default (Sorted by DPS)">
  {#snippet template(_args)}
    <div class="bg-slate-950 p-6">
      <div class="bg-slate-900 border border-slate-700 rounded-lg p-6">
        <h3 class="text-sm font-semibold uppercase tracking-wider text-slate-300 mb-4">
          Actor Breakdown
        </h3>
        <ActorBreakdownTable
          actors={mockActors}
          sortBy="dps"
          sortDirection="desc"
          onSort={() => {}}
        />
      </div>
    </div>
  {/snippet}
</Story>

<Story name="Sorted by Name (Ascending)">
  {#snippet template(_args)}
    <div class="bg-slate-950 p-6">
      <div class="bg-slate-900 border border-slate-700 rounded-lg p-6">
        <h3 class="text-sm font-semibold uppercase tracking-wider text-slate-300 mb-4">
          Actor Breakdown
        </h3>
        <ActorBreakdownTable
          actors={actorsSortedByName}
          sortBy="name"
          sortDirection="asc"
          onSort={() => {}}
        />
      </div>
    </div>
  {/snippet}
</Story>

<Story name="Sorted by Total Damage">
  {#snippet template(_args)}
    <div class="bg-slate-950 p-6">
      <div class="bg-slate-900 border border-slate-700 rounded-lg p-6">
        <h3 class="text-sm font-semibold uppercase tracking-wider text-slate-300 mb-4">
          Actor Breakdown
        </h3>
        <ActorBreakdownTable
          actors={mockActors}
          sortBy="damage"
          sortDirection="desc"
          onSort={() => {}}
        />
      </div>
    </div>
  {/snippet}
</Story>

<Story name="Single Actor">
  {#snippet template(_args)}
    <div class="bg-slate-950 p-6">
      <div class="bg-slate-900 border border-slate-700 rounded-lg p-6">
        <h3 class="text-sm font-semibold uppercase tracking-wider text-slate-300 mb-4">
          Actor Breakdown
        </h3>
        <ActorBreakdownTable
          actors={singleActor}
          sortBy="dps"
          sortDirection="desc"
          onSort={() => {}}
        />
      </div>
    </div>
  {/snippet}
</Story>

<Story name="Empty">
  {#snippet template(_args)}
    <div class="bg-slate-950 p-6">
      <div class="bg-slate-900 border border-slate-700 rounded-lg p-6">
        <h3 class="text-sm font-semibold uppercase tracking-wider text-slate-300 mb-4">
          Actor Breakdown
        </h3>
        <ActorBreakdownTable actors={[]} sortBy="dps" sortDirection="desc" onSort={() => {}} />
      </div>
    </div>
  {/snippet}
</Story>
