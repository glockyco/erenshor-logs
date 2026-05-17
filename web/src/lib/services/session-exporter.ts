import type {
  CombatLogFile,
  CombatLogSession,
  DerivedData,
  Session,
  SessionEndedPayload,
  SessionSnapshotPayload,
} from "$lib/types";
import { VERSION } from "$lib/version";
import { downloadJSON } from "$lib/utils/download";
import { calculateSessionStats } from "./combat-analyzer";

const SCHEMA_VERSION = "2.0.0";
const DERIVED_ALGORITHM_VERSION = "2.0.0";

export function createCombatLogFile(sessions: Session[], exportedAtMs = Date.now()): CombatLogFile {
  if (sessions.length === 0) throw new Error("Cannot export an empty session list");

  return {
    format: "erenshor.logs.export",
    schemaVersion: SCHEMA_VERSION,
    exportedAtMs,
    producer: {
      name: "ErenshorLogsWeb",
      webVersion: VERSION,
    },
    sessions: sessions.map((session) => createCombatLogSession(session, exportedAtMs)),
  };
}

export function createCombatLogSession(
  session: Session,
  computedAtMs = Date.now()
): CombatLogSession {
  return {
    snapshot: createSessionSnapshot(session),
    events: session.events,
    ended: session.state === "ended" ? createSessionEnded(session) : undefined,
    derived: createDerivedData(session, computedAtMs),
  };
}

export function exportSession(session: Session): void {
  const timestamp = new Date().toISOString().replace(/[:.]/g, "-").slice(0, -5);
  const shortId = session.id.slice(0, 8);
  const filename = `erenshor-session-${shortId}-${timestamp}.erenshorlog`;

  downloadJSON(createCombatLogFile([session]), filename);
}

export function exportSessions(sessions: Session[]): void {
  const timestamp = new Date().toISOString().replace(/[:.]/g, "-").slice(0, -5);
  const filename = `erenshor-sessions-${sessions.length}-${timestamp}.erenshorlog`;

  downloadJSON(createCombatLogFile(sessions), filename);
}

function createSessionSnapshot(session: Session): SessionSnapshotPayload {
  return {
    sessionId: session.id,
    state: session.state,
    mode: session.mode,
    startedAtUtcMs: session.startedAtUtcMs,
    endedAtUtcMs: session.endedAtUtcMs,
    endReason: session.endReason,
    durationMs: session.durationMs,
    producer: session.producer,
    playerActorId: session.playerActorId,
    registryRevision: session.registryRevision,
    lastEventSeq: session.lastEventSeq,
    eventCount: session.eventCount,
    completeness: session.completeness,
    loss: session.loss,
    registries: session.registries,
    diagnostics: session.diagnostics,
  };
}

function createSessionEnded(session: Session): SessionEndedPayload | undefined {
  if (
    session.endedAtUtcMs === undefined ||
    session.endReason === undefined ||
    session.durationMs === undefined
  ) {
    return undefined;
  }

  return {
    sessionId: session.id,
    endedAtUtcMs: session.endedAtUtcMs,
    endedAtEventSeq: session.lastEventSeq,
    reason: session.endReason,
    durationMs: session.durationMs,
    diagnostics: session.diagnostics,
  };
}

function createDerivedData(session: Session, computedAtMs: number): DerivedData {
  const summary = calculateSessionStats(session, session.durationMs);

  return {
    algorithmVersion: DERIVED_ALGORITHM_VERSION,
    computedAtMs,
    computedFromEventSeq: session.lastEventSeq,
    summary: {
      totalDamage: summary.totalDamage,
      totalHealing: summary.totalHealing,
      totalDamageTaken: summary.totalDamageTaken,
      totalHealingReceived: summary.totalHealingReceived,
      durationMs: summary.durationMs,
    },
  };
}
