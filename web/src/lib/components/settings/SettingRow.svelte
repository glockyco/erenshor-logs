<script lang="ts">
  import { Heading, Text } from "$lib/components/ui/typography";
  import type { Snippet } from "svelte";

  interface Props {
    label: string;
    helpText?: string;
    children: Snippet;
  }

  let { label, helpText, children }: Props = $props();
</script>

<div class="space-y-2">
  <div class="flex flex-col gap-1">
    <Heading level={4} variant="label">{label}</Heading>
    {#if helpText}
      <Text variant="muted">{helpText}</Text>
    {/if}
  </div>
  <div>
    {@render children()}
  </div>
</div>
