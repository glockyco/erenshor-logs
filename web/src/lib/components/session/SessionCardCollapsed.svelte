<script lang="ts">
  import { X, Download } from "@lucide/svelte";
  import * as HoverCard from "$lib/components/ui/hover-card";
  import type { Session } from "$lib/types";
  import { formatNumber, formatDps, formatDuration, getSessionEnemies } from "$lib/utils";
  import { calculateSessionStats } from "$lib/services";

  interface Props {
    session: Session;
    isActive?: boolean;
    onclick?: () => void;
    ondelete?: () => void;
    onexport?: () => void;
  }

  let { session, isActive = false, onclick, ondelete, onexport }: Props = $props();

  // Calculate stats for tooltip
  const duration = $derived(
    session.endTime ? session.endTime - session.startTime : Date.now() - session.startTime
  );
  const stats = $derived(calculateSessionStats(session.events, duration));
  const enemyInfo = $derived(getSessionEnemies(session));
  const isLive = $derived(!session.endTime);

  // Generate initials from enemy name (max 3 chars)
  function getInitials(name: string): string {
    if (!name) return "?";

    // Split on spaces, commas, and hyphens
    const words = name.split(/[\s,-]+/).filter((w) => w.length > 0);

    if (words.length === 0) return "?";

    // Take first letter of each word
    const initials = words.map((w) => w[0].toUpperCase()).join("");

    // Truncate to max 3 chars
    return initials.length > 3 ? initials.slice(0, 3) : initials;
  }

  const initials = $derived(getInitials(enemyInfo.primaryEnemy));
</script>

<HoverCard.Root openDelay={200}>
  <HoverCard.Trigger>
    <button
      type="button"
      class="group relative w-full cursor-pointer rounded-lg bg-stone-800 border-2 p-3 text-center transition-all hover:bg-stone-800/80 {isActive
        ? 'border-amber-600 shadow-lg shadow-amber-600/20'
        : 'border-stone-700 hover:border-amber-600/50'}"
      onclick={() => onclick?.()}
      aria-label="Session: {enemyInfo.primaryEnemy}"
      aria-pressed={isActive}
    >
      <!-- Status dot (top-right corner) -->
      {#if isLive}
        <div
          class="absolute top-1 right-1 h-2 w-2 rounded-full bg-amber-500 animate-pulse"
          aria-label="Live session"
        ></div>
      {/if}

      <!-- Initials (large, centered) -->
      <div class="text-2xl font-mono font-bold text-stone-200">
        {initials}
      </div>

      <!-- Action buttons (hidden, show on hover) -->
      <div
        class="absolute inset-x-0 bottom-0 flex gap-0.5 opacity-0 transition-opacity group-hover:opacity-100"
      >
        {#if onexport}
          <button
            type="button"
            onclick={(e) => {
              e.stopPropagation();
              onexport?.();
            }}
            class="flex-1 rounded-bl-md py-1 text-stone-600 transition-all hover:bg-cyan-500/20 hover:text-cyan-400"
            aria-label="Export session"
          >
            <Download class="h-3 w-3 mx-auto" />
          </button>
        {/if}
        {#if ondelete}
          <button
            type="button"
            onclick={(e) => {
              e.stopPropagation();
              ondelete?.();
            }}
            class="flex-1 rounded-br-md py-1 text-stone-600 transition-all hover:bg-rose-500/20 hover:text-rose-400"
            aria-label="Delete session"
          >
            <X class="h-3 w-3 mx-auto" />
          </button>
        {/if}
      </div>
    </button>
  </HoverCard.Trigger>

  <HoverCard.Content side="right" align="center">
    <div class="space-y-2">
      <p class="font-semibold text-stone-100">{enemyInfo.primaryEnemy}</p>
      {#if enemyInfo.totalEnemies > 1}
        <p class="text-xs text-stone-400">+{enemyInfo.totalEnemies - 1} other enemies</p>
      {/if}
      <div class="mt-3 space-y-1 text-xs text-stone-300">
        <div class="flex justify-between">
          <span class="text-stone-400">Damage:</span>
          <span class="font-mono">{formatNumber(stats.totalDamage)}</span>
        </div>
        <div class="flex justify-between">
          <span class="text-stone-400">DPS:</span>
          <span class="font-mono">{formatDps(stats.dps)}</span>
        </div>
        <div class="flex justify-between">
          <span class="text-stone-400">Duration:</span>
          <span class="font-mono">{formatDuration(duration)}</span>
        </div>
        {#if isLive}
          <div class="mt-2 flex items-center gap-1 text-xs text-amber-500">
            <span class="w-2 h-2 rounded-full bg-amber-500 animate-pulse"></span>
            LIVE
          </div>
        {/if}
      </div>
    </div>
  </HoverCard.Content>
</HoverCard.Root>
