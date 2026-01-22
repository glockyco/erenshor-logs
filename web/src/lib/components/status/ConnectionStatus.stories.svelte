<script module>
  import { defineMeta } from "@storybook/addon-svelte-csf";
  import ConnectionStatus from "./ConnectionStatus.svelte";

  const { Story } = defineMeta({
    title: "Features/ConnectionStatus",
    component: ConnectionStatus,
    tags: ["autodocs"],
  });
</script>

<Story name="Connected">
  {#snippet template(_args)}
    <div class="bg-slate-950 p-6 rounded-lg">
      <ConnectionStatus status="connected" />
    </div>
  {/snippet}
</Story>

<Story name="Connecting">
  {#snippet template(_args)}
    <div class="bg-slate-950 p-6 rounded-lg">
      <ConnectionStatus status="connecting" />
    </div>
  {/snippet}
</Story>

<Story name="Disconnected">
  {#snippet template(_args)}
    <div class="bg-slate-950 p-6 rounded-lg">
      <ConnectionStatus status="disconnected" />
    </div>
  {/snippet}
</Story>

<Story name="All States">
  {#snippet template(_args)}
    <div class="bg-slate-950 p-6 rounded-lg space-y-4">
      <div>
        <p class="text-xs text-slate-500 mb-2">Connected:</p>
        <ConnectionStatus status="connected" />
      </div>
      <div>
        <p class="text-xs text-slate-500 mb-2">Connecting:</p>
        <ConnectionStatus status="connecting" />
      </div>
      <div>
        <p class="text-xs text-slate-500 mb-2">Disconnected:</p>
        <ConnectionStatus status="disconnected" />
      </div>
    </div>
  {/snippet}
</Story>
