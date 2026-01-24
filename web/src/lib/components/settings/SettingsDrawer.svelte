<script lang="ts">
  import * as Drawer from "$lib/components/ui/drawer";
  import { Button } from "$lib/components/ui";
  import { Heading } from "$lib/components/ui/typography";
  import { Settings, X } from "@lucide/svelte";
  import ConnectionSettings from "./ConnectionSettings.svelte";

  interface Props {
    open?: boolean;
  }

  let { open = $bindable(false) }: Props = $props();
</script>

<Button
  variant="ghost"
  size="icon"
  onclick={() => (open = true)}
  aria-label="Open settings"
  class="text-stone-400 hover:text-stone-200"
>
  <Settings class="h-5 w-5" />
</Button>

<Drawer.Root bind:open direction="right">
  <Drawer.Portal>
    <Drawer.Overlay />
    <Drawer.Content class="!w-full sm:!w-auto sm:max-w-md">
      <Drawer.Header class="!flex-row items-center justify-between">
        <Heading level={2} variant="section">Settings</Heading>
        <Button
          variant="ghost"
          size="icon"
          onclick={() => (open = false)}
          aria-label="Close settings"
          class="text-stone-400 hover:text-stone-200"
        >
          <X class="h-5 w-5" />
        </Button>
      </Drawer.Header>

      <div class="space-y-6 overflow-y-auto px-4 pb-4">
        <ConnectionSettings />
      </div>
    </Drawer.Content>
  </Drawer.Portal>
</Drawer.Root>
