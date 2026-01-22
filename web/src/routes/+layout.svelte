<script lang="ts">
  import "../app.css";
  import { createWebSocketClient, type WebSocketCallbacks } from "$lib/services";
  import {
    setConnecting,
    setConnected,
    setDisconnected,
    setError,
    addSession,
    endSession,
    appendEvents,
  } from "$lib/state";

  let { children } = $props();

  // Initialize WebSocket client in browser only
  $effect(() => {
    const callbacks: WebSocketCallbacks = {
      onConnecting: setConnecting,
      onConnected: (handshake) => {
        setConnected(handshake);
        if (handshake.session) {
          addSession(handshake.session);
        }
      },
      onSessionStart: (message) => addSession(message.session),
      onSessionEnd: (message) => endSession(message.sessionId, message.endTime),
      onCombatEvents: (message) => appendEvents(message.sessionId, message.events),
      onDisconnected: setDisconnected,
      onError: setError,
    };

    const client = createWebSocketClient(callbacks);

    return () => {
      client.disconnect();
    };
  });
</script>

{@render children()}
