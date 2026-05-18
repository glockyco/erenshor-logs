import { describe, expect, it } from "vitest";
import type { ConnectionError } from "$lib/types";
import { getConnectionAlert } from "./connection-alert";

function error(code: ConnectionError["code"], message: string): ConnectionError {
  return { code, message, timestamp: 1_700_000_000_000 };
}

describe("getConnectionAlert", () => {
  it("returns no alert when there is no connection error", () => {
    expect(getConnectionAlert(null)).toBeNull();
  });

  it("surfaces legacy mod errors as update-required alerts", () => {
    const alert = getConnectionAlert(
      error(
        "legacy_mod",
        "An old Erenshor Logs mod connected. Update the mod to a protocol v2 build."
      )
    );

    expect(alert).toEqual({
      title: "Old mod connected",
      message: "An old Erenshor Logs mod connected. Update the mod to a protocol v2 build.",
      tone: "error",
    });
  });

  it("surfaces generic parse errors as invalid live data", () => {
    const alert = getConnectionAlert(error("parse_error", "Unknown frame kind: legacy"));

    expect(alert).toEqual({
      title: "Invalid live data",
      message: "Unknown frame kind: legacy",
      tone: "error",
    });
  });

  it("surfaces preview mismatches with an update-specific title", () => {
    const alert = getConnectionAlert(
      error("preview_mismatch", "Unsupported protocol version: 2.0.0")
    );

    expect(alert).toEqual({
      title: "Preview mod is out of date",
      message: "Unsupported protocol version: 2.0.0",
      tone: "error",
    });
  });
});
