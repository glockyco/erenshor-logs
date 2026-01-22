<script lang="ts">
  import type { Session } from "$lib/types";
  import {
    sessions,
    setActiveSession,
    activeSessionId,
    deleteSession,
    clearAllSessions,
  } from "$lib/state";
  import SessionCard from "./SessionCard.svelte";
  import NoSessions from "$lib/components/empty/NoSessions.svelte";
  import { Button } from "$lib/components/ui";

  interface Props {
    /** Optional sessions override for testing/Storybook */
    sessionsList?: Session[];
    /** Optional active session ID override for testing/Storybook */
    activeId?: string | null;
  }

  let { sessionsList, activeId }: Props = $props();

  const sortedSessions = $derived(
    sessionsList
      ? [...sessionsList].sort((a, b) => b.startTime - a.startTime)
      : Array.from(sessions.values()).sort((a, b) => b.startTime - a.startTime)
  );

  const effectiveActiveId = $derived(activeId !== undefined ? activeId : activeSessionId.value);

  function handleClearAll() {
    if (window.confirm("Delete all sessions? This cannot be undone.")) {
      clearAllSessions();
    }
  }

  function handleDelete(sessionId: string) {
    if (window.confirm("Delete this session? This cannot be undone.")) {
      deleteSession(sessionId);
    }
  }
</script>

<div class="flex flex-col gap-4">
  {#if sortedSessions.length === 0}
    <NoSessions />
  {:else}
    <div class="flex items-center justify-between">
      <h2 class="text-sm font-semibold uppercase tracking-wider text-slate-300">
        Sessions ({sortedSessions.length})
      </h2>
      <Button variant="ghost" size="sm" onclick={handleClearAll}>Clear All</Button>
    </div>
    <div class="space-y-2">
      {#each sortedSessions as session (session.id)}
        <SessionCard
          {session}
          isActive={session.id === effectiveActiveId}
          onclick={() => setActiveSession(session.id)}
          ondelete={() => handleDelete(session.id)}
        />
      {/each}
    </div>
  {/if}
</div>
