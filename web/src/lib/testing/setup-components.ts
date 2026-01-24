/**
 * Component test setup - runs before component tests.
 *
 * THIS FILE IS CURRENTLY UNUSED - component tests are disabled.
 *
 * When component tests are enabled (uncomment in vitest.config.ts),
 * this file will handle component-specific test setup:
 * - Configure testing-library
 * - Set up component test utilities
 * - Configure jsdom environment
 *
 * Component tests will run in jsdom environment with full SvelteKit support.
 */

import { afterEach } from "vitest";
import {
  resetConnectionState,
  resetSessionsState,
  resetUiState,
  resetClockState,
} from "$lib/state";

// =============================================================================
// Component Test Cleanup
// =============================================================================

afterEach(() => {
  // Clean browser APIs (jsdom provides real implementations)
  localStorage.clear();
  sessionStorage.clear();

  // Clean application state
  resetConnectionState();
  resetSessionsState();
  resetUiState();
  resetClockState();

  // TODO: Add testing-library cleanup when component tests are added:
  // import { cleanup } from "@testing-library/svelte";
  // cleanup();
});
