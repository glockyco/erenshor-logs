<script lang="ts">
  import type { Session } from "$lib/types";
  import { formatTime, formatDuration, formatNumber, formatDps } from "$lib/utils";
  import { calculateSessionStats } from "$lib/services";
  import { clsx } from "clsx";
  import { X } from "@lucide/svelte";

  interface Props {
    session: Session;
    isActive?: boolean;
    onclick?: () => void;
    ondelete?: () => void;
  }

  let { session, isActive = false, onclick, ondelete }: Props = $props();

  const duration = $derived(
    session.endTime ? session.endTime - session.startTime : Date.now() - session.startTime
  );

  const stats = $derived(calculateSessionStats(session.events, duration));
</script>

<button
  class={clsx(
    "group relative w-full rounded-lg border bg-slate-900 p-4 text-left transition-all",
    isActive
      ? "border-cyan-500/50 shadow-[0_0_20px_rgb(6_182_212_/_0.3)]"
      : "border-slate-700 hover:border-slate-600"
  )}
  {onclick}
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
      <div
        role="button"
        tabindex="0"
        onclick={(e) => {
          e.stopPropagation();
          ondelete?.();
        }}
        onkeydown={(e) => {
          if (e.key === "Enter" || e.key === " ") {
            e.preventDefault();
            e.stopPropagation();
            ondelete?.();
          }
        }}
        class="cursor-pointer text-slate-600 opacity-0 transition hover:text-rose-400 group-hover:opacity-100"
        aria-label="Delete session"
      >
        <X class="h-4 w-4" />
      </div>
    {/if}
  </div>
</button>
