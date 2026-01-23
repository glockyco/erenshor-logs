// Session state management with localStorage persistence
// Uses Svelte 5 runes for reactive state

import { z } from "zod";
import { SvelteMap } from "svelte/reactivity";
import type { Session, SessionInfo, CombatEvent } from "$lib/types";
import { StoredSessionsSchema } from "$lib/types/schemas";
import { calculateSessionStats } from "$lib/services";
import { STORAGE_KEYS } from "$lib/utils/constants";
import { loadFromStorage, saveToStorage, removeFromStorage } from "$lib/utils/storage";
import { now } from "./clock.svelte";

// Constants

/**
 * Timeout for auto-ending stale sessions (milliseconds).
 * A session is considered stale if it has no endTime and the last event
 * timestamp is older than this timeout. This handles cases where the mod
 * crashes or fails to send a combatEnd event.
 *
 * Set to 10 seconds to provide buffer for network/processing delays while
 * still detecting hung sessions quickly. The mod's inactivity timeout is 5s,
 * so under normal operation the mod will send combatEnd before this triggers.
 */
const STALE_SESSION_TIMEOUT_MS = 10_000; // 10 seconds

// State
export const sessions = new SvelteMap<string, Session>();

/**
 * Timer ID for stale session cleanup interval.
 * Null when not running.
 */
let cleanupIntervalId: number | null = null;

const state = $state({
  activeSessionId: null as string | null,
});

export const activeSessionId = {
  get value() {
    return state.activeSessionId;
  },
};

// Derived state
const _activeSession = $derived.by(() => {
  if (!state.activeSessionId) return null;
  return sessions.get(state.activeSessionId) ?? null;
});

export const activeSession = {
  get value() {
    return _activeSession;
  },
};

const _activeSessionStats = $derived.by(() => {
  if (!_activeSession) return null;

  // Calculate duration with explicit conditionals to ensure proper dependency tracking
  let durationMs: number;
  if (_activeSession.endTime !== undefined) {
    // Session ended - use fixed end time
    durationMs = _activeSession.endTime - _activeSession.startTime;
  } else {
    // Session live - use current time
    durationMs = now.value - _activeSession.startTime;
  }

  return calculateSessionStats(_activeSession.events, durationMs);
});

export const activeSessionStats = {
  get value() {
    return _activeSessionStats;
  },
};

const ACTIVE_SESSION_KEY = `${STORAGE_KEYS.SESSIONS}-active`;

// SSR-safe initialization from localStorage
const storedSessions = loadFromStorage(STORAGE_KEYS.SESSIONS, StoredSessionsSchema);
if (storedSessions) {
  storedSessions.forEach(([id, session]) => {
    sessions.set(id, session);
  });

  // Validate activeSessionId exists
  const storedActiveId = loadFromStorage(ACTIVE_SESSION_KEY, z.string());
  if (storedActiveId && sessions.has(storedActiveId)) {
    state.activeSessionId = storedActiveId;
  }
}

/**
 * Check for stale sessions and auto-end them.
 * A session is considered stale if:
 * 1. It has no endTime (still "running")
 * 2. Last event timestamp is > STALE_SESSION_TIMEOUT_MS ago
 *
 * This handles cases where mod crashes or fails to send combatEnd event.
 */
function checkForStaleSessions(): void {
  const nowMs = now.value;

  for (const [sessionId, session] of sessions.entries()) {
    // Skip already-ended sessions
    if (session.endTime !== undefined) {
      continue;
    }

    // Find most recent event timestamp
    const lastEventTime =
      session.events.length > 0
        ? Math.max(...session.events.map((e) => e.timestamp))
        : session.startTime;

    // Check if stale
    const inactiveMs = nowMs - lastEventTime;
    if (inactiveMs > STALE_SESSION_TIMEOUT_MS) {
      console.warn(
        `Auto-ending stale session ${sessionId} (${(inactiveMs / 1000).toFixed(1)}s inactive)`
      );
      endSession(sessionId, lastEventTime + STALE_SESSION_TIMEOUT_MS);
    }
  }
}

/**
 * Initialize persistence effects. Must be called from a component context.
 * Returns a cleanup function.
 */
export function initSessionsPersistence(): () => void {
  const cleanup = $effect.root(() => {
    // Persist sessions to localStorage on changes
    $effect(() => {
      saveToStorage(STORAGE_KEYS.SESSIONS, Array.from(sessions.entries()));
    });

    // Persist active session ID separately
    $effect(() => {
      if (state.activeSessionId) {
        saveToStorage(ACTIVE_SESSION_KEY, state.activeSessionId);
      } else {
        removeFromStorage(ACTIVE_SESSION_KEY);
      }
    });
  });

  // Start stale session cleanup interval (checks every second)
  cleanupIntervalId = window.setInterval(checkForStaleSessions, 1000);

  // Return cleanup function
  return () => {
    cleanup();
    if (cleanupIntervalId !== null) {
      window.clearInterval(cleanupIntervalId);
      cleanupIntervalId = null;
    }
  };
}

// Functions

/**
 * Add a new session from SessionInfo (handshake or sessionStart message).
 * Ignores duplicate session IDs to prevent overwriting existing sessions.
 */
export function addSession(info: SessionInfo): void {
  if (sessions.has(info.id)) {
    console.warn(`Session ${info.id} already exists, ignoring duplicate sessionStart`);
    return;
  }

  const session: Session = {
    id: info.id,
    startTime: info.startTime,
    events: [],
  };

  sessions.set(info.id, session);

  // Set as active if no active session
  if (!state.activeSessionId) {
    state.activeSessionId = info.id;
  }
}

/**
 * Append combat events to a session.
 * Logs warning if session doesn't exist (orphaned events).
 */
export function appendEvents(sessionId: string, events: CombatEvent[]): void {
  const session = sessions.get(sessionId);

  if (!session) {
    console.warn(
      `Received events for unknown session ${sessionId}, dropping ${events.length} events`
    );
    return;
  }

  // Create new object to trigger SvelteMap reactivity and persistence
  sessions.set(sessionId, {
    ...session,
    events: [...session.events, ...events],
  });
}

/**
 * Mark a session as ended.
 */
export function endSession(sessionId: string, endTime: number): void {
  const session = sessions.get(sessionId);

  if (!session) {
    console.warn(`Attempted to end unknown session ${sessionId}`);
    return;
  }

  // Create new object to trigger SvelteMap reactivity and persistence
  sessions.set(sessionId, {
    ...session,
    endTime,
  });
}

/**
 * Delete a session from storage.
 */
export function deleteSession(sessionId: string): void {
  sessions.delete(sessionId);

  // Clear active if deleted
  if (state.activeSessionId === sessionId) {
    state.activeSessionId = null;
  }
}

/**
 * Clear all sessions from storage.
 */
export function clearAllSessions(): void {
  sessions.clear();
  state.activeSessionId = null;
}

/**
 * Set the active session for viewing.
 */
export function setActiveSession(sessionId: string | null): void {
  if (sessionId && !sessions.has(sessionId)) {
    console.warn(`Cannot set active session to unknown ID ${sessionId}`);
    return;
  }

  state.activeSessionId = sessionId;
}

/**
 * Reset sessions state to initial values. For testing only.
 */
export function resetSessionsState(): void {
  sessions.clear();
  state.activeSessionId = null;
}
