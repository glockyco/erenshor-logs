import { describe, expect, it } from "vitest";
import { installStorageMock } from "./storage-mock";

describe("MockStorage", () => {
  it("installs fresh Storage-compatible objects", () => {
    localStorage.setItem("old", "value");

    installStorageMock();

    expect(localStorage.getItem("old")).toBeNull();
    expect(typeof localStorage.getItem).toBe("function");
    expect(typeof localStorage.setItem).toBe("function");
    expect(typeof localStorage.clear).toBe("function");
  });

  it("behaves like browser Storage for supported operations", () => {
    localStorage.setItem("number" as unknown as string, 123 as unknown as string);
    expect(localStorage.getItem("number")).toBe("123");

    localStorage.setItem(null as unknown as string, null as unknown as string);
    expect(localStorage.getItem("null")).toBe("null");

    expect(localStorage.getItem("nonexistent")).toBeNull();

    localStorage.clear();
    expect(localStorage.length).toBe(0);

    localStorage.setItem("a", "1");
    localStorage.setItem("b", "2");
    expect(localStorage.length).toBe(2);

    localStorage.clear();
    localStorage.setItem("first", "1");
    expect(localStorage.key(0)).toBe("first");
    expect(localStorage.key(1)).toBeNull();

    localStorage.setItem("remove-me", "value");
    expect(localStorage.getItem("remove-me")).toBe("value");
    localStorage.removeItem("remove-me");
    expect(localStorage.getItem("remove-me")).toBeNull();
  });
});
