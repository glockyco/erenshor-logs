<script lang="ts">
  import type { ConnectionStatus as ConnectionStatusType } from "$lib/types";
  import ConnectionStatus from "$lib/components/status/ConnectionStatus.svelte";
  import { typography, effects } from "$lib/design";

  interface Props {
    connectionStatus: ConnectionStatusType;
  }

  let { connectionStatus }: Props = $props();
</script>

<header class="sticky top-0 z-50 border-b border-slate-800 bg-slate-950/90 backdrop-blur-sm">
  <div class="container mx-auto flex items-center justify-between px-4 py-4">
    <h1 class={typography.h1 + " text-cyan-400 " + effects.glow}>Erenshor Logs</h1>
    <ConnectionStatus status={connectionStatus} />
  </div>
</header>
