<script lang="ts">
  import { X } from "@lucide/svelte";
  import type { Session } from "$lib/types";
  import { formatNumber, formatDps, formatDuration, formatTime } from "$lib/utils";

  interface Props {
    session: Session;
    isActive?: boolean;
    onclick?: () => void;
    ondelete?: () => void;
  }

  let { session, isActive = false, onclick, ondelete }: Props = $props();

  // Calculate stats
  const duration = $derived(
    session.endTime
      ? new Date(session.endTime).getTime() - new Date(session.startTime).getTime()
      : Date.now() - new Date(session.startTime).getTime()
  );

  const totalDamage = $derived(
    session.events.reduce(
      (sum, e) => (e.eventType.startsWith("damage") && e.amount ? sum + e.amount : sum),
      0
    )
  );

  const dps = $derived(totalDamage / (duration / 1000));
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
  <p class="text-xs text-stone-400">{formatTime(session.startTime)}</p>

  <!-- Total Damage (Hero Number) -->
  <p class="mt-1 font-mono text-2xl font-bold text-amber-500">
    {formatNumber(totalDamage)}
  </p>

  <!-- DPS · Duration -->
  <p class="mt-1 font-mono text-xs text-stone-500">
    {formatDps(dps)} DPS · {formatDuration(duration)}
  </p>

  <!-- Delete Button (appears on hover) -->
  {#if ondelete}
    <button
      type="button"
      onclick={(e) => {
        e.stopPropagation();
        ondelete?.();
      }}
      class="absolute right-3 top-3 rounded-md p-1.5 text-stone-600 opacity-0 transition-all hover:bg-rose-500/20 hover:text-rose-400 group-hover:opacity-100"
      aria-label="Delete session"
    >
      <X class="h-4 w-4" />
    </button>
  {/if}
</div>
