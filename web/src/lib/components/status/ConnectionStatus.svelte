<script lang="ts">
  import type { ConnectionStatus as Status } from "$lib/types";

  interface Props {
    status: Status;
  }

  let { status }: Props = $props();

  const statusConfig = $derived(
    {
      connected: {
        label: "Connected",
        color: "bg-lime-500",
        textColor: "text-lime-400",
      },
      connecting: {
        label: "Connecting...",
        color: "bg-amber-500",
        textColor: "text-amber-400",
      },
      disconnected: {
        label: "Disconnected",
        color: "bg-rose-500",
        textColor: "text-rose-400",
      },
    }[status]
  );
</script>

<div
  class="flex items-center gap-2"
  role="status"
  aria-live="polite"
  aria-label={statusConfig.label}
>
  <span
    class={`h-2 w-2 rounded-full ${statusConfig.color} ${status === "connecting" ? "animate-pulse" : ""}`}
    aria-hidden="true"
  ></span>
  <span class={`text-sm font-medium ${statusConfig.textColor} hidden sm:inline`}>
    {statusConfig.label}
  </span>
</div>
