import { afterEach, describe, expect, it, vi } from "vitest";
import { createWebSocketClient, type WebSocketCallbacks } from "./websocket";
import hello from "../../../../shared/protocol/fixtures/live-v3/hello.json";
import eventBatch from "../../../../shared/protocol/fixtures/live-v3/event-batch.json";
import { liveDiagnostics, resetLiveDiagnosticsState } from "$lib/state/live-diagnostics.svelte";

class FakeWebSocket {
  static instances: FakeWebSocket[] = [];

  onopen: (() => void) | null = null;
  onmessage: ((event: MessageEvent) => void) | null = null;
  onerror: (() => void) | null = null;
  onclose: ((event: CloseEvent) => void) | null = null;

  constructor(public readonly url: string) {
    FakeWebSocket.instances.push(this);
  }

  close(): void {
    this.onclose?.(new CloseEvent("close", { code: 1000, wasClean: true }));
  }

  receive(data: unknown): void {
    this.onmessage?.(new MessageEvent("message", { data: JSON.stringify(data) }));
  }

  closeWith(code: number, wasClean: boolean): void {
    this.onclose?.(new CloseEvent("close", { code, wasClean }));
  }
}

describe("createWebSocketClient", () => {
  afterEach(() => {
    resetLiveDiagnosticsState();
    vi.unstubAllGlobals();
    FakeWebSocket.instances = [];
    vi.restoreAllMocks();
  });

  it("passes legacy mod parse errors through as compatibility errors", () => {
    vi.stubGlobal("WebSocket", FakeWebSocket);
    const onError = vi.fn<WebSocketCallbacks["onError"]>();

    const client = createWebSocketClient(
      {
        onConnecting: vi.fn(),
        onConnected: vi.fn(),
        onFrame: vi.fn(),
        onDisconnected: vi.fn(),
        onError,
      },
      { url: "ws://localhost:38729", autoReconnect: true }
    );

    FakeWebSocket.instances[0].receive({
      type: "handshake",
      protocolVersion: "1.0.0",
      modVersion: "2025.1.1.1",
    });

    expect(onError).toHaveBeenCalledWith(
      "legacy_mod",
      expect.stringContaining("old Erenshor Logs mod")
    );

    client.disconnect();
  });

  it("surfaces fatal capture health from hello frames", () => {
    vi.stubGlobal("WebSocket", FakeWebSocket);
    const onError = vi.fn<WebSocketCallbacks["onError"]>();

    createWebSocketClient(
      {
        onConnecting: vi.fn(),
        onConnected: vi.fn(),
        onFrame: vi.fn(),
        onDisconnected: vi.fn(),
        onError,
      },
      { url: "ws://localhost:38729", autoReconnect: true }
    );

    FakeWebSocket.instances[0].receive({
      ...hello,
      payload: {
        ...hello.payload,
        health: { status: "fatal", captureAvailable: false },
      },
    });

    expect(onError).toHaveBeenCalledWith("capture_unavailable", "Combat capture is unavailable.");
  });

  it("surfaces degraded stream health from stats frames", () => {
    vi.stubGlobal("WebSocket", FakeWebSocket);
    const onError = vi.fn<WebSocketCallbacks["onError"]>();

    createWebSocketClient(
      {
        onConnecting: vi.fn(),
        onConnected: vi.fn(),
        onFrame: vi.fn(),
        onDisconnected: vi.fn(),
        onError,
      },
      { url: "ws://localhost:38729", autoReconnect: true }
    );

    FakeWebSocket.instances[0].receive(hello);
    FakeWebSocket.instances[0].receive({
      protocol: "erenshor.logs.live",
      protocolVersion: "3.0.0",
      schemaVersion: "3.0.0",
      frameId: 8,
      kind: "stats",
      sentAtMs: 1800000000500,
      producer: hello.producer,
      payload: {
        uptimeMs: 120000,
        connectedClients: 1,
        capturedEvents: 1,
        projectedEvents: 1,
        sentEvents: 1,
        sentFrames: 1,
        droppedEvents: 1,
        droppedFrames: 0,
        projectionErrors: 1,
        serializationErrors: 0,
        clientSendErrors: 0,
        hookWarnings: 0,
        attributionFailures: 0,
        diagnosticsEmitted: 1,
        diagnosticsSuppressed: 0,
        queueDepth: 0,
        registryRevision: 3,
        healthStatus: "degraded",
      },
    });

    expect(onError).toHaveBeenCalledWith("stream_degraded", "Some live data was skipped.");
  });

  it("does not report connection lost before a valid hello", () => {
    vi.stubGlobal("WebSocket", FakeWebSocket);
    const onError = vi.fn<WebSocketCallbacks["onError"]>();

    createWebSocketClient(
      {
        onConnecting: vi.fn(),
        onConnected: vi.fn(),
        onFrame: vi.fn(),
        onDisconnected: vi.fn(),
        onError,
      },
      { url: "ws://localhost:38729", autoReconnect: true }
    );

    FakeWebSocket.instances[0].closeWith(1006, false);

    expect(onError).not.toHaveBeenCalledWith("unexpected_disconnect", expect.any(String));
  });

  it("records recoverable parse errors without surfacing invalid live data", () => {
    resetLiveDiagnosticsState();
    vi.stubGlobal("WebSocket", FakeWebSocket);
    const onError = vi.fn<WebSocketCallbacks["onError"]>();

    createWebSocketClient(
      {
        onConnecting: vi.fn(),
        onConnected: vi.fn(),
        onFrame: vi.fn(),
        onDisconnected: vi.fn(),
        onError,
      },
      { url: "ws://localhost:38729", autoReconnect: true }
    );

    FakeWebSocket.instances[0].receive(hello);
    FakeWebSocket.instances[0].receive({
      ...eventBatch,
      payload: { ...eventBatch.payload, events: [] },
    });

    expect(onError).not.toHaveBeenCalledWith("parse_error", expect.any(String));
    expect(liveDiagnostics.value.health).toBe("recovering");
    expect(liveDiagnostics.value.recent[0]).toMatchObject({
      code: "invalid_structure",
      kind: "eventBatch",
      frameId: 4,
    });
  });

  it("reports connection lost after a valid hello", () => {
    vi.stubGlobal("WebSocket", FakeWebSocket);
    const onError = vi.fn<WebSocketCallbacks["onError"]>();

    createWebSocketClient(
      {
        onConnecting: vi.fn(),
        onConnected: vi.fn(),
        onFrame: vi.fn(),
        onDisconnected: vi.fn(),
        onError,
      },
      { url: "ws://localhost:38729", autoReconnect: true }
    );

    FakeWebSocket.instances[0].receive(hello);
    FakeWebSocket.instances[0].closeWith(1006, false);

    expect(onError).toHaveBeenCalledWith(
      "unexpected_disconnect",
      "Connection closed unexpectedly (code: 1006)"
    );
  });
});
