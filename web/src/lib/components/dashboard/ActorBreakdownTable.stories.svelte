<script module lang="ts">
  import { defineMeta } from "@storybook/addon-svelte-csf";
  import ActorBreakdownTable from "./ActorBreakdownTable.svelte";
  import { createActorStats } from "$lib/testing";
  import type { ActorStats } from "$lib/types";

  // Create realistic mock actors with bidirectional metrics
  const mockActors: ActorStats[] = [
    createActorStats({
      actorId: "player-1",
      actorName: "Adventurer",
      actorType: "player",
      totalDamage: 4125000,
      dps: 13750.0,
      percentOfTotalDamage: 45.2,
      damageTaken: 1850000,
      dtps: 6166.7,
      percentOfTotalDamageTaken: 38.5,
      totalMitigated: 450000,
      mitigationRate: 19.6,
      totalMissedAgainst: 15,
      avoidanceRate: 8.3,
    }),
    createActorStats({
      actorId: "pet-1",
      actorName: "Wolf",
      actorType: "pet",
      totalDamage: 2350000,
      dps: 7833.3,
      percentOfTotalDamage: 25.8,
      damageTaken: 1200000,
      dtps: 4000.0,
      percentOfTotalDamageTaken: 25.0,
      totalMitigated: 180000,
      mitigationRate: 13.0,
      totalMissedAgainst: 22,
      avoidanceRate: 12.1,
    }),
    createActorStats({
      actorId: "sim-1",
      actorName: "SimPlayer1",
      actorType: "simPlayer",
      totalDamage: 1775000,
      dps: 5916.7,
      percentOfTotalDamage: 19.5,
      damageTaken: 980000,
      dtps: 3266.7,
      percentOfTotalDamageTaken: 20.4,
      totalHealing: 850000,
      hps: 2833.3,
      percentOfTotalHealing: 72.4,
    }),
    createActorStats({
      actorId: "npc-1",
      actorName: "Boss Skeleton",
      actorType: "npc",
      totalDamage: 865000,
      dps: 2883.3,
      percentOfTotalDamage: 9.5,
      damageTaken: 4200000,
      dtps: 14000.0,
      percentOfTotalDamageTaken: 87.3,
      totalMitigated: 980000,
      mitigationRate: 18.9,
    }),
  ];

  const { Story } = defineMeta({
    title: "Data Display/ActorBreakdownTable",
    component: ActorBreakdownTable,
    tags: ["autodocs"],
  });
</script>

<Story name="Damage Dealt Tab (Default)">
  {#snippet template(_args)}
    <div class="bg-slate-950 p-6">
      <div class="bg-slate-900 border border-slate-700 rounded-lg p-6">
        <h3 class="text-sm font-semibold uppercase tracking-wider text-slate-300 mb-4">
          Actor Breakdown
        </h3>
        <ActorBreakdownTable
          actors={mockActors}
          activeTab="damageDealt"
          factionFilter="all"
          sortBy="dps"
          sortDirection="desc"
          onTabChange={() => {}}
          onFactionChange={() => {}}
          onSort={() => {}}
        />
      </div>
    </div>
  {/snippet}
</Story>

<Story name="Damage Taken Tab">
  {#snippet template(_args)}
    <div class="bg-slate-950 p-6">
      <div class="bg-slate-900 border border-slate-700 rounded-lg p-6">
        <h3 class="text-sm font-semibold uppercase tracking-wider text-slate-300 mb-4">
          Actor Breakdown - Damage Taken
        </h3>
        <ActorBreakdownTable
          actors={mockActors}
          activeTab="damageTaken"
          factionFilter="all"
          sortBy="dtps"
          sortDirection="desc"
          onTabChange={() => {}}
          onFactionChange={() => {}}
          onSort={() => {}}
        />
      </div>
    </div>
  {/snippet}
</Story>

<Story name="Healing Done Tab">
  {#snippet template(_args)}
    <div class="bg-slate-950 p-6">
      <div class="bg-slate-900 border border-slate-700 rounded-lg p-6">
        <h3 class="text-sm font-semibold uppercase tracking-wider text-slate-300 mb-4">
          Actor Breakdown - Healing
        </h3>
        <ActorBreakdownTable
          actors={mockActors}
          activeTab="healingDone"
          factionFilter="all"
          sortBy="hps"
          sortDirection="desc"
          onTabChange={() => {}}
          onFactionChange={() => {}}
          onSort={() => {}}
        />
      </div>
    </div>
  {/snippet}
</Story>

<Story name="Friendly Faction Only">
  {#snippet template(_args)}
    <div class="bg-slate-950 p-6">
      <div class="bg-slate-900 border border-slate-700 rounded-lg p-6">
        <h3 class="text-sm font-semibold uppercase tracking-wider text-slate-300 mb-4">
          Actor Breakdown - Friendly Only
        </h3>
        <ActorBreakdownTable
          actors={mockActors}
          activeTab="damageDealt"
          factionFilter="friendly"
          sortBy="dps"
          sortDirection="desc"
          onTabChange={() => {}}
          onFactionChange={() => {}}
          onSort={() => {}}
        />
      </div>
    </div>
  {/snippet}
</Story>

<Story name="Empty State">
  {#snippet template(_args)}
    <div class="bg-slate-950 p-6">
      <div class="bg-slate-900 border border-slate-700 rounded-lg p-6">
        <h3 class="text-sm font-semibold uppercase tracking-wider text-slate-300 mb-4">
          Actor Breakdown - Empty
        </h3>
        <ActorBreakdownTable
          actors={[]}
          activeTab="damageDealt"
          factionFilter="all"
          sortBy="dps"
          sortDirection="desc"
          onTabChange={() => {}}
          onFactionChange={() => {}}
          onSort={() => {}}
        />
      </div>
    </div>
  {/snippet}
</Story>
