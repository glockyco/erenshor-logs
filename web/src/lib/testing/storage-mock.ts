/**
 * Mock Storage implementation for tests.
 *
 * ## Why This Exists
 *
 * jsdom 27.4.0 has a bug where `localStorage` and `sessionStorage` are
 * initialized as plain objects instead of proper Storage instances, causing
 * `localStorage.getItem is not a function` errors.
 *
 * Affects: Vitest 4.0.18 + jsdom 27.4.0
 * Tracked: https://github.com/jsdom/jsdom/issues (search for "localStorage")
 *
 * ## When to Remove
 *
 * After upgrading jsdom, run tests. If the "MockStorage necessity check"
 * test fails, the bug is fixed - delete this file and remove the conditional
 * check in setup.ts.
 *
 * ## Differences from Real Storage
 *
 * - No quota limits (infinite storage, but supports simulated quota errors)
 * - No `storage` event dispatching (cross-tab communication)
 * - No bracket notation support (localStorage.foo or localStorage["foo"])
 *
 * These limitations are acceptable because our tests don't rely on these
 * behaviors.
 *
 * @internal - Do not use directly. Installed globally in test setup.
 */
class MockStorage implements Storage {
  private store = new Map<string, string>();
  private failureMode?: "quota" | "security";

  get length(): number {
    return this.store.size;
  }

  clear(): void {
    if (this.failureMode === "security") {
      throw new DOMException("SecurityError", "SecurityError");
    }
    this.store.clear();
  }

  getItem(key: string): string | null {
    if (this.failureMode === "security") {
      throw new DOMException("SecurityError", "SecurityError");
    }
    // Coerce key to string to match browser behavior
    return this.store.get(String(key)) ?? null;
  }

  key(index: number): string | null {
    if (this.failureMode === "security") {
      throw new DOMException("SecurityError", "SecurityError");
    }
    return Array.from(this.store.keys())[index] ?? null;
  }

  removeItem(key: string): void {
    if (this.failureMode === "security") {
      throw new DOMException("SecurityError", "SecurityError");
    }
    // Coerce key to string to match browser behavior
    this.store.delete(String(key));
  }

  setItem(key: string, value: string): void {
    if (this.failureMode === "quota") {
      throw new DOMException("QuotaExceededError", "QuotaExceededError");
    }
    if (this.failureMode === "security") {
      throw new DOMException("SecurityError", "SecurityError");
    }
    // Coerce both to strings to match browser behavior
    this.store.set(String(key), String(value));
  }

  /**
   * Configure the mock to throw errors for testing error handling.
   *
   * @param mode - Error mode ('quota' or 'security'), or undefined to reset
   *
   * @example
   * ```typescript
   * const storage = globalThis.localStorage as MockStorage;
   * storage.setFailureMode('quota');
   * expect(() => storage.setItem('key', 'value')).toThrow();
   * storage.setFailureMode(); // Reset to normal
   * ```
   */
  setFailureMode(mode?: "quota" | "security"): void {
    this.failureMode = mode;
  }

  /**
   * Get internal state for debugging (not part of Storage API).
   *
   * @internal
   */
  _debug(): Map<string, string> {
    return new Map(this.store);
  }
}

/**
 * Install mock Storage instances on globalThis for testing.
 *
 * This patches over a jsdom bug where localStorage/sessionStorage methods
 * are undefined in certain Vitest + jsdom version combinations.
 *
 * Creates FRESH instances on each call to ensure test isolation.
 */
export function installStorageMock(): void {
  Object.defineProperty(globalThis, "localStorage", {
    value: new MockStorage(),
    writable: true,
    configurable: true,
  });

  Object.defineProperty(globalThis, "sessionStorage", {
    value: new MockStorage(),
    writable: true,
    configurable: true,
  });
}
