/**
 * Session import service.
 * Validates and imports JSON session files.
 */

import { z } from "zod";
import { SessionSchema } from "$lib/types/schemas";
import type { Session } from "$lib/types";

/**
 * Schema for single session export format.
 */
const ExportedSessionSchema = z.object({
  version: z.string(),
  exportedAt: z.number(),
  session: SessionSchema,
});

/**
 * Schema for multiple sessions export format.
 */
const ExportedSessionsSchema = z.object({
  version: z.string(),
  exportedAt: z.number(),
  sessions: z.array(SessionSchema),
});

/**
 * Result of import validation.
 */
export type ImportResult =
  | { success: true; sessions: Session[] }
  | { success: false; error: string };

/**
 * Validates and parses imported JSON session data.
 * Supports both single-session and multi-session exports.
 *
 * @param jsonText - Raw JSON text from imported file
 * @returns Import result with parsed sessions or error message
 */
export function importSessions(jsonText: string): ImportResult {
  let parsed: unknown;

  // Parse JSON
  try {
    parsed = JSON.parse(jsonText);
  } catch (err) {
    return {
      success: false,
      error: `Invalid JSON: ${err instanceof Error ? err.message : "Parse error"}`,
    };
  }

  // Try parsing as single session export
  const singleResult = ExportedSessionSchema.safeParse(parsed);
  if (singleResult.success) {
    return {
      success: true,
      sessions: [singleResult.data.session],
    };
  }

  // Try parsing as multiple sessions export
  const multiResult = ExportedSessionsSchema.safeParse(parsed);
  if (multiResult.success) {
    return {
      success: true,
      sessions: multiResult.data.sessions,
    };
  }

  // Try parsing as raw session (for backwards compatibility or direct exports)
  const rawSessionResult = SessionSchema.safeParse(parsed);
  if (rawSessionResult.success) {
    return {
      success: true,
      sessions: [rawSessionResult.data],
    };
  }

  // Try parsing as array of raw sessions
  const rawSessionsResult = z.array(SessionSchema).safeParse(parsed);
  if (rawSessionsResult.success) {
    return {
      success: true,
      sessions: rawSessionsResult.data,
    };
  }

  // All parsing attempts failed
  return {
    success: false,
    error:
      "File does not match expected session export format. Check that the file is a valid Erenshor combat log export.",
  };
}

/**
 * Reads a file and returns its text content.
 * Helper for file input handling.
 *
 * @param file - File to read
 * @returns Promise resolving to file text content
 */
export function readFileAsText(file: File): Promise<string> {
  return new Promise((resolve, reject) => {
    const reader = new FileReader();
    reader.onload = () => resolve(reader.result as string);
    reader.onerror = () => reject(new Error("Failed to read file"));
    reader.readAsText(file);
  });
}
