import { describe, it, expect, beforeEach } from "vitest";
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

    it("clears any existing error", () => {
      setError("connection_failed", "Test error");

      setConnecting();

      expect(connectionError.value).toBeNull();
    });
  });

  describe("setConnected", () => {
    it("sets status to connected", () => {
      const handshake = {
        type: "handshake" as const,
        protocolVersion: "1.0.0",
        modVersion: "0.5.0",
        session: undefined,
      };

      setConnected(handshake);

      expect(connectionStatus.value).toBe("connected");
    });

    it("stores protocol and mod versions", () => {
      const handshake = {
        type: "handshake" as const,
        protocolVersion: "1.2.3",
        modVersion: "0.8.1",
        session: undefined,
      };

      setConnected(handshake);

      expect(protocolVersion.value).toBe("1.2.3");
      expect(modVersion.value).toBe("0.8.1");
    });

    it("clears any existing error", () => {
      setError("connection_failed", "Test error");

      const handshake = {
        type: "handshake" as const,
        protocolVersion: "1.0.0",
        modVersion: "0.5.0",
        session: undefined,
      };

      setConnected(handshake);

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
      const handshake = {
        type: "handshake" as const,
        protocolVersion: "1.0.0",
        modVersion: "0.5.0",
        session: undefined,
      };
      setConnected(handshake);

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
      const handshake = {
        type: "handshake" as const,
        protocolVersion: "1.0.0",
        modVersion: "0.5.0",
        session: undefined,
      };
      setConnected(handshake);
      setError("connection_failed", "Test");

      resetConnectionState();

      expect(connectionStatus.value).toBe("disconnected");
      expect(connectionError.value).toBeNull();
      expect(protocolVersion.value).toBeNull();
      expect(modVersion.value).toBeNull();
    });
  });
});
