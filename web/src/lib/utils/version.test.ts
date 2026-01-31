import { describe, it, expect } from "vitest";
import { parseVersion, compareVersions, isModOutdated } from "./version";

describe("version utilities", () => {
  describe("parseVersion", () => {
    describe("valid production versions", () => {
      it("parses valid CalVer version", () => {
        const result = parseVersion("2026.1.24.72108205");

        expect(result).toEqual({
          year: 2026,
          month: 1,
          day: 24,
          revision: 72108205,
        });
      });

      it("parses version with single-digit month and day", () => {
        const result = parseVersion("2026.1.5.999");

        expect(result).toEqual({
          year: 2026,
          month: 1,
          day: 5,
          revision: 999,
        });
      });

      it("parses version with leading zeros (backward compatibility)", () => {
        const result = parseVersion("2026.01.09.123");

        expect(result).toEqual({
          year: 2026,
          month: 1,
          day: 9,
          revision: 123,
        });
      });

      it("trims whitespace before parsing", () => {
        const result = parseVersion("  2026.1.24.123  ");

        expect(result).toEqual({
          year: 2026,
          month: 1,
          day: 24,
          revision: 123,
        });
      });
    });

    describe("dirty/fallback builds", () => {
      it("returns null for dirty version", () => {
        const result = parseVersion("2026.1.24.123-dirty-20260124-204219");

        expect(result).toBeNull();
      });

      it("returns null for fallback version", () => {
        const result = parseVersion("0.0.0-20260124-204219");

        expect(result).toBeNull();
      });

      it("returns null for any string containing dash", () => {
        expect(parseVersion("2026-1-24-123")).toBeNull();
        expect(parseVersion("2026.1.24.123-suffix")).toBeNull();
      });
    });

    describe("invalid formats", () => {
      it("returns null for wrong segment count (too few)", () => {
        expect(parseVersion("2026.1.24")).toBeNull();
      });

      it("returns null for wrong segment count (too many)", () => {
        expect(parseVersion("2026.1.24.5.6")).toBeNull();
      });

      it("returns null for non-numeric segments", () => {
        expect(parseVersion("2026.1.24.abc")).toBeNull();
        expect(parseVersion("abc.1.24.123")).toBeNull();
      });

      it("returns null for empty string", () => {
        expect(parseVersion("")).toBeNull();
      });

      it("returns null for whitespace-only string", () => {
        expect(parseVersion("   ")).toBeNull();
      });

      it("returns null for negative numbers", () => {
        expect(parseVersion("2026.1.24.-123")).toBeNull();
      });

      it("returns null for revision exceeding safe integer range", () => {
        // MAX_SAFE_INTEGER + 1
        const unsafeRevision = (Number.MAX_SAFE_INTEGER + 1).toString();
        const result = parseVersion(`2026.1.24.${unsafeRevision}`);

        expect(result).toBeNull();
      });
    });
  });

  describe("compareVersions", () => {
    describe("equal versions", () => {
      it("returns 0 for identical versions", () => {
        const a = { year: 2026, month: 1, day: 24, revision: 100 };
        const b = { year: 2026, month: 1, day: 24, revision: 100 };

        expect(compareVersions(a, b)).toBe(0);
      });
    });

    describe("year comparison", () => {
      it("returns -1 when first year is older", () => {
        const a = { year: 2025, month: 12, day: 31, revision: 999 };
        const b = { year: 2026, month: 1, day: 1, revision: 1 };

        expect(compareVersions(a, b)).toBe(-1);
      });

      it("returns 1 when first year is newer", () => {
        const a = { year: 2027, month: 1, day: 1, revision: 1 };
        const b = { year: 2026, month: 12, day: 31, revision: 999 };

        expect(compareVersions(a, b)).toBe(1);
      });
    });

    describe("month comparison", () => {
      it("returns -1 when same year, first month is older", () => {
        const a = { year: 2026, month: 1, day: 31, revision: 999 };
        const b = { year: 2026, month: 2, day: 1, revision: 1 };

        expect(compareVersions(a, b)).toBe(-1);
      });

      it("returns 1 when same year, first month is newer", () => {
        const a = { year: 2026, month: 12, day: 1, revision: 1 };
        const b = { year: 2026, month: 1, day: 31, revision: 999 };

        expect(compareVersions(a, b)).toBe(1);
      });
    });

    describe("day comparison", () => {
      it("returns -1 when same year and month, first day is older", () => {
        const a = { year: 2026, month: 1, day: 20, revision: 999 };
        const b = { year: 2026, month: 1, day: 24, revision: 1 };

        expect(compareVersions(a, b)).toBe(-1);
      });

      it("returns 1 when same year and month, first day is newer", () => {
        const a = { year: 2026, month: 1, day: 30, revision: 1 };
        const b = { year: 2026, month: 1, day: 5, revision: 999 };

        expect(compareVersions(a, b)).toBe(1);
      });
    });

    describe("revision comparison", () => {
      it("returns -1 when same date, first revision is older", () => {
        const a = { year: 2026, month: 1, day: 24, revision: 100 };
        const b = { year: 2026, month: 1, day: 24, revision: 200 };

        expect(compareVersions(a, b)).toBe(-1);
      });

      it("returns 1 when same date, first revision is newer", () => {
        const a = { year: 2026, month: 1, day: 24, revision: 999 };
        const b = { year: 2026, month: 1, day: 24, revision: 100 };

        expect(compareVersions(a, b)).toBe(1);
      });
    });
  });

  describe("isModOutdated", () => {
    describe("mod is outdated", () => {
      it("returns true when mod is older than web (different day)", () => {
        const result = isModOutdated("2026.1.20.100", "2026.1.24.200");

        expect(result).toBe(true);
      });

      it("returns true when mod is older (same day, different revision)", () => {
        const result = isModOutdated("2026.1.24.100", "2026.1.24.200");

        expect(result).toBe(true);
      });

      it("returns true when mod is older (different month)", () => {
        const result = isModOutdated("2026.1.24.999", "2026.2.1.1");

        expect(result).toBe(true);
      });

      it("returns true when mod is older (different year)", () => {
        const result = isModOutdated("2025.12.31.999", "2026.1.1.1");

        expect(result).toBe(true);
      });
    });

    describe("mod is not outdated", () => {
      it("returns false when versions are equal", () => {
        const result = isModOutdated("2026.1.24.200", "2026.1.24.200");

        expect(result).toBe(false);
      });

      it("returns false when mod is newer than web (edge case)", () => {
        const result = isModOutdated("2026.1.30.999", "2026.1.24.200");

        expect(result).toBe(false);
      });
    });

    describe("fail open: unparseable versions", () => {
      it("returns false when mod version is dirty", () => {
        const result = isModOutdated("2026.1.24.123-dirty-20260124-204219", "2026.1.30.999");

        expect(result).toBe(false);
      });

      it("returns false when mod version is fallback", () => {
        const result = isModOutdated("0.0.0-20260124-204219", "2026.1.24.200");

        expect(result).toBe(false);
      });

      it("returns false when web version is dirty (unlikely in production)", () => {
        const result = isModOutdated("2026.1.20.100", "2026.1.24.200-dirty-...");

        expect(result).toBe(false);
      });

      it("returns false when both versions are unparseable", () => {
        const result = isModOutdated("2026.1.24.123-dirty-...", "2026.1.30.999-dirty-...");

        expect(result).toBe(false);
      });

      it("returns false when mod version is malformed", () => {
        const result = isModOutdated("invalid", "2026.1.24.200");

        expect(result).toBe(false);
      });

      it("returns false when web version is malformed", () => {
        const result = isModOutdated("2026.1.20.100", "invalid");

        expect(result).toBe(false);
      });

      it("returns false for empty strings", () => {
        expect(isModOutdated("", "2026.1.24.200")).toBe(false);
        expect(isModOutdated("2026.1.24.200", "")).toBe(false);
      });
    });
  });
});
