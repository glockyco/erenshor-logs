<script lang="ts">
  import WelcomeScreen from "$lib/components/empty/WelcomeScreen.svelte";
  import SessionList from "$lib/components/session/SessionList.svelte";
  import StatsPanel from "$lib/components/combat/StatsPanel.svelte";
  import ActorTable from "$lib/components/combat/ActorTable.svelte";
  import { sessions, activeSession, activeSessionStats } from "$lib/state/sessions.svelte";

  const hasSessions = $derived(sessions.size > 0);
  const session = $derived(activeSession.value);
  const stats = $derived(activeSessionStats.value);
  const isLive = $derived(session ? !session.endTime : false);
  const duration = $derived(
    session
      ? session.endTime
        ? session.endTime - session.startTime
        : Date.now() - session.startTime
      : 0
  );
</script>

{#if !hasSessions}
  <WelcomeScreen />
{:else}
  <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
    <!-- Sidebar: Session List -->
    <div class="lg:col-span-1 space-y-6">
      <SessionList />
    </div>

    <!-- Main Content -->
    <div class="lg:col-span-2 space-y-6">
      <StatsPanel {stats} {isLive} {duration} />
      <ActorTable {stats} />
    </div>
  </div>
{/if}
