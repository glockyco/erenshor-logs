// Session state management with localStorage persistence
// Uses Svelte 5 runes for reactive state

import { z } from "zod";
import { SvelteMap } from "svelte/reactivity";
import type { Session, SessionInfo, CombatEvent } from "$lib/types";
import { StoredSessionsSchema } from "$lib/types/schemas";
import { calculateSessionStats } from "$lib/services";
import { STORAGE_KEYS } from "$lib/utils/constants";
import { loadFromStorage, saveToStorage, removeFromStorage } from "$lib/utils/storage";

// State
export const sessions = new SvelteMap<string, Session>();
export let activeSessionId = $state<string | null>(null);

// Derived state
export const activeSession = $derived.by(() => {
  if (!activeSessionId) return null;
  return sessions.get(activeSessionId) ?? null;
});

export const activeSessionStats = $derived.by(() => {
  if (!activeSession) return null;

  const durationMs = activeSession.endTime
    ? activeSession.endTime - activeSession.startTime
    : Date.now() - activeSession.startTime;

  return calculateSessionStats(activeSession.events, durationMs);
});

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
    activeSessionId = storedActiveId;
  }
}

// Persist to localStorage on changes
$effect(() => {
  saveToStorage(STORAGE_KEYS.SESSIONS, Array.from(sessions.entries()));
});

// Persist active session ID separately
$effect(() => {
  if (activeSessionId) {
    saveToStorage(ACTIVE_SESSION_KEY, activeSessionId);
  } else {
    removeFromStorage(ACTIVE_SESSION_KEY);
  }
});

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
  if (!activeSessionId) {
    activeSessionId = info.id;
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

  // Mutate in place - Svelte 5 tracks this
  session.events.push(...events);
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

  session.endTime = endTime;
}

/**
 * Delete a session from storage.
 */
export function deleteSession(sessionId: string): void {
  sessions.delete(sessionId);

  // Clear active if deleted
  if (activeSessionId === sessionId) {
    activeSessionId = null;
  }
}

/**
 * Clear all sessions from storage.
 */
export function clearAllSessions(): void {
  sessions.clear();
  activeSessionId = null;
}

/**
 * Set the active session for viewing.
 */
export function setActiveSession(sessionId: string | null): void {
  if (sessionId && !sessions.has(sessionId)) {
    console.warn(`Cannot set active session to unknown ID ${sessionId}`);
    return;
  }

  activeSessionId = sessionId;
}
