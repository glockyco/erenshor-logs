// WebSocket message parsing with Zod validation

import { WebSocketMessageSchema } from "$lib/types/schemas";
import type { WebSocketMessage, ParseError } from "$lib/types/protocol";

/**
 * Parse and validate a WebSocket message from JSON string.
 * Returns either a validated message or a parse error.
 */
export function parseMessage(json: string): WebSocketMessage | ParseError {
  // Parse JSON
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

  // Check for type field before Zod validation for better error messages
  if (typeof parsed !== "object" || parsed === null || !("type" in parsed)) {
    return {
      code: "missing_type",
      message: "Message missing 'type' field",
      raw: json.slice(0, 200),
    };
  }

  const messageType = (parsed as Record<string, unknown>).type;
  const knownTypes = ["handshake", "sessionStart", "sessionEnd", "combatEvents"];

  // Check for unknown message type before full validation
  if (typeof messageType !== "string" || !knownTypes.includes(messageType)) {
    return {
      code: "unknown_type",
      message: `Unknown message type: ${String(messageType)}`,
      raw: json.slice(0, 200),
    };
  }

  // Validate with Zod
  const result = WebSocketMessageSchema.safeParse(parsed);

  if (result.success) {
    return result.data;
  }

  // Structure validation failed
  return {
    code: "invalid_structure",
    message: result.error.issues.map((i) => `${i.path.join(".")}: ${i.message}`).join("; "),
    raw: json.slice(0, 200),
  };
}

/**
 * Type guard to check if result is a parse error.
 */
export function isParseError(result: WebSocketMessage | ParseError): result is ParseError {
  return "code" in result && !("type" in result);
}
