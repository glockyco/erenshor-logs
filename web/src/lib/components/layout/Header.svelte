<script lang="ts">
  import { Heading } from "$lib/components/ui/typography";
  import ConnectionStatus from "$lib/components/status/ConnectionStatus.svelte";
  import { connectionStatus } from "$lib/state/connection.svelte";
</script>

<header class="border-b-2 border-amber-600/50 pb-4">
  <div class="flex items-center justify-between">
    <Heading level={1} variant="app">Erenshor Logs</Heading>
    <ConnectionStatus status={connectionStatus.value} />
  </div>
</header>
