<script module>
  import { defineMeta } from "@storybook/addon-svelte-csf";
  import SessionList from "./SessionList.svelte";
  import { createActiveSession, createCompletedSession, createDamageEvent } from "$lib/testing";

  const mockSessions = [
    createActiveSession({
      id: "session-1",
      startTime: Date.now() - 323000,
      events: [createDamageEvent({ amount: 1245 }), createDamageEvent({ amount: 892 })],
    }),
    createCompletedSession({
      id: "session-2",
      startTime: Date.now() - 600000,
      endTime: Date.now() - 400000,
      events: [createDamageEvent({ amount: 3421 })],
    }),
    createCompletedSession({
      id: "session-3",
      startTime: Date.now() - 900000,
      endTime: Date.now() - 800000,
      events: [createDamageEvent({ amount: 567 })],
    }),
  ];

  const noop = () => {};

  const { Story } = defineMeta({
    title: "Session/SessionList",
    component: SessionList,
    tags: ["autodocs"],
  });
</script>

<Story name="Empty State">
  {#snippet template(_args)}
    <div class="bg-slate-950 p-6 rounded-lg max-w-sm">
      <SessionList
        sessions={[]}
        activeSessionId={null}
        onSessionSelect={noop}
        onSessionDelete={noop}
        onClearAll={noop}
      />
    </div>
  {/snippet}
</Story>

<Story name="With Sessions">
  {#snippet template(_args)}
    <div class="bg-slate-950 p-6 rounded-lg max-w-sm">
      <SessionList
        sessions={mockSessions}
        activeSessionId="session-1"
        onSessionSelect={noop}
        onSessionDelete={noop}
        onClearAll={noop}
      />
    </div>
  {/snippet}
</Story>

<Story name="No Active Session">
  {#snippet template(_args)}
    <div class="bg-slate-950 p-6 rounded-lg max-w-sm">
      <SessionList
        sessions={mockSessions}
        activeSessionId={null}
        onSessionSelect={noop}
        onSessionDelete={noop}
        onClearAll={noop}
      />
    </div>
  {/snippet}
</Story>
