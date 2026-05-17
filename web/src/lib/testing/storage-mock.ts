/**
 * Deterministic Storage implementation for tests.
 *
 * The unit-test environment can expose incompatible localStorage implementations
 * before jsdom finishes setup. Installing this mock as the first setup file
 * keeps module-level state initialization quiet and gives each test isolated
 * browser storage.
 *
 * Differences from real Storage:
 * - No quota limits unless a test explicitly enables quota failure mode.
 * - No cross-tab `storage` events.
 * - No bracket notation support (`localStorage.foo`).
 *
 * These limitations are acceptable because application code uses the Storage
 * methods directly.
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
    this.store.delete(String(key));
  }

  setItem(key: string, value: string): void {
    if (this.failureMode === "quota") {
      throw new DOMException("QuotaExceededError", "QuotaExceededError");
    }
    if (this.failureMode === "security") {
      throw new DOMException("SecurityError", "SecurityError");
    }
    this.store.set(String(key), String(value));
  }

  setFailureMode(mode?: "quota" | "security"): void {
    this.failureMode = mode;
  }

  _debug(): Map<string, string> {
    return new Map(this.store);
  }
}

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
