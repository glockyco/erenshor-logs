// WebSocket connection state
// Uses Svelte 5 runes for reactive state

import type {
  ConnectionStatus,
  ConnectionError,
  ConnectionErrorCode,
  HandshakeMessage,
} from "$lib/types";

// State
const state = $state({
  connectionStatus: "disconnected" as ConnectionStatus,
  connectionError: null as ConnectionError | null,
  protocolVersion: null as string | null,
  modVersion: null as string | null,
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

// Functions

/**
 * Set connection status to connecting.
 */
export function setConnecting(): void {
  state.connectionStatus = "connecting";
  state.connectionError = null;
}

/**
 * Set connection status to connected with handshake data.
 */
export function setConnected(handshake: HandshakeMessage): void {
  state.connectionStatus = "connected";
  state.connectionError = null;
  state.protocolVersion = handshake.protocolVersion;
  state.modVersion = handshake.modVersion;
}

/**
 * Set connection status to disconnected.
 */
export function setDisconnected(): void {
  state.connectionStatus = "disconnected";
  state.protocolVersion = null;
  state.modVersion = null;
}

/**
 * Set a connection error.
 */
export function setError(code: ConnectionErrorCode, message: string): void {
  state.connectionError = {
    code,
    message,
    timestamp: Date.now(),
  };
}

/**
 * Clear the current connection error.
 */
export function clearError(): void {
  state.connectionError = null;
}

/**
 * Reset connection state to initial values. For testing only.
 */
export function resetConnectionState(): void {
  state.connectionStatus = "disconnected";
  state.connectionError = null;
  state.protocolVersion = null;
  state.modVersion = null;
}
