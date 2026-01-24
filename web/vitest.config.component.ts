import { defineConfig } from "vitest/config";
import { sveltekit } from "@sveltejs/kit/vite";
import tailwindcss from "@tailwindcss/vite";

/**
 * Component test configuration - Full browser environment for Svelte components.
 *
 * THIS CONFIG IS CURRENTLY DISABLED - uncomment in vitest.config.ts to enable.
 *
 * This config will be used when component tests are added:
 * - Component rendering tests
 * - User interaction tests
 * - Visual regression tests
 *
 * Why jsdom + SvelteKit?
 * - Components need DOM
 * - Need to test actual component behavior
 * - SvelteKit plugin handles Svelte compilation
 *
 * To enable component testing:
 * 1. Uncomment this config in vitest.config.ts
 * 2. Install: pnpm add -D @testing-library/svelte @testing-library/user-event
 * 3. Create component test files (*.test.ts or *.test.svelte)
 * 4. Run: pnpm test:component
 */
export default defineConfig({
  plugins: [tailwindcss(), sveltekit()],

  test: {
    name: "component",
    globals: true,
    environment: "jsdom",
    setupFiles: ["./src/lib/testing/setup-components.ts"],
    include: ["src/lib/components/**/*.test.ts", "src/lib/components/**/*.test.svelte"],

    // Performance
    pool: "threads",
    isolate: true,
  },
});
