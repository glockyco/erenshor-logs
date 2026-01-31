// App settings state with localStorage persistence

import { AppSettingsSchema } from "$lib/types/schemas";
import { STORAGE_KEYS, DEFAULT_WEBSOCKET_URL, RECONNECT_INTERVAL_MS } from "$lib/utils/constants";
import { loadFromStorage, saveToStorage } from "$lib/utils/storage";

// Internal state
const state = $state({
  websocket: {
    url: DEFAULT_WEBSOCKET_URL,
    autoReconnect: true,
    reconnectInterval: RECONNECT_INTERVAL_MS,
  },
  // Track the URL that's currently active (connected)
  activeUrl: DEFAULT_WEBSOCKET_URL,
});

// =============================================================================
// Exported Getters
// =============================================================================

export const websocketUrl = {
  get value() {
    return state.websocket.url;
  },
};

export const autoReconnect = {
  get value() {
    return state.websocket.autoReconnect;
  },
};

export const reconnectInterval = {
  get value() {
    return state.websocket.reconnectInterval;
  },
};

export const activeWebsocketUrl = {
  get value() {
    return state.activeUrl;
  },
};

export const settingsChanged = {
  get value() {
    return state.websocket.url !== state.activeUrl;
  },
};

// =============================================================================
// Exported Setters
// =============================================================================

export function setWebSocketUrl(url: string): void {
  state.websocket.url = url;
}

export function setAutoReconnect(enabled: boolean): void {
  state.websocket.autoReconnect = enabled;
}

export function setReconnectInterval(ms: number): void {
  state.websocket.reconnectInterval = ms;
}

export function markSettingsApplied(): void {
  state.activeUrl = state.websocket.url;
}

export function resetSettings(): void {
  state.websocket = {
    url: DEFAULT_WEBSOCKET_URL,
    autoReconnect: true,
    reconnectInterval: RECONNECT_INTERVAL_MS,
  };
  state.activeUrl = DEFAULT_WEBSOCKET_URL;
}

// =============================================================================
// Persistence
// =============================================================================

/**
 * Initialize settings persistence.
 *
 * Loads stored settings from localStorage once, then sets up reactive
 * persistence to save changes automatically.
 *
 * Must be called from a component context (uses $effect).
 *
 * @returns cleanup function for effect disposal
 */
export function initSettingsPersistence(): () => void {
  // Load from localStorage (runs once on initialization)
  const stored = loadFromStorage(STORAGE_KEYS.APP_SETTINGS, AppSettingsSchema);
  if (stored) {
    state.websocket = stored.websocket;
    state.activeUrl = stored.websocket.url;
  }

  // Set up reactive persistence (runs on changes)
  return $effect.root(() => {
    $effect(() => {
      saveToStorage(STORAGE_KEYS.APP_SETTINGS, {
        websocket: {
          url: state.websocket.url,
          autoReconnect: state.websocket.autoReconnect,
          reconnectInterval: state.websocket.reconnectInterval,
        },
      });
    });
  });
}
