<script module>
  import { defineMeta } from "@storybook/addon-svelte-csf";
  import DashboardPreview from "./DashboardPreview.svelte";

  const { Story } = defineMeta({
    title: "Features/DashboardPreview",
    component: DashboardPreview,
    tags: ["autodocs"],
  });
</script>

<Story name="Default">
  {#snippet template(_args)}
    <div class="min-h-screen bg-slate-950">
      <DashboardPreview />
    </div>
  {/snippet}
</Story>
