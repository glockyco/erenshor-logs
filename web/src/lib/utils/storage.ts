// SSR-safe localStorage utilities with Zod validation

import { z } from "zod";

/**
 * Load and validate data from localStorage.
 * Returns null if not in browser, key doesn't exist, or validation fails.
 */
export function loadFromStorage<T>(key: string, schema: z.ZodType<T>): T | null {
  const storage = getUsableStorage();
  if (!storage) return null;

  try {
    const stored = storage.getItem(key);
    if (!stored) return null;

    const parsed: unknown = JSON.parse(stored);
    const result = schema.safeParse(parsed);

    if (result.success) {
      return result.data;
    }

    console.warn(
      `Invalid data in localStorage key "${key}":`,
      result.error.issues.map((i) => `${i.path.join(".")}: ${i.message}`).join("; ")
    );
    return null;
  } catch (error) {
    console.error(`Failed to load "${key}" from localStorage:`, error);
    return null;
  }
}

/**
 * Save data to localStorage.
 * No-op if not in browser.
 */
export function saveToStorage<T>(key: string, data: T): void {
  const storage = getUsableStorage();
  if (!storage) return;

  try {
    storage.setItem(key, JSON.stringify(data));
  } catch (error) {
    console.error(`Failed to save "${key}" to localStorage:`, error);
  }
}

/**
 * Remove key from localStorage.
 * No-op if not in browser.
 */
export function removeFromStorage(key: string): void {
  const storage = getUsableStorage();
  if (!storage) return;
  storage.removeItem(key);
}

/**
 * Check if running in browser environment.
 */
export function isBrowser(): boolean {
  return typeof window !== "undefined";
}

function getUsableStorage(): Storage | null {
  if (!isBrowser()) return null;

  const storage = globalThis.localStorage;
  return typeof storage?.getItem === "function" &&
    typeof storage.setItem === "function" &&
    typeof storage.removeItem === "function"
    ? storage
    : null;
}
