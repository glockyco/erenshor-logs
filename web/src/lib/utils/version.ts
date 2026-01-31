// Version parsing and comparison utilities for CalVer versions

/**
 * Parsed version parts from YYYY.M.D.REVISION format
 */
export interface VersionParts {
  year: number;
  month: number;
  day: number;
  revision: number;
}

/**
 * Parse a CalVer version string into numeric components.
 *
 * Format: YYYY.M.D.REVISION (e.g., "2026.1.24.72108205")
 *
 * Returns null for:
 * - Dirty/fallback builds (containing "-")
 * - Wrong segment count (not exactly 4 parts)
 * - Non-numeric segments
 * - Revision numbers exceeding MAX_SAFE_INTEGER
 * - Empty or whitespace-only strings
 *
 * Trims leading/trailing whitespace before parsing.
 *
 * @param version - The version string to parse
 * @returns Parsed version parts or null if invalid
 *
 * @example
 * parseVersion("2026.1.24.72108205") // { year: 2026, month: 1, day: 24, revision: 72108205 }
 * parseVersion("2026.1.24.123-dirty-...") // null (dirty build)
 * parseVersion("0.0.0-20260124-...") // null (fallback)
 */
export function parseVersion(version: string): VersionParts | null {
  const trimmed = version.trim();

  // Reject dirty/fallback builds (contain dash)
  if (trimmed.includes("-")) return null;

  // Empty after trim
  if (!trimmed) return null;

  const parts = trimmed.split(".");
  if (parts.length !== 4) return null;

  const [year, month, day, revision] = parts.map((p) => parseInt(p, 10));

  // Validate all parsed successfully and are safe integers
  if (
    [year, month, day, revision].some(
      (n) => !Number.isFinite(n) || n < 0 || !Number.isSafeInteger(n)
    )
  ) {
    return null;
  }

  return { year, month, day, revision };
}

/**
 * Compare two parsed version objects.
 *
 * Uses lexicographic comparison: year → month → day → revision
 *
 * @param a - First version
 * @param b - Second version
 * @returns -1 if a < b, 0 if equal, 1 if a > b
 *
 * @example
 * compareVersions(
 *   { year: 2026, month: 1, day: 20, revision: 100 },
 *   { year: 2026, month: 1, day: 24, revision: 200 }
 * ) // -1 (a is older)
 */
export function compareVersions(a: VersionParts, b: VersionParts): -1 | 0 | 1 {
  // Compare year
  if (a.year < b.year) return -1;
  if (a.year > b.year) return 1;

  // Same year, compare month
  if (a.month < b.month) return -1;
  if (a.month > b.month) return 1;

  // Same year+month, compare day
  if (a.day < b.day) return -1;
  if (a.day > b.day) return 1;

  // Same date, compare revision
  if (a.revision < b.revision) return -1;
  if (a.revision > b.revision) return 1;

  // Equal
  return 0;
}

/**
 * Check if the mod version is outdated relative to the web app version.
 *
 * Returns true only when both versions parse successfully AND the mod version
 * is strictly older than the web version.
 *
 * Returns false for any unparseable input (fail open — never nag on ambiguous data).
 *
 * @param modVersion - Version from the mod (via WebSocket handshake)
 * @param webVersion - Version of the web app (build-time constant)
 * @returns true if mod is outdated, false otherwise
 *
 * @example
 * isModOutdated("2026.1.20.100", "2026.1.24.200") // true
 * isModOutdated("2026.1.24.200", "2026.1.24.200") // false (equal)
 * isModOutdated("2026.1.24.123-dirty-...", "2026.1.24.200") // false (unparseable)
 */
export function isModOutdated(modVersion: string, webVersion: string): boolean {
  const modParts = parseVersion(modVersion);
  const webParts = parseVersion(webVersion);

  // Fail open: if either version is unparseable, don't nag
  if (!modParts || !webParts) return false;

  // Check if mod < web
  return compareVersions(modParts, webParts) === -1;
}
