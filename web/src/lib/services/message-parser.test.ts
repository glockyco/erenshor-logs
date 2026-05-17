import { describe, expect, it } from "vitest";
import hello from "../../../../shared/protocol/fixtures/live/hello.json";
import { isParseError, parseMessage } from "./message-parser";

const asJson = (value: unknown) => JSON.stringify(value);

describe("parseMessage", () => {
  it("parses protocol v2 hello envelopes", () => {
    const result = parseMessage(asJson(hello));

    expect(isParseError(result)).toBe(false);
    if (isParseError(result)) return;
    expect(result.protocol).toBe("erenshor.logs.live");
    expect(result.kind).toBe("hello");
  });

  it("reports legacy type-based frames as old mod connections", () => {
    const result = parseMessage(
      asJson({ type: "handshake", protocolVersion: "1.0.0", modVersion: "1.0.0" })
    );

    expect(isParseError(result)).toBe(true);
    if (isParseError(result)) {
      expect(result.code).toBe("legacy_mod");
      expect(result.message).toContain("old Erenshor Logs mod");
      expect(result.message).toContain("1.0.0");
    }
  });

  it("rejects unsupported major protocol versions", () => {
    const result = parseMessage(asJson({ ...hello, protocolVersion: "3.0.0" }));

    expect(isParseError(result)).toBe(true);
    if (isParseError(result)) {
      expect(result.code).toBe("unsupported_version");
    }
  });

  it("rejects unknown protocols", () => {
    const result = parseMessage(asJson({ ...hello, protocol: "other.protocol" }));

    expect(isParseError(result)).toBe(true);
    if (isParseError(result)) {
      expect(result.code).toBe("unknown_protocol");
    }
  });

  it("rejects unknown frame kinds", () => {
    const result = parseMessage(asJson({ ...hello, kind: "legacy" }));

    expect(isParseError(result)).toBe(true);
    if (isParseError(result)) {
      expect(result.code).toBe("unknown_kind");
    }
  });

  it("returns parse error for invalid JSON", () => {
    const result = parseMessage("{invalid}");

    expect(isParseError(result)).toBe(true);
    if (isParseError(result)) {
      expect(result.code).toBe("invalid_json");
      expect(result.message).toContain("JSON");
    }
  });

  it("returns invalid structure for malformed v2 envelopes", () => {
    const result = parseMessage(asJson({ ...hello, payload: {} }));

    expect(isParseError(result)).toBe(true);
    if (isParseError(result)) {
      expect(result.code).toBe("invalid_structure");
    }
  });
});
