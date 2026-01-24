<script lang="ts">
  import { formatDps, formatNumber, formatPercent } from "$lib/utils";
  import { calculateRate, calculatePercentage } from "$lib/services/combat-analyzer";
  import { Heading, Numeric } from "$lib/components/ui/typography";
  import type { AbilityStats } from "$lib/types";

  type Perspective = "damageDealt" | "damageTaken" | "healingDone" | "healingReceived";

  interface Props {
    abilities: AbilityStats[];
    actorTotal: number;
    durationMs: number;
    perspective: Perspective;
  }

  let { abilities, actorTotal, durationMs, perspective }: Props = $props();

  // Abilities already filtered in ActorTable, just sort by amount
  const sortedAbilities = $derived(
    [...abilities].sort((a, b) => {
      const aVal =
        perspective === "damageDealt" || perspective === "damageTaken" ? a.damage : a.healing;
      const bVal =
        perspective === "damageDealt" || perspective === "damageTaken" ? b.damage : b.healing;
      return bVal - aVal; // Descending
    })
  );

  // Context-aware rate label
  const rateLabel = $derived(
    perspective === "damageDealt"
      ? "DPS"
      : perspective === "damageTaken"
        ? "DTPS"
        : perspective === "healingDone"
          ? "HPS"
          : "HRPS"
  );
</script>

{#if sortedAbilities.length > 0}
  <div class="py-4 px-4 md:px-8">
    <Heading level={4} variant="label" class="mb-3">Ability Breakdown</Heading>
    <div class="overflow-x-auto -mx-4 px-4 md:mx-0 md:px-0">
      <table class="w-full text-sm min-w-[800px]">
        <thead class="border-b border-stone-700">
          <tr>
            <th class="px-3 py-2 text-left">
              <Heading level={6} variant="label" class="text-stone-500">Ability</Heading>
            </th>
            <th class="px-3 py-2 text-right">
              <Heading level={6} variant="label" class="text-stone-500">Uses</Heading>
            </th>
            <th class="px-3 py-2 text-right">
              <Heading level={6} variant="label" class="text-stone-500">Crits</Heading>
            </th>
            <th class="px-3 py-2 text-right">
              <Heading level={6} variant="label" class="text-stone-500">Hits</Heading>
            </th>
            <th class="px-3 py-2 text-right">
              <Heading level={6} variant="label" class="text-stone-500">Misses</Heading>
            </th>
            <th class="px-3 py-2 text-right">
              <Heading level={6} variant="label" class="text-stone-500">Avg</Heading>
            </th>
            <th class="px-3 py-2 text-right">
              <Heading level={6} variant="label" class="text-stone-500">Total</Heading>
            </th>
            <th class="px-3 py-2 text-right">
              <Heading level={6} variant="label" class="text-stone-500">{rateLabel}</Heading>
            </th>
            <th class="px-3 py-2 text-right">
              <Heading level={6} variant="label" class="text-stone-500">%</Heading>
            </th>
          </tr>
        </thead>
        <tbody>
          {#each sortedAbilities as ability (ability.abilityName)}
            {@const amount =
              perspective === "damageDealt" || perspective === "damageTaken"
                ? ability.damage
                : ability.healing}
            {@const avg =
              perspective === "damageDealt" || perspective === "damageTaken"
                ? ability.avgDamage
                : ability.avgHealing}
            {@const uses = ability.hits + ability.crits + ability.misses}
            {@const dps = calculateRate(amount, durationMs)}
            {@const percentage = calculatePercentage(amount, actorTotal)}
            <tr class="border-b border-stone-800 hover:bg-stone-800/30">
              <td class="px-3 py-2 text-stone-200">{ability.abilityName}</td>
              <td class="px-3 py-2 text-right">
                <Numeric variant="small" color="muted">{uses}</Numeric>
              </td>
              <td class="px-3 py-2 text-right">
                <Numeric variant="small" color="crit">{ability.crits}</Numeric>
              </td>
              <td class="px-3 py-2 text-right">
                <Numeric variant="small" color="hit">{ability.hits}</Numeric>
              </td>
              <td class="px-3 py-2 text-right">
                <Numeric variant="small" color="miss">{ability.misses}</Numeric>
              </td>
              <td class="px-3 py-2 text-right">
                <Numeric variant="small" color="muted">{formatNumber(avg)}</Numeric>
              </td>
              <td class="px-3 py-2 text-right">
                <Numeric variant="small" color="muted">{formatNumber(amount)}</Numeric>
              </td>
              <td class="px-3 py-2 text-right">
                <Numeric variant="small" color="primary">{formatDps(dps)}</Numeric>
              </td>
              <td class="px-3 py-2 text-right">
                <Numeric variant="small" color="muted">{formatPercent(percentage / 100)}</Numeric>
              </td>
            </tr>
          {/each}
        </tbody>
      </table>
    </div>
  </div>
{/if}
