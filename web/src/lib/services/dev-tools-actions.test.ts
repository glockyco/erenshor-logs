import { beforeEach, describe, expect, it } from "vitest";
import {
  connectionError,
  connectionStatus,
  modVersion,
  resetConnectionState,
} from "$lib/state/connection.svelte";
import {
  simulateLegacyModConnectionError,
  simulateMalformedLiveFrameError,
  simulateUnexpectedDisconnect,
  simulateHealthyModConnection,
} from "./dev-tools-actions";

describe("dev tools actions", () => {
  beforeEach(() => {
    resetConnectionState();
  });

  it("can simulate the old mod compatibility error", () => {
    simulateLegacyModConnectionError();

    expect(connectionStatus.value).toBe("disconnected");
    expect(connectionError.value?.code).toBe("legacy_mod");
    expect(connectionError.value?.message).toContain("old Erenshor Logs mod");
    expect(connectionError.value?.message).toContain("protocol v2");
  });

  it("can simulate malformed live data", () => {
    simulateMalformedLiveFrameError();

    expect(connectionError.value?.code).toBe("parse_error");
    expect(connectionError.value?.message).toBe("Received malformed protocol v2 live data.");
  });

  it("can simulate an unexpected disconnect", () => {
    simulateUnexpectedDisconnect();

    expect(connectionStatus.value).toBe("disconnected");
    expect(connectionError.value?.code).toBe("unexpected_disconnect");
  });

  it("can simulate a healthy mod connection", () => {
    simulateHealthyModConnection();

    expect(connectionStatus.value).toBe("connected");
    expect(connectionError.value).toBeNull();
    expect(modVersion.value).toBe("2026.1.1.1000");
  });
});
