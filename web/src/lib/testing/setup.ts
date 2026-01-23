import { afterEach } from "vitest";
import {
  resetConnectionState,
  resetSessionsState,
  resetUiState,
  resetClockState,
} from "$lib/state";

/**
 * Global test setup - runs before all tests.
 * Automatically cleans up state after each test to ensure isolation.
 */

afterEach(() => {
  // Clean browser APIs
  localStorage.clear();
  sessionStorage.clear();

  // Clean application state
  resetConnectionState();
  resetSessionsState();
  resetUiState();
  resetClockState();
});
