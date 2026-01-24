<script lang="ts">
  import {
    websocketUrl,
    setWebSocketUrl,
    autoReconnect,
    setAutoReconnect,
    reconnectInterval,
    setReconnectInterval,
    settingsChanged,
    resetSettings,
  } from "$lib/state/settings.svelte";
  import { reconnectWebSocket } from "$lib/state/connection.svelte";
  import { AppSettingsSchema } from "$lib/types/schemas";
  import { Button, Checkbox, Input } from "$lib/components/ui";
  import { Text } from "$lib/components/ui/typography";
  import { AlertTriangle } from "@lucide/svelte";
  import SettingRow from "./SettingRow.svelte";

  let localUrl = $state(websocketUrl.value);
  let localInterval = $state(reconnectInterval.value);
  let urlError = $state<string | null>(null);
  let intervalError = $state<string | null>(null);

  // Validate and save URL on input
  function handleUrlChange() {
    const result = AppSettingsSchema.shape.websocket.shape.url.safeParse(localUrl);
    if (!result.success) {
      urlError = result.error.issues[0].message;
      return;
    }
    setWebSocketUrl(result.data);
    urlError = null;
  }

  // Validate and save interval on input
  function handleIntervalChange() {
    const result =
      AppSettingsSchema.shape.websocket.shape.reconnectInterval.safeParse(localInterval);
    if (!result.success) {
      intervalError = result.error.issues[0].message;
      return;
    }
    setReconnectInterval(result.data);
    intervalError = null;
  }

  function handleAutoReconnectChange(checked: boolean) {
    setAutoReconnect(checked);
  }

  function handleReconnect() {
    reconnectWebSocket();
  }

  function handleReset() {
    resetSettings();
    // Update local state to match reset values
    localUrl = websocketUrl.value;
    localInterval = reconnectInterval.value;
    urlError = null;
    intervalError = null;
    // Note: No need to call reconnectWebSocket() - the settings change
    // will trigger the $effect in +layout.svelte to recreate the client
  }
</script>

<section class="space-y-4">
  {#if settingsChanged.value}
    <div
      role="alert"
      class="flex items-center gap-3 rounded-lg border border-amber-600/50 bg-amber-900/30 p-4"
    >
      <AlertTriangle class="h-5 w-5 flex-shrink-0 text-amber-400" />
      <Text variant="warning" as="span" class="flex-1">URL changed - reconnect to apply</Text>
    </div>
  {/if}

  <SettingRow label="WebSocket URL">
    <Input
      type="url"
      bind:value={localUrl}
      oninput={handleUrlChange}
      error={!!urlError}
      class="font-mono text-sm"
      placeholder="ws://localhost:38729"
      aria-describedby={urlError ? "url-error" : undefined}
    />
    {#if urlError}
      <p id="url-error" class="mt-1 text-sm text-rose-400">{urlError}</p>
    {/if}
  </SettingRow>

  <div role="separator" class="border-t border-stone-700"></div>

  <Checkbox
    checked={autoReconnect.value}
    label="Auto-reconnect on disconnect"
    onchange={handleAutoReconnectChange}
  />

  <SettingRow label="Reconnect interval">
    <div class="flex items-center gap-2">
      <Input
        type="number"
        bind:value={localInterval}
        oninput={handleIntervalChange}
        error={!!intervalError}
        min={1000}
        max={30000}
        step={100}
        class="w-32"
        aria-describedby={intervalError ? "interval-error" : undefined}
      />
      <Text variant="muted" as="span">milliseconds</Text>
    </div>
    {#if intervalError}
      <p id="interval-error" class="mt-1 text-sm text-rose-400">{intervalError}</p>
    {/if}
  </SettingRow>

  <div role="separator" class="border-t border-stone-700"></div>

  <div class="flex gap-3">
    <Button size="sm" onclick={handleReconnect} class="flex-1">Reconnect Now</Button>
    <Button size="sm" variant="secondary" onclick={handleReset} class="flex-1"
      >Reset to Defaults</Button
    >
  </div>
</section>
