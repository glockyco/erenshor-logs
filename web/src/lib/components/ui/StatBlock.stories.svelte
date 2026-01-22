<script module>
  import { defineMeta } from "@storybook/addon-svelte-csf";
  import StatBlock from "./StatBlock.svelte";

  const { Story } = defineMeta({
    title: "UI/StatBlock",
    component: StatBlock,
    tags: ["autodocs"],
  });
</script>

<Story name="Basic">
  {#snippet template(_args)}
    <StatBlock label="Total Damage" value="12,458" />
  {/snippet}
</Story>

<Story name="With Positive Sublabel">
  {#snippet template(_args)}
    <StatBlock
      label="DPS"
      value="156.3"
      sublabel="+15% vs last session"
      sublabelVariant="positive"
    />
  {/snippet}
</Story>

<Story name="With Negative Sublabel">
  {#snippet template(_args)}
    <StatBlock label="Deaths" value="3" sublabel="+2 vs last session" sublabelVariant="negative" />
  {/snippet}
</Story>

<Story name="With Default Sublabel">
  {#snippet template(_args)}
    <StatBlock label="Duration" value="5m 23s" sublabel="Active combat time" />
  {/snippet}
</Story>

<Story name="Stats Grid">
  {#snippet template(_args)}
    <div class="grid grid-cols-2 gap-6 bg-slate-900 p-6 rounded-lg md:grid-cols-4">
      <StatBlock label="Total Damage" value="12,458" />
      <StatBlock label="DPS" value="156.3" sublabel="+15%" sublabelVariant="positive" />
      <StatBlock label="Total Healing" value="8,234" />
      <StatBlock label="HPS" value="103.2" sublabel="-5%" sublabelVariant="negative" />
    </div>
  {/snippet}
</Story>

<Story name="In Card Context">
  {#snippet template(_args)}
    <div class="bg-slate-900 border border-slate-700 rounded-lg p-6">
      <h3 class="text-sm font-semibold uppercase tracking-wider text-slate-300 mb-4">
        Session Summary
      </h3>
      <div class="grid grid-cols-2 gap-6">
        <StatBlock label="Total Damage" value="45,892" sublabel="23 abilities used" />
        <StatBlock
          label="Avg DPS"
          value="287.4"
          sublabel="+12% vs average"
          sublabelVariant="positive"
        />
      </div>
    </div>
  {/snippet}
</Story>
