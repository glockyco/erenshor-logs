<script lang="ts">
  import { connectionStatus } from "$lib/state";
  import type { ConnectionStatus } from "$lib/types";
  import { clsx } from "clsx";

  interface Props {
    status?: ConnectionStatus;
  }

  let { status }: Props = $props();

  const effectiveStatus = $derived<ConnectionStatus>(status ?? connectionStatus.value);

  const statusConfig = {
    connected: { color: "bg-emerald-500", label: "Connected", pulse: true },
    connecting: { color: "bg-yellow-500", label: "Connecting", pulse: false },
    disconnected: { color: "bg-red-500", label: "Disconnected", pulse: false },
  };

  const config = $derived(statusConfig[effectiveStatus]);
</script>

<div class="flex items-center gap-2">
  <span class={clsx("h-2 w-2 rounded-full", config.color, config.pulse && "animate-pulse")}></span>
  <span class="text-sm text-slate-400">{config.label}</span>
</div>
