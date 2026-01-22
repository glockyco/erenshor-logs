<script module>
  import { defineMeta } from "@storybook/addon-svelte-csf";
  import FactionFilter from "./FactionFilter.svelte";

  const { Story } = defineMeta({
    title: "UI/FactionFilter",
    component: FactionFilter,
    tags: ["autodocs"],
  });
</script>

<Story name="All Selected">
  {#snippet template(_args)}
    <div class="bg-slate-950 p-6 rounded-lg">
      <FactionFilter active="all" onFilterChange={(f) => console.log(f)} />
    </div>
  {/snippet}
</Story>

<Story name="Friendly Selected">
  {#snippet template(_args)}
    <div class="bg-slate-950 p-6 rounded-lg">
      <FactionFilter active="friendly" onFilterChange={(f) => console.log(f)} />
    </div>
  {/snippet}
</Story>

<Story name="Hostile Selected">
  {#snippet template(_args)}
    <div class="bg-slate-950 p-6 rounded-lg">
      <FactionFilter active="hostile" onFilterChange={(f) => console.log(f)} />
    </div>
  {/snippet}
</Story>

<Story name="In Dark Context">
  {#snippet template(_args)}
    <div class="bg-slate-950 p-6 rounded-lg">
      <div class="bg-slate-900 p-4 rounded-lg border border-slate-800">
        <div class="flex items-center justify-between">
          <span class="text-slate-300 text-sm font-medium">Filter Actors:</span>
          <FactionFilter active="all" onFilterChange={(f) => console.log(f)} />
        </div>
      </div>
    </div>
  {/snippet}
</Story>

<Story name="Interactive">
  {#snippet template(_args)}
    <div class="bg-slate-950 p-6 rounded-lg">
      <div class="space-y-4">
        <p class="text-slate-400 text-sm">Click buttons to see console output</p>
        <FactionFilter
          active="friendly"
          onFilterChange={(filter) => {
            console.log("Filter changed to:", filter);
            alert(`Switched to: ${filter}`);
          }}
        />
      </div>
    </div>
  {/snippet}
</Story>
