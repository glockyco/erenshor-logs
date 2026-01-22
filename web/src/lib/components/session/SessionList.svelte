<script lang="ts">
  import type { Session } from "$lib/types";
  import SessionCard from "./SessionCard.svelte";
  import NoSessions from "$lib/components/empty/NoSessions.svelte";
  import { Button } from "$lib/components/ui";

  interface Props {
    sessions: Session[];
    activeSessionId: string | null;
    onSessionSelect: (id: string) => void;
    onSessionDelete: (id: string) => void;
    onClearAll: () => void;
  }

  let { sessions, activeSessionId, onSessionSelect, onSessionDelete, onClearAll }: Props = $props();
</script>

<div class="flex flex-col gap-4">
  {#if sessions.length === 0}
    <NoSessions />
  {:else}
    <div class="flex items-center justify-between">
      <h2 class="text-sm font-semibold uppercase tracking-wider text-slate-300">
        Sessions ({sessions.length})
      </h2>
      <Button variant="ghost" size="sm" onclick={onClearAll}>Clear All</Button>
    </div>
    <div class="space-y-2">
      {#each sessions as session (session.id)}
        <SessionCard
          {session}
          isActive={session.id === activeSessionId}
          onclick={() => onSessionSelect(session.id)}
          ondelete={() => onSessionDelete(session.id)}
        />
      {/each}
    </div>
  {/if}
</div>
