import { beforeEach, afterEach } from "vitest";
import { installStorageMock } from "./storage-mock";
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
// Storage Mocking
// =============================================================================

beforeEach(() => {
  // Install fresh Storage mocks for each test to ensure isolation
  // Only needed if jsdom's native Storage is broken
  if (typeof globalThis.localStorage?.getItem !== "function") {
    installStorageMock();
  }
});

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
