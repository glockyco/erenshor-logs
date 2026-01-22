<script lang="ts">
  import type { SessionStats } from "$lib/types";
  import Card from "$lib/components/ui/Card.svelte";
  import Badge from "$lib/components/ui/Badge.svelte";
  import { formatDps, formatNumber, formatDuration } from "$lib/utils";

  interface Props {
    stats: SessionStats | null;
    isLive: boolean;
    duration: number; // ms - live-updating for active sessions
  }

  let { stats, isLive, duration }: Props = $props();
</script>

<Card title={isLive ? "Live Combat" : "Combat Session"}>
  {#snippet actions()}
    {#if isLive}
      <Badge variant="player">
        <span class="flex items-center gap-1">
          <span class="h-1.5 w-1.5 rounded-full bg-cyan-400 animate-pulse-dot"></span>
          LIVE
        </span>
      </Badge>
    {/if}
  {/snippet}

  {#if !stats}
    <div class="text-center py-12 text-slate-500">
      <p>No combat data yet</p>
    </div>
  {:else}
    <!-- Hero Stats: DPS and HPS -->
    <div class="grid grid-cols-2 gap-8 mb-6 pb-6 border-b border-slate-800">
      <!-- DPS -->
      <div class="text-center">
        <div class="text-xs uppercase tracking-wider text-slate-400 mb-2">DPS</div>
        <div class="text-3xl font-mono font-bold text-cyan-400 mb-1">
          {formatDps(stats.dps)}
        </div>
        <div class="text-xs text-slate-500">Damage per Second</div>
      </div>

      <!-- HPS -->
      <div class="text-center">
        <div class="text-xs uppercase tracking-wider text-slate-400 mb-2">HPS</div>
        <div class="text-3xl font-mono font-bold text-emerald-400 mb-1">
          {formatDps(stats.hps)}
        </div>
        <div class="text-xs text-slate-500">Healing per Second</div>
      </div>
    </div>

    <!-- Supporting Stats: Totals and Duration -->
    <div class="grid grid-cols-3 gap-6">
      <div>
        <div class="text-xs uppercase tracking-wider text-slate-400 mb-1">Total Damage</div>
        <div class="text-lg font-mono font-semibold text-slate-200">
          {formatNumber(stats.totalDamage)}
        </div>
      </div>

      <div>
        <div class="text-xs uppercase tracking-wider text-slate-400 mb-1">Total Healing</div>
        <div class="text-lg font-mono font-semibold text-slate-200">
          {formatNumber(stats.totalHealing)}
        </div>
      </div>

      <div>
        <div class="text-xs uppercase tracking-wider text-slate-400 mb-1">Duration</div>
        <div class="text-lg font-mono font-semibold text-slate-200">
          {formatDuration(duration)}
        </div>
      </div>
    </div>
  {/if}
</Card>

<style>
  .animate-pulse-dot {
    animation: pulse-dot 2s cubic-bezier(0.4, 0, 0.6, 1) infinite;
  }

  @keyframes pulse-dot {
    0%,
    100% {
      opacity: 1;
    }
    50% {
      opacity: 0.3;
    }
  }
</style>
