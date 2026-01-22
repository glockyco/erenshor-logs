// WebSocket connection state
// Uses Svelte 5 runes for reactive state

import type { HandshakeMessage } from "$lib/types/protocol";

export type ConnectionStatus = "disconnected" | "connecting" | "connected";

export interface ConnectionError {
  code: "connection_failed" | "parse_error" | "unexpected_disconnect";
  message: string;
  timestamp: number;
}

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
export function setError(error: Omit<ConnectionError, "timestamp">): void {
  connectionError = {
    ...error,
    timestamp: Date.now(),
  };
}

/**
 * Clear the current connection error.
 */
export function clearError(): void {
  connectionError = null;
}
