<script module>
  import { defineMeta } from "@storybook/addon-svelte-csf";
  import TabSelect from "./TabSelect.svelte";

  const { Story } = defineMeta({
    title: "UI/TabSelect",
    component: TabSelect,
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

  // Many options for testing scroll
  const manyTabs = [
    { value: "tab1", label: "Overview" },
    { value: "tab2", label: "Damage Dealt" },
    { value: "tab3", label: "Damage Taken" },
    { value: "tab4", label: "Healing Done" },
    { value: "tab5", label: "Healing Received" },
    { value: "tab6", label: "Abilities" },
    { value: "tab7", label: "Timeline" },
    { value: "tab8", label: "Statistics" },
  ];
</script>

<Story name="Default">
  {#snippet template(_args)}
    <div class="bg-slate-950 p-6 rounded-lg">
      <div class="max-w-xs">
        <p class="text-slate-400 text-sm mb-3">Click to open dropdown</p>
        <TabSelect tabs={actorTabs} active="damageDealt" onTabChange={(v) => console.log(v)} />
      </div>
    </div>
  {/snippet}
</Story>

<Story name="Damage Taken Selected">
  {#snippet template(_args)}
    <div class="bg-slate-950 p-6 rounded-lg">
      <div class="max-w-xs">
        <TabSelect tabs={actorTabs} active="damageTaken" onTabChange={(v) => console.log(v)} />
      </div>
    </div>
  {/snippet}
</Story>

<Story name="Binary Options">
  {#snippet template(_args)}
    <div class="bg-slate-950 p-6 rounded-lg">
      <div class="max-w-xs">
        <TabSelect tabs={binaryTabs} active="offense" onTabChange={(v) => console.log(v)} />
      </div>
    </div>
  {/snippet}
</Story>

<Story name="Many Options">
  {#snippet template(_args)}
    <div class="bg-slate-950 p-6 rounded-lg">
      <div class="max-w-xs">
        <p class="text-slate-400 text-sm mb-3">Dropdown with 8 options</p>
        <TabSelect tabs={manyTabs} active="tab5" onTabChange={(v) => console.log(v)} />
      </div>
    </div>
  {/snippet}
</Story>

<Story name="Interactive">
  {#snippet template(_args)}
    <div class="bg-slate-950 p-6 rounded-lg">
      <div class="max-w-xs space-y-4">
        <p class="text-slate-400 text-sm">Select an option to see console output</p>
        <TabSelect
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
