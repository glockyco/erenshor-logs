// WebSocket connection state
// Uses Svelte 5 runes for reactive state

import type {
  ConnectionStatus,
  ConnectionError,
  ConnectionErrorCode,
  LiveEnvelope,
} from "$lib/types";
import type { WebSocketClient } from "$lib/services";

const state = $state({
  connectionStatus: "disconnected" as ConnectionStatus,
  connectionError: null as ConnectionError | null,
  protocolVersion: null as string | null,
  modVersion: null as string | null,
  client: null as WebSocketClient | null,
});

export const connectionStatus = {
  get value() {
    return state.connectionStatus;
  },
};

export const connectionError = {
  get value() {
    return state.connectionError;
  },
};

export const protocolVersion = {
  get value() {
    return state.protocolVersion;
  },
};

export const modVersion = {
  get value() {
    return state.modVersion;
  },
};

export function setConnecting(): void {
  state.connectionStatus = "connecting";
  state.connectionError = null;
}

export function setConnected(hello: LiveEnvelope): void {
  state.connectionStatus = "connected";
  state.connectionError = null;
  state.protocolVersion = hello.protocolVersion;
  state.modVersion =
    hello.kind === "hello"
      ? ((hello.payload as { producer?: { modVersion?: string } }).producer?.modVersion ?? null)
      : null;
}

export function setDisconnected(): void {
  state.connectionStatus = "disconnected";
  state.protocolVersion = null;
  state.modVersion = null;
}

export function setError(code: ConnectionErrorCode, message: string): void {
  state.connectionError = {
    code,
    message,
    timestamp: Date.now(),
  };
}

export function clearError(): void {
  state.connectionError = null;
}

export function setClient(client: WebSocketClient | null): void {
  state.client = client;
}

export function reconnectWebSocket(): void {
  if (state.client) {
    state.client.reconnect();
  }
}

export function resetConnectionState(): void {
  state.connectionStatus = "disconnected";
  state.connectionError = null;
  state.protocolVersion = null;
  state.modVersion = null;
  state.client = null;
}
