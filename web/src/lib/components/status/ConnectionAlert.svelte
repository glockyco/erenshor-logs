<script lang="ts">
  import { AlertTriangle, X } from "@lucide/svelte";
  import type { ConnectionError } from "$lib/types";
  import { Button } from "$lib/components/ui";
  import { getConnectionAlert } from "$lib/utils/connection-alert";
  import { cn } from "$lib/utils";
  import { fade } from "svelte/transition";

  interface Props {
    error: ConnectionError | null;
    ondismiss: () => void;
    class?: string;
  }

  let { error, ondismiss, class: className }: Props = $props();
  const alert = $derived(getConnectionAlert(error));
</script>

{#if alert}
  <div
    class={cn(
      "flex items-center gap-4 rounded-lg border border-rose-600/50 bg-rose-950/40 p-4 shadow-lg",
      className
    )}
    role="alert"
    aria-live="assertive"
    aria-labelledby="connection-alert-title"
    transition:fade={{ duration: 200 }}
  >
    <AlertTriangle class="h-5 w-5 flex-shrink-0 text-rose-300" aria-hidden="true" />

    <div class="min-w-0 flex-1 space-y-1 text-stone-100">
      <h2 id="connection-alert-title" class="text-sm font-semibold text-rose-100">
        {alert.title}
      </h2>
      <p class="text-sm text-stone-200">{alert.message}</p>
    </div>

    <Button
      variant="ghost"
      size="icon"
      onclick={ondismiss}
      aria-label="Dismiss connection alert"
      class="flex-shrink-0 text-stone-200 hover:text-stone-50"
    >
      <X class="h-4 w-4" aria-hidden="true" />
    </Button>
  </div>
{/if}
