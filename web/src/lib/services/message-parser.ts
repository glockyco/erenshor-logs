import { LiveEnvelopeSchema } from "$lib/types/schemas";
import type { LiveEnvelope, ParseError } from "$lib/types/protocol";

const KNOWN_KINDS = new Set([
  "hello",
  "sessionSnapshot",
  "registryDelta",
  "events",
  "sessionEnded",
  "error",
  "heartbeat",
  "serverStats",
]);

export function parseMessage(json: string): LiveEnvelope | ParseError {
  let parsed: unknown;
  try {
    parsed = JSON.parse(json);
  } catch (error) {
    return {
      code: "invalid_json",
      message: error instanceof Error ? error.message : "Invalid JSON",
      raw: json.slice(0, 200),
    };
  }

  if (typeof parsed !== "object" || parsed === null || !("protocol" in parsed)) {
    return {
      code: "missing_protocol",
      message: "Message missing 'protocol' field",
      raw: json.slice(0, 200),
    };
  }

  const record = parsed as Record<string, unknown>;
  if (record.protocol !== "erenshor.logs.live") {
    return {
      code: "unknown_protocol",
      message: `Unknown protocol: ${String(record.protocol)}`,
      raw: json.slice(0, 200),
    };
  }

  if (typeof record.protocolVersion !== "string" || !record.protocolVersion.startsWith("2.")) {
    return {
      code: "unsupported_version",
      message: `Unsupported protocol version: ${String(record.protocolVersion)}`,
      raw: json.slice(0, 200),
    };
  }

  if (typeof record.kind !== "string" || !KNOWN_KINDS.has(record.kind)) {
    return {
      code: "unknown_kind",
      message: `Unknown message kind: ${String(record.kind)}`,
      raw: json.slice(0, 200),
    };
  }

  const result = LiveEnvelopeSchema.safeParse(parsed);
  if (result.success) return result.data;

  return {
    code: "invalid_structure",
    message: result.error.issues.map((i) => `${i.path.join(".")}: ${i.message}`).join("; "),
    raw: json.slice(0, 200),
  };
}

export function isParseError(result: LiveEnvelope | ParseError): result is ParseError {
  return "code" in result && !("protocol" in result);
}
