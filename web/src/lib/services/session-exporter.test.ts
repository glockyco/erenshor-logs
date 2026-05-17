import { describe, expect, it } from "vitest";
import singleSessionFile from "../../../../shared/protocol/fixtures/export/single-session.json";
import { importSessions } from "./session-importer";
import { createCombatLogFile } from "./session-exporter";

describe("createCombatLogFile", () => {
  it("exports sessions in protocol v2 file shape", () => {
    const imported = importSessions(JSON.stringify(singleSessionFile));
    if (!imported.success) throw new Error(imported.error);

    const exported = createCombatLogFile(imported.sessions, 1800000023000);

    expect(exported.format).toBe("erenshor.logs.export");
    expect(exported.schemaVersion).toBe("2.0.0");
    expect(exported.sessions).toHaveLength(1);
    expect(exported.sessions[0].snapshot.sessionId).toBe("session-1");
    expect(exported.sessions[0].snapshot.registries.actors.a1.name).toBe("Player");
    expect(exported.sessions[0].events[0].eventSeq).toBe(1);
    expect(exported.sessions[0].derived?.summary.totalDamage).toBe(350);
  });

  it("fails fast when exporting an empty session list", () => {
    expect(() => createCombatLogFile([], 1800000023000)).toThrow(
      "Cannot export an empty session list"
    );
  });
});
