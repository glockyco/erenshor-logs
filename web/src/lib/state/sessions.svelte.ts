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

// State
export const sessions = new SvelteMap<string, Session>();

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

/**
 * Initialize session persistence.
 *
 * Loads stored sessions from localStorage once, then sets up reactive
 * persistence to save changes automatically.
 *
 * Must be called from a component context (uses $effect).
 *
 * @returns cleanup function for effect disposal
 */
export function initSessionsPersistence(): () => void {
  // Load from localStorage (runs once on initialization)
  const storedSessions = loadFromStorage(STORAGE_KEYS.SESSIONS, StoredSessionsSchema);
  if (storedSessions) {
    // Load all sessions regardless of event count
    storedSessions.forEach(([id, session]) => {
      sessions.set(id, session);
    });

    // Validate activeSessionId exists
    const storedActiveId = loadFromStorage(ACTIVE_SESSION_KEY, z.string());
    if (storedActiveId && sessions.has(storedActiveId)) {
      state.activeSessionId = storedActiveId;
    }
  }

  // Set up reactive persistence (runs on changes)
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

  // Return cleanup function
  return cleanup;
}

// Functions

/**
 * Add a new session from SessionInfo (handshake or sessionStart message).
 * Handles duplicate session IDs by replacing empty sessions or warning about non-empty duplicates.
 */
export function addSession(info: SessionInfo): void {
  const existing = sessions.get(info.id);

  if (existing) {
    // If existing session is empty, allow replacement to handle reconnection edge cases
    if (existing.events.length === 0) {
      console.log(`Replacing empty session ${info.id} with fresh session (duplicate ID)`);
      // Fall through to create new session
    } else {
      // Existing session has events - don't overwrite data
      console.warn(`Session ${info.id} already exists, ignoring duplicate sessionStart`);
      return;
    }
  }

  const session: Session = {
    id: info.id,
    startTime: info.startTime,
    events: [],
  };

  sessions.set(info.id, session);

  // Always set new session as active for hands-off second-screen usage
  state.activeSessionId = info.id;
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
 * All sessions are preserved regardless of event count.
 */
export function endSession(sessionId: string, endTime: number): void {
  const session = sessions.get(sessionId);

  if (!session) {
    console.warn(`Attempted to end unknown session ${sessionId}`);
    return;
  }

  // Mark session as ended (preserve all sessions regardless of event count)
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
