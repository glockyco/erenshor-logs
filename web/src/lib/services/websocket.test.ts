import { afterEach, describe, expect, it, vi } from "vitest";
import { createWebSocketClient, type WebSocketCallbacks } from "./websocket";

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
});
