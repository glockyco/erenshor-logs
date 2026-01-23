<script lang="ts">
  import type { SessionStats } from "$lib/types";
  import Card from "$lib/components/ui/Card.svelte";
  import Badge from "$lib/components/ui/Badge.svelte";
  import { formatDps, formatNumber, formatDuration } from "$lib/utils";
  import { typography } from "$lib/design";

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
      <div class="text-center space-y-1">
        <div class={typography.label}>Damage Dealt</div>
        <div class={typography.hero + " text-cyan-400"}>
          {formatDps(stats.dps)}
        </div>
        <div class={typography.metadata}>DPS</div>
      </div>

      <!-- Damage Taken -->
      <div class="text-center space-y-1">
        <div class={typography.label}>Damage Taken</div>
        <div class={typography.hero + " text-rose-400"}>
          {formatDps(stats.dtps)}
        </div>
        <div class={typography.metadata}>DTPS</div>
      </div>

      <!-- Healing Done -->
      <div class="text-center space-y-1">
        <div class={typography.label}>Healing Done</div>
        <div class={typography.hero + " text-emerald-400"}>
          {formatDps(stats.hps)}
        </div>
        <div class={typography.metadata}>HPS</div>
      </div>
    </div>

    <!-- Supporting Stats: Totals, Mitigation, and Duration -->
    <div class="grid grid-cols-2 gap-x-6 gap-y-4">
      <div class="space-y-1">
        <div class={typography.label}>Total Damage</div>
        <div class={typography.body + " font-mono text-slate-200"}>
          {formatNumber(stats.totalDamage)}
        </div>
      </div>

      <div class="space-y-1">
        <div class={typography.label}>Total Taken</div>
        <div class={typography.body + " font-mono text-slate-200"}>
          {formatNumber(stats.totalDamageTaken)}
        </div>
      </div>

      <div class="space-y-1">
        <div class={typography.label}>Total Healing</div>
        <div class={typography.body + " font-mono text-slate-200"}>
          {formatNumber(stats.totalHealing)}
        </div>
      </div>

      <div class="space-y-1">
        <div class={typography.label}>Mitigation</div>
        <div class={typography.body + " font-mono text-slate-200"}>
          {stats.mitigationRate.toFixed(1)}%
        </div>
      </div>

      <div class="col-span-2 space-y-1">
        <div class={typography.label}>Duration</div>
        <div class={typography.body + " font-mono text-slate-200"}>
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
