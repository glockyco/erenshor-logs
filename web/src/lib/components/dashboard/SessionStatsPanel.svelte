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
    <!-- Hero Stats: DPS, DTPS, and HPS -->
    <div class="grid grid-cols-3 gap-8 mb-6 pb-6 border-b border-slate-800">
      <!-- Damage Dealt -->
      <div class="text-center">
        <div class="text-xs uppercase tracking-wider text-slate-400 mb-2">Damage Dealt</div>
        <div class="text-3xl font-mono font-bold text-cyan-400 mb-1">
          {formatDps(stats.dps)}
        </div>
        <div class="text-xs text-slate-500">DPS</div>
      </div>

      <!-- Damage Taken -->
      <div class="text-center">
        <div class="text-xs uppercase tracking-wider text-slate-400 mb-2">Damage Taken</div>
        <div class="text-3xl font-mono font-bold text-rose-400 mb-1">
          {formatDps(stats.dtps)}
        </div>
        <div class="text-xs text-slate-500">DTPS</div>
      </div>

      <!-- Healing Done -->
      <div class="text-center">
        <div class="text-xs uppercase tracking-wider text-slate-400 mb-2">Healing Done</div>
        <div class="text-3xl font-mono font-bold text-emerald-400 mb-1">
          {formatDps(stats.hps)}
        </div>
        <div class="text-xs text-slate-500">HPS</div>
      </div>
    </div>

    <!-- Supporting Stats: Totals, Mitigation, and Duration -->
    <div class="grid grid-cols-2 gap-x-6 gap-y-4">
      <div>
        <div class="text-xs uppercase tracking-wider text-slate-400 mb-1">Total Damage</div>
        <div class="text-lg font-mono font-semibold text-slate-200">
          {formatNumber(stats.totalDamage)}
        </div>
      </div>

      <div>
        <div class="text-xs uppercase tracking-wider text-slate-400 mb-1">Total Taken</div>
        <div class="text-lg font-mono font-semibold text-slate-200">
          {formatNumber(stats.totalDamageTaken)}
        </div>
      </div>

      <div>
        <div class="text-xs uppercase tracking-wider text-slate-400 mb-1">Total Healing</div>
        <div class="text-lg font-mono font-semibold text-slate-200">
          {formatNumber(stats.totalHealing)}
        </div>
      </div>

      <div>
        <div class="text-xs uppercase tracking-wider text-slate-400 mb-1">Mitigation</div>
        <div class="text-lg font-mono font-semibold text-slate-200">
          {stats.mitigationRate.toFixed(1)}%
        </div>
      </div>

      <div class="col-span-2">
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
