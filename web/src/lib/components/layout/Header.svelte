<script lang="ts">
  import ConnectionStatus from "$lib/components/status/ConnectionStatus.svelte";
  import { connectionStatus } from "$lib/state/connection.svelte";
</script>

<header class="border-b-2 border-amber-600/50 pb-4">
  <div class="flex items-center justify-between">
    <h1 class="font-fantasy text-4xl font-bold text-amber-500">Erenshor Logs</h1>
    <ConnectionStatus status={connectionStatus.value} />
  </div>
</header>
