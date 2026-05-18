import type { ConnectionError } from "$lib/types";

export interface ConnectionAlert {
  title: string;
  message: string;
  tone: "error";
}

export function getConnectionAlert(error: ConnectionError | null): ConnectionAlert | null {
  if (!error) {
    return null;
  }

  switch (error.code) {
    case "legacy_mod":
      return {
        title: "Old mod connected",
        message: error.message,
        tone: "error",
      };
    case "parse_error":
      return {
        title: "Invalid live data",
        message: error.message,
        tone: "error",
      };
    case "preview_mismatch":
      return {
        title: "Preview mod is out of date",
        message: error.message,
        tone: "error",
      };
    case "stream_degraded":
      return {
        title: "Some live data was skipped",
        message: error.message,
        tone: "error",
      };
    case "capture_unavailable":
      return {
        title: "Combat capture is unavailable",
        message: error.message,
        tone: "error",
      };
    case "connection_failed":
      return {
        title: "Connection failed",
        message: error.message,
        tone: "error",
      };
    case "unexpected_disconnect":
      return {
        title: "Connection lost",
        message: error.message,
        tone: "error",
      };
  }
}
