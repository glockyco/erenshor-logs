// Session state management with localStorage persistence
// Uses Svelte 5 runes for reactive state

import type { Session } from "$lib/types/session";
import type { SessionInfo } from "$lib/types/protocol";
import type { CombatEvent } from "$lib/types/events";
import { SvelteMap } from "svelte/reactivity";
import { calculateSessionStats } from "$lib/services/combat-analyzer";
import { STORAGE_KEYS } from "$lib/utils/constants";

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

// SSR-safe initialization from localStorage
if (typeof window !== "undefined") {
  try {
    const stored = localStorage.getItem(STORAGE_KEYS.SESSIONS);
    if (stored) {
      const parsed: Array<[string, Session]> = JSON.parse(stored);
      parsed.forEach(([id, session]) => {
        sessions.set(id, session);
      });

      // Validate activeSessionId exists
      const storedActiveId = localStorage.getItem(`${STORAGE_KEYS.SESSIONS}-active`);
      if (storedActiveId && sessions.has(storedActiveId)) {
        activeSessionId = storedActiveId;
      }
    }
  } catch (error) {
    console.error("Failed to load sessions from localStorage:", error);
  }
}

// Persist to localStorage on changes
$effect(() => {
  if (typeof window === "undefined") return;

  try {
    const serialized = JSON.stringify(Array.from(sessions.entries()));
    localStorage.setItem(STORAGE_KEYS.SESSIONS, serialized);
  } catch (error) {
    console.error("Failed to save sessions to localStorage:", error);
  }
});

// Persist active session ID separately
$effect(() => {
  if (typeof window === "undefined") return;

  if (activeSessionId) {
    localStorage.setItem(`${STORAGE_KEYS.SESSIONS}-active`, activeSessionId);
  } else {
    localStorage.removeItem(`${STORAGE_KEYS.SESSIONS}-active`);
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
 * Mark a session as ended with endTime.
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
