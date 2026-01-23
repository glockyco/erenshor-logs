<script lang="ts">
  import { formatDps, formatNumber } from "$lib/utils";
  import type { AbilityStats } from "$lib/types";

  interface Props {
    abilities: AbilityStats[];
    actorTotal: number;
    durationMs: number;
    mode: "damage" | "healing";
  }

  let { abilities, actorTotal, durationMs, mode }: Props = $props();

  // Filter and sort abilities based on mode
  const relevantAbilities = $derived(
    abilities
      .filter((a) => (mode === "damage" ? a.damage > 0 : a.healing > 0))
      .sort((a, b) => {
        const aVal = mode === "damage" ? a.damage : a.healing;
        const bVal = mode === "damage" ? b.damage : b.healing;
        return bVal - aVal; // Descending order
      })
  );

  const calculateDps = (amount: number): number => {
    return durationMs > 0 ? (amount / durationMs) * 1000 : 0;
  };

  const calculatePercentage = (amount: number): number => {
    return actorTotal > 0 ? (amount / actorTotal) * 100 : 0;
  };
</script>

{#if relevantAbilities.length > 0}
  <div class="py-4 px-8">
    <h4 class="mb-3 text-xs font-semibold uppercase tracking-wider text-amber-600">
      Ability Breakdown
    </h4>
    <table class="w-full text-xs">
      <thead class="border-b border-stone-700">
        <tr>
          <th class="px-3 py-2 text-left text-[10px] uppercase tracking-wider text-stone-500">
            Ability
          </th>
          <th class="px-3 py-2 text-right text-[10px] uppercase tracking-wider text-stone-500">
            Uses
          </th>
          <th class="px-3 py-2 text-right text-[10px] uppercase tracking-wider text-stone-500">
            Crits
          </th>
          <th class="px-3 py-2 text-right text-[10px] uppercase tracking-wider text-stone-500">
            Hits
          </th>
          <th class="px-3 py-2 text-right text-[10px] uppercase tracking-wider text-stone-500">
            Misses
          </th>
          <th class="px-3 py-2 text-right text-[10px] uppercase tracking-wider text-stone-500">
            Avg
          </th>
          <th class="px-3 py-2 text-right text-[10px] uppercase tracking-wider text-stone-500">
            Total
          </th>
          <th class="px-3 py-2 text-right text-[10px] uppercase tracking-wider text-stone-500">
            DPS
          </th>
          <th class="px-3 py-2 text-right text-[10px] uppercase tracking-wider text-stone-500">
            %
          </th>
        </tr>
      </thead>
      <tbody>
        {#each relevantAbilities as ability (ability.abilityName)}
          {@const amount = mode === "damage" ? ability.damage : ability.healing}
          {@const avg = mode === "damage" ? ability.avgDamage : ability.avgHealing}
          {@const uses = ability.hits + ability.crits + ability.misses}
          {@const dps = calculateDps(amount)}
          {@const percentage = calculatePercentage(amount)}
          <tr class="border-b border-stone-800 hover:bg-stone-800/30">
            <td class="px-3 py-2 text-stone-200">{ability.abilityName}</td>
            <td class="px-3 py-2 text-right font-mono text-stone-300">{uses}</td>
            <td class="px-3 py-2 text-right font-mono text-amber-400">{ability.crits}</td>
            <td class="px-3 py-2 text-right font-mono text-emerald-400">{ability.hits}</td>
            <td class="px-3 py-2 text-right font-mono text-rose-400">{ability.misses}</td>
            <td class="px-3 py-2 text-right font-mono text-stone-300">{formatNumber(avg)}</td>
            <td class="px-3 py-2 text-right font-mono text-stone-300">{formatNumber(amount)}</td>
            <td class="px-3 py-2 text-right font-mono text-amber-500">{formatDps(dps)}</td>
            <td class="px-3 py-2 text-right font-mono text-stone-300">{percentage.toFixed(1)}%</td>
          </tr>
        {/each}
      </tbody>
    </table>
  </div>
{/if}
