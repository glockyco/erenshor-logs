<script lang="ts">
  import { onMount } from "svelte";
  import { VERSION } from "$lib/version";

  let { children } = $props();

  onMount(() => {
    console.log(`[Erenshor Logs v${VERSION}] Initialized`);
  });
</script>

{@render children()}
