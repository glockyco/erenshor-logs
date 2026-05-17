// WebSocket client with auto-reconnection

import type { LiveEnvelope } from "$lib/types";
import { parseMessage, isParseError } from "./message-parser";
import { DEFAULT_WEBSOCKET_URL, RECONNECT_INTERVAL_MS } from "$lib/utils/constants";

export interface WebSocketCallbacks {
  onConnecting: () => void;
  onConnected: (hello: LiveEnvelope) => void;
  onFrame: (message: LiveEnvelope) => void;
  onDisconnected: () => void;
  onError: (
    code: "connection_failed" | "parse_error" | "legacy_mod" | "unexpected_disconnect",
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
  let receivedHello = false;

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
      // Wait for hello frame to confirm connection.
    };

    socket.onmessage = (event: MessageEvent) => {
      const result = parseMessage(event.data as string);

      if (isParseError(result)) {
        callbacks.onError(
          result.code === "legacy_mod" ? "legacy_mod" : "parse_error",
          result.message
        );
        return;
      }

      handleMessage(result);
    };

    socket.onerror = () => {
      // Error details are unavailable in the browser WebSocket API.
    };

    socket.onclose = (event: CloseEvent) => {
      socket = null;
      callbacks.onDisconnected();

      if (shouldReconnect) {
        if (!event.wasClean && receivedHello) {
          callbacks.onError(
            "unexpected_disconnect",
            `Connection closed unexpectedly (code: ${event.code})`
          );
        }
        scheduleReconnect();
      }
    };
  }

  function handleMessage(message: LiveEnvelope): void {
    if (message.kind === "hello") {
      receivedHello = true;
      callbacks.onConnected(message);
    }
    callbacks.onFrame(message);
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
    if (reconnectTimeout) {
      clearTimeout(reconnectTimeout);
      reconnectTimeout = null;
    }

    if (socket) {
      shouldReconnect = false;

      socket.onopen = null;
      socket.onmessage = null;
      socket.onerror = null;
      socket.onclose = null;

      socket.close();
      socket = null;
    }

    shouldReconnect = autoReconnect;
    connect(true);
  }

  connect();

  return { disconnect, reconnect };
}
