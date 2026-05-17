<script lang="ts">
  import { X, Download } from "@lucide/svelte";
  import type { Session } from "$lib/types";
  import {
    formatNumber,
    formatDps,
    formatDuration,
    formatTime,
    getSessionEnemies,
  } from "$lib/utils";
  import { calculateSessionStats } from "$lib/services";

  interface Props {
    session: Session;
    isActive?: boolean;
    onclick?: () => void;
    ondelete?: () => void;
    onexport?: () => void;
  }

  let { session, isActive = false, onclick, ondelete, onexport }: Props = $props();

  // Calculate stats
  const duration = $derived(
    session.endedAtUtcMs !== undefined
      ? session.endedAtUtcMs - session.startedAtUtcMs
      : Date.now() - session.startedAtUtcMs
  );

  const stats = $derived(calculateSessionStats(session, duration));
  const totalDamage = $derived(stats.totalDamage);
  const dps = $derived(stats.dps);

  // Calculate enemy info
  const enemyInfo = $derived(getSessionEnemies(session));
</script>

<div
  role="button"
  tabindex="0"
  class={`group relative w-full cursor-pointer rounded-lg bg-stone-800 border-2 p-4 text-left transition-all hover:bg-stone-800/80 ${
    isActive ? "border-amber-600 shadow-lg" : "border-stone-700 shadow-md hover:border-amber-600/50"
  }`}
  onclick={() => onclick?.()}
  onkeydown={(e) => {
    if (e.key === "Enter" || e.key === " ") {
      e.preventDefault();
      onclick?.();
    }
  }}
  aria-pressed={isActive}
>
  <!-- Timestamp -->
  <p class="text-xs text-stone-400">{formatTime(session.startedAtUtcMs)}</p>

  <!-- Enemy Name (Hero) -->
  <p class="mt-2 text-2xl font-bold text-stone-300">
    {enemyInfo.primaryEnemy}
  </p>

  <!-- Enemy Count (if multiple) -->
  {#if enemyInfo.totalEnemies > 1}
    <p class="mt-1 text-xs text-stone-500">
      +{enemyInfo.totalEnemies - 1} other enemies
    </p>
  {/if}

  <!-- Stats Footer -->
  <p class="mt-2 font-mono text-xs text-stone-500">
    {formatNumber(totalDamage)} dmg · {formatDps(dps)} DPS · {formatDuration(duration)}
  </p>

  <!-- Action Buttons (appear on hover) -->
  <div class="absolute right-3 top-3 flex gap-1 opacity-0 transition-all group-hover:opacity-100">
    {#if onexport}
      <button
        type="button"
        onclick={(e) => {
          e.stopPropagation();
          onexport?.();
        }}
        class="rounded-md p-1.5 text-stone-600 transition-all hover:bg-cyan-500/20 hover:text-cyan-400"
        aria-label="Export session"
      >
        <Download class="h-4 w-4" />
      </button>
    {/if}
    {#if ondelete}
      <button
        type="button"
        onclick={(e) => {
          e.stopPropagation();
          ondelete?.();
        }}
        class="rounded-md p-1.5 text-stone-600 transition-all hover:bg-rose-500/20 hover:text-rose-400"
        aria-label="Delete session"
      >
        <X class="h-4 w-4" />
      </button>
    {/if}
  </div>
</div>
