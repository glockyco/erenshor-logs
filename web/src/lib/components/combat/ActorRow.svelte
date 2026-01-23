<script lang="ts">
  import { formatDps, formatNumber } from "$lib/utils";

  interface ActorBreakdown {
    actorName: string;
    actorType: "player" | "simPlayer" | "npc" | "pet";
    total: number;
    dps: number;
  }

  interface Props {
    actor: ActorBreakdown;
    rank: number;
    maxValue: number;
  }

  let { actor, rank, maxValue }: Props = $props();

  const percentage = $derived(maxValue > 0 ? (actor.total / maxValue) * 100 : 0);

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
</script>

<tr class="border-b border-stone-700 hover:bg-stone-700/30">
  <!-- Rank -->
  <td class="px-3 py-3 font-mono text-stone-500">
    {rank}
  </td>

  <!-- Actor Name with Badge -->
  <td class="px-4 py-3">
    <div class="flex items-center gap-2">
      <span
        class={`inline-flex items-center gap-1 px-2 py-0.5 text-xs font-medium rounded-full ${colors.bg} ${colors.text} border ${colors.border}`}
      >
        <span class={`w-1.5 h-1.5 rounded-full ${colors.dot}`}></span>
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
    <div class="absolute inset-y-0 right-0 bg-amber-500/20" style:width="{percentage}%"></div>
    <div class="relative text-right font-mono text-stone-200">
      {percentage.toFixed(1)}%
    </div>
  </td>
</tr>
