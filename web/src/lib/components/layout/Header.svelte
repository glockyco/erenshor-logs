<script lang="ts">
  import { Heading } from "$lib/components/ui/typography";
  import ConnectionStatus from "$lib/components/status/ConnectionStatus.svelte";
  import { SettingsDrawer } from "$lib/components/settings";
  import { connectionStatus } from "$lib/state/connection.svelte";

  let settingsOpen = $state(false);
</script>

<header class="border-b-2 border-amber-600/50 pb-4">
  <div class="flex items-center justify-between gap-2">
    <Heading level={1} variant="app" class="truncate">Erenshor Logs</Heading>
    <div class="flex items-center gap-2 sm:gap-3">
      <ConnectionStatus status={connectionStatus.value} />
      <SettingsDrawer bind:open={settingsOpen} />
    </div>
  </div>
</header>
