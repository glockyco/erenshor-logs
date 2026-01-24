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

  let localUrl = $state(websocketUrl.value);
  let localInterval = $state(reconnectInterval.value);
  let urlError = $state<string | null>(null);
  let intervalError = $state<string | null>(null);

  function handleUrlChange() {
    const result = AppSettingsSchema.shape.websocket.shape.url.safeParse(localUrl);
    if (!result.success) {
      urlError = result.error.issues[0].message;
      return;
    }
    setWebSocketUrl(result.data);
    urlError = null;
  }

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
    localUrl = websocketUrl.value;
    localInterval = reconnectInterval.value;
    urlError = null;
    intervalError = null;
  }
</script>

<section class="space-y-6">
  {#if settingsChanged.value}
    <div
      role="alert"
      class="flex items-center gap-3 rounded-lg border border-amber-600/50 bg-amber-900/30 p-4"
    >
      <AlertTriangle class="h-5 w-5 flex-shrink-0 text-amber-400" />
      <Text variant="warning" as="span" class="flex-1">URL changed - reconnect to apply</Text>
    </div>
  {/if}

  <div class="space-y-2">
    <label for="websocket-url" class="block text-sm font-medium text-stone-300">
      WebSocket URL
    </label>
    <Input
      id="websocket-url"
      type="url"
      bind:value={localUrl}
      oninput={handleUrlChange}
      error={!!urlError}
      class="font-mono text-sm"
      placeholder="ws://localhost:38729"
      aria-describedby={urlError ? "url-error" : undefined}
    />
    {#if urlError}
      <p id="url-error" class="text-sm text-rose-400">{urlError}</p>
    {/if}
  </div>

  <div>
    <Checkbox
      checked={autoReconnect.value}
      label="Auto-reconnect on disconnect"
      onchange={handleAutoReconnectChange}
    />
  </div>

  <div class="space-y-2">
    <label for="reconnect-interval" class="block text-sm font-medium text-stone-300">
      Reconnect interval
    </label>
    <div class="flex items-center gap-2">
      <Input
        id="reconnect-interval"
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
      <Text variant="muted" as="span" class="text-sm">milliseconds</Text>
    </div>
    {#if intervalError}
      <p id="interval-error" class="text-sm text-rose-400">{intervalError}</p>
    {/if}
  </div>

  <div class="flex gap-3 pt-2">
    <Button size="sm" onclick={handleReconnect} class="flex-1">Reconnect Now</Button>
    <Button size="sm" variant="secondary" onclick={handleReset} class="flex-1">
      Reset to Defaults
    </Button>
  </div>
</section>
