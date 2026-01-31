import { describe, it, expect } from "vitest";

describe("MockStorage necessity check", () => {
  it("detects when jsdom localStorage is fixed", () => {
    // When this test fails, jsdom is fixed - remove storage-mock.ts
    const hasWorkingStorage =
      typeof globalThis.localStorage?.getItem === "function" &&
      typeof globalThis.localStorage?.setItem === "function" &&
      typeof globalThis.localStorage?.clear === "function";

    // Currently we expect jsdom to be broken, so this should be false
    // When jsdom is fixed, this will be true and the test will fail
    expect(hasWorkingStorage).toBe(false);

    if (hasWorkingStorage) {
      console.warn(
        "\n⚠️  jsdom now has working localStorage!\n" +
          "   Remove web/src/lib/testing/storage-mock.ts\n" +
          "   Remove the conditional check in setup.ts\n" +
          "   Delete this test file\n"
      );
    }
  });

  it("verifies mock behaves like real Storage", () => {
    // Type coercion
    localStorage.setItem("number" as unknown as string, 123 as unknown as string);
    expect(localStorage.getItem("number")).toBe("123");

    localStorage.setItem(null as unknown as string, null as unknown as string);
    expect(localStorage.getItem("null")).toBe("null");

    // Null for missing keys
    expect(localStorage.getItem("nonexistent")).toBe(null);

    // Length tracking
    localStorage.clear();
    expect(localStorage.length).toBe(0);
    localStorage.setItem("a", "1");
    localStorage.setItem("b", "2");
    expect(localStorage.length).toBe(2);

    // key() method
    localStorage.clear();
    localStorage.setItem("first", "1");
    expect(localStorage.key(0)).toBe("first");
    expect(localStorage.key(1)).toBe(null);

    // removeItem
    localStorage.setItem("remove-me", "value");
    expect(localStorage.getItem("remove-me")).toBe("value");
    localStorage.removeItem("remove-me");
    expect(localStorage.getItem("remove-me")).toBe(null);
  });
});
