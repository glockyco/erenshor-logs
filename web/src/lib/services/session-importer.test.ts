import { describe, expect, it } from "vitest";
import singleSessionFile from "../../../../shared/protocol/fixtures/export/single-session.json";
import multiSessionFile from "../../../../shared/protocol/fixtures/export/multi-session.json";
import { importSessions } from "./session-importer";

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

  it("imports all sessions from protocol v2 multi-session export files", () => {
    const result = importSessions(JSON.stringify(multiSessionFile));

    expect(result.success).toBe(true);
    if (!result.success) return;
    expect(result.sessions.map((session) => session.id)).toEqual(["session-1", "session-2"]);
  });

  it("rejects legacy wrapper files instead of silently migrating", () => {
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
        "File does not match the Erenshor Logs v2 export format or uses an unsupported schema version.",
    });
  });
});
