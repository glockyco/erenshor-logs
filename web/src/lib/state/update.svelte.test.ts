import { beforeEach, describe, expect, it } from "vitest";
import type { LiveEnvelope } from "$lib/types";
import { VERSION } from "$lib/version";
import {
  dismissedVersion,
  dismissUpdate,
  resetUpdateState,
  updateAvailable,
} from "./update.svelte";
import { resetConnectionState, setConnected, setDisconnected } from "./connection.svelte";

function helloWithModVersion(modVersion: string): LiveEnvelope {
  return {
    protocol: "erenshor.logs.live",
    protocolVersion: "3.0.0",
    schemaVersion: "3.0.0",
    frameId: 1,
    kind: "hello",
    sentAtMs: 1_800_000_000_000,
    producer: { name: "ErenshorLogsMod", modVersion },
    payload: {
      capabilities: ["eventBatch", "diagnosticBatch", "stats"],
      health: { status: "healthy", captureAvailable: true },
      patches: [],
      limits: { maxFrameBytes: 262144, maxEventsPerBatch: 256, diagnosticRingSize: 32 },
      diagnosticSummary: { fatal: 0, error: 0, warning: 0, info: 0 },
    },
  };
}

describe("update state", () => {
  beforeEach(() => {
    resetUpdateState();
    resetConnectionState();
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
    it("shows update when connected with older mod version in clean builds", () => {
      if (VERSION.includes("-")) {
        expect(updateAvailable.value).toBe(false);
        return;
      }

      setConnected(helloWithModVersion("2026.1.1.100"));

      expect(updateAvailable.value).toBe(true);
    });

    it("does not show update when disconnected", () => {
      expect(updateAvailable.value).toBe(false);
    });

    it("does not show update when versions match in clean builds", () => {
      if (VERSION.includes("-")) return;

      setConnected(helloWithModVersion(VERSION));

      expect(updateAvailable.value).toBe(false);
    });

    it("does not show update when mod version is dirty but same", () => {
      setConnected(helloWithModVersion(VERSION));

      expect(updateAvailable.value).toBe(false);
    });

    it("does not show update when mod version is fallback", () => {
      setConnected(helloWithModVersion("0.0.0-20260124-204219"));

      expect(updateAvailable.value).toBe(false);
    });

    it("does not show update when mod is newer than web in clean builds", () => {
      if (VERSION.includes("-")) return;

      setConnected(helloWithModVersion("2099.12.31.999999999"));

      expect(updateAvailable.value).toBe(false);
    });
  });

  describe("dismissal", () => {
    it("sets dismissedVersion to current VERSION", () => {
      dismissUpdate();

      expect(dismissedVersion.value).toBe(VERSION);
    });

    it("hides update after dismissal in clean builds", () => {
      if (VERSION.includes("-")) return;

      setConnected(helloWithModVersion("2026.1.1.100"));
      expect(updateAvailable.value).toBe(true);

      dismissUpdate();

      expect(updateAvailable.value).toBe(false);
    });

    it("persists dismissal across disconnect and reconnect in clean builds", () => {
      if (VERSION.includes("-")) return;

      const hello = helloWithModVersion("2026.1.1.100");
      setConnected(hello);
      dismissUpdate();
      expect(updateAvailable.value).toBe(false);

      setDisconnected();
      expect(updateAvailable.value).toBe(false);

      setConnected(hello);

      expect(updateAvailable.value).toBe(false);
    });

    it("remains dismissed after mod update matches web version in clean builds", () => {
      if (VERSION.includes("-")) return;

      setConnected(helloWithModVersion("2026.1.1.100"));
      expect(updateAvailable.value).toBe(true);

      dismissUpdate();
      expect(updateAvailable.value).toBe(false);

      setDisconnected();
      setConnected(helloWithModVersion(VERSION));

      expect(updateAvailable.value).toBe(false);
    });

    it("works even when mod version is unparseable", () => {
      setConnected(helloWithModVersion("0.0.0-20260131-123456"));
      expect(updateAvailable.value).toBe(false);

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

    it("resets updateAvailable state in clean builds", () => {
      if (VERSION.includes("-")) return;

      setConnected(helloWithModVersion("2026.1.1.100"));
      dismissUpdate();
      expect(updateAvailable.value).toBe(false);

      resetUpdateState();

      expect(updateAvailable.value).toBe(true);
    });
  });

  describe("localStorage persistence", () => {
    it("dismissUpdate sets state immediately", () => {
      expect(dismissedVersion.value).toBeNull();

      dismissUpdate();

      expect(dismissedVersion.value).toBe(VERSION);
    });
  });
});
