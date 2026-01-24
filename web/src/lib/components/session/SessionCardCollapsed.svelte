<script lang="ts" module>
  // Shared state to ensure only one tooltip is open at a time
  let currentOpenId = $state<string | null>(null);
</script>

<script lang="ts">
  import * as HoverCard from "$lib/components/ui/hover-card";
  import type { Session } from "$lib/types";
  import { formatNumber, formatDps, formatDuration, getSessionEnemies } from "$lib/utils";
  import { calculateSessionStats } from "$lib/services";

  interface Props {
    session: Session;
    isActive?: boolean;
    onclick?: () => void;
  }

  let { session, isActive = false, onclick }: Props = $props();

  const instanceId = crypto.randomUUID();

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

  const isOpen = $derived(currentOpenId === instanceId);

  function handleOpenChange(open: boolean) {
    if (open) {
      currentOpenId = instanceId;
    } else if (currentOpenId === instanceId) {
      currentOpenId = null;
    }
  }
</script>

<HoverCard.Root openDelay={0} open={isOpen} onOpenChange={handleOpenChange}>
  <HoverCard.Trigger class="block w-full">
    <div
      role="button"
      tabindex="0"
      class="group relative w-full cursor-pointer rounded-lg bg-stone-800 border-2 p-3 text-center transition-all hover:bg-stone-800/80 {isActive
        ? 'border-amber-600'
        : 'border-stone-700 hover:border-amber-600/50'}"
      onclick={() => onclick?.()}
      onkeydown={(e) => {
        if (e.key === "Enter" || e.key === " ") {
          e.preventDefault();
          onclick?.();
        }
      }}
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
    </div>
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
