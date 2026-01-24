<script lang="ts">
  import * as Drawer from "$lib/components/ui/drawer";
  import { Button } from "$lib/components/ui";
  import { Settings } from "@lucide/svelte";
  import ConnectionSettings from "./ConnectionSettings.svelte";

  interface Props {
    open?: boolean;
    onReconnect?: () => void;
  }

  let { open = $bindable(false), onReconnect }: Props = $props();
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
    <Drawer.Content class="w-full sm:max-w-md">
      <Drawer.Header>
        <Drawer.Title>Settings</Drawer.Title>
        <Drawer.Description>Configure your application preferences</Drawer.Description>
      </Drawer.Header>

      <div class="space-y-6 overflow-y-auto p-6">
        <ConnectionSettings {onReconnect} />
      </div>
    </Drawer.Content>
  </Drawer.Portal>
</Drawer.Root>
