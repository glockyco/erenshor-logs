import { afterEach, describe, expect, it, vi } from "vitest";
import { createWebSocketClient, type WebSocketCallbacks } from "./websocket";
import hello from "../../../../shared/protocol/fixtures/live/hello.json";

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
