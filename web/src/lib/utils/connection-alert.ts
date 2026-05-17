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
