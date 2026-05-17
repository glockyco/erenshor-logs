<script lang="ts">
  import type { ConnectionError } from "$lib/types";
  import { getConnectionAlert } from "$lib/utils/connection-alert";

  interface Props {
    error: ConnectionError | null;
    ondismiss: () => void;
  }

  let { error, ondismiss }: Props = $props();
  const alert = $derived(getConnectionAlert(error));
</script>

{#if alert}
  <section
    class="rounded-lg border border-rose-500/40 bg-rose-950/50 p-4 shadow-lg"
    role="alert"
    aria-live="assertive"
    aria-labelledby="connection-alert-title"
  >
    <div class="flex items-start justify-between gap-4">
      <div class="space-y-1">
        <h2 id="connection-alert-title" class="text-sm font-semibold text-rose-200">
          {alert.title}
        </h2>
        <p class="text-sm text-rose-100/90">{alert.message}</p>
      </div>
      <button
        type="button"
        class="rounded px-2 py-1 text-sm text-rose-100 hover:bg-rose-900/60 focus:outline-none focus:ring-2 focus:ring-rose-300"
        aria-label="Dismiss connection alert"
        onclick={ondismiss}
      >
        Dismiss
      </button>
    </div>
  </section>
{/if}
