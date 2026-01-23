<script lang="ts">
  import { ChevronDown, ChevronRight, Download, AlertTriangle } from "@lucide/svelte";
  import type { Session } from "$lib/types";
  import {
    analyzeUnknownEvents,
    exportSignaturesToCSV,
    exportSignaturesToJSON,
  } from "$lib/services";
  import { formatNumber } from "$lib/utils";
  import { SvelteSet } from "svelte/reactivity";

  interface Props {
    session: Session;
    duration: number;
  }

  let { session }: Props = $props();

  // Analyze unknown events
  const analysis = $derived(analyzeUnknownEvents(session.events));
  const hasUnknown = $derived(analysis.summary.unknown > 0);

  // Track expanded signatures
  const expandedSignatures = new SvelteSet<string>();

  function toggleSignature(key: string) {
    if (expandedSignatures.has(key)) {
      expandedSignatures.delete(key);
    } else {
      expandedSignatures.add(key);
    }
  }

  function getSignatureKey(sig: (typeof analysis.signatures)[0]): string {
    return `${sig.sourceMethod}|${sig.damageType}|${sig.contextState}`;
  }

  function downloadCSV() {
    const csv = exportSignaturesToCSV(analysis.signatures);
    const blob = new Blob([csv], { type: "text/csv" });
    const url = URL.createObjectURL(blob);
    const a = document.createElement("a");
    a.href = url;
    a.download = `attribution-debug-${session.id}.csv`;
    a.click();
    URL.revokeObjectURL(url);
  }

  function downloadJSON() {
    const json = exportSignaturesToJSON(analysis.signatures, analysis.summary);
    const blob = new Blob([json], { type: "application/json" });
    const url = URL.createObjectURL(blob);
    const a = document.createElement("a");
    a.href = url;
    a.download = `attribution-debug-${session.id}.json`;
    a.click();
    URL.revokeObjectURL(url);
  }

  function copyStackTrace(stackTrace: string[]) {
    navigator.clipboard.writeText(stackTrace.join("\n"));
  }

  function getContextStateLabel(state: string): string {
    switch (state) {
      case "empty":
        return "Empty (no context)";
      case "wrong":
        return "Wrong (has context)";
      case "partial":
        return "Partial (unknown context)";
      default:
        return state;
    }
  }

  function getContextStateColor(state: string): string {
    switch (state) {
      case "empty":
        return "text-red-400";
      case "wrong":
        return "text-yellow-400";
      case "partial":
        return "text-orange-400";
      default:
        return "text-stone-400";
    }
  }
</script>

