<script lang="ts">
  import SessionStatsPanel from "$lib/components/dashboard/SessionStatsPanel.svelte";
  import ActorBreakdownTable from "$lib/components/dashboard/ActorBreakdownTable.svelte";
  import Card from "$lib/components/ui/Card.svelte";
  import { Swords } from "@lucide/svelte";
  import type { SessionStats, ActorStats } from "$lib/types";

  // Mock data for preview - realistic game numbers
  const previewDuration = 331000; // 5m 31s

  const previewStats: SessionStats = {
    totalDamage: 4125000,
    totalHealing: 372500,
    durationMs: previewDuration,
    dps: 12458.3,
    hps: 1247.8,
    actorBreakdown: [
      {
        actorId: "preview-player",
        actorName: "Adventurer",
        actorType: "player",
        totalDamage: 1862500,
        totalHealing: 0,
        dps: 5625.0,
        hps: 0,
        percentOfTotalDamage: 45.2,
        percentOfTotalHealing: 0,
        abilityBreakdown: [],
      },
      {
        actorId: "preview-pet",
        actorName: "Dire Wolf",
        actorType: "pet",
        totalDamage: 1064250,
        totalHealing: 0,
        dps: 3212.5,
        hps: 0,
        percentOfTotalDamage: 25.8,
        percentOfTotalHealing: 0,
        abilityBreakdown: [],
      },
      {
        actorId: "preview-sim1",
        actorName: "Aeryn",
        actorType: "simPlayer",
        totalDamage: 804375,
        totalHealing: 0,
        dps: 2428.3,
        hps: 0,
        percentOfTotalDamage: 19.5,
        percentOfTotalHealing: 0,
        abilityBreakdown: [],
      },
      {
        actorId: "preview-sim2",
        actorName: "Gideon",
        actorType: "simPlayer",
        totalDamage: 393875,
        totalHealing: 0,
        dps: 1190.5,
        hps: 0,
        percentOfTotalDamage: 9.5,
        percentOfTotalHealing: 0,
        abilityBreakdown: [],
      },
    ],
  };

  const previewActors: ActorStats[] = previewStats.actorBreakdown;
</script>

<div class="relative min-h-[calc(100vh-80px)] p-6">
  <!-- Ghost Dashboard (30% opacity, unclickable) -->
  <div class="pointer-events-none select-none opacity-30" aria-hidden="true">
    <div class="space-y-6">
      <SessionStatsPanel stats={previewStats} isLive={true} duration={previewDuration} />

      <Card title="Actor Breakdown">
        <ActorBreakdownTable
          actors={previewActors}
          sortBy="dps"
          sortDirection="desc"
          onSort={() => {}}
        />
      </Card>
    </div>
  </div>

  <!-- Centered Overlay -->
  <div class="absolute inset-0 flex items-center justify-center p-6">
    <div
      class="max-w-md rounded-lg border-2 border-cyan-400/20 bg-slate-900/95 p-8 shadow-2xl shadow-cyan-400/10 backdrop-blur-sm"
    >
      <!-- Icon with glow -->
      <div class="mb-6 flex justify-center">
        <Swords
          class="h-20 w-20 text-cyan-400"
          style="filter: drop-shadow(0 0 12px rgb(34 211 238 / 0.5));"
          aria-hidden="true"
        />
      </div>

      <!-- Title -->
      <h2 class="mb-3 text-center text-2xl font-bold text-cyan-400">Welcome to Erenshor Logs</h2>

      <!-- Subtitle -->
      <p class="mb-6 text-center text-slate-400">
        The dashboard will appear here<br />once you enter combat in-game.
      </p>

      <!-- Steps -->
      <div class="mb-6 space-y-3 rounded-md bg-slate-800/50 p-4">
        <div class="flex items-start gap-3">
          <div
            class="flex h-6 w-6 flex-shrink-0 items-center justify-center rounded-full bg-cyan-400/10 text-sm font-bold text-cyan-400"
          >
            1
          </div>
          <p class="text-sm text-slate-300">Launch Erenshor with mod installed</p>
        </div>

        <div class="flex items-start gap-3">
          <div
            class="flex h-6 w-6 flex-shrink-0 items-center justify-center rounded-full bg-cyan-400/10 text-sm font-bold text-cyan-400"
          >
            2
          </div>
          <p class="text-sm text-slate-300">Enter combat with any enemy</p>
        </div>

        <div class="flex items-start gap-3">
          <div
            class="flex h-6 w-6 flex-shrink-0 items-center justify-center rounded-full bg-cyan-400/10 text-sm font-bold text-cyan-400"
          >
            3
          </div>
          <p class="text-sm text-slate-300">See real-time DPS analysis appear here</p>
        </div>
      </div>

      <!-- Status indicator with pulsing dot -->
      <div class="flex items-center justify-center gap-2 text-sm text-slate-500">
        <span class="h-2 w-2 animate-pulse-slow rounded-full bg-cyan-400"></span>
        <span>Waiting for combat data...</span>
      </div>
    </div>
  </div>
</div>
