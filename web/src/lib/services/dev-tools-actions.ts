import { setConnected, setDisconnected, setError } from "$lib/state/connection.svelte";

export function simulateHealthyModConnection(): void {
  setConnected({
    protocol: "erenshor.logs.live",
    protocolVersion: "2.0.0",
    schemaVersion: "2.0.0",
    kind: "hello",
    frameSeq: 1,
    sentAtMs: Date.now(),
    payload: {
      producer: { name: "ErenshorLogsMod", modVersion: "2026.1.1.1000" },
      capabilities: ["registryDelta", "sessionSnapshot"],
    },
  });
}

export function simulateLegacyModConnectionError(): void {
  setDisconnected();
  setError(
    "legacy_mod",
    "An old Erenshor Logs mod connected. Update the mod to a protocol v2 build."
  );
}

export function simulateMalformedLiveFrameError(): void {
  setError("parse_error", "Received malformed protocol v2 live data.");
}

export function simulateUnexpectedDisconnect(): void {
  setDisconnected();
  setError("unexpected_disconnect", "Connection closed unexpectedly (code: 1006)");
}
