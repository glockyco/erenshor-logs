// UI preferences state with localStorage persistence
// Uses Svelte 5 runes for reactive state

import { SvelteSet } from "svelte/reactivity";
import { STORAGE_KEYS } from "$lib/utils/constants";

// State
export const collapsedActors = new SvelteSet<string>();
export let sortBy = $state<"damage" | "dps" | "name">("damage");
export let sortDirection = $state<"asc" | "desc">("desc");

// SSR-safe initialization from localStorage
if (typeof window !== "undefined") {
  try {
    const stored = localStorage.getItem(STORAGE_KEYS.PREFERENCES);
    if (stored) {
      const parsed = JSON.parse(stored);

      // Restore collapsed actors
      if (Array.isArray(parsed.collapsedActors)) {
        parsed.collapsedActors.forEach((id: string) => {
          collapsedActors.add(id);
        });
      }

      // Restore sort preferences
      if (parsed.sortBy) {
        sortBy = parsed.sortBy;
      }
      if (parsed.sortDirection) {
        sortDirection = parsed.sortDirection;
      }
    }
  } catch (error) {
    console.error("Failed to load UI preferences from localStorage:", error);
  }
}

// Persist to localStorage on changes
$effect(() => {
  if (typeof window === "undefined") return;

  try {
    const preferences = {
      collapsedActors: Array.from(collapsedActors),
      sortBy,
      sortDirection,
    };
    localStorage.setItem(STORAGE_KEYS.PREFERENCES, JSON.stringify(preferences));
  } catch (error) {
    console.error("Failed to save UI preferences to localStorage:", error);
  }
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
export function setSortBy(field: "damage" | "dps" | "name"): void {
  sortBy = field;
}

/**
 * Set the sort direction for actor breakdown.
 */
export function setSortDirection(direction: "asc" | "desc"): void {
  sortDirection = direction;
}
