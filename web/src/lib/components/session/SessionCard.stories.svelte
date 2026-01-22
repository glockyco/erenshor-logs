<script module>
  import { defineMeta } from "@storybook/addon-svelte-csf";
  import SessionCard from "./SessionCard.svelte";

  const mockSession = {
    id: "session-1",
    startTime: Date.now() - 323000, // 5m 23s ago
    events: [
      /** @type {{ id: string, eventType: "damagePhysical", timestamp: number, source: { id: string, name: string, type: "player" }, target: { id: string, name: string, type: "npc" }, amount: number }} */
      ({
        id: "event-1",
        eventType: "damagePhysical",
        timestamp: Date.now() - 300000,
        source: { id: "player-1", name: "Player", type: "player" },
        target: { id: "enemy-1", name: "Goblin", type: "npc" },
        amount: 45,
      }),
      /** @type {{ id: string, eventType: "damagePhysical", timestamp: number, source: { id: string, name: string, type: "player" }, target: { id: string, name: string, type: "npc" }, amount: number }} */
      ({
        id: "event-2",
        eventType: "damagePhysical",
        timestamp: Date.now() - 250000,
        source: { id: "player-1", name: "Player", type: "player" },
        target: { id: "enemy-1", name: "Goblin", type: "npc" },
        amount: 67,
      }),
      /** @type {{ id: string, eventType: "damagePhysical", timestamp: number, source: { id: string, name: string, type: "player" }, target: { id: string, name: string, type: "npc" }, amount: number }} */
      ({
        id: "event-3",
        eventType: "damagePhysical",
        timestamp: Date.now() - 200000,
        source: { id: "player-1", name: "Player", type: "player" },
        target: { id: "enemy-1", name: "Goblin", type: "npc" },
        amount: 89,
      }),
    ],
  };

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
      <SessionCard session={mockSession} ondelete={() => window.alert("Delete clicked")} />
    </div>
  {/snippet}
</Story>
