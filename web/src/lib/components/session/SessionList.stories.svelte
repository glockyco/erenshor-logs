<script module>
  import { defineMeta } from "@storybook/addon-svelte-csf";
  import SessionList from "./SessionList.svelte";

  /** @type {Array<{ id: string, startTime: number, endTime?: number, events: Array<{ id: string, eventType: "damagePhysical", timestamp: number, source: { id: string, name: string, type: "player" }, target: { id: string, name: string, type: "npc" }, amount: number }> }>} */
  const mockSessions = [
    {
      id: "session-1",
      startTime: Date.now() - 323000,
      events: [
        {
          id: "event-1",
          eventType: "damagePhysical",
          timestamp: Date.now() - 300000,
          source: { id: "player-1", name: "Player", type: "player" },
          target: { id: "enemy-1", name: "Goblin", type: "npc" },
          amount: 1245,
        },
        {
          id: "event-2",
          eventType: "damagePhysical",
          timestamp: Date.now() - 250000,
          source: { id: "player-1", name: "Player", type: "player" },
          target: { id: "enemy-1", name: "Goblin", type: "npc" },
          amount: 892,
        },
      ],
    },
    {
      id: "session-2",
      startTime: Date.now() - 600000,
      endTime: Date.now() - 400000,
      events: [
        {
          id: "event-3",
          eventType: "damagePhysical",
          timestamp: Date.now() - 550000,
          source: { id: "player-1", name: "Player", type: "player" },
          target: { id: "enemy-2", name: "Orc", type: "npc" },
          amount: 3421,
        },
      ],
    },
    {
      id: "session-3",
      startTime: Date.now() - 900000,
      endTime: Date.now() - 800000,
      events: [
        {
          id: "event-4",
          eventType: "damagePhysical",
          timestamp: Date.now() - 850000,
          source: { id: "player-1", name: "Player", type: "player" },
          target: { id: "enemy-3", name: "Troll", type: "npc" },
          amount: 567,
        },
      ],
    },
  ];

  const { Story } = defineMeta({
    title: "Session/SessionList",
    component: SessionList,
    tags: ["autodocs"],
  });
</script>

<Story name="Empty State">
  {#snippet template(_args)}
    <div class="bg-slate-950 p-6 rounded-lg max-w-sm">
      <SessionList sessionsList={[]} />
    </div>
  {/snippet}
</Story>

<Story name="With Sessions">
  {#snippet template(_args)}
    <div class="bg-slate-950 p-6 rounded-lg max-w-sm">
      <SessionList sessionsList={mockSessions} activeId="session-1" />
    </div>
  {/snippet}
</Story>
