<script lang="ts">
  import ConnectionStatus from "$lib/components/status/ConnectionStatus.svelte";
  import { connectionStatus } from "$lib/state/connection.svelte";
</script>

<header class="border-b-2 border-amber-600/50 pb-4">
  <h1 class="font-fantasy text-4xl font-bold text-amber-500">Erenshor Logs</h1>
  <div class="flex items-center justify-between mt-2">
    <p class="text-stone-400 text-sm">Combat Analysis & Statistics</p>
    <ConnectionStatus status={connectionStatus.value} />
  </div>
</header>
