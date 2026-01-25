<script lang="ts">
  import "../app.css";
  import { createWebSocketClient, type WebSocketCallbacks } from "$lib/services";
  import {
    setConnecting,
    setConnected,
    setDisconnected,
    setError,
    setClient,
    addSession,
    endSession,
    appendEvents,
    completeHydration,
    initSessionsPersistence,
    initUiPersistence,
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

  let { children } = $props();

  // Initialize state persistence and clock
  $effect(() => {
    const cleanupHydration = completeHydration();
    const cleanupSessions = initSessionsPersistence();
    const cleanupUi = initUiPersistence();
    const cleanupSettings = initSettingsPersistence();
    const cleanupClock = subscribeToClock();

    return () => {
      cleanupHydration();
      cleanupSessions();
      cleanupUi();
      cleanupSettings();
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
      onConnected: (handshake) =>
        untrack(() => {
          setConnected(handshake);
          if (handshake.session) {
            addSession(handshake.session);
          }
          // Mark settings as applied when successfully connected
          markSettingsApplied();
        }),
      onSessionStart: (message) => untrack(() => addSession(message.session)),
      onSessionEnd: (message) => untrack(() => endSession(message.sessionId, message.endTime)),
      onCombatEvents: (message) => untrack(() => appendEvents(message.sessionId, message.events)),
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
