import { afterEach } from "vitest";
import {
  resetConnectionState,
  resetSessionsState,
  resetUiState,
  resetClockState,
  resetUpdateState,
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
  // Clean storage (now guaranteed to work with MockStorage)
  localStorage.clear();
  sessionStorage.clear();

  // Clean application state
  resetConnectionState();
  resetSessionsState();
  resetUiState();
  resetClockState();
  resetUpdateState();
});
