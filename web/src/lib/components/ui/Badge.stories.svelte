<script module>
  import { defineMeta } from "@storybook/addon-svelte-csf";
  import Badge from "./Badge.svelte";

  const { Story } = defineMeta({
    title: "Foundation/Badge",
    component: Badge,
    tags: ["autodocs"],
  });
</script>

<Story name="Default">
  {#snippet template(_args)}
    <Badge>Default</Badge>
  {/snippet}
</Story>

<Story name="Player">
  {#snippet template(_args)}
    <Badge variant="player">Player</Badge>
  {/snippet}
</Story>

<Story name="SimPlayer">
  {#snippet template(_args)}
    <Badge variant="simPlayer">SimPlayer</Badge>
  {/snippet}
</Story>

<Story name="NPC">
  {#snippet template(_args)}
    <Badge variant="npc">NPC</Badge>
  {/snippet}
</Story>

<Story name="Pet">
  {#snippet template(_args)}
    <Badge variant="pet">Pet</Badge>
  {/snippet}
</Story>

<Story name="All Variants">
  {#snippet template(_args)}
    <div class="flex flex-wrap gap-2">
      <Badge>Default</Badge>
      <Badge variant="player">Player</Badge>
      <Badge variant="simPlayer">SimPlayer</Badge>
      <Badge variant="npc">NPC</Badge>
      <Badge variant="pet">Pet</Badge>
    </div>
  {/snippet}
</Story>

<Story name="In Context">
  {#snippet template(_args)}
    <div class="bg-slate-900 p-4 rounded-lg">
      <div class="flex items-center gap-3 text-slate-300">
        <Badge variant="player">Player</Badge>
        <span class="font-semibold">Valorian</span>
        <span class="text-slate-500">dealt</span>
        <span class="font-mono text-cyan-400">1,234</span>
        <span class="text-slate-500">damage</span>
      </div>
    </div>
  {/snippet}
</Story>
