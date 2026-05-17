import { CombatLogFileSchema } from "$lib/types/schemas";
import type { CombatLogSession, Session } from "$lib/types";

const INVALID_FORMAT_ERROR =
  "File does not match the Erenshor Logs v2 export format or uses an unsupported schema version.";

export type ImportResult =
  | { success: true; sessions: Session[] }
  | { success: false; error: string };

export function importSessions(jsonText: string): ImportResult {
  let parsed: unknown;

  try {
    parsed = JSON.parse(jsonText);
  } catch (err) {
    return {
      success: false,
      error: `Invalid JSON: ${err instanceof Error ? err.message : "Parse error"}`,
    };
  }

  const result = CombatLogFileSchema.safeParse(parsed);
  if (!result.success) {
    return {
      success: false,
      error: INVALID_FORMAT_ERROR,
    };
  }

  return {
    success: true,
    sessions: result.data.sessions.map(toSession),
  };
}

export function readFileAsText(file: File): Promise<string> {
  return new Promise((resolve, reject) => {
    const reader = new FileReader();
    reader.onload = () => resolve(reader.result as string);
    reader.onerror = () => reject(new Error("Failed to read file"));
    reader.readAsText(file);
  });
}

function toSession(logSession: CombatLogSession): Session {
  const { snapshot, ended } = logSession;

  return {
    id: snapshot.sessionId,
    mode: snapshot.mode,
    state: ended ? "ended" : snapshot.state,
    startedAtUtcMs: snapshot.startedAtUtcMs,
    endedAtUtcMs: ended?.endedAtUtcMs ?? snapshot.endedAtUtcMs,
    endReason: ended?.reason ?? snapshot.endReason,
    durationMs: ended?.durationMs ?? snapshot.durationMs,
    producer: snapshot.producer,
    playerActorId: snapshot.playerActorId,
    registryRevision: snapshot.registryRevision,
    lastEventSeq: snapshot.lastEventSeq,
    eventCount: snapshot.eventCount,
    completeness: snapshot.completeness,
    loss: snapshot.loss,
    registries: snapshot.registries,
    diagnostics: ended?.diagnostics ?? snapshot.diagnostics,
    events: logSession.events,
    protocolErrors: [],
  };
}
