<script module>
  import { defineMeta } from "@storybook/addon-svelte-csf";
  import NoSessions from "./NoSessions.svelte";

  const { Story } = defineMeta({
    title: "Empty/NoSessions",
    component: NoSessions,
    tags: ["autodocs"],
  });
</script>

<Story name="Default">
  {#snippet template(_args)}
    <div class="bg-slate-950 p-6 rounded-lg">
      <NoSessions />
    </div>
  {/snippet}
</Story>
