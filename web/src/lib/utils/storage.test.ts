import { describe, expect, it, vi } from "vitest";
import { z } from "zod";
import { loadFromStorage } from "./storage";

describe("loadFromStorage", () => {
  it("treats malformed test storage as unavailable without logging an error", () => {
    const originalStorage = globalThis.localStorage;
    const errorSpy = vi.spyOn(console, "error").mockImplementation(() => {});

    Object.defineProperty(globalThis, "localStorage", {
      value: {},
      configurable: true,
      writable: true,
    });

    try {
      expect(loadFromStorage("key", z.string())).toBeNull();
      expect(errorSpy).not.toHaveBeenCalled();
    } finally {
      Object.defineProperty(globalThis, "localStorage", {
        value: originalStorage,
        configurable: true,
        writable: true,
      });
      errorSpy.mockRestore();
    }
  });
});
