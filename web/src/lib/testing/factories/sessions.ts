import type { Session, SessionInfo } from "$lib/types";

/**
 * Creates a SessionInfo with unique ID and deterministic start time.
 */
export function createSessionInfo(overrides: Partial<SessionInfo> = {}): SessionInfo {
  return {
    id: crypto.randomUUID(),
    startTime: 0,
    ...overrides,
  };
}

/**
 * Creates a complete Session with unique ID and empty events array.
 * Use createActiveSession or createCompletedSession for specific session states.
 */
export function createSession(overrides: Partial<Session> = {}): Session {
  return {
    id: crypto.randomUUID(),
    startTime: 0,
    events: [],
    ...overrides,
  };
}

/**
 * Creates an active (ongoing) session without an endTime.
 */
export function createActiveSession(overrides: Partial<Session> = {}): Session {
  return createSession({
    startTime: 0,
    ...overrides,
    endTime: undefined, // Explicitly no end time
  });
}

/**
 * Creates a completed session with a specific duration.
 *
 * @param durationMs Duration of the session in milliseconds
 * @param overrides Additional properties to override
 * @returns Session with startTime of 0 and endTime of durationMs
 */
export function createSessionWithDuration(
  durationMs: number,
  overrides: Partial<Session> = {}
): Session {
  return createSession({
    startTime: 0,
    endTime: durationMs,
    ...overrides,
  });
}

/**
 * Creates a completed session with default 5-minute duration.
 */
export function createCompletedSession(overrides: Partial<Session> = {}): Session {
  return createSessionWithDuration(300000, overrides); // 5 minutes
}

/**
 * Creates a short completed session with 30-second duration.
 * Useful for testing duration formatting.
 */
export function createShortSession(overrides: Partial<Session> = {}): Session {
  return createSessionWithDuration(30000, overrides); // 30 seconds
}

/**
 * Creates a long completed session with 1-hour duration.
 * Useful for testing duration formatting.
 */
export function createLongSession(overrides: Partial<Session> = {}): Session {
  return createSessionWithDuration(3600000, overrides); // 1 hour
}
