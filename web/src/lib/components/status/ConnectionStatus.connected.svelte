<script lang="ts">
  import { connectionStatus } from "$lib/state";
  import ConnectionStatus from "./ConnectionStatus.svelte";
</script>

<ConnectionStatus status={connectionStatus.value} />
