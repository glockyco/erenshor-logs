// Hydration state management
// Tracks whether client-side hydration is complete, preventing flashes of
// wrong content when localStorage loads asynchronously during module import.
//
// During SSR: hydrated = false (no window object)
// During hydration: hydrated = false initially
// After component mount: hydrated = true (module-level localStorage load complete)
//
// Use: Guard localStorage-dependent rendering with `{#if hydrated.value}`
// See: AGENTS.md - Hydration Pattern section

const state = $state({
  hydrated: false,
});

/**
 * Hydration state getter.
 *
 * Returns true after client-side hydration is complete and module-level
 * localStorage loads have finished. Used to guard conditional rendering
 * that depends on persisted state.
 *
 * @example
 * ```svelte
 * <script lang="ts">
 *   import { hydrated } from "$lib/state";
 *   import { sessions } from "$lib/state/sessions.svelte";
 *
 *   const hasSessions = $derived(hydrated.value && sessions.size > 0);
 * </script>
 *
 * {#if !hydrated.value}
 *   <LoadingScreen />
 * {:else if !hasSessions}
 *   <WelcomeScreen />
 * {:else}
 *   <AppUI />
 * {/if}
 * ```
 */
export const hydrated = {
  get value() {
    return state.hydrated;
  },
};

/**
 * Mark hydration as complete. Should be called once during app initialization
 * in the root layout, after module-level localStorage loads have finished.
 *
 * Must be called from a component context (uses $effect). Returns a cleanup
 * function for proper lifecycle management.
 *
 * Note: During SSR, this effect won't execute since effects are component-level.
 * The hydrated state starts as false on both SSR and client, ensuring both render
 * identically during hydration.
 *
 * @returns cleanup function
 *
 * @example
 * ```typescript
 * // In root layout
 * $effect(() => {
 *   const cleanup = completeHydration();
 *   const cleanupSessions = initSessionsPersistence();
 *   // ... other cleanup functions
 *
 *   return () => {
 *     cleanup();
 *     cleanupSessions();
 *     // ... cleanup all
 *   };
 * });
 * ```
 */
export function completeHydration(): () => void {
  return $effect.root(() => {
    // Mark as hydrated - runs in browser after module-level localStorage loads
    // SSR never reaches this since effects are component-level only
    state.hydrated = true;

    // No cleanup needed - hydration is permanent
    return () => {};
  });
}

/**
 * Reset hydration state to initial value. For testing only.
 * Allows tests to simulate the hydration sequence.
 *
 * @internal
 */
export function resetHydrationState(): void {
  state.hydrated = false;
}
