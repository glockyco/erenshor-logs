import { defineConfig } from "vitest/config";

/**
 * Vitest configuration with multiple test projects.
 *
 * This configuration separates unit tests (fast, node environment) from component
 * tests (browser environment, when added). Each test type gets an optimized
 * environment without affecting the other.
 *
 * Current setup:
 * - Unit tests: Active (services, utils, state) - Node.js environment
 * - Component tests: Disabled until component tests are written
 *
 * To run specific test types:
 * - pnpm test              # All tests
 * - pnpm test:unit         # Unit tests only (fast)
 * - pnpm test:component    # Component tests only (when enabled)
 * - pnpm test:coverage     # With coverage report
 *
 * To enable component tests:
 * 1. Uncomment "./vitest.config.component.ts" below
 * 2. Install @testing-library/svelte
 * 3. Add component test files
 */
export default defineConfig({
  test: {
    projects: [
      "./vitest.config.unit.ts",
      // Uncomment when component tests are added:
      // "./vitest.config.component.ts",
    ],
  },
});
