// Configuration constants for WebSocket and localStorage

export const DEFAULT_WEBSOCKET_URL = "ws://localhost:38729";
export const RECONNECT_INTERVAL_MS = 2000;

export const STORAGE_KEYS = {
  SESSIONS: "erenshor-sessions",
  PREFERENCES: "erenshor-preferences",
  APP_SETTINGS: "erenshor-settings",
} as const;
