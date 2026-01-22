<script lang="ts">
  import { connectionStatus } from "$lib/state";
  import Header from "./Header.svelte";
</script>

<Header connectionStatus={connectionStatus.value} />
