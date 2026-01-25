import { describe, it, expect, beforeEach } from "vitest";
import { hydrated, completeHydration, resetHydrationState } from "./hydration.svelte";

describe("hydration state", () => {
  beforeEach(() => {
    resetHydrationState();
  });

  it("starts as not hydrated", () => {
    expect(hydrated.value).toBe(false);
  });

  it("marks as hydrated after completeHydration", () => {
    completeHydration();
    expect(hydrated.value).toBe(true);
  });

  it("can be reset for testing", () => {
    completeHydration();
    expect(hydrated.value).toBe(true);

    resetHydrationState();
    expect(hydrated.value).toBe(false);
  });

  it("remains hydrated after reset and re-hydration", () => {
    completeHydration();
    expect(hydrated.value).toBe(true);

    resetHydrationState();
    expect(hydrated.value).toBe(false);

    completeHydration();
    expect(hydrated.value).toBe(true);
  });
});
