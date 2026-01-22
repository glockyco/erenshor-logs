<script module>
  import { defineMeta } from "@storybook/addon-svelte-csf";
  import Header from "./Header.svelte";

  const { Story } = defineMeta({
    title: "Layout/Header",
    component: Header,
    tags: ["autodocs"],
  });
</script>

<Story name="Connected">
  {#snippet template(_args)}
    <div class="bg-slate-950 rounded-lg">
      <Header connectionStatus="connected" />
    </div>
  {/snippet}
</Story>

<Story name="Connecting">
  {#snippet template(_args)}
    <div class="bg-slate-950 rounded-lg">
      <Header connectionStatus="connecting" />
    </div>
  {/snippet}
</Story>

<Story name="Disconnected">
  {#snippet template(_args)}
    <div class="bg-slate-950 rounded-lg">
      <Header connectionStatus="disconnected" />
    </div>
  {/snippet}
</Story>
