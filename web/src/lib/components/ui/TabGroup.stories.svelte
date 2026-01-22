<script module>
  import { defineMeta } from "@storybook/addon-svelte-csf";
  import TabGroup from "./TabGroup.svelte";

  const { Story } = defineMeta({
    title: "UI/TabGroup",
    component: TabGroup,
    tags: ["autodocs"],
  });

  // Actor breakdown tabs (real use case)
  const actorTabs = [
    { value: "damageDealt", label: "Damage Dealt" },
    { value: "damageTaken", label: "Damage Taken" },
    { value: "healingDone", label: "Healing Done" },
    { value: "healingReceived", label: "Healing Received" },
  ];

  // Simple binary tabs
  const binaryTabs = [
    { value: "offense", label: "Offense" },
    { value: "defense", label: "Defense" },
  ];

  // Many tabs for testing horizontal layout
  const manyTabs = [
    { value: "tab1", label: "Overview" },
    { value: "tab2", label: "Damage" },
    { value: "tab3", label: "Healing" },
    { value: "tab4", label: "Defense" },
    { value: "tab5", label: "Abilities" },
    { value: "tab6", label: "Timeline" },
  ];
</script>

<Story name="Default">
  {#snippet template(_args)}
    <div class="bg-slate-950 p-6 rounded-lg">
      <TabGroup tabs={actorTabs} active="damageDealt" onTabChange={(v) => console.log(v)} />
    </div>
  {/snippet}
</Story>

<Story name="Damage Taken Selected">
  {#snippet template(_args)}
    <div class="bg-slate-950 p-6 rounded-lg">
      <TabGroup tabs={actorTabs} active="damageTaken" onTabChange={(v) => console.log(v)} />
    </div>
  {/snippet}
</Story>

<Story name="Binary Tabs">
  {#snippet template(_args)}
    <div class="bg-slate-950 p-6 rounded-lg">
      <TabGroup tabs={binaryTabs} active="offense" onTabChange={(v) => console.log(v)} />
    </div>
  {/snippet}
</Story>

<Story name="Many Tabs">
  {#snippet template(_args)}
    <div class="bg-slate-950 p-6 rounded-lg">
      <TabGroup tabs={manyTabs} active="tab3" onTabChange={(v) => console.log(v)} />
    </div>
  {/snippet}
</Story>

<Story name="Interactive">
  {#snippet template(_args)}
    <div class="bg-slate-950 p-6 rounded-lg">
      <div class="space-y-4">
        <p class="text-slate-400 text-sm">Click tabs to see console output</p>
        <TabGroup
          tabs={actorTabs}
          active="healingDone"
          onTabChange={(value) => {
            console.log("Tab changed to:", value);
            alert(`Switched to: ${actorTabs.find((t) => t.value === value)?.label}`);
          }}
        />
      </div>
    </div>
  {/snippet}
</Story>
