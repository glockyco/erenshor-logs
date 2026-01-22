// UI preferences state with localStorage persistence
// Uses Svelte 5 runes for reactive state

import { SvelteSet } from "svelte/reactivity";
import type { SortBy, SortDirection } from "$lib/types";
import { UIPreferencesSchema } from "$lib/types/schemas";
import { STORAGE_KEYS } from "$lib/utils/constants";
import { loadFromStorage, saveToStorage } from "$lib/utils/storage";

// State
export const collapsedActors = new SvelteSet<string>();
export let sortBy = $state<SortBy>("damage");
export let sortDirection = $state<SortDirection>("desc");

// SSR-safe initialization from localStorage
const stored = loadFromStorage(STORAGE_KEYS.PREFERENCES, UIPreferencesSchema);
if (stored) {
  stored.collapsedActors.forEach((id) => collapsedActors.add(id));
  sortBy = stored.sortBy;
  sortDirection = stored.sortDirection;
}

// Persist to localStorage on changes
$effect(() => {
  saveToStorage(STORAGE_KEYS.PREFERENCES, {
    collapsedActors: Array.from(collapsedActors),
    sortBy,
    sortDirection,
  });
});

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
  sortBy = field;
}

/**
 * Set the sort direction for actor breakdown.
 */
export function setSortDirection(direction: SortDirection): void {
  sortDirection = direction;
}
