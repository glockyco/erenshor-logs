import { setConnected, setDisconnected, setError } from "$lib/state/connection.svelte";

export function simulateHealthyModConnection(): void {
  setConnected({
    protocol: "erenshor.logs.live",
    protocolVersion: "3.0.0",
    schemaVersion: "3.0.0",
    frameId: 1,
    kind: "hello",
    sentAtMs: Date.now(),
    producer: { name: "ErenshorLogsMod", modVersion: "2026.1.1.1000" },
    payload: {
      capabilities: ["eventBatch", "diagnosticBatch", "stats"],
      health: { status: "healthy", captureAvailable: true },
      patches: [],
      limits: { maxFrameBytes: 262144, maxEventsPerBatch: 256, diagnosticRingSize: 32 },
      diagnosticSummary: { fatal: 0, error: 0, warning: 0, info: 0 },
    },
  });
}

export function simulateLegacyModConnectionError(): void {
  setDisconnected();
  setError(
    "legacy_mod",
    "An old Erenshor Logs mod connected. Update the mod to a protocol v3 build."
  );
}

export function simulateMalformedLiveFrameError(): void {
  setError("parse_error", "Received malformed protocol v3 live data.");
}

export function simulateUnexpectedDisconnect(): void {
  setDisconnected();
  setError("unexpected_disconnect", "Connection closed unexpectedly (code: 1006)");
}
