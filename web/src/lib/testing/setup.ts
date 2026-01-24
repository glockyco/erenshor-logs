import { afterEach } from "vitest";
import {
  resetConnectionState,
  resetSessionsState,
  resetUiState,
  resetClockState,
} from "$lib/state";

/**
 * Global test setup for unit tests.
 *
 * Automatically cleans up state after each test to ensure isolation.
 */

// =============================================================================
// Test Cleanup
// =============================================================================

afterEach(() => {
  // Clean storage
  localStorage.clear();
  sessionStorage.clear();

  // Clean application state
  resetConnectionState();
  resetSessionsState();
  resetUiState();
  resetClockState();
});
