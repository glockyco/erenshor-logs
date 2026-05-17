<script lang="ts">
  import "../app.css";
  import { createWebSocketClient, type WebSocketCallbacks } from "$lib/services";
  import {
    setConnecting,
    setConnected,
    setDisconnected,
    setError,
    setClient,
    applyLiveEnvelope,
    completeHydration,
    initSessionsPersistence,
    initUiPersistence,
    initUpdatePersistence,
    subscribeToClock,
  } from "$lib/state";
  import {
    websocketUrl,
    autoReconnect,
    reconnectInterval,
    markSettingsApplied,
    initSettingsPersistence,
  } from "$lib/state/settings.svelte";
  import { untrack } from "svelte";
  import DevToolsPanel from "$lib/components/debug/DevToolsPanel.svelte";

  let { children } = $props();

  // Initialize state persistence and clock
  // Data is already loaded at module-level in each state file
  // These functions only set up reactive persistence (saving to localStorage)
  $effect(() => {
    const cleanupHydration = completeHydration();
    const cleanupSessions = initSessionsPersistence();
    const cleanupUi = initUiPersistence();
    const cleanupSettings = initSettingsPersistence();
    const cleanupUpdate = initUpdatePersistence();
    const cleanupClock = subscribeToClock();

    return () => {
      cleanupHydration();
      cleanupSessions();
      cleanupUi();
      cleanupSettings();
      cleanupUpdate();
      cleanupClock();
    };
  });

  // Initialize WebSocket client in browser only
  // Recreate client when URL or connection settings change
  $effect(() => {
    // Track settings to reactively recreate client when they change
    const currentUrl = websocketUrl.value;
    const currentAutoReconnect = autoReconnect.value;
    const currentReconnectInterval = reconnectInterval.value;

    // Use untrack to prevent callbacks from being tracked as dependencies
    // Without this, state mutations in callbacks trigger infinite reruns
    const callbacks: WebSocketCallbacks = {
      onConnecting: () => untrack(() => setConnecting()),
      onConnected: (hello) =>
        untrack(() => {
          setConnected(hello);
          markSettingsApplied();
        }),
      onFrame: (message) => untrack(() => applyLiveEnvelope(message)),
      onDisconnected: () => untrack(() => setDisconnected()),
      onError: (code, msg) => untrack(() => setError(code, msg)),
    };

    // Create client with current settings
    const client = createWebSocketClient(callbacks, {
      url: currentUrl,
      autoReconnect: currentAutoReconnect,
      reconnectInterval: currentReconnectInterval,
    });

    // Store client reference so it can be manually reconnected
    setClient(client);

    return () => {
      setClient(null);
      client.disconnect();
    };
  });
</script>

{@render children()}

<DevToolsPanel />
