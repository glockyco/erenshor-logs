<script module>
  import { defineMeta } from "@storybook/addon-svelte-csf";
  import SessionCard from "./SessionCard.svelte";
  import { createActiveSession, createDamageEvent } from "$lib/testing";

  const mockSession = createActiveSession({
    id: "session-1",
    startTime: Date.now() - 323000, // 5m 23s ago
    events: [
      createDamageEvent({ amount: 45 }),
      createDamageEvent({ amount: 67 }),
      createDamageEvent({ amount: 89 }),
    ],
  });

  const { Story } = defineMeta({
    title: "Session/SessionCard",
    component: SessionCard,
    tags: ["autodocs"],
  });
</script>

<Story name="Inactive">
  {#snippet template(_args)}
    <div class="bg-slate-950 p-6 rounded-lg max-w-sm">
      <SessionCard session={mockSession} isActive={false} />
    </div>
  {/snippet}
</Story>

<Story name="Active">
  {#snippet template(_args)}
    <div class="bg-slate-950 p-6 rounded-lg max-w-sm">
      <SessionCard session={mockSession} isActive={true} />
    </div>
  {/snippet}
</Story>

<Story name="With Delete">
  {#snippet template(_args)}
    <div class="bg-slate-950 p-6 rounded-lg max-w-sm">
      <SessionCard session={mockSession} ondelete={() => {}} />
    </div>
  {/snippet}
</Story>
