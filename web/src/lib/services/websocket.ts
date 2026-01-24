// WebSocket client with auto-reconnection

import type {
  WebSocketMessage,
  HandshakeMessage,
  SessionStartMessage,
  SessionEndMessage,
  CombatEventsMessage,
} from "$lib/types";
import { parseMessage, isParseError } from "./message-parser";
import { DEFAULT_WEBSOCKET_URL, RECONNECT_INTERVAL_MS } from "$lib/utils/constants";

export interface WebSocketCallbacks {
  onConnecting: () => void;
  onConnected: (handshake: HandshakeMessage) => void;
  onSessionStart: (message: SessionStartMessage) => void;
  onSessionEnd: (message: SessionEndMessage) => void;
  onCombatEvents: (message: CombatEventsMessage) => void;
  onDisconnected: () => void;
  onError: (
    code: "connection_failed" | "parse_error" | "unexpected_disconnect",
    message: string
  ) => void;
}

export interface WebSocketClient {
  disconnect: () => void;
  reconnect: () => void;
}

export interface WebSocketConfig {
  url?: string;
  autoReconnect?: boolean;
  reconnectInterval?: number;
}

/**
 * Create a WebSocket client that auto-connects and reconnects on disconnect.
 * Returns a client object with a disconnect method to stop reconnection.
 */
export function createWebSocketClient(
  callbacks: WebSocketCallbacks,
  config: WebSocketConfig = {}
): WebSocketClient {
  const {
    url = DEFAULT_WEBSOCKET_URL,
    autoReconnect = true,
    reconnectInterval = RECONNECT_INTERVAL_MS,
  } = config;

  let socket: WebSocket | null = null;
  let reconnectTimeout: ReturnType<typeof setTimeout> | null = null;
  let shouldReconnect = autoReconnect;

  function connect(force: boolean = false): void {
    if (!shouldReconnect && !force) {
      return;
    }

    callbacks.onConnecting();

    try {
      socket = new WebSocket(url);
    } catch (error) {
      callbacks.onError(
        "connection_failed",
        error instanceof Error ? error.message : "Failed to create WebSocket"
      );
      scheduleReconnect();
      return;
    }

    socket.onopen = () => {
      // Wait for handshake message to confirm connection
    };

    socket.onmessage = (event: MessageEvent) => {
      const result = parseMessage(event.data as string);

      if (isParseError(result)) {
        callbacks.onError("parse_error", result.message);
        return;
      }

      handleMessage(result);
    };

    socket.onerror = () => {
      // Error details not available in browser WebSocket API
      // The onclose handler will be called after this
    };

    socket.onclose = (event: CloseEvent) => {
      socket = null;
      callbacks.onDisconnected();

      if (shouldReconnect) {
        if (!event.wasClean) {
          callbacks.onError(
            "unexpected_disconnect",
            `Connection closed unexpectedly (code: ${event.code})`
          );
        }
        scheduleReconnect();
      }
    };
  }

  function handleMessage(message: WebSocketMessage): void {
    switch (message.type) {
      case "handshake":
        callbacks.onConnected(message);
        break;
      case "sessionStart":
        callbacks.onSessionStart(message);
        break;
      case "sessionEnd":
        callbacks.onSessionEnd(message);
        break;
      case "combatEvents":
        callbacks.onCombatEvents(message);
        break;
    }
  }

  function scheduleReconnect(): void {
    if (!shouldReconnect) {
      return;
    }
    reconnectTimeout = setTimeout(connect, reconnectInterval);
  }

  function disconnect(): void {
    shouldReconnect = false;

    if (reconnectTimeout) {
      clearTimeout(reconnectTimeout);
      reconnectTimeout = null;
    }

    if (socket) {
      socket.close();
      socket = null;
    }
  }

  function reconnect(): void {
    // Clear any pending reconnect timers
    if (reconnectTimeout) {
      clearTimeout(reconnectTimeout);
      reconnectTimeout = null;
    }

    // Close existing socket cleanly if it exists
    if (socket) {
      // Temporarily disable reconnection to prevent onclose from scheduling another
      shouldReconnect = false;

      // Remove event handlers to prevent duplicate events during cleanup
      socket.onopen = null;
      socket.onmessage = null;
      socket.onerror = null;
      socket.onclose = null;

      // Close the socket (works in any state: CONNECTING, OPEN, CLOSING)
      socket.close();
      socket = null;
    }

    // Restore shouldReconnect based on config
    shouldReconnect = autoReconnect;

    // Attempt new connection with force=true
    // This bypasses the shouldReconnect check, allowing manual reconnect
    // even when autoReconnect is disabled
    connect(true);
  }

  // Auto-connect on creation
  connect();

  return { disconnect, reconnect };
}
