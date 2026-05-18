import { beforeEach, describe, expect, it } from "vitest";
import diagnosticBatch from "../../../../shared/protocol/fixtures/live-v3/diagnostic-batch.json";
import eventBatch from "../../../../shared/protocol/fixtures/live-v3/event-batch.json";
import statsFrame from "../../../../shared/protocol/fixtures/live-v3/stats.json";
import {
  liveDiagnostics,
  recordDiagnosticBatch,
  recordParseError,
  recordStats,
  recordValidFrame,
  resetLiveDiagnosticsState,
} from "./live-diagnostics.svelte";
import type { LiveEnvelope, ParseError } from "$lib/types";

const invalidFrame = (frameId: number): ParseError => ({
  code: "invalid_structure",
  message: "payload.events.0.action: Invalid option",
  rawHash: `hash-${frameId}`,
  header: {
    protocol: "erenshor.logs.live",
    protocolVersion: "3.0.0",
    schemaVersion: "3.0.0",
    kind: "eventBatch",
    frameId,
    sessionId: "session-1",
  },
});

describe("live diagnostics state", () => {
  beforeEach(() => {
    resetLiveDiagnosticsState();
  });

  it("records recoverable parse errors without starting fatal", () => {
    recordParseError(invalidFrame(42));

    expect(liveDiagnostics.value.consecutiveInvalidFrames).toBe(1);
    expect(liveDiagnostics.value.totalInvalidFrames).toBe(1);
    expect(liveDiagnostics.value.health).toBe("recovering");
    expect(liveDiagnostics.value.recent[0]).toMatchObject({
      code: "invalid_structure",
      frameId: 42,
      kind: "eventBatch",
      rawHash: "hash-42",
    });
  });

  it("marks stream fatal after three consecutive parse errors", () => {
    recordParseError(invalidFrame(1));
    recordParseError(invalidFrame(2));
    recordParseError(invalidFrame(3));

    expect(liveDiagnostics.value.consecutiveInvalidFrames).toBe(3);
    expect(liveDiagnostics.value.health).toBe("fatal");
  });

  it("valid frames recover a stream made fatal by consecutive recoverable parse errors", () => {
    recordParseError(invalidFrame(1));
    recordParseError(invalidFrame(2));
    recordParseError(invalidFrame(3));

    recordValidFrame(eventBatch as LiveEnvelope);

    expect(liveDiagnostics.value.consecutiveInvalidFrames).toBe(0);
    expect(liveDiagnostics.value.health).toBe("healthy");
  });

  it("valid frames reset consecutive parse errors and remember frame metadata", () => {
    recordParseError(invalidFrame(41));

    recordValidFrame(eventBatch as LiveEnvelope);

    expect(liveDiagnostics.value.consecutiveInvalidFrames).toBe(0);
    expect(liveDiagnostics.value.health).toBe("healthy");
    expect(liveDiagnostics.value.lastValidFrameId).toBe(4);
    expect(liveDiagnostics.value.lastValidFrameAtMs).toBe(1800000000300);
  });

  it("keeps only the last twenty diagnostics", () => {
    for (let frameId = 1; frameId <= 25; frameId += 1) {
      recordParseError(invalidFrame(frameId));
      recordValidFrame(eventBatch as LiveEnvelope);
    }

    expect(liveDiagnostics.value.recent).toHaveLength(20);
    expect(liveDiagnostics.value.recent[0].frameId).toBe(6);
    expect(liveDiagnostics.value.recent[19].frameId).toBe(25);
  });

  it("applies diagnostic batches and stats without session storage", () => {
    recordDiagnosticBatch(diagnosticBatch as LiveEnvelope);
    recordStats(statsFrame as LiveEnvelope);

    expect(liveDiagnostics.value.health).toBe("degraded");
    expect(liveDiagnostics.value.recent[0]).toMatchObject({
      code: "projection.failed",
      severity: "error",
      impact: "eventDropped",
      frameId: 4,
    });
    expect(liveDiagnostics.value.latestStats?.projectionErrors).toBe(2);
    expect(liveDiagnostics.value.latestStats?.healthStatus).toBe("degraded");
  });
});
