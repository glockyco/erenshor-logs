<script lang="ts">
  import SessionCardConnected from "./SessionCard.connected.svelte";
  import SessionCardCollapsed from "./SessionCardCollapsed.svelte";
  import { sessions, setActiveSession, activeSession } from "$lib/state/sessions.svelte";

  interface Props {
    collapsed?: boolean;
  }

  let { collapsed = false }: Props = $props();

  const sessionsList = $derived(Array.from(sessions.values()));
  const sortedSessions = $derived([...sessionsList].sort((a, b) => b.startTime - a.startTime));
</script>

{#if sessionsList.length === 0}
  {#if collapsed}
    <!-- Collapsed empty state: minimal -->
    <div class="py-6 text-center">
      <p class="text-xs text-stone-600">No sessions</p>
    </div>
  {:else}
    <!-- Expanded empty state: detailed -->
    <div class="py-12 text-center">
      <p class="text-sm text-stone-500">No sessions recorded</p>
      <p class="mt-1 text-xs text-stone-600">Combat data will appear here</p>
    </div>
  {/if}
{:else if collapsed}
  <!-- Collapsed: show compact cards -->
  <div class="space-y-3">
    {#each sortedSessions as session (session.id)}
      <SessionCardCollapsed
        {session}
        isActive={activeSession.value?.id === session.id}
        onclick={() => setActiveSession(session.id)}
      />
    {/each}
  </div>
{:else}
  <!-- Expanded: show full cards -->
  <div class="space-y-3">
    {#each sortedSessions as session (session.id)}
      <SessionCardConnected {session} />
    {/each}
  </div>
{/if}
