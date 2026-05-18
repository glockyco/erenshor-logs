import { describe, expect, it } from "vitest";
import hello from "../../../../shared/protocol/fixtures/live-v3/hello.json";
import sessionOpened from "../../../../shared/protocol/fixtures/live-v3/session-opened.json";
import registryDelta from "../../../../shared/protocol/fixtures/live-v3/registry-delta.json";
import eventBatch from "../../../../shared/protocol/fixtures/live-v3/event-batch.json";
import diagnosticBatch from "../../../../shared/protocol/fixtures/live-v3/diagnostic-batch.json";
import stats from "../../../../shared/protocol/fixtures/live-v3/stats.json";
import sessionClosed from "../../../../shared/protocol/fixtures/live-v3/session-closed.json";
import singleSessionExport from "../../../../shared/protocol/fixtures/export/single-session.json";
import multiSessionExport from "../../../../shared/protocol/fixtures/export/multi-session.json";
import demoExport from "../../../static/demo/sessions.json";
import { CombatLogFileSchema, LiveEnvelopeSchema } from "$lib/types/schemas";

const liveFixtures = [
  hello,
  sessionOpened,
  registryDelta,
  eventBatch,
  diagnosticBatch,
  stats,
  sessionClosed,
];

const clone = <T>(value: T): T => JSON.parse(JSON.stringify(value)) as T;

describe("protocol v3 fixtures", () => {
  it.each(liveFixtures)("validates live fixture %#", (fixture) => {
    expect(() => LiveEnvelopeSchema.parse(fixture)).not.toThrow();
  });

  it("validates single-session export", () => {
    const file = CombatLogFileSchema.parse(singleSessionExport);

    expect(file.sessions).toHaveLength(1);
    expect(file.sessions[0].events).toHaveLength(6);
  });

  it("fixtures include health-affecting raid event families", () => {
    const file = CombatLogFileSchema.parse(singleSessionExport);
    const allEvents = file.sessions.flatMap((session) => session.events);

    expect(allEvents.some((event) => event.kind === "damage")).toBe(true);
    expect(allEvents.some((event) => event.kind === "heal")).toBe(true);
    expect(allEvents.some((event) => event.kind === "resource")).toBe(true);
    expect(allEvents.some((event) => event.kind === "effect")).toBe(true);
    expect(allEvents.some((event) => event.kind === "death")).toBe(true);
    expect(allEvents.some((event) => event.kind === "mechanic")).toBe(true);
  });

  it("validates multi-session export", () => {
    const file = CombatLogFileSchema.parse(multiSessionExport);

    expect(file.sessions).toHaveLength(2);
  });

  it("validates demo export", () => {
    const file = CombatLogFileSchema.parse(demoExport);

    expect(file.sessions).toHaveLength(4);
    expect(file.sessions.reduce((sum, session) => sum + session.events.length, 0)).toBe(1483);
    expect(
      file.sessions.flatMap((session) =>
        session.events.filter((event) => event.attribution === "unknown" && event.debug)
      )
    ).toHaveLength(16);
  });

  it("rejects event batches with gaps", () => {
    const gappedEvents = clone(eventBatch);
    gappedEvents.payload.events[0].eventSeq = 2;

    expect(() => LiveEnvelopeSchema.parse(gappedEvents)).toThrow();
  });

  it("allows connection-scoped heartbeat frames without a session", () => {
    expect(() =>
      LiveEnvelopeSchema.parse({
        protocol: "erenshor.logs.live",
        protocolVersion: "3.0.0",
        schemaVersion: "3.0.0",
        frameId: 8,
        kind: "heartbeat",
        sentAtMs: 1_800_000_000_700,
        producer: {
          name: "ErenshorLogsMod",
          modVersion: "2026.5.17.95539912",
        },
        payload: {},
      })
    ).not.toThrow();
  });

  it("rejects partial session-opened frames without loss counters", () => {
    const partialSessionOpened = clone(sessionOpened) as {
      payload: { completeness: string; loss?: unknown };
    };
    partialSessionOpened.payload.completeness = "partial";
    delete partialSessionOpened.payload.loss;

    expect(() => LiveEnvelopeSchema.parse(partialSessionOpened)).toThrow();
  });
});
