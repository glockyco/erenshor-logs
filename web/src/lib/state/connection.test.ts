import { describe, it, expect, beforeEach } from "vitest";
import hello from "../../../../shared/protocol/fixtures/live/hello.json";
import {
  connectionStatus,
  connectionError,
  protocolVersion,
  modVersion,
  setConnecting,
  setConnected,
  setDisconnected,
  setError,
  clearError,
  resetConnectionState,
} from "./connection.svelte";
import type { LiveEnvelope } from "$lib/types";

const helloFrame = hello as LiveEnvelope;

describe("connection state", () => {
  beforeEach(() => {
    resetConnectionState();
  });

  describe("initial state", () => {
    it("starts as disconnected", () => {
      expect(connectionStatus.value).toBe("disconnected");
    });

    it("has no error", () => {
      expect(connectionError.value).toBeNull();
    });

    it("has no protocol version", () => {
      expect(protocolVersion.value).toBeNull();
    });

    it("has no mod version", () => {
      expect(modVersion.value).toBeNull();
    });
  });

  describe("setConnecting", () => {
    it("sets status to connecting", () => {
      setConnecting();

      expect(connectionStatus.value).toBe("connecting");
    });

    it("keeps existing errors visible while reconnecting", () => {
      setError("unexpected_disconnect", "Connection lost");

      setConnecting();

      expect(connectionError.value?.code).toBe("unexpected_disconnect");
      expect(connectionError.value?.message).toBe("Connection lost");
    });
  });

  describe("setConnected", () => {
    it("sets status to connected", () => {
      setConnected(helloFrame);

      expect(connectionStatus.value).toBe("connected");
    });

    it("stores protocol and mod versions", () => {
      setConnected(helloFrame);

      expect(protocolVersion.value).toBe("2.0.0");
      expect(modVersion.value).toBe("2026.5.17.14");
    });

    it("clears any existing error", () => {
      setError("connection_failed", "Test error");

      setConnected(helloFrame);

      expect(connectionError.value).toBeNull();
    });
  });

  describe("setDisconnected", () => {
    it("sets status to disconnected", () => {
      setConnecting();

      setDisconnected();

      expect(connectionStatus.value).toBe("disconnected");
    });

    it("clears protocol and mod versions", () => {
      setConnected(helloFrame);

      setDisconnected();

      expect(protocolVersion.value).toBeNull();
      expect(modVersion.value).toBeNull();
    });

    it("does not clear error", () => {
      setError("unexpected_disconnect", "Connection lost");

      setDisconnected();

      expect(connectionError.value).not.toBeNull();
      expect(connectionError.value!.code).toBe("unexpected_disconnect");
    });
  });

  describe("setError", () => {
    it("creates connection error with code and message", () => {
      setError("connection_failed", "Failed to connect");

      expect(connectionError.value).not.toBeNull();
      expect(connectionError.value!.code).toBe("connection_failed");
      expect(connectionError.value!.message).toBe("Failed to connect");
    });

    it("includes timestamp", () => {
      const before = Date.now();

      setError("parse_error", "Invalid message");

      const after = Date.now();

      expect(connectionError.value!.timestamp).toBeGreaterThanOrEqual(before);
      expect(connectionError.value!.timestamp).toBeLessThanOrEqual(after);
    });

    it("replaces previous error", () => {
      setError("connection_failed", "First error");

      setError("parse_error", "Second error");

      expect(connectionError.value!.code).toBe("parse_error");
      expect(connectionError.value!.message).toBe("Second error");
    });
  });

  describe("clearError", () => {
    it("removes existing error", () => {
      setError("connection_failed", "Test error");

      clearError();

      expect(connectionError.value).toBeNull();
    });

    it("is safe to call when no error exists", () => {
      clearError();

      expect(connectionError.value).toBeNull();
    });
  });

  describe("resetConnectionState", () => {
    it("resets all state to initial values", () => {
      setConnected(helloFrame);
      setError("connection_failed", "Test");

      resetConnectionState();

      expect(connectionStatus.value).toBe("disconnected");
      expect(connectionError.value).toBeNull();
      expect(protocolVersion.value).toBeNull();
      expect(modVersion.value).toBeNull();
    });
  });
});
