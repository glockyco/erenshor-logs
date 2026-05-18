import { LiveEnvelopeSchema } from "$lib/types/schemas";
import type { LiveEnvelope, ParseError } from "$lib/types/protocol";

const MAX_LIVE_MESSAGE_BYTES = 262_144;

const KNOWN_KINDS = new Set([
  "hello",
  "sessionOpened",
  "registryDelta",
  "eventBatch",
  "diagnosticBatch",
  "stats",
  "heartbeat",
  "sessionClosed",
]);

export function parseMessage(json: string): LiveEnvelope | ParseError {
  if (json.length > MAX_LIVE_MESSAGE_BYTES) {
    return {
      code: "message_too_large",
      message: `Live message exceeded ${MAX_LIVE_MESSAGE_BYTES} bytes`,
      rawHash: hashRaw(json),
    };
  }

  let parsed: unknown;
  try {
    parsed = JSON.parse(json);
  } catch (error) {
    return {
      code: "invalid_json",
      message: error instanceof Error ? error.message : "Invalid JSON",
      raw: json.slice(0, 200),
      rawHash: hashRaw(json),
    };
  }

  if (typeof parsed !== "object" || parsed === null) {
    return {
      code: "missing_protocol",
      message: "Message missing 'protocol' field",
      raw: json.slice(0, 200),
      rawHash: hashRaw(json),
    };
  }

  const record = parsed as Record<string, unknown>;
  if (!("protocol" in record)) {
    if ("type" in record) {
      const modVersion =
        typeof record.modVersion === "string" ? ` Mod version: ${record.modVersion}.` : "";
      const protocolVersion =
        typeof record.protocolVersion === "string"
          ? ` Protocol version: ${record.protocolVersion}.`
          : "";
      return {
        code: "legacy_mod",
        message:
          "An old Erenshor Logs mod connected. Update the mod to a protocol v3 build." +
          protocolVersion +
          modVersion,
        raw: json.slice(0, 200),
        rawHash: hashRaw(json),
      };
    }

    return {
      code: "missing_protocol",
      message: "Message missing 'protocol' field",
      raw: json.slice(0, 200),
      rawHash: hashRaw(json),
      header: extractHeader(record),
    };
  }

  const header = extractHeader(record);

  if (record.protocol !== "erenshor.logs.live") {
    return {
      code: "unknown_protocol",
      message: `Unknown protocol: ${String(record.protocol)}`,
      raw: json.slice(0, 200),
      rawHash: hashRaw(json),
      header,
    };
  }

  if (typeof record.protocolVersion !== "string" || !record.protocolVersion.startsWith("3.")) {
    return {
      code: "unsupported_version",
      message: `Unsupported protocol version: ${String(record.protocolVersion)}`,
      raw: json.slice(0, 200),
      rawHash: hashRaw(json),
      header,
    };
  }

  if (typeof record.kind !== "string" || !KNOWN_KINDS.has(record.kind)) {
    return {
      code: "unknown_kind",
      message: `Unknown message kind: ${String(record.kind)}`,
      raw: json.slice(0, 200),
      rawHash: hashRaw(json),
      header,
    };
  }

  const result = LiveEnvelopeSchema.safeParse(parsed);
  if (result.success) return result.data;

  return {
    code: "invalid_structure",
    message: result.error.issues.map((i) => `${i.path.join(".")}: ${i.message}`).join("; "),
    raw: json.slice(0, 200),
    rawHash: hashRaw(json),
    header,
  };
}

export function isParseError(result: LiveEnvelope | ParseError): result is ParseError {
  return "code" in result && !("protocol" in result);
}

function extractHeader(record: Record<string, unknown>): ParseError["header"] {
  const header: NonNullable<ParseError["header"]> = {};

  if (typeof record.protocol === "string") header.protocol = record.protocol;
  if (typeof record.protocolVersion === "string") header.protocolVersion = record.protocolVersion;
  if (typeof record.schemaVersion === "string") header.schemaVersion = record.schemaVersion;
  if (typeof record.kind === "string") header.kind = record.kind;
  if (
    typeof record.frameId === "number" &&
    Number.isInteger(record.frameId) &&
    record.frameId > 0
  ) {
    header.frameId = record.frameId;
  }
  if (typeof record.sessionId === "string") header.sessionId = record.sessionId;

  return Object.keys(header).length === 0 ? undefined : header;
}

function hashRaw(value: string): string {
  let hash = 5381;
  for (let index = 0; index < value.length; index += 1) {
    hash = ((hash << 5) + hash) ^ value.charCodeAt(index);
  }

  return (hash >>> 0).toString(16).padStart(8, "0");
}
