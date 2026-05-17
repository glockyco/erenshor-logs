import { CombatLogFileSchema } from "$lib/types/schemas";
import type { CombatLogSession, Session } from "$lib/types";

const INVALID_FORMAT_ERROR =
  "File does not match the Erenshor Logs v2 export format or uses an unsupported schema version.";
const LEGACY_FORMAT_ERROR =
  "This log uses the old Erenshor Logs v1 format. Importing v1 logs is no longer supported; use a protocol v2 export.";

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

  if (isLegacyExport(parsed)) {
    return {
      success: false,
      error: LEGACY_FORMAT_ERROR,
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

function isLegacyExport(value: unknown): boolean {
  return (
    typeof value === "object" &&
    value !== null &&
    "version" in value &&
    "exportedAt" in value &&
    "sessions" in value &&
    !("format" in value)
  );
}

function toSession(logSession: CombatLogSession): Session {
  const { snapshot, ended, events } = logSession;
  const lastEvent = events.length > 0 ? events[events.length - 1] : undefined;
  const endedAtUtcMs =
    ended?.endedAtUtcMs ??
    snapshot.endedAtUtcMs ??
    snapshot.startedAtUtcMs + (lastEvent?.offsetMs ?? 0);
  const durationMs =
    ended?.durationMs ?? snapshot.durationMs ?? endedAtUtcMs - snapshot.startedAtUtcMs;

  return {
    id: snapshot.sessionId,
    mode: "imported",
    state: "ended",
    startedAtUtcMs: snapshot.startedAtUtcMs,
    endedAtUtcMs,
    endReason: ended?.reason ?? snapshot.endReason,
    durationMs,
    producer: snapshot.producer,
    playerActorId: snapshot.playerActorId,
    registryRevision: snapshot.registryRevision,
    lastEventSeq: lastEvent?.eventSeq ?? 0,
    eventCount: events.length,
    completeness: snapshot.completeness,
    loss: snapshot.loss,
    registries: snapshot.registries,
    diagnostics: ended?.diagnostics ?? snapshot.diagnostics,
    events,
    protocolErrors: [],
  };
}
