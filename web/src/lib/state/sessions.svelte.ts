// Session state management with localStorage persistence
// Uses Svelte 5 runes for reactive state

import { z } from "zod";
import { SvelteMap } from "svelte/reactivity";
import type {
  CombatEventRecord,
  LiveEnvelope,
  ProtocolError,
  RegistryDeltaPayload,
  Session,
  SessionEndedPayload,
  SessionSnapshotPayload,
} from "$lib/types";
import { StoredSessionsSchema } from "$lib/types/schemas";
import { calculateSessionStats } from "$lib/services";
import { STORAGE_KEYS } from "$lib/utils/constants";
import { loadFromStorage, saveToStorage, removeFromStorage } from "$lib/utils/storage";
import { now } from "./clock.svelte";

export const sessions = new SvelteMap<string, Session>();

const state = $state({
  activeSessionId: null as string | null,
});

const errors = $state<{ values: ProtocolError[] }>({ values: [] });

export const activeSessionId = {
  get value() {
    return state.activeSessionId;
  },
};

export const protocolErrors = {
  get value() {
    return errors.values;
  },
};

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

  const durationMs =
    _activeSession.endedAtUtcMs !== undefined
      ? _activeSession.endedAtUtcMs - _activeSession.startedAtUtcMs
      : now.value - _activeSession.startedAtUtcMs;

  return calculateSessionStats(_activeSession, durationMs);
});

export const activeSessionStats = {
  get value() {
    return _activeSessionStats;
  },
};

const ACTIVE_SESSION_KEY = `${STORAGE_KEYS.SESSIONS}-active`;

const storedSessions = loadFromStorage(STORAGE_KEYS.SESSIONS, StoredSessionsSchema);
if (storedSessions) {
  storedSessions.forEach(([id, session]) => {
    sessions.set(id, session);
  });

  const storedActiveId = loadFromStorage(ACTIVE_SESSION_KEY, z.string());
  if (storedActiveId && sessions.has(storedActiveId)) {
    state.activeSessionId = storedActiveId;
  }
}

export function initSessionsPersistence(): () => void {
  const cleanup = $effect.root(() => {
    $effect(() => {
      saveToStorage(STORAGE_KEYS.SESSIONS, Array.from(sessions.entries()));
    });

    $effect(() => {
      if (state.activeSessionId) {
        saveToStorage(ACTIVE_SESSION_KEY, state.activeSessionId);
      } else {
        removeFromStorage(ACTIVE_SESSION_KEY);
      }
    });
  });

  return cleanup;
}

export function applyLiveEnvelope(envelope: LiveEnvelope): void {
  switch (envelope.kind) {
    case "hello":
      return;
    case "sessionSnapshot":
      applySessionSnapshot(envelope.payload as SessionSnapshotPayload);
      return;
    case "registryDelta":
      applyRegistryDelta(envelope.sessionId!, envelope.payload as RegistryDeltaPayload);
      return;
    case "events": {
      const payload = envelope.payload as {
        eventSeqStart: number;
        events: CombatEventRecord[];
      };
      appendProtocolEvents(envelope.sessionId!, payload.events, payload.eventSeqStart);
      return;
    }

    case "sessionEnded":
      applySessionEnded(envelope.payload as SessionEndedPayload);
      return;
    case "error": {
      const payload = envelope.payload as {
        code: string;
        message: string;
        sessionId?: string;
        eventSeq?: number;
      };
      recordProtocolError({
        code: payload.code,
        message: payload.message,
        sessionId: payload.sessionId,
        eventSeq: payload.eventSeq,
      });
      return;
    }
    case "heartbeat":
    case "serverStats":
      return;
  }
}

export function applySessionSnapshot(snapshot: SessionSnapshotPayload): void {
  const session: Session = {
    id: snapshot.sessionId,
    mode: snapshot.mode,
    state: snapshot.state,
    startedAtUtcMs: snapshot.startedAtUtcMs,
    endedAtUtcMs: snapshot.endedAtUtcMs,
    endReason: snapshot.endReason,
    durationMs: snapshot.durationMs,
    producer: snapshot.producer,
    playerActorId: snapshot.playerActorId,
    registryRevision: snapshot.registryRevision,
    lastEventSeq: 0,
    eventCount: 0,
    completeness: snapshot.completeness,
    loss: snapshot.loss,
    registries: snapshot.registries,
    diagnostics: snapshot.diagnostics,
    events: [],
    protocolErrors: [],
  };
  sessions.set(snapshot.sessionId, session);
  state.activeSessionId = snapshot.sessionId;
}

