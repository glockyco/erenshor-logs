<script lang="ts">
  import type { Session } from "$lib/types";
  import { formatTime, formatDuration, formatNumber, formatDps } from "$lib/utils";
  import { calculateSessionStats } from "$lib/services";
  import { now, subscribeToClock } from "$lib/state";
  import { clsx } from "clsx";
  import { X } from "@lucide/svelte";

  interface Props {
    session: Session;
    isActive?: boolean;
    onclick?: () => void;
    ondelete?: () => void;
  }

  let { session, isActive = false, onclick, ondelete }: Props = $props();

  // Subscribe to clock only for active sessions (no endTime)
  $effect(() => {
    if (!session.endTime) {
      return subscribeToClock();
    }
  });

  const duration = $derived(
    session.endTime ? session.endTime - session.startTime : now.value - session.startTime
  );

  const stats = $derived(calculateSessionStats(session.events, duration));
</script>

<div
  role="button"
  tabindex="0"
  class={clsx(
    "group relative w-full rounded-lg border p-4 text-left transition-all cursor-pointer",
    isActive
      ? "border-cyan-400 bg-slate-800"
      : "border-slate-700 bg-slate-900 hover:border-slate-600"
  )}
  {onclick}
  onkeydown={(e) => {
    if (e.key === "Enter" || e.key === " ") {
      e.preventDefault();
      onclick?.();
    }
  }}
>
  <div class="flex items-start justify-between">
    <div class="flex-1">
      <p class="text-sm text-slate-400">{formatTime(session.startTime)}</p>
      <p class="text-lg font-mono font-bold text-cyan-400">{formatNumber(stats.totalDamage)}</p>
      <p class="font-mono text-xs text-slate-500">
        {formatDps(stats.dps)} DPS · {formatDuration(duration)}
      </p>
    </div>
    {#if ondelete}
      <button
        type="button"
        onclick={(e) => {
          e.stopPropagation();
          ondelete?.();
        }}
        class="cursor-pointer text-slate-600 opacity-0 transition hover:text-rose-400 group-hover:opacity-100"
        aria-label="Delete session"
      >
        <X class="h-4 w-4" />
      </button>
    {/if}
  </div>
</div>
