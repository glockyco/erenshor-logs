<script module>
  import { defineMeta } from "@storybook/addon-svelte-csf";
  import ResponsiveLayout from "./ResponsiveLayout.svelte";

  const { Story } = defineMeta({
    title: "Layout/ResponsiveLayout",
    component: ResponsiveLayout,
    tags: ["autodocs"],
  });
</script>

<Story name="Default">
  {#snippet template(_args)}
    <div class="bg-slate-950 rounded-lg">
      <ResponsiveLayout>
        {#snippet sidebar()}
          <div class="space-y-4">
            <h2 class="text-lg font-semibold text-cyan-400 uppercase tracking-wider">Sessions</h2>
            <div class="space-y-2">
              <div class="rounded-lg border border-cyan-900/50 bg-slate-900 p-4">
                <p class="font-mono text-sm text-slate-300">Session 1</p>
                <p class="font-mono text-xs text-slate-500">12m 34s</p>
              </div>
              <div class="rounded-lg border border-slate-800 bg-slate-900 p-4">
                <p class="font-mono text-sm text-slate-300">Session 2</p>
                <p class="font-mono text-xs text-slate-500">8m 12s</p>
              </div>
              <div class="rounded-lg border border-slate-800 bg-slate-900 p-4">
                <p class="font-mono text-sm text-slate-300">Session 3</p>
                <p class="font-mono text-xs text-slate-500">5m 47s</p>
              </div>
            </div>
          </div>
        {/snippet}
        {#snippet main()}
          <div class="space-y-6">
            <h2 class="text-lg font-semibold text-cyan-400 uppercase tracking-wider">
              Session Detail
            </h2>
            <div class="grid grid-cols-2 gap-4">
              <div class="rounded-lg bg-slate-900 p-4">
                <p class="text-xs text-slate-500 uppercase tracking-wider">DPS</p>
                <p class="font-mono text-2xl font-bold text-cyan-400">156</p>
              </div>
              <div class="rounded-lg bg-slate-900 p-4">
                <p class="text-xs text-slate-500 uppercase tracking-wider">HPS</p>
                <p class="font-mono text-2xl font-bold text-emerald-400">43</p>
              </div>
              <div class="rounded-lg bg-slate-900 p-4">
                <p class="text-xs text-slate-500 uppercase tracking-wider">Total Damage</p>
                <p class="font-mono text-2xl font-bold text-slate-300">117,480</p>
              </div>
              <div class="rounded-lg bg-slate-900 p-4">
                <p class="text-xs text-slate-500 uppercase tracking-wider">Duration</p>
                <p class="font-mono text-2xl font-bold text-slate-300">12m 34s</p>
              </div>
            </div>
          </div>
        {/snippet}
      </ResponsiveLayout>
    </div>
  {/snippet}
</Story>
