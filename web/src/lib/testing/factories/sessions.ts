import type { Session, SessionInfo } from "$lib/types";

let sessionCounter = 0;

/**
 * Creates a SessionInfo with sequential ID and relative start time.
 */
export function createSessionInfo(overrides: Partial<SessionInfo> = {}): SessionInfo {
  return {
    id: `session-${++sessionCounter}`,
    startTime: Date.now(),
    ...overrides,
  };
}

/**
 * Creates a complete Session with sequential ID and empty events array.
 * Use createActiveSession or createCompletedSession for specific session states.
 */
export function createSession(overrides: Partial<Session> = {}): Session {
  return {
    id: `session-${++sessionCounter}`,
    startTime: Date.now(),
    events: [],
    ...overrides,
  };
}

/**
 * Creates an active (ongoing) session without an endTime.
 */
export function createActiveSession(overrides: Partial<Session> = {}): Session {
  return createSession({
    startTime: Date.now() - 300000, // Started 5 minutes ago
    ...overrides,
    endTime: undefined, // Explicitly no end time
  });
}

/**
 * Creates a completed session with an endTime.
 */
export function createCompletedSession(overrides: Partial<Session> = {}): Session {
  const startTime = Date.now() - 600000; // Started 10 minutes ago
  return createSession({
    startTime,
    endTime: startTime + 300000, // Lasted 5 minutes
    ...overrides,
  });
}

/**
 * Creates a short completed session (useful for testing duration formatting).
 */
export function createShortSession(overrides: Partial<Session> = {}): Session {
  const startTime = Date.now() - 120000; // Started 2 minutes ago
  return createSession({
    startTime,
    endTime: startTime + 30000, // Lasted 30 seconds
    ...overrides,
  });
}

/**
 * Creates a long completed session (useful for testing duration formatting).
 */
export function createLongSession(overrides: Partial<Session> = {}): Session {
  const startTime = Date.now() - 7200000; // Started 2 hours ago
  return createSession({
    startTime,
    endTime: startTime + 3600000, // Lasted 1 hour
    ...overrides,
  });
}

/**
 * Resets the session counter. Useful for deterministic test snapshots.
 */
export function resetSessionCounter(): void {
  sessionCounter = 0;
}
