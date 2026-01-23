<script lang="ts">
  import { formatDps, formatNumber, formatPercent } from "$lib/utils";
  import { getActorIcon } from "$lib/utils/actor-icons";
  import AbilityBreakdownTable from "./AbilityBreakdownTable.svelte";
  import type { AbilityStats } from "$lib/types";

  type Perspective = "damageDealt" | "damageTaken" | "healingDone" | "healingReceived";

  interface ActorBreakdown {
    actorName: string;
    actorType: "player" | "simPlayer" | "npc" | "pet";
    actorClass?: string;
    total: number;
    dps: number;
    percentage: number;
    abilityBreakdown: AbilityStats[];
  }

  interface Props {
    actor: ActorBreakdown;
    rank: number;
    durationMs: number;
    perspective: Perspective;
    expanded: boolean;
    onToggleExpand: () => void;
  }

  let { actor, rank, durationMs, perspective, expanded, onToggleExpand }: Props = $props();

  // Badge color mapping
  const badgeColors = {
    player: {
      bg: "bg-amber-500/20",
      text: "text-amber-400",
      border: "border-amber-500/30",
      dot: "bg-amber-400",
    },
    simPlayer: {
      bg: "bg-lime-500/20",
      text: "text-lime-400",
      border: "border-lime-500/30",
      dot: "bg-lime-400",
    },
    npc: {
      bg: "bg-rose-500/20",
      text: "text-rose-400",
      border: "border-rose-500/30",
      dot: "bg-rose-400",
    },
    pet: {
      bg: "bg-violet-500/20",
      text: "text-violet-400",
      border: "border-violet-500/30",
      dot: "bg-violet-400",
    },
  };

  const colors = $derived(badgeColors[actor.actorType]);
  const IconComponent = $derived(getActorIcon(actor.actorType, actor.actorClass));
</script>

<!-- Main Actor Row -->
<tr class={`border-b border-stone-700 ${expanded ? "bg-stone-800/50" : "hover:bg-stone-700/30"}`}>
  <!-- Expand/Collapse Button -->
  <td class="px-3 py-3">
    {#if actor.abilityBreakdown && actor.abilityBreakdown.length > 0}
      <button
        type="button"
        class="text-amber-500 hover:text-amber-400 transition-colors cursor-pointer"
        onclick={onToggleExpand}
        aria-label={expanded ? "Collapse abilities" : "Expand abilities"}
      >
        {expanded ? "▼" : "▶"}
      </button>
    {/if}
  </td>

  <!-- Rank -->
  <td class="px-3 py-3 font-mono text-stone-500">
    {rank}
  </td>

  <!-- Actor Name with Icon Badge -->
  <td class="px-4 py-3">
    <div class="flex items-center gap-2">
      <span
        class={`inline-flex items-center gap-1.5 px-2 py-1 text-xs font-medium rounded-full ${colors.bg} ${colors.text} border ${colors.border}`}
      >
        <IconComponent class="h-4 w-4" />
      </span>
      <span class="font-medium text-stone-200">{actor.actorName}</span>
    </div>
  </td>

  <!-- DPS -->
  <td class="px-4 py-3 text-right font-mono font-semibold text-amber-500">
    {formatDps(actor.dps)}
  </td>

  <!-- Total -->
  <td class="px-4 py-3 text-right font-mono text-stone-200">
    {formatNumber(actor.total)}
  </td>

  <!-- Percentage Bar -->
  <td class="px-4 py-3 relative">
    <div class="absolute inset-y-0 right-0 bg-amber-500/20" style:width="{actor.percentage}%"></div>
    <div class="relative text-right font-mono text-stone-200">
      {formatPercent(actor.percentage / 100)}
    </div>
  </td>
</tr>

<!-- Ability Breakdown Row (conditionally rendered) -->
{#if expanded && actor.abilityBreakdown && actor.abilityBreakdown.length > 0}
  <tr class="bg-stone-900/50">
    <td colspan="6" class="px-3 py-0">
      <AbilityBreakdownTable
        abilities={actor.abilityBreakdown}
        actorTotal={actor.total}
        {durationMs}
        {perspective}
      />
    </td>
  </tr>
{/if}
