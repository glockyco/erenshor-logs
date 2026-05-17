<script lang="ts">
  import { VERSION } from "$lib/version";
  import { modVersion } from "$lib/state/connection.svelte";
  import {
    simulateHealthyModConnection,
    simulateLegacyModConnectionError,
    simulateMalformedLiveFrameError,
    simulateUnexpectedDisconnect,
  } from "$lib/services/dev-tools-actions";

  let expanded = $state(false);
</script>

{#if import.meta.env.DEV}
  <div class="fixed bottom-4 right-4 z-50">
    {#if !expanded}
      <!-- Collapsed state: small badge -->
      <button
        onclick={() => (expanded = true)}
        class="bg-stone-800 border border-amber-500/50 px-3 py-1.5 rounded text-xs font-mono text-amber-400 hover:bg-stone-700 transition-colors shadow-lg"
        aria-label="Open developer tools"
      >
        DEV
      </button>
    {:else}
      <!-- Expanded state: full panel -->
      <div class="bg-stone-800 border border-amber-500/50 rounded shadow-xl min-w-64">
        <!-- Header -->
        <div class="flex items-center justify-between p-3 border-b border-stone-700">
          <span class="text-xs font-semibold text-amber-400">Dev Tools</span>
          <button
            onclick={() => (expanded = false)}
            class="text-stone-400 hover:text-stone-300 text-lg leading-none"
            aria-label="Close developer tools"
          >
            ×
          </button>
        </div>

        <!-- Content -->
        <div class="p-3 space-y-3">
          <!-- Version info -->
          <div class="text-xs text-stone-400 font-mono space-y-1">
            <div>Web: {VERSION}</div>
            <div>Mod: {modVersion.value ?? "Not connected"}</div>
          </div>

          <!-- Actions -->
          <div class="border-t border-stone-700 pt-3 space-y-2">
            <button
              onclick={simulateHealthyModConnection}
              class="w-full px-3 py-1.5 bg-stone-700 hover:bg-stone-600 rounded text-xs text-amber-400 transition-colors"
            >
              Simulate Healthy Mod
            </button>
            <button
              onclick={simulateLegacyModConnectionError}
              class="w-full px-3 py-1.5 bg-stone-700 hover:bg-stone-600 rounded text-xs text-amber-400 transition-colors"
            >
              Simulate Old Mod Error
            </button>
            <button
              onclick={simulateMalformedLiveFrameError}
              class="w-full px-3 py-1.5 bg-stone-700 hover:bg-stone-600 rounded text-xs text-amber-400 transition-colors"
            >
              Simulate Parse Error
            </button>
            <button
              onclick={simulateUnexpectedDisconnect}
              class="w-full px-3 py-1.5 bg-stone-700 hover:bg-stone-600 rounded text-xs text-amber-400 transition-colors"
            >
              Simulate Disconnect
            </button>
          </div>
        </div>
      </div>
    {/if}
  </div>
{/if}
