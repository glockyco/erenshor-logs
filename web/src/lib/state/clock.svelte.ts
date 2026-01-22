// Reactive clock for live-updating timestamps
// Reference-counted: starts when first subscriber, stops when last unsubscribes

const state = $state({ now: Date.now() });

let intervalId: ReturnType<typeof setInterval> | null = null;
let subscriberCount = 0;

/**
 * Current timestamp that updates every second (when subscribed).
 * Use subscribeToClock() to start receiving updates.
 */
export const now = {
  get value() {
    return state.now;
  },
};

/**
 * Subscribe to clock updates. Returns cleanup function.
 * Clock starts on first subscriber, stops when all unsubscribe.
 *
 * @example
 * // In a Svelte component:
 * import { now, subscribeToClock } from "$lib/state";
 * import { onMount } from "svelte";
 *
 * onMount(() => subscribeToClock());
 *
 * const duration = $derived(now.value - startTime);
 */
export function subscribeToClock(intervalMs = 1000): () => void {
  subscriberCount++;

  if (subscriberCount === 1) {
    // First subscriber - start the clock
    state.now = Date.now(); // Sync immediately
    intervalId = setInterval(() => {
      state.now = Date.now();
    }, intervalMs);
  }

  return () => {
    subscriberCount--;
    if (subscriberCount === 0 && intervalId !== null) {
      clearInterval(intervalId);
      intervalId = null;
    }
  };
}

/**
 * Force an immediate clock update. Useful for testing.
 */
export function tickClock(): void {
  state.now = Date.now();
}
