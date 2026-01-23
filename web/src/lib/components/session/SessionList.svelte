<script lang="ts">
  import SessionCardConnected from "./SessionCard.connected.svelte";
  import { sessions, clearAllSessions } from "$lib/state/sessions.svelte";

  const sessionsList = $derived(Array.from(sessions.values()));
  const sortedSessions = $derived([...sessionsList].sort((a, b) => b.startTime - a.startTime));
</script>

<div class="space-y-3">
  <div class="flex items-center justify-between mb-4">
    <h2 class="text-sm font-semibold uppercase tracking-wider text-amber-600">
      Sessions ({sessionsList.length})
    </h2>
    {#if sessionsList.length > 0}
      <button
        type="button"
        onclick={clearAllSessions}
        class="text-xs text-stone-400 hover:text-amber-500 transition cursor-pointer"
      >
        Clear All
      </button>
    {/if}
  </div>

  {#if sessionsList.length === 0}
    <div class="py-12 text-center">
      <p class="text-sm text-stone-500">No sessions recorded</p>
      <p class="mt-1 text-xs text-stone-600">Combat data will appear here</p>
    </div>
  {:else}
    {#each sortedSessions as session (session.id)}
      <SessionCardConnected {session} />
    {/each}
  {/if}
</div>
