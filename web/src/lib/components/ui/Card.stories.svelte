<script module>
  import { defineMeta } from "@storybook/addon-svelte-csf";
  import Card from "./Card.svelte";
  import Button from "./Button.svelte";

  const { Story } = defineMeta({
    title: "UI/Card",
    component: Card,
    tags: ["autodocs"],
  });
</script>

<Story name="Default">
  {#snippet template(_args)}
    <Card>
      <p class="text-slate-300">This is a basic card with just content. No title or actions.</p>
    </Card>
  {/snippet}
</Story>

<Story name="With Title">
  {#snippet template(_args)}
    <Card title="Session Summary">
      <p class="text-slate-300">
        Card content goes here. The title appears in a header section with uppercase styling.
      </p>
    </Card>
  {/snippet}
</Story>

<Story name="With Title and Actions">
  {#snippet template(_args)}
    <Card title="Combat Sessions">
      {#snippet actions()}
        <Button variant="ghost" size="sm">Clear All</Button>
      {/snippet}
      <p class="text-slate-300">This card has both a title and action buttons in the header.</p>
    </Card>
  {/snippet}
</Story>

<Story name="Multiple Cards">
  {#snippet template(_args)}
    <div class="space-y-4">
      <Card title="Damage Breakdown">
        <p class="text-slate-300">First card content</p>
      </Card>
      <Card title="Healing Breakdown">
        <p class="text-slate-300">Second card content</p>
      </Card>
    </div>
  {/snippet}
</Story>
