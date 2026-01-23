<script lang="ts">
  import "../app.css";
  import Header from "$lib/components/layout/Header.connected.svelte";
  import { createWebSocketClient, type WebSocketCallbacks } from "$lib/services";
  import {
    setConnecting,
    setConnected,
    setDisconnected,
    setError,
    addSession,
    endSession,
    appendEvents,
    initSessionsPersistence,
    initUiPersistence,
  } from "$lib/state";
  import { untrack } from "svelte";

  let { children } = $props();

  // Initialize state persistence
  $effect(() => {
    const cleanupSessions = initSessionsPersistence();
    const cleanupUi = initUiPersistence();

    return () => {
      cleanupSessions();
      cleanupUi();
    };
  });

  // Initialize WebSocket client in browser only
  $effect(() => {
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
        }),
      onSessionStart: (message) => untrack(() => addSession(message.session)),
      onSessionEnd: (message) => untrack(() => endSession(message.sessionId, message.endTime)),
      onCombatEvents: (message) => untrack(() => appendEvents(message.sessionId, message.events)),
      onDisconnected: () => untrack(() => setDisconnected()),
      onError: (code, msg) => untrack(() => setError(code, msg)),
    };

    const client = createWebSocketClient(callbacks);

    return () => {
      client.disconnect();
    };
  });
</script>

<div class="flex h-screen flex-col bg-slate-950 text-slate-100">
  <Header />
  <div class="flex-1 overflow-hidden">
    {@render children()}
  </div>
</div>
