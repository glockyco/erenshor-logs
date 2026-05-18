import type {
  DiagnosticBatchPayload,
  DiagnosticImpact,
  DiagnosticRecord,
  DiagnosticSeverity,
  LiveEnvelope,
  ParseError,
  ParseErrorHeader,
  StatsPayload,
} from "$lib/types";

const MAX_RECENT_DIAGNOSTICS = 20;
const FATAL_CONSECUTIVE_INVALID_FRAMES = 3;

type StreamHealth = "healthy" | "recovering" | "degraded" | "fatal";

export interface LiveDiagnosticEntry {
  id: string;
  code: string;
  severity: DiagnosticSeverity;
  impact: DiagnosticImpact;
  component: string;
  operation: string;
  message: string;
  frameId?: number;
  kind?: string;
  sessionIdHash?: string;
  rawHash?: string;
  path?: string;
  firstSeenAtMs: number;
  lastSeenAtMs: number;
  count: number;
  suppressedCount: number;
}

export interface LiveDiagnosticsSnapshot {
  recent: LiveDiagnosticEntry[];
  consecutiveInvalidFrames: number;
  totalInvalidFrames: number;
  lastValidFrameAtMs: number | null;
  lastValidFrameId: number | null;
  lastHelloAtMs: number | null;
  latestStats: StatsPayload | null;
  health: StreamHealth;
}

const state = $state<LiveDiagnosticsSnapshot>({
  recent: [],
  consecutiveInvalidFrames: 0,
  totalInvalidFrames: 0,
  lastValidFrameAtMs: null,
  lastValidFrameId: null,
  lastHelloAtMs: null,
  latestStats: null,
  health: "healthy",
});

export const liveDiagnostics = {
  get value(): LiveDiagnosticsSnapshot {
    return state;
  },
};

export function recordParseError(error: ParseError): void {
  state.consecutiveInvalidFrames += 1;
  state.totalInvalidFrames += 1;

  appendDiagnostic(fromParseError(error));

  if (
    isFatalParseError(error) ||
    state.consecutiveInvalidFrames >= FATAL_CONSECUTIVE_INVALID_FRAMES
  ) {
    state.health = "fatal";
    return;
  }

  state.health = state.health === "degraded" ? "degraded" : "recovering";
}

export function recordValidFrame(envelope: LiveEnvelope): void {
  state.consecutiveInvalidFrames = 0;
  state.lastValidFrameAtMs = envelope.sentAtMs;
  state.lastValidFrameId = envelope.frameId;

  if (envelope.kind === "hello") {
    state.lastHelloAtMs = envelope.sentAtMs;
  }

  if (state.health === "recovering" || state.health === "fatal") {
    state.health = "healthy";
  }
}

export function recordDiagnosticBatch(envelope: LiveEnvelope): void {
  if (envelope.kind !== "diagnosticBatch") return;

  const payload = envelope.payload as DiagnosticBatchPayload;
  for (const diagnostic of payload.diagnostics) {
    appendDiagnostic(fromProtocolDiagnostic(diagnostic));
    if (diagnostic.severity === "fatal" || diagnostic.impact === "streamFatal") {
      state.health = "fatal";
    } else if (diagnostic.severity === "error" || diagnostic.impact !== "none") {
      state.health = state.health === "fatal" ? "fatal" : "degraded";
    }
  }
}

export function recordStats(envelope: LiveEnvelope): void {
  if (envelope.kind !== "stats") return;

  const payload = envelope.payload as StatsPayload;
  state.latestStats = payload;

  if (payload.healthStatus === "fatal") {
    state.health = "fatal";
  } else if (payload.healthStatus === "degraded") {
    state.health = state.health === "fatal" ? "fatal" : "degraded";
  }
}

export function resetLiveDiagnosticsState(): void {
  state.recent = [];
  state.consecutiveInvalidFrames = 0;
  state.totalInvalidFrames = 0;
  state.lastValidFrameAtMs = null;
  state.lastValidFrameId = null;
  state.lastHelloAtMs = null;
  state.latestStats = null;
  state.health = "healthy";
}

function appendDiagnostic(diagnostic: LiveDiagnosticEntry): void {
  state.recent = [...state.recent, diagnostic].slice(-MAX_RECENT_DIAGNOSTICS);
}

function fromParseError(error: ParseError): LiveDiagnosticEntry {
  const now = Date.now();
  return {
    id: `parse-${state.totalInvalidFrames}`,
    code: error.code,
    severity: isFatalParseError(error) ? "fatal" : "warning",
    impact: isFatalParseError(error) ? "streamFatal" : "frameSkipped",
    component: "web.protocol",
    operation: "parseMessage",
    message: error.message,
    frameId: error.header?.frameId,
    kind: error.header?.kind,
    sessionIdHash: hashSessionId(error.header),
    rawHash: error.rawHash,
    path: firstIssuePath(error.message),
    firstSeenAtMs: now,
    lastSeenAtMs: now,
    count: 1,
    suppressedCount: 0,
  };
}

function fromProtocolDiagnostic(diagnostic: DiagnosticRecord): LiveDiagnosticEntry {
  return {
    id: diagnostic.id,
    code: diagnostic.code,
    severity: diagnostic.severity,
    impact: diagnostic.impact,
    component: diagnostic.component,
    operation: diagnostic.operation,
    message: diagnostic.message,
    frameId: diagnostic.frameId,
    sessionIdHash: diagnostic.sessionId ? hashString(diagnostic.sessionId) : undefined,
    path: typeof diagnostic.details?.path === "string" ? diagnostic.details.path : undefined,
    firstSeenAtMs: diagnostic.firstSeenAtMs,
    lastSeenAtMs: diagnostic.lastSeenAtMs,
    count: diagnostic.count,
    suppressedCount: diagnostic.suppressedCount,
  };
}

function isFatalParseError(error: ParseError): boolean {
  return error.code === "unsupported_version" || error.code === "unknown_kind";
}

function hashSessionId(header: ParseErrorHeader | undefined): string | undefined {
  return header?.sessionId ? hashString(header.sessionId) : undefined;
}

function hashString(value: string): string {
  let hash = 5381;
  for (let index = 0; index < value.length; index += 1) {
    hash = ((hash << 5) + hash) ^ value.charCodeAt(index);
  }

  return (hash >>> 0).toString(16).padStart(8, "0");
}

function firstIssuePath(message: string): string | undefined {
  const separator = message.indexOf(":");
  if (separator <= 0) return undefined;
  return message.slice(0, separator);
}
