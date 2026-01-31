// UI preferences state with localStorage persistence
// Uses Svelte 5 runes for reactive state

import { SvelteSet } from "svelte/reactivity";
import type { SortBy, SortDirection, ActorBreakdownTab, FactionFilter } from "$lib/types";
import { UIPreferencesSchema } from "$lib/types/schemas";
import { STORAGE_KEYS } from "$lib/utils/constants";
import { loadFromStorage, saveToStorage } from "$lib/utils/storage";

// State
export const collapsedActors = new SvelteSet<string>();

const uiState = $state({
  sortBy: "damage" as SortBy,
  sortDirection: "desc" as SortDirection,
  actorBreakdownTab: "damageDealt" as ActorBreakdownTab,
  factionFilter: "all" as FactionFilter,
  sidebarCollapsed: true,
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

export const actorBreakdownTab = {
  get value() {
    return uiState.actorBreakdownTab;
  },
  set value(val: ActorBreakdownTab) {
    uiState.actorBreakdownTab = val;
  },
};

export const factionFilter = {
  get value() {
    return uiState.factionFilter;
  },
  set value(val: FactionFilter) {
    uiState.factionFilter = val;
  },
};

export const sidebarCollapsed = {
  get value() {
    return uiState.sidebarCollapsed;
  },
  toggle() {
    uiState.sidebarCollapsed = !uiState.sidebarCollapsed;
  },
};

// SSR-safe initialization from localStorage
// Runs at module evaluation time, before any component renders
const stored = loadFromStorage(STORAGE_KEYS.PREFERENCES, UIPreferencesSchema);
if (stored) {
  stored.collapsedActors.forEach((id) => collapsedActors.add(id));
  uiState.sortBy = stored.sortBy;
  uiState.sortDirection = stored.sortDirection;
  uiState.actorBreakdownTab = stored.actorBreakdownTab;
  uiState.factionFilter = stored.factionFilter;
  if (stored.sidebarCollapsed !== undefined) {
    uiState.sidebarCollapsed = stored.sidebarCollapsed;
  }
}

/**
 * Initialize persistence effects. Must be called from a component context.
 * Sets up reactive persistence to save changes to localStorage.
 * Does NOT load data - that happens at module-level above.
 *
 * @returns cleanup function for effect disposal
 */
export function initUiPersistence(): () => void {
  const cleanup = $effect.root(() => {
    // Persist to localStorage on changes
    $effect(() => {
      saveToStorage(STORAGE_KEYS.PREFERENCES, {
        collapsedActors: Array.from(collapsedActors),
        sortBy: uiState.sortBy,
        sortDirection: uiState.sortDirection,
        actorBreakdownTab: uiState.actorBreakdownTab,
        factionFilter: uiState.factionFilter,
        sidebarCollapsed: uiState.sidebarCollapsed,
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

/**
 * Set the active actor breakdown tab.
 * Resets sort to tab-appropriate default.
 */
export function setActorBreakdownTab(tab: ActorBreakdownTab): void {
  uiState.actorBreakdownTab = tab;
  // Reset sort to tab default
  switch (tab) {
    case "damageDealt":
      uiState.sortBy = "dps";
      break;
    case "damageTaken":
      uiState.sortBy = "dtps";
      break;
    case "healingDone":
      uiState.sortBy = "hps";
      break;
    case "healingReceived":
      uiState.sortBy = "hrps";
      break;
  }
  uiState.sortDirection = "desc";
}

/**
 * Set the faction filter for actor breakdown.
 */
export function setFactionFilter(filter: FactionFilter): void {
  uiState.factionFilter = filter;
}

/**
 * Reset UI state to initial values. For testing only.
 */
export function resetUiState(): void {
  collapsedActors.clear();
  uiState.sortBy = "damage";
  uiState.sortDirection = "desc";
  uiState.actorBreakdownTab = "damageDealt";
  uiState.factionFilter = "all";
  uiState.sidebarCollapsed = true;
}
