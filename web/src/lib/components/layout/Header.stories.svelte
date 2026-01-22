<script module>
  import { defineMeta } from "@storybook/addon-svelte-csf";
  import Header from "./Header.svelte";

  const { Story } = defineMeta({
    title: "Layout/Header",
    component: Header,
    tags: ["autodocs"],
  });
</script>

<Story name="Default">
  {#snippet template(_args)}
    <div class="bg-slate-950 rounded-lg">
      <Header />
    </div>
  {/snippet}
</Story>
