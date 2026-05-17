import { describe, expect, it } from "vitest";
import singleSessionFile from "../../../../shared/protocol/fixtures/export/single-session.json";
import multiSessionFile from "../../../../shared/protocol/fixtures/export/multi-session.json";
import { importSessions } from "./session-importer";

const clone = <T>(value: T): T => JSON.parse(JSON.stringify(value)) as T;

describe("importSessions", () => {
  it("imports protocol v2 single-session export files", () => {
    const result = importSessions(JSON.stringify(singleSessionFile));

    expect(result.success).toBe(true);
    if (!result.success) return;
    expect(result.sessions).toHaveLength(1);
    expect(result.sessions[0].id).toBe("session-1");
    expect(result.sessions[0].registries.actors.a2.name).toBe("Backstabber");
    expect(result.sessions[0].events).toHaveLength(6);
    expect(result.sessions[0].protocolErrors).toEqual([]);
  });

  it("freezes active exports as ended imported sessions", () => {
    const file = clone(singleSessionFile);
    const exportedSession = file.sessions[0] as {
      snapshot: {
        state: "active" | "ended";
        mode: "automatic" | "manual" | "imported";
        endedAtUtcMs?: number;
        endReason?: string;
        durationMs?: number;
      };
      ended?: unknown;
    };
    exportedSession.snapshot.state = "active";
    exportedSession.snapshot.mode = "automatic";
    delete exportedSession.snapshot.endedAtUtcMs;
    delete exportedSession.snapshot.endReason;
    delete exportedSession.snapshot.durationMs;
    delete exportedSession.ended;

    const result = importSessions(JSON.stringify(file));

    expect(result.success).toBe(true);
    if (!result.success) return;
    expect(result.sessions[0]).toMatchObject({
      mode: "imported",
      state: "ended",
      endedAtUtcMs: 1800000018000,
      durationMs: 18000,
      lastEventSeq: 6,
      eventCount: 6,
    });
    expect(result.sessions[0].endReason).toBeUndefined();
  });

  it("imports all sessions from protocol v2 multi-session export files", () => {
    const result = importSessions(JSON.stringify(multiSessionFile));

    expect(result.success).toBe(true);
    if (!result.success) return;
    expect(result.sessions.map((session) => session.id)).toEqual(["session-1", "session-2"]);
  });

  it("rejects legacy wrapper files with a clear compatibility message", () => {
    const result = importSessions(
      JSON.stringify({
        version: "1.0.0",
        exportedAt: 1,
        sessions: [],
      })
    );

    expect(result).toEqual({
      success: false,
      error:
        "This log uses the old Erenshor Logs v1 format. Importing v1 logs is no longer supported; use a protocol v2 export.",
    });
  });
});
