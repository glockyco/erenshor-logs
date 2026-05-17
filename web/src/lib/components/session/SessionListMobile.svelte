<script lang="ts">
  import SessionCardConnected from "./SessionCard.connected.svelte";
  import { sessions } from "$lib/state/sessions.svelte";

  interface Props {
    onSessionSelect?: () => void;
  }

  let { onSessionSelect }: Props = $props();

  const sessionsList = $derived(Array.from(sessions.values()));
  const sortedSessions = $derived(
    [...sessionsList].sort((a, b) => b.startedAtUtcMs - a.startedAtUtcMs)
  );

  const handleSessionClick = () => {
    // Auto-close drawer when session is selected on mobile
    onSessionSelect?.();
  };
</script>

{#if sessionsList.length === 0}
  <div class="py-12 text-center">
    <p class="text-sm text-stone-500">No sessions recorded</p>
    <p class="mt-1 text-xs text-stone-600">Combat data will appear here</p>
  </div>
{:else}
  <div class="space-y-3">
    {#each sortedSessions as session (session.id)}
      <SessionCardConnected {session} onSelect={handleSessionClick} />
    {/each}
  </div>
{/if}
