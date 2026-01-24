<script lang="ts">
  import {
    websocketUrl,
    setWebSocketUrl,
    settingsChanged,
    markSettingsApplied,
  } from "$lib/state/settings.svelte";
  import { AppSettingsSchema } from "$lib/types/schemas";
  import { Button, Input, QRCode } from "$lib/components/ui";
  import { AlertTriangle, QrCode } from "@lucide/svelte";
  import SettingRow from "./SettingRow.svelte";

  interface Props {
    onReconnect?: () => void;
  }

  let { onReconnect }: Props = $props();

  let localUrl = $state(websocketUrl.value);
  let error = $state<string | null>(null);
  let showQR = $state(false);

  // Validate and save URL on input
  function handleUrlChange() {
    const result = AppSettingsSchema.shape.websocket.shape.url.safeParse(localUrl);
    if (!result.success) {
      error = result.error.issues[0].message;
      return;
    }
    setWebSocketUrl(result.data);
    error = null;
  }

  function handleReconnect() {
    if (onReconnect) {
      onReconnect();
    }
    markSettingsApplied();
  }

  function toggleQR() {
    showQR = !showQR;
  }
</script>

<section aria-labelledby="connection-heading" class="space-y-4">
  <h3 id="connection-heading" class="text-lg font-semibold uppercase tracking-wide text-stone-200">
    Connection
  </h3>

  {#if settingsChanged.value}
    <div
      role="alert"
      class="flex items-center gap-3 rounded-lg border border-amber-600/50 bg-amber-900/30 p-4"
    >
      <AlertTriangle class="h-5 w-5 flex-shrink-0 text-amber-400" />
      <div class="flex-1">
        <p class="text-sm font-medium text-amber-200">Settings changed. Reconnect to apply.</p>
      </div>
      <Button variant="secondary" size="sm" onclick={handleReconnect}>Reconnect Now</Button>
    </div>
  {/if}

  <SettingRow
    label="WebSocket URL"
    helpText="Enter the WebSocket server address. Replace 'localhost' with your PC's IP for remote connections."
  >
    <Input
      type="url"
      bind:value={localUrl}
      oninput={handleUrlChange}
      error={!!error}
      class="font-mono text-sm"
      placeholder="ws://localhost:38729"
      aria-describedby={error ? "url-error" : undefined}
    />
    {#if error}
      <p id="url-error" class="mt-1 text-xs text-rose-400" role="alert">
        {error}
      </p>
    {/if}
  </SettingRow>

  <div class="flex flex-col gap-3">
    <Button variant="ghost" size="sm" onclick={toggleQR} class="self-start">
      <QrCode class="h-4 w-4" />
      {showQR ? "Hide" : "Show"} QR Code
    </Button>

    {#if showQR}
      <div class="flex justify-center">
        <QRCode data={localUrl} size={200} alt="WebSocket URL QR Code for mobile scanning" />
      </div>
      <p class="text-center text-xs text-stone-400">
        Scan with your mobile device to connect instantly
      </p>
    {/if}
  </div>
</section>
