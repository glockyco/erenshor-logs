<script module>
  import { defineMeta } from "@storybook/addon-svelte-csf";
  import SessionStatsPanel from "./SessionStatsPanel.svelte";
  import { createActiveSession, createDamageEvent, createHealEvent } from "$lib/testing";
  import { calculateSessionStats } from "$lib/services";

  // Create mock session with realistic data
  // Typical DPS: 10-15k per character over ~5 minute fight
  const duration = 323000; // 5m 23s

  // Simulate ~12.5k DPS over 323 seconds = ~4,037,500 total damage
  const mockEvents = [
    ...Array.from(
      { length: 800 },
      () => createDamageEvent({ amount: Math.floor(Math.random() * 8000) + 2000 }) // 2k-10k per hit
    ),
    ...Array.from(
      { length: 100 },
      () => createHealEvent({ amount: Math.floor(Math.random() * 1500) + 500 }) // 500-2k per heal
    ),
  ];

  const session = createActiveSession({
    startTime: Date.now() - duration,
    events: mockEvents,
  });

  const stats = calculateSessionStats(session.events, duration);

  // High DPS test (~15k DPS)
  const highDuration = 600000; // 10 minutes
  const highNumberEvents = Array.from(
    { length: 1800 },
    () => createDamageEvent({ amount: Math.floor(Math.random() * 8000) + 3000 }) // 3k-11k per hit
  );

  const highSession = createActiveSession({
    startTime: Date.now() - highDuration,
    events: highNumberEvents,
  });

  const highStats = calculateSessionStats(highSession.events, highDuration);

  // Low activity (~2k DPS)
  const lowDuration = 120000; // 2 minutes
  const lowEvents = Array.from({ length: 50 }, () =>
    createDamageEvent({ amount: Math.floor(Math.random() * 8000) + 1000 })
  );

  const lowSession = createActiveSession({
    startTime: Date.now() - lowDuration,
    events: lowEvents,
  });

  const lowStats = calculateSessionStats(lowSession.events, lowDuration);

  const { Story } = defineMeta({
    title: "Dashboard/SessionStatsPanel",
    component: SessionStatsPanel,
    tags: ["autodocs"],
  });
</script>

<Story name="Active Combat">
  {#snippet template(_args)}
    <div class="bg-slate-950 p-6">
      <SessionStatsPanel {stats} isLive={true} duration={323000} />
    </div>
  {/snippet}
</Story>

<Story name="Completed Session">
  {#snippet template(_args)}
    <div class="bg-slate-950 p-6">
      <SessionStatsPanel {stats} isLive={false} duration={323000} />
    </div>
  {/snippet}
</Story>

<Story name="Low Activity">
  {#snippet template(_args)}
    <div class="bg-slate-950 p-6">
      <SessionStatsPanel stats={lowStats} isLive={true} duration={120000} />
    </div>
  {/snippet}
</Story>

<Story name="High Numbers">
  {#snippet template(_args)}
    <div class="bg-slate-950 p-6">
      <SessionStatsPanel stats={highStats} isLive={false} duration={600000} />
    </div>
  {/snippet}
</Story>

<Story name="No Data">
  {#snippet template(_args)}
    <div class="bg-slate-950 p-6">
      <SessionStatsPanel stats={null} isLive={false} duration={0} />
    </div>
  {/snippet}
</Story>
