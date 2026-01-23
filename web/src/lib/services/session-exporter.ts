/**
 * Session export service.
 * Exports sessions as JSON for sharing and debugging.
 */

import type { Session } from "$lib/types";
import { downloadJSON } from "$lib/utils/download";

/**
 * Format for exported session data.
 * Simple wrapper around Session with metadata.
 */
export interface ExportedSession {
  version: string;
  exportedAt: number;
  session: Session;
}

/**
 * Format for exporting multiple sessions.
 */
export interface ExportedSessions {
  version: string;
  exportedAt: number;
  sessions: Session[];
}

const EXPORT_VERSION = "1.0.0";

/**
 * Exports a single session as a downloadable JSON file.
 * Generates filename: erenshor-session-{sessionId}-{timestamp}.json
 *
 * @param session - Session to export
 */
export function exportSession(session: Session): void {
  const exported: ExportedSession = {
    version: EXPORT_VERSION,
    exportedAt: Date.now(),
    session,
  };

  const timestamp = new Date().toISOString().replace(/[:.]/g, "-").slice(0, -5);
  const shortId = session.id.slice(0, 8);
  const filename = `erenshor-session-${shortId}-${timestamp}`;

  downloadJSON(exported, filename);
}

/**
 * Exports multiple sessions as a single downloadable JSON file.
 * Generates filename: erenshor-sessions-{count}-{timestamp}.json
 *
 * @param sessions - Array of sessions to export
 */
export function exportSessions(sessions: Session[]): void {
  if (sessions.length === 0) {
    console.warn("exportSessions called with empty array");
    return;
  }

  const exported: ExportedSessions = {
    version: EXPORT_VERSION,
    exportedAt: Date.now(),
    sessions,
  };

  const timestamp = new Date().toISOString().replace(/[:.]/g, "-").slice(0, -5);
  const filename = `erenshor-sessions-${sessions.length}-${timestamp}`;

  downloadJSON(exported, filename);
}
