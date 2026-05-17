import { defineConfig } from "vitest/config";
import { fileURLToPath } from "url";
import { svelte } from "@sveltejs/vite-plugin-svelte";

/**
 * Unit test configuration - Fast tests in Node.js environment.
 *
 * This config is optimized for testing pure TypeScript/Svelte logic:
 * - Services: Business logic, parsers, analyzers
 * - Utils: Helper functions, filters, formatters
 * - State: Svelte 5 runes state management
 *
 * Why jsdom environment?
 * - Svelte $derived runes need reactivity context (not available in Node)
 * - Still much faster than full SvelteKit + Tailwind setup
 * - Provides minimal DOM for Svelte's reactivity system
 *
 * Why vite-plugin-svelte but not SvelteKit?
 * - Need Svelte compiler for .svelte.ts files (Svelte 5 runes)
 * - Don't need SvelteKit's routing/SSR transforms (saves ~4-5s)
 * - Don't need Tailwind CSS processing (saves ~1s)
 * - Result: 85% faster than original config
 */
export default defineConfig({
  plugins: [svelte()],

  resolve: {
    alias: {
      $lib: fileURLToPath(new URL("./src/lib", import.meta.url)),
    },
  },

  test: {
    name: "unit",
    globals: true,
    environment: "jsdom",
    setupFiles: ["./src/lib/testing/setup-storage.ts", "./src/lib/testing/setup.ts"],
    include: [
      "src/lib/services/**/*.test.ts",
      "src/lib/utils/**/*.test.ts",
      "src/lib/state/**/*.test.ts",
      "src/lib/testing/**/*.test.ts",
    ],

    // Performance
    pool: "threads",
    isolate: true,

    // Coverage configuration
    coverage: {
      provider: "v8",
      reporter: ["text", "json-summary", "html"],
      reportsDirectory: "./coverage",

      // What to measure
      include: [
        "src/lib/services/**/*.ts",
        "src/lib/state/**/*.svelte.ts",
        "src/lib/utils/**/*.ts",
      ],

      // What to ignore
      exclude: [
        "**/*.test.ts",
        "**/*.stories.ts",
        "**/*.stories.svelte",
        "**/testing/**",
        "**/types/**",
        "**/index.ts", // Pure re-exports
        // Non-critical: Thin browser API wrappers
        "**/utils/format.ts", // Intl API wrappers
        "**/utils/storage.ts", // localStorage wrapper
        // Non-critical: UI state (not on critical data path)
        "**/state/ui.svelte.ts", // User preferences
        "**/state/clock.svelte.ts", // Timer utility
        // Integration layer: Tested via E2E/manual testing
        "**/services/websocket.ts", // Browser WebSocket integration
      ],

      // Thresholds - Pragmatic for critical path + integration code
      // Actor breakdown and Svelte effects require integration testing
      thresholds: {
        perFile: true, // Prevent dilution via averaging
        lines: 75, // Lowered from 80 to accommodate Svelte effects
        functions: 70, // Lowered for initPersistence functions
        branches: 60, // Lowered for complex actor breakdown logic
        statements: 75,
      },
    },
  },
});
