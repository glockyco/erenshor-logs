// Update notification state with localStorage persistence
// Uses Svelte 5 runes for reactive state

import { modVersion } from "./connection.svelte";
import { VERSION } from "$lib/version";
import { isModOutdated } from "$lib/utils/version";
import { DismissedUpdateSchema } from "$lib/types/schemas";
import { STORAGE_KEYS } from "$lib/utils/constants";
import { loadFromStorage, saveToStorage } from "$lib/utils/storage";

// =============================================================================
// State
// =============================================================================

const state = $state({
  dismissedVersion: null as string | null,
});

// SSR-safe initialization from localStorage
// Runs at module evaluation time, before any component renders
const stored = loadFromStorage(STORAGE_KEYS.DISMISSED_UPDATE, DismissedUpdateSchema);
if (stored) {
  state.dismissedVersion = stored;
}

// =============================================================================
// Derived State
// =============================================================================

// Cross-module reactivity: reads modVersion from connection.svelte.ts
// Changes to modVersion will trigger re-evaluation of this derived state
const _updateAvailable = $derived.by(() => {
  // Not connected or no version info
  if (modVersion.value === null) return false;

  // Already dismissed for this web version
  if (state.dismissedVersion === VERSION) return false;

  // Check if mod is outdated (fail open on unparseable versions)
  return isModOutdated(modVersion.value, VERSION);
});

// =============================================================================
// Exported Getters
// =============================================================================

/**
 * Whether an update notification should be shown.
 *
 * Returns true when:
 * - Connected to a mod (modVersion is not null)
 * - Mod version is parseable and older than web version
 * - User has not dismissed this web version's update
 *
 * Returns false when:
 * - Not connected
 * - Versions match
 * - Mod or web version is unparseable (dirty/fallback builds)
 * - User has dismissed the update for this web version
 */
export const updateAvailable = {
  get value() {
    return _updateAvailable;
  },
};

/**
 * The web version that was dismissed by the user.
 *
 * Null if no dismissal has occurred.
 * Used for debugging and testing.
 */
export const dismissedVersion = {
  get value() {
    return state.dismissedVersion;
  },
};

// =============================================================================
// Exported Functions
// =============================================================================

/**
 * Dismiss the update notification for the current web version.
 *
 * Stores the current VERSION in localStorage. When a new web version
 * deploys, the dismissed version will no longer match and the banner
 * will reappear.
 *
 * This is keyed on web version (not mod version) so that users see
 * the notification again when a new update is available, even if they
 * previously dismissed it for an older web version.
 */
export function dismissUpdate(): void {
  state.dismissedVersion = VERSION;
}

/**
 * Initialize persistence effects. Must be called from a component context.
 * Sets up reactive persistence to save changes to localStorage.
 * Does NOT load data - that happens at module-level above.
 *
 * @returns cleanup function for effect disposal
 */
export function initUpdatePersistence(): () => void {
  return $effect.root(() => {
    // Persist dismissedVersion to localStorage on changes
    $effect(() => {
      if (state.dismissedVersion !== null) {
        saveToStorage(STORAGE_KEYS.DISMISSED_UPDATE, state.dismissedVersion);
      }
    });
  });
}

/**
 * Reset update state to initial values. For testing only.
 *
 * @internal
 */
export function resetUpdateState(): void {
  state.dismissedVersion = null;
}
