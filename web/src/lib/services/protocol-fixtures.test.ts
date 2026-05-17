import { describe, expect, it } from "vitest";
import hello from "../../../../shared/protocol/fixtures/live/hello.json";
import sessionSnapshot from "../../../../shared/protocol/fixtures/live/session-snapshot.json";
import registryDelta from "../../../../shared/protocol/fixtures/live/registry-delta.json";
import events from "../../../../shared/protocol/fixtures/live/events.json";
import sessionEnded from "../../../../shared/protocol/fixtures/live/session-ended.json";
import errorFrame from "../../../../shared/protocol/fixtures/live/error.json";
import singleSessionExport from "../../../../shared/protocol/fixtures/export/single-session.json";
import multiSessionExport from "../../../../shared/protocol/fixtures/export/multi-session.json";
import demoExport from "../../../static/demo/sessions.json";
import { CombatLogFileSchema, LiveEnvelopeSchema } from "$lib/types/schemas";

const liveFixtures = [hello, sessionSnapshot, registryDelta, events, sessionEnded, errorFrame];

const clone = <T>(value: T): T => JSON.parse(JSON.stringify(value)) as T;

describe("protocol v2 fixtures", () => {
  it.each(liveFixtures)("validates live fixture %#", (fixture) => {
    expect(() => LiveEnvelopeSchema.parse(fixture)).not.toThrow();
  });

  it("validates single-session export", () => {
    const file = CombatLogFileSchema.parse(singleSessionExport);

    expect(file.sessions).toHaveLength(1);
    expect(file.sessions[0].events).toHaveLength(6);
  });

  it("validates multi-session export", () => {
    const file = CombatLogFileSchema.parse(multiSessionExport);

    expect(file.sessions).toHaveLength(2);
  });

  it("validates demo export", () => {
    const file = CombatLogFileSchema.parse(demoExport);

    expect(file.sessions).toHaveLength(4);
    expect(file.sessions.reduce((sum, session) => sum + session.events.length, 0)).toBe(1483);
  });

  it("rejects event batches with gaps", () => {
    const gappedEvents = clone(events);
    gappedEvents.payload.events[1].eventSeq = 3;

    expect(() => LiveEnvelopeSchema.parse(gappedEvents)).toThrow();
  });

  it("allows connection-scoped heartbeat frames without a session", () => {
    expect(() =>
      LiveEnvelopeSchema.parse({
        protocol: "erenshor.logs.live",
        protocolVersion: "2.0.0",
        schemaVersion: "2.0.0",
        kind: "heartbeat",
        frameSeq: 7,
        sentAtMs: 1_764_000_000_000,
        payload: {},
      })
    ).not.toThrow();
  });

  it("rejects partial snapshots without loss counters", () => {
    const partialSnapshot = clone(sessionSnapshot) as {
      payload: { completeness: string; loss?: unknown };
    };
    partialSnapshot.payload.completeness = "partial";
    delete partialSnapshot.payload.loss;

    expect(() => LiveEnvelopeSchema.parse(partialSnapshot)).toThrow();
  });
});