export function applyRegistryDelta(sessionId: string, delta: RegistryDeltaPayload): void {
  const session = sessions.get(sessionId);
  if (!session) {
    recordProtocolError({
      code: "unknown_session",
      message: `Received registry delta for unknown session ${sessionId}`,
      sessionId,
    });
    return;
  }

  sessions.set(sessionId, {
    ...session,
    registryRevision: delta.revision,
    registries: {
      revision: delta.revision,
      actors: { ...session.registries.actors, ...(delta.actors ?? {}) },
      abilities: { ...session.registries.abilities, ...(delta.abilities ?? {}) },
      effects: { ...session.registries.effects, ...(delta.effects ?? {}) },
    },
  });
}

export function appendProtocolEvents(
  sessionId: string,
  events: CombatEventRecord[],
  eventSeqStart: number
): void {
  const session = sessions.get(sessionId);
  if (!session) {
    recordProtocolError({
      code: "unknown_session",
      message: `Received events for unknown session ${sessionId}`,
      sessionId,
    });
    return;
  }

  if (events.length === 0) return;

  const expectedStart = session.lastEventSeq + 1;
  const actualStart = eventSeqStart;
  if (actualStart !== expectedStart) {
    const error = {
      code: "event_sequence_gap",
      message: `Expected eventSeq ${expectedStart}, received ${actualStart}`,
      sessionId,
      eventSeq: actualStart,
    };
    recordProtocolError(error);
    sessions.set(sessionId, {
      ...session,
      completeness: "partial",
      protocolErrors: [...session.protocolErrors, error],
    });
    return;
  }

  for (let index = 0; index < events.length; index += 1) {
    const expectedSeq = expectedStart + index;
    if (events[index].eventSeq !== expectedSeq) {
      const error = {
        code: "event_sequence_gap",
        message: `Expected eventSeq ${expectedSeq}, received ${events[index].eventSeq}`,
        sessionId,
        eventSeq: events[index].eventSeq,
      };
      recordProtocolError(error);
      sessions.set(sessionId, {
        ...session,
        completeness: "partial",
        protocolErrors: [...session.protocolErrors, error],
      });
      return;
    }
  }

  const lastEventSeq = events[events.length - 1].eventSeq;
  sessions.set(sessionId, {
    ...session,
    events: [...session.events, ...events],
    lastEventSeq,
    eventCount: session.eventCount + events.length,
  });
}

export function applySessionEnded(ended: SessionEndedPayload): void {
  const session = sessions.get(ended.sessionId);
  if (!session) {
    recordProtocolError({
      code: "unknown_session",
      message: `Received session end for unknown session ${ended.sessionId}`,
      sessionId: ended.sessionId,
    });
    return;
  }

  sessions.set(ended.sessionId, {
    ...session,
    state: "ended",
    endedAtUtcMs: ended.endedAtUtcMs,
    endReason: ended.reason,
    durationMs: ended.durationMs,
    diagnostics: ended.diagnostics ?? session.diagnostics,
  });
}

function recordProtocolError(error: ProtocolError): void {
  errors.values = [...errors.values, error];
}

export function deleteSession(sessionId: string): void {
  sessions.delete(sessionId);

  if (state.activeSessionId === sessionId) {
    state.activeSessionId = null;
  }
}

export function clearAllSessions(): void {
  sessions.clear();
  errors.values = [];
  state.activeSessionId = null;
}

export function setActiveSession(sessionId: string | null): void {
  if (sessionId && !sessions.has(sessionId)) return;

  state.activeSessionId = sessionId;
}

export function resetSessionsState(): void {
  sessions.clear();
  errors.values = [];
  state.activeSessionId = null;
}
