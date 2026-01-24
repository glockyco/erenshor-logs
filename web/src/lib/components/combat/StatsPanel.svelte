<script lang="ts">
  import type { SessionStats } from "$lib/types";
  import { formatDps, formatNumber, formatDuration } from "$lib/utils";
  import { Heading } from "$lib/components/ui/typography";

  interface Props {
    stats: SessionStats | null;
    isLive?: boolean;
    duration: number;
  }

  let { stats, isLive = false, duration }: Props = $props();
</script>

<div class="bg-stone-800 border-2 border-stone-700 rounded-lg shadow-lg">
  <div class="border-b border-stone-700 px-6 py-4 flex items-center justify-between">
    <Heading variant="section">Combat Session</Heading>
    {#if isLive}
      <div
        class="flex items-center gap-1 text-xs font-semibold text-amber-500 bg-amber-950 px-2 py-1 rounded"
      >
        <span class="w-2 h-2 rounded-full bg-amber-500 animate-pulse"></span>
        LIVE
      </div>
    {/if}
  </div>

  <div class="p-6">
    {#if !stats}
      <div class="py-12 text-center text-stone-500">
        <p>No combat data yet</p>
      </div>
    {:else}
      <!-- Hero Stats Grid: DPS, DTPS, HPS -->
      <div class="grid grid-cols-3 gap-8 mb-6 pb-6 border-b border-stone-700">
        <!-- Damage Dealt -->
        <div class="text-center space-y-1">
          <div class="text-xs uppercase tracking-wider text-stone-400">Damage Dealt</div>
          <div class="text-4xl font-mono font-bold text-amber-500">
            {formatDps(stats.dps)}
          </div>
          <div class="text-xs text-stone-500">DPS</div>
        </div>

        <!-- Damage Taken -->
        <div class="text-center space-y-1">
          <div class="text-xs uppercase tracking-wider text-stone-400">Damage Taken</div>
          <div class="text-4xl font-mono font-bold text-rose-400">
            {formatDps(stats.dtps)}
          </div>
          <div class="text-xs text-stone-500">DTPS</div>
        </div>

        <!-- Healing Done -->
        <div class="text-center space-y-1">
          <div class="text-xs uppercase tracking-wider text-stone-400">Healing Done</div>
          <div class="text-4xl font-mono font-bold text-lime-500">
            {formatDps(stats.hps)}
          </div>
          <div class="text-xs text-stone-500">HPS</div>
        </div>
      </div>

      <!-- Supporting Stats: Single Row -->
      <div class="grid grid-cols-2 md:grid-cols-4 gap-6">
        <div class="space-y-1">
          <div class="text-xs uppercase tracking-wider text-stone-400">Total Damage</div>
          <div class="text-base font-mono font-semibold text-stone-200">
            {formatNumber(stats.totalDamage)}
          </div>
        </div>

        <div class="space-y-1">
          <div class="text-xs uppercase tracking-wider text-stone-400">Total Taken</div>
          <div class="text-base font-mono font-semibold text-stone-200">
            {formatNumber(stats.totalDamageTaken)}
          </div>
        </div>

        <div class="space-y-1">
          <div class="text-xs uppercase tracking-wider text-stone-400">Total Healing</div>
          <div class="text-base font-mono font-semibold text-stone-200">
            {formatNumber(stats.totalHealing)}
          </div>
        </div>

        <div class="space-y-1">
          <div class="text-xs uppercase tracking-wider text-stone-400">Duration</div>
          <div class="text-base font-mono font-semibold text-stone-200">
            {formatDuration(duration)}
          </div>
        </div>
      </div>
    {/if}
  </div>
</div>
