<script lang="ts">
  import type { SessionStats } from "$lib/types";
  import { formatDps, formatNumber, formatDuration } from "$lib/utils";
  import { Heading, Numeric } from "$lib/components/ui/typography";

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
          <Heading level={6} variant="label">Damage Dealt</Heading>
          <Numeric variant="hero" color="primary" as="div">
            {formatDps(stats.dps)}
          </Numeric>
          <div class="text-xs text-stone-500">DPS</div>
        </div>

        <!-- Damage Taken -->
        <div class="text-center space-y-1">
          <Heading level={6} variant="label">Damage Taken</Heading>
          <Numeric variant="hero" color="damage" as="div">
            {formatDps(stats.dtps)}
          </Numeric>
          <div class="text-xs text-stone-500">DTPS</div>
        </div>

        <!-- Healing Done -->
        <div class="text-center space-y-1">
          <Heading level={6} variant="label">Healing Done</Heading>
          <Numeric variant="hero" color="healing" as="div">
            {formatDps(stats.hps)}
          </Numeric>
          <div class="text-xs text-stone-500">HPS</div>
        </div>
      </div>

      <!-- Supporting Stats: Single Row -->
      <div class="grid grid-cols-2 md:grid-cols-4 gap-6">
        <div class="space-y-1">
          <Heading level={6} variant="label">Total Damage</Heading>
          <Numeric variant="large" color="muted" as="div">
            {formatNumber(stats.totalDamage)}
          </Numeric>
        </div>

        <div class="space-y-1">
          <Heading level={6} variant="label">Total Taken</Heading>
          <Numeric variant="large" color="muted" as="div">
            {formatNumber(stats.totalDamageTaken)}
          </Numeric>
        </div>

        <div class="space-y-1">
          <Heading level={6} variant="label">Total Healing</Heading>
          <Numeric variant="large" color="muted" as="div">
            {formatNumber(stats.totalHealing)}
          </Numeric>
        </div>

        <div class="space-y-1">
          <Heading level={6} variant="label">Duration</Heading>
          <Numeric variant="large" color="muted" as="div">
            {formatDuration(duration)}
          </Numeric>
        </div>
      </div>
    {/if}
  </div>
</div>
