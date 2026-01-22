<script lang="ts">
  import ResponsiveLayout from "$lib/components/layout/ResponsiveLayout.svelte";
  import SessionList from "$lib/components/session/SessionList.connected.svelte";
  import SessionStatsPanel from "$lib/components/dashboard/SessionStatsPanel.svelte";
  import ActorBreakdownTable from "$lib/components/dashboard/ActorBreakdownTable.svelte";
  import Card from "$lib/components/ui/Card.svelte";

  import { activeSession, activeSessionStats } from "$lib/state/sessions.svelte";
  import { sortBy, sortDirection, setSortBy, setSortDirection } from "$lib/state/ui.svelte";
  import { now, subscribeToClock } from "$lib/state/clock.svelte";
  import type { SortBy } from "$lib/types";

  // Active session
  const session = $derived(activeSession.value);

  // Pre-calculated stats
  const stats = $derived(activeSessionStats.value);

  // Is live
  const isLive = $derived(session?.endTime === undefined);

  // Live-updating duration
  const duration = $derived(
    session
      ? isLive
        ? now.value - session.startTime
        : (session.endTime ?? now.value) - session.startTime
      : 0
  );

  // Subscribe to clock for live sessions
  $effect(() => {
    if (isLive) {
      return subscribeToClock();
    }
  });

  // Sort actors
  const sortedActors = $derived.by(() => {
    if (!stats?.actorBreakdown) return [];

    const actors = [...stats.actorBreakdown];
    const field = sortBy.value;
    const direction = sortDirection.value;

    actors.sort((a, b) => {
      let comparison = 0;

      if (field === "name") {
        comparison = a.actorName.localeCompare(b.actorName);
      } else if (field === "dps") {
        comparison = a.dps - b.dps;
      } else if (field === "damage") {
        comparison = a.totalDamage - b.totalDamage;
      }

      return direction === "asc" ? comparison : -comparison;
    });

    return actors;
  });

  // Handle sort
  function handleSort(field: SortBy) {
    if (sortBy.value === field) {
      setSortDirection(sortDirection.value === "asc" ? "desc" : "asc");
    } else {
      setSortBy(field);
      setSortDirection("desc");
    }
  }
</script>

<ResponsiveLayout>
  {#snippet sidebar()}
    <SessionList />
  {/snippet}

  {#snippet main()}
    {#if session}
      <div class="space-y-6 p-6">
        <!-- Session Stats Panel -->
        <SessionStatsPanel {stats} {isLive} {duration} />

        <!-- Actor Breakdown Table -->
        {#if stats && sortedActors.length > 0}
          <Card title="Actor Breakdown">
            <ActorBreakdownTable
              actors={sortedActors}
              sortBy={sortBy.value}
              sortDirection={sortDirection.value}
              onSort={handleSort}
            />
          </Card>
        {/if}
      </div>
    {/if}
  {/snippet}
</ResponsiveLayout>
