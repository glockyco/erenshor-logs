// UI preferences state with localStorage persistence
// Uses Svelte 5 runes for reactive state

import { SvelteSet } from "svelte/reactivity";
import type { SortBy, SortDirection } from "$lib/types";
import { UIPreferencesSchema } from "$lib/types/schemas";
import { STORAGE_KEYS } from "$lib/utils/constants";
import { loadFromStorage, saveToStorage } from "$lib/utils/storage";

// State
export const collapsedActors = new SvelteSet<string>();

const uiState = $state({
  sortBy: "damage" as SortBy,
  sortDirection: "desc" as SortDirection,
});

export const sortBy = {
  get value() {
    return uiState.sortBy;
  },
  set value(val: SortBy) {
    uiState.sortBy = val;
  },
};

export const sortDirection = {
  get value() {
    return uiState.sortDirection;
  },
  set value(val: SortDirection) {
    uiState.sortDirection = val;
  },
};

// SSR-safe initialization from localStorage
const stored = loadFromStorage(STORAGE_KEYS.PREFERENCES, UIPreferencesSchema);
if (stored) {
  stored.collapsedActors.forEach((id) => collapsedActors.add(id));
  uiState.sortBy = stored.sortBy;
  uiState.sortDirection = stored.sortDirection;
}

/**
 * Initialize persistence effects. Must be called from a component context.
 * Returns a cleanup function.
 */
export function initUiPersistence(): () => void {
  const cleanup = $effect.root(() => {
    // Persist to localStorage on changes
    $effect(() => {
      saveToStorage(STORAGE_KEYS.PREFERENCES, {
        collapsedActors: Array.from(collapsedActors),
        sortBy: uiState.sortBy,
        sortDirection: uiState.sortDirection,
      });
    });
  });

  return cleanup;
}

// Functions

/**
 * Toggle an actor's collapsed state in the breakdown view.
 */
export function toggleActor(actorId: string): void {
  if (collapsedActors.has(actorId)) {
    collapsedActors.delete(actorId);
  } else {
    collapsedActors.add(actorId);
  }
}

/**
 * Set the sort field for actor breakdown.
 */
export function setSortBy(field: SortBy): void {
  uiState.sortBy = field;
}

/**
 * Set the sort direction for actor breakdown.
 */
export function setSortDirection(direction: SortDirection): void {
  uiState.sortDirection = direction;
}