{#if hasUnknown}
  <div class="bg-stone-800 border-2 border-stone-700 rounded-lg shadow-lg">
    <!-- Header -->
    <div class="border-b border-stone-700 px-6 py-4 flex items-center justify-between">
      <div class="flex items-center gap-2">
        <AlertTriangle class="w-5 h-5 text-amber-500" />
        <h3 class="font-fantasy text-lg font-semibold text-amber-500">Attribution Debug Panel</h3>
      </div>
      <div class="flex gap-2">
        <button
          onclick={downloadCSV}
          class="flex items-center gap-1 px-3 py-1.5 text-sm text-stone-300 hover:text-stone-100 border border-stone-600 hover:border-stone-500 rounded transition"
        >
          <Download class="w-4 h-4" />
          CSV
        </button>
        <button
          onclick={downloadJSON}
          class="flex items-center gap-1 px-3 py-1.5 text-sm text-stone-300 hover:text-stone-100 border border-stone-600 hover:border-stone-500 rounded transition"
        >
          <Download class="w-4 h-4" />
          JSON
        </button>
      </div>
    </div>

    <div class="p-6 space-y-6">
      <!-- Attribution Summary -->
      <div class="grid grid-cols-4 gap-4">
        <div class="space-y-1">
          <div class="text-xs uppercase tracking-wider text-stone-400">Total Events</div>
          <div class="text-2xl font-mono font-bold text-stone-200">
            {formatNumber(analysis.summary.total)}
          </div>
        </div>
        <div class="space-y-1">
          <div class="text-xs uppercase tracking-wider text-stone-400">Attributed</div>
          <div class="text-2xl font-mono font-bold text-emerald-400">
            {formatNumber(analysis.summary.attributed)}
          </div>
          <div class="text-xs text-stone-500">
            {((analysis.summary.attributed / analysis.summary.total) * 100).toFixed(1)}%
          </div>
        </div>
        <div class="space-y-1">
          <div class="text-xs uppercase tracking-wider text-stone-400">Unknown</div>
          <div class="text-2xl font-mono font-bold text-amber-500">
            {formatNumber(analysis.summary.unknown)}
          </div>
          <div class="text-xs text-stone-500">
            {((analysis.summary.unknown / analysis.summary.total) * 100).toFixed(1)}%
          </div>
        </div>
        <div class="space-y-1">
          <div class="text-xs uppercase tracking-wider text-stone-400">Inferred</div>
          <div class="text-2xl font-mono font-bold text-cyan-400">
            {formatNumber(analysis.summary.inferred)}
          </div>
          <div class="text-xs text-stone-500">
            {((analysis.summary.inferred / analysis.summary.total) * 100).toFixed(1)}%
          </div>
        </div>
      </div>

      <!-- Unknown Signatures Table -->
      {#if analysis.signatures.length > 0}
        <div class="space-y-2">
          <h4 class="text-sm font-semibold text-stone-300 uppercase tracking-wider">
            Unknown Events by Source (sorted by impact)
          </h4>

          <div class="space-y-1">
            {#each analysis.signatures as signature (getSignatureKey(signature))}
              {@const key = getSignatureKey(signature)}
              {@const isExpanded = expandedSignatures.has(key)}
              <div class="border border-stone-700 rounded">
                <!-- Signature Row -->
                <button
                  onclick={() => toggleSignature(key)}
                  class="w-full px-4 py-3 flex items-center gap-3 hover:bg-stone-700/30 transition text-left"
                >
                  <!-- Expand Icon -->
                  {#if isExpanded}
                    <ChevronDown class="w-4 h-4 text-stone-400 flex-shrink-0" />
                  {:else}
                    <ChevronRight class="w-4 h-4 text-stone-400 flex-shrink-0" />
                  {/if}

                  <!-- Method + Damage Type -->
                  <div class="flex-1 min-w-0">
                    <div class="font-mono text-sm text-amber-400 truncate">
                      {signature.sourceMethod}
                      {#if signature.damageType}
                        <span class="text-stone-400">({signature.damageType})</span>
                      {/if}
                    </div>
                    <div class="text-xs {getContextStateColor(signature.contextState)} mt-0.5">
                      Context: {getContextStateLabel(signature.contextState)}
                    </div>
                  </div>

                  <!-- Stats -->
                  <div class="flex items-center gap-6 flex-shrink-0">
                    <div class="text-right">
                      <div class="text-sm font-mono text-stone-300">
                        {formatNumber(signature.eventCount)}
                      </div>
                      <div class="text-xs text-stone-500">events</div>
                    </div>
                    <div class="text-right">
                      <div class="text-sm font-mono text-stone-300">
                        {formatNumber(signature.totalDamage)}
                      </div>
                      <div class="text-xs text-stone-500">damage</div>
                    </div>
                  </div>
                </button>

                <!-- Expanded Details -->
                {#if isExpanded}
                  <div class="px-4 pb-4 space-y-3 border-t border-stone-700 bg-stone-900/30">
                    <!-- Common Parameters -->
                    {#if signature.commonParameters && Object.keys(signature.commonParameters).length > 0}
                      <div class="pt-3">
                        <div
                          class="text-xs font-semibold text-stone-400 uppercase tracking-wider mb-2"
                        >
                          Common Parameters
                        </div>
                        <div class="grid grid-cols-2 gap-x-4 gap-y-1">
                          {#each Object.entries(signature.commonParameters) as [key, value] (key)}
                            <div class="flex gap-2 text-sm">
                              <span class="text-stone-500">{key}:</span>
                              <span class="font-mono text-stone-300">{value}</span>
                            </div>
                          {/each}
                        </div>
                      </div>
                    {/if}

                    <!-- Stack Traces -->
                    {#if signature.uniqueStackTraces.length > 0}
                      <div>
                        <div
                          class="text-xs font-semibold text-stone-400 uppercase tracking-wider mb-2"
                        >
                          Stack Trace
                          {#if signature.uniqueStackTraces.length > 1}
                            <span class="text-stone-500">
                              ({signature.uniqueStackTraces.length} unique)
                            </span>
                          {/if}
                        </div>
                        {#each signature.uniqueStackTraces.slice(0, 1) as stackTrace, i (i)}
                          <div class="bg-stone-950 border border-stone-700 rounded p-3 space-y-1">
                            {#each stackTrace as frame, j (j)}
                              <div class="text-xs font-mono text-stone-400">{frame}</div>
                            {/each}
                            <button
                              onclick={() => copyStackTrace(stackTrace)}
                              class="mt-2 text-xs text-stone-500 hover:text-stone-300 transition"
                            >
                              Copy to Clipboard
                            </button>
                          </div>
                        {/each}
                      </div>
                    {/if}
                  </div>
                {/if}
              </div>
            {/each}
          </div>
        </div>
      {/if}
    </div>
  </div>
{/if}
