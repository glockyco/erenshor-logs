import { beforeEach, describe, expect, it } from "vitest";
import eventsFrame from "../../../../shared/protocol/fixtures/live/events.json";
import registryDeltaFrame from "../../../../shared/protocol/fixtures/live/registry-delta.json";
import sessionEndedFrame from "../../../../shared/protocol/fixtures/live/session-ended.json";
import snapshotFrame from "../../../../shared/protocol/fixtures/live/session-snapshot.json";
import {
  activeSessionId,
  applyLiveEnvelope,
  deleteSession,
  protocolErrors,
  resetSessionsState,
  sessions,
  setActiveSession,
} from "./sessions.svelte";
import type { LiveEnvelope } from "$lib/types";

const clone = <T>(value: T): T => JSON.parse(JSON.stringify(value)) as T;

describe("protocol v2 session state", () => {
  beforeEach(() => resetSessionsState());

  it("replaces retained session state when a snapshot arrives", () => {
    applyLiveEnvelope(snapshotFrame as LiveEnvelope);
    applyLiveEnvelope(eventsFrame as LiveEnvelope);

    expect(sessions.get("session-1")?.events).toHaveLength(6);

    applyLiveEnvelope(snapshotFrame as LiveEnvelope);

    expect(sessions.get("session-1")?.events).toHaveLength(0);
    expect(sessions.get("session-1")?.registries.actors.a1.name).toBe("Player");
    expect(activeSessionId.value).toBe("session-1");
  });

  it("applies registry deltas before event batches", () => {
    applyLiveEnvelope(snapshotFrame as LiveEnvelope);
    applyLiveEnvelope(registryDeltaFrame as LiveEnvelope);
    applyLiveEnvelope(eventsFrame as LiveEnvelope);

    const session = sessions.get("session-1")!;
    expect(session.registries.effects.ef1.name).toBe("Poisoned Wound");
    expect(session.events[0].eventSeq).toBe(1);
    expect(session.lastEventSeq).toBe(6);
  });

  it("accepts replayed catch-up events after a non-empty snapshot", () => {
    const snapshot = clone(snapshotFrame) as LiveEnvelope;
    snapshot.payload = {
      ...(snapshot.payload as Record<string, unknown>),
      lastEventSeq: 6,
      eventCount: 6,
    };

    applyLiveEnvelope(snapshot);
    applyLiveEnvelope(eventsFrame as LiveEnvelope);

    const session = sessions.get("session-1")!;
    expect(session.events).toHaveLength(6);
    expect(session.lastEventSeq).toBe(6);
    expect(session.eventCount).toBe(6);
    expect(protocolErrors.value).not.toContainEqual(
      expect.objectContaining({ code: "event_sequence_gap", sessionId: "session-1" })
    );
  });

  it("marks sequence gaps as visible protocol errors", () => {
    applyLiveEnvelope(snapshotFrame as LiveEnvelope);
    const payload = (eventsFrame as LiveEnvelope).payload as Record<string, unknown>;
    applyLiveEnvelope({
      ...(eventsFrame as LiveEnvelope),
      payload: {
        ...payload,
        eventSeqStart: 2,
      },
    } as LiveEnvelope);

    expect(protocolErrors.value).toContainEqual(
      expect.objectContaining({ code: "event_sequence_gap", sessionId: "session-1" })
    );
    expect(sessions.get("session-1")?.completeness).toBe("partial");
    expect(sessions.get("session-1")?.events).toHaveLength(0);
  });

  it("records session end metadata", () => {
    applyLiveEnvelope(snapshotFrame as LiveEnvelope);
    applyLiveEnvelope(sessionEndedFrame as LiveEnvelope);

    const session = sessions.get("session-1")!;
    expect(session.state).toBe("ended");
    expect(session.endedAtUtcMs).toBe(1800000019000);
    expect(session.endReason).toBe("inactivity");
  });

  it("records error frames as protocol errors", () => {
    applyLiveEnvelope({
      protocol: "erenshor.logs.live",
      protocolVersion: "2.0.0",
      schemaVersion: "2.0.0",
      kind: "error",
      frameSeq: 1,
      sentAtMs: 1800000021000,
      payload: {
        code: "hookCompatibilityWarning",
        severity: "warning",
        message: "Optional hook missing.",
        recoverable: true,
        sessionId: "session-1",
      },
    } as LiveEnvelope);

    expect(protocolErrors.value).toContainEqual(
      expect.objectContaining({
        code: "hookCompatibilityWarning",
        message: "Optional hook missing.",
        sessionId: "session-1",
      })
    );
  });

  it("keeps active session selection guarded", () => {
    applyLiveEnvelope(snapshotFrame as LiveEnvelope);
    setActiveSession("missing");

    expect(activeSessionId.value).toBe("session-1");

    setActiveSession(null);
    expect(activeSessionId.value).toBeNull();
  });

  it("deletes sessions and clears active selection", () => {
    applyLiveEnvelope(snapshotFrame as LiveEnvelope);
    deleteSession("session-1");

    expect(sessions.has("session-1")).toBe(false);
    expect(activeSessionId.value).toBeNull();
  });

  it("rejects events for unknown sessions", () => {
    const orphanFrame = clone(eventsFrame) as LiveEnvelope;
    orphanFrame.sessionId = "missing";
    orphanFrame.payload = { ...(orphanFrame.payload as object), sessionId: "missing" };

    applyLiveEnvelope(orphanFrame);

    expect(protocolErrors.value).toContainEqual(
      expect.objectContaining({ code: "unknown_session", sessionId: "missing" })
    );
  });
});
