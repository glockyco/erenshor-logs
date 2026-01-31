import { describe, it, expect, beforeEach } from "vitest";
import { VERSION } from "$lib/version";
import {
  updateAvailable,
  dismissedVersion,
  dismissUpdate,
  resetUpdateState,
} from "./update.svelte";
import { setConnected, setDisconnected, resetConnectionState } from "./connection.svelte";

// Note: Tests work with generated VERSION which may be dirty during development.
// Version comparison tests (updateAvailable) will fail-open when VERSION is dirty,
// which is the correct behavior. Persistence tests work regardless of VERSION format.

describe("update state", () => {
  beforeEach(() => {
    resetUpdateState();
    resetConnectionState();
    // localStorage.clear() is handled by global afterEach in setup.ts
  });

  describe("initial state", () => {
    it("starts with no update available", () => {
      expect(updateAvailable.value).toBe(false);
    });

    it("starts with no dismissed version", () => {
      expect(dismissedVersion.value).toBeNull();
    });
  });

  describe("update detection", () => {
    it("shows update when connected with older mod version (clean builds only)", () => {
      // Skip if VERSION is dirty - can't test version comparison
      if (VERSION.includes("-")) {
        expect(updateAvailable.value).toBe(false);
        return;
      }

      // Connect with an older mod version
      const handshake = {
        type: "handshake" as const,
        protocolVersion: "0.1.0",
        modVersion: "2026.1.1.100", // Older than current VERSION
        session: undefined,
      };

      setConnected(handshake);

      expect(updateAvailable.value).toBe(true);
    });

    it("does not show update when disconnected", () => {
      // modVersion is null when disconnected
      expect(updateAvailable.value).toBe(false);
    });

    it("does not show update when versions match (clean builds only)", () => {
      // Skip if VERSION is dirty
      if (VERSION.includes("-")) {
        return;
      }

      // Connect with same version as web app
      const handshake = {
        type: "handshake" as const,
        protocolVersion: "0.1.0",
        modVersion: VERSION,
        session: undefined,
      };

      setConnected(handshake);

      expect(updateAvailable.value).toBe(false);
    });

    it("does not show update when mod version is dirty but same", () => {
      // Dirty suffixes are stripped, then versions are compared normally
      const handshake = {
        type: "handshake" as const,
        protocolVersion: "0.1.0",
        modVersion: VERSION, // Same version, just happens to be dirty
        session: undefined,
      };

      setConnected(handshake);

      expect(updateAvailable.value).toBe(false);
    });

    it("does not show update when mod version is fallback", () => {
      // Fallback versions are not parseable, fail open
      const handshake = {
        type: "handshake" as const,
        protocolVersion: "0.1.0",
        modVersion: "0.0.0-20260124-204219",
        session: undefined,
      };

      setConnected(handshake);

      expect(updateAvailable.value).toBe(false);
    });

    it("does not show update when mod is newer than web (dev scenario, clean builds only)", () => {
      // Skip if VERSION is dirty
      if (VERSION.includes("-")) {
        return;
      }

      // Edge case: mod has a newer version (shouldn't happen in production)
      const handshake = {
        type: "handshake" as const,
        protocolVersion: "0.1.0",
        modVersion: "2099.12.31.999999999",
        session: undefined,
      };

      setConnected(handshake);

      expect(updateAvailable.value).toBe(false);
    });
  });

  describe("dismissal", () => {
    it("sets dismissedVersion to current VERSION", () => {
      dismissUpdate();

      expect(dismissedVersion.value).toBe(VERSION);
    });

    it("hides update after dismissal (clean builds only)", () => {
      // Skip if VERSION is dirty
      if (VERSION.includes("-")) {
        return;
      }

      // Connect with older mod
      const handshake = {
        type: "handshake" as const,
        protocolVersion: "0.1.0",
        modVersion: "2026.1.1.100",
        session: undefined,
      };

      setConnected(handshake);
      expect(updateAvailable.value).toBe(true);

      // Dismiss
      dismissUpdate();

      expect(updateAvailable.value).toBe(false);
    });

    it("persists dismissal across disconnect and reconnect (clean builds only)", () => {
      // Skip if VERSION is dirty
      if (VERSION.includes("-")) {
        return;
      }

      // Connect with older mod
      const handshake = {
        type: "handshake" as const,
        protocolVersion: "0.1.0",
        modVersion: "2026.1.1.100",
        session: undefined,
      };

      setConnected(handshake);
      dismissUpdate();
      expect(updateAvailable.value).toBe(false);

      // Disconnect
      setDisconnected();
      expect(updateAvailable.value).toBe(false);

      // Reconnect with same outdated mod
      setConnected(handshake);

      // Should still be dismissed
      expect(updateAvailable.value).toBe(false);
    });

    it("remains dismissed after mod update matches web version (clean builds only)", () => {
      // Skip if VERSION is dirty
      if (VERSION.includes("-")) {
        return;
      }

      // Connect with old mod
      const oldHandshake = {
        type: "handshake" as const,
        protocolVersion: "0.1.0",
        modVersion: "2026.1.1.100",
        session: undefined,
      };

      setConnected(oldHandshake);
      expect(updateAvailable.value).toBe(true);

      // Dismiss
      dismissUpdate();
      expect(updateAvailable.value).toBe(false);

      // User updates mod to match web version
      setDisconnected();

      const newHandshake = {
        type: "handshake" as const,
        protocolVersion: "0.1.0",
        modVersion: VERSION,
        session: undefined,
      };

      setConnected(newHandshake);

      // Should not show banner (versions match)
      expect(updateAvailable.value).toBe(false);
    });

    it("works even when mod version is unparseable", () => {
      // Edge case: dismissing while connected to unparseable mod (fallback format)
      const handshake = {
        type: "handshake" as const,
        protocolVersion: "0.1.0",
        modVersion: "0.0.0-20260131-123456", // Unparseable even after stripping
        session: undefined,
      };

      setConnected(handshake);
      expect(updateAvailable.value).toBe(false); // Not shown (unparseable)

      // Should not throw
      dismissUpdate();

      expect(dismissedVersion.value).toBe(VERSION);
    });
  });

  describe("resetUpdateState", () => {
    it("clears dismissedVersion", () => {
      dismissUpdate();
      expect(dismissedVersion.value).toBe(VERSION);

      resetUpdateState();

      expect(dismissedVersion.value).toBeNull();
    });

    it("resets updateAvailable state (clean builds only)", () => {
      // Skip if VERSION is dirty
      if (VERSION.includes("-")) {
        return;
      }

      // Connect with older mod
      const handshake = {
        type: "handshake" as const,
        protocolVersion: "0.1.0",
        modVersion: "2026.1.1.100",
        session: undefined,
      };

      setConnected(handshake);
      dismissUpdate();
      expect(updateAvailable.value).toBe(false);

      // Reset
      resetUpdateState();

      // Should show again (no longer dismissed)
      expect(updateAvailable.value).toBe(true);
    });
  });

  describe("localStorage persistence", () => {
    it("dismissUpdate sets state immediately", () => {
      // Verify state updates work (persistence via $effect is tested in integration)
      expect(dismissedVersion.value).toBeNull();

      dismissUpdate();

      expect(dismissedVersion.value).toBe(VERSION);
    });
  });
});
