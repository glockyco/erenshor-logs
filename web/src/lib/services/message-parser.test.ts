import { describe, it, expect } from "vitest";
import { parseMessage, isParseError } from "./message-parser";
import type {
  HandshakeMessage,
  SessionStartMessage,
  SessionEndMessage,
  CombatEventsMessage,
} from "$lib/types";

describe("parseMessage", () => {
  describe("handshake messages", () => {
    it("parses handshake without session field", () => {
      const json = JSON.stringify({
        type: "handshake",
        protocolVersion: "0.1.0",
        modVersion: "0.1.0",
      });

      const result = parseMessage(json);

      expect(isParseError(result)).toBe(false);
      if (isParseError(result)) return;
      expect(result.type).toBe("handshake");

      const handshake = result as HandshakeMessage;
      expect(handshake.protocolVersion).toBe("0.1.0");
      expect(handshake.modVersion).toBe("0.1.0");
      expect(handshake.session).toBeUndefined();
    });

    it("parses handshake with session", () => {
      const json = JSON.stringify({
        type: "handshake",
        protocolVersion: "0.1.0",
        modVersion: "0.1.0",
        session: { id: "abc123", startTime: 1234567890 },
      });

      const result = parseMessage(json);

      expect(isParseError(result)).toBe(false);
      if (isParseError(result)) return;
      const handshake = result as HandshakeMessage;
      expect(handshake.session).toBeDefined();
      expect(handshake.session!.id).toBe("abc123");
      expect(handshake.session!.startTime).toBe(1234567890);
    });

    it("rejects handshake with explicit null session", () => {
      const json = JSON.stringify({
        type: "handshake",
        protocolVersion: "0.1.0",
        modVersion: "0.1.0",
        session: null,
      });

      const result = parseMessage(json);

      expect(isParseError(result)).toBe(true);
      if (isParseError(result)) {
        expect(result.code).toBe("invalid_structure");
      }
    });

    it("stores protocol and mod versions", () => {
      const json = JSON.stringify({
        type: "handshake",
        protocolVersion: "1.2.3",
        modVersion: "4.5.6",
      });

      const result = parseMessage(json);

      const handshake = result as HandshakeMessage;
      expect(handshake.protocolVersion).toBe("1.2.3");
      expect(handshake.modVersion).toBe("4.5.6");
    });
  });

  describe("sessionStart messages", () => {
    it("parses sessionStart message", () => {
      const json = JSON.stringify({
        type: "sessionStart",
        session: { id: "test-session", startTime: 123456 },
      });

      const result = parseMessage(json);

      expect(isParseError(result)).toBe(false);
      if (isParseError(result)) return;
      expect(result.type).toBe("sessionStart");

      const message = result as SessionStartMessage;
      expect(message.session.id).toBe("test-session");
      expect(message.session.startTime).toBe(123456);
    });

    it("requires session field", () => {
      const json = JSON.stringify({
        type: "sessionStart",
      });

      const result = parseMessage(json);

      expect(isParseError(result)).toBe(true);
      if (isParseError(result)) {
        expect(result.code).toBe("invalid_structure");
      }
    });
  });

  describe("sessionEnd messages", () => {
    it("parses sessionEnd message", () => {
      const json = JSON.stringify({
        type: "sessionEnd",
        sessionId: "test-session",
        endTime: 456789,
      });

      const result = parseMessage(json);

      expect(isParseError(result)).toBe(false);
      if (isParseError(result)) return;
      expect(result.type).toBe("sessionEnd");

      const message = result as SessionEndMessage;
      expect(message.sessionId).toBe("test-session");
      expect(message.endTime).toBe(456789);
    });

    it("requires sessionId field", () => {
      const json = JSON.stringify({
        type: "sessionEnd",
        endTime: 456789,
      });

      const result = parseMessage(json);

      expect(isParseError(result)).toBe(true);
      if (isParseError(result)) {
        expect(result.code).toBe("invalid_structure");
      }
    });

    it("requires endTime field", () => {
      const json = JSON.stringify({
        type: "sessionEnd",
        sessionId: "test-session",
      });

      const result = parseMessage(json);

      expect(isParseError(result)).toBe(true);
      if (isParseError(result)) {
        expect(result.code).toBe("invalid_structure");
      }
    });
  });

  describe("combatEvents messages", () => {
    it("parses combatEvents message with empty array", () => {
      const json = JSON.stringify({
        type: "combatEvents",
        sessionId: "test-session",
        events: [],
      });

      const result = parseMessage(json);

      expect(isParseError(result)).toBe(false);
      if (isParseError(result)) return;
      expect(result.type).toBe("combatEvents");

      const message = result as CombatEventsMessage;
      expect(message.sessionId).toBe("test-session");
      expect(message.events).toEqual([]);
    });

    it("parses combatEvents message with events", () => {
      const json = JSON.stringify({
        type: "combatEvents",
        sessionId: "test-session",
        events: [
          {
            id: "event-1",
            timestamp: 1000,
            eventType: "damagePhysical",
          },
          {
            id: "event-2",
            timestamp: 2000,
            eventType: "healSpell",
          },
        ],
      });

      const result = parseMessage(json);

      expect(isParseError(result)).toBe(false);
      if (isParseError(result)) return;
      const message = result as CombatEventsMessage;
      expect(message.events).toHaveLength(2);
      expect(message.events[0].id).toBe("event-1");
      expect(message.events[1].id).toBe("event-2");
    });

    it("requires sessionId field", () => {
      const json = JSON.stringify({
        type: "combatEvents",
        events: [],
      });

      const result = parseMessage(json);

      expect(isParseError(result)).toBe(true);
      if (isParseError(result)) {
        expect(result.code).toBe("invalid_structure");
      }
    });

    it("requires events field", () => {
      const json = JSON.stringify({
        type: "combatEvents",
        sessionId: "test-session",
      });

      const result = parseMessage(json);

      expect(isParseError(result)).toBe(true);
      if (isParseError(result)) {
        expect(result.code).toBe("invalid_structure");
      }
    });
  });

  describe("error handling", () => {
    it("returns parse error for invalid JSON", () => {
      const result = parseMessage("{invalid}");

      expect(isParseError(result)).toBe(true);
      if (isParseError(result)) {
        expect(result.code).toBe("invalid_json");
        expect(result.message).toContain("JSON");
      }
    });

    it("returns parse error for missing type", () => {
      const json = JSON.stringify({ foo: "bar" });
      const result = parseMessage(json);

      expect(isParseError(result)).toBe(true);
      if (isParseError(result)) {
        expect(result.code).toBe("missing_type");
        expect(result.message).toContain("type");
      }
    });

    it("returns parse error for unknown type", () => {
      const json = JSON.stringify({ type: "unknownType" });
      const result = parseMessage(json);

      expect(isParseError(result)).toBe(true);
      if (isParseError(result)) {
        expect(result.code).toBe("unknown_type");
        expect(result.message).toContain("unknownType");
      }
    });

    it("returns parse error for malformed structure", () => {
      const json = JSON.stringify({
        type: "handshake",
        // Missing required fields
      });
      const result = parseMessage(json);

      expect(isParseError(result)).toBe(true);
      if (isParseError(result)) {
        expect(result.code).toBe("invalid_structure");
      }
    });

    it("includes truncated raw message in error", () => {
      const longJson = '{"invalid": "' + "x".repeat(300) + '"}';
      const result = parseMessage(longJson);

      expect(isParseError(result)).toBe(true);
      if (isParseError(result)) {
        expect(result.raw).toBeDefined();
        expect(result.raw!.length).toBeLessThanOrEqual(200);
      }
    });
  });

  describe("type discrimination", () => {
    it("correctly types handshake via discriminated union", () => {
      const json = JSON.stringify({
        type: "handshake",
        protocolVersion: "0.1.0",
        modVersion: "0.1.0",
      });

      const result = parseMessage(json);

      if (!isParseError(result)) {
        // TypeScript should narrow based on result.type
        if (result.type === "handshake") {
          expect(result.protocolVersion).toBeDefined();
          expect(result.modVersion).toBeDefined();
        }
      }
    });
  });
});
