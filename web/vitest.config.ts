import { defineConfig } from "vitest/config";
import { sveltekit } from "@sveltejs/kit/vite";
import tailwindcss from "@tailwindcss/vite";

export default defineConfig({
  plugins: [tailwindcss(), sveltekit()],
  test: {
    globals: true,
    environment: "jsdom",
    setupFiles: ["./src/lib/testing/setup.ts"],
    include: ["src/**/*.test.ts"],

    // Performance & isolation
    pool: "threads", // Parallel execution
    isolate: true, // Each test file in own environment

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
      ],

      // Thresholds - 80% modern best practice
      thresholds: {
        perFile: true, // Prevent dilution via averaging
        lines: 80,
        functions: 80,
        branches: 80,
        statements: 80,
      },
    },
  },
});
