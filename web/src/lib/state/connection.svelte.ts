// WebSocket connection state
// Uses Svelte 5 runes for reactive state

import type {
  ConnectionStatus,
  ConnectionError,
  ConnectionErrorCode,
  HandshakeMessage,
} from "$lib/types";

// State
export let connectionStatus = $state<ConnectionStatus>("disconnected");
export let connectionError = $state<ConnectionError | null>(null);
export let protocolVersion = $state<string | null>(null);
export let modVersion = $state<string | null>(null);

// Functions

/**
 * Set connection status to connecting.
 */
export function setConnecting(): void {
  connectionStatus = "connecting";
  connectionError = null;
}

/**
 * Set connection status to connected with handshake data.
 */
export function setConnected(handshake: HandshakeMessage): void {
  connectionStatus = "connected";
  connectionError = null;
  protocolVersion = handshake.protocolVersion;
  modVersion = handshake.modVersion;
}

/**
 * Set connection status to disconnected.
 */
export function setDisconnected(): void {
  connectionStatus = "disconnected";
  protocolVersion = null;
  modVersion = null;
}

/**
 * Set a connection error.
 */
export function setError(code: ConnectionErrorCode, message: string): void {
  connectionError = {
    code,
    message,
    timestamp: Date.now(),
  };
}

/**
 * Clear the current connection error.
 */
export function clearError(): void {
  connectionError = null;
}
