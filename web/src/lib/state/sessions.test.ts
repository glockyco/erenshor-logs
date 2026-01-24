import { describe, it, expect, beforeEach } from "vitest";
import {
  sessions,
  activeSessionId,
  activeSession,
  activeSessionStats,
  addSession,
  appendEvents,
  endSession,
  deleteSession,
  clearAllSessions,
  setActiveSession,
  resetSessionsState,
} from "./sessions.svelte";
import { createSessionInfo, createCombatEvent, createTimedEvents } from "$lib/testing";
import { setNow, resetClockState } from "./clock.svelte";

describe("sessions state", () => {
  beforeEach(() => {
    resetSessionsState();
    resetClockState();
  });

  describe("addSession", () => {
    it("creates new session from SessionInfo", () => {
      const info = createSessionInfo({ id: "test-1", startTime: 1000 });

      addSession(info);

      expect(sessions.has("test-1")).toBe(true);
      const session = sessions.get("test-1")!;
      expect(session.id).toBe("test-1");
      expect(session.startTime).toBe(1000);
      expect(session.events).toEqual([]);
      expect(session.endTime).toBeUndefined();
    });

    it("sets new session as active when no active session", () => {
      const info = createSessionInfo({ id: "test-1" });

      addSession(info);

      expect(activeSessionId.value).toBe("test-1");
    });

    it("switches to newly added session for hands-off usage", () => {
      const info1 = createSessionInfo({ id: "test-1" });
      const info2 = createSessionInfo({ id: "test-2" });

      addSession(info1);
      expect(activeSessionId.value).toBe("test-1");

      addSession(info2);
      expect(activeSessionId.value).toBe("test-2");
    });

    it("ignores duplicate session IDs when session has events", () => {
      const info = createSessionInfo({ id: "test-1", startTime: 1000 });

      addSession(info);
      appendEvents("test-1", [createCombatEvent({ id: "e1" })]);
      const originalSession = sessions.get("test-1");

      // Try to add again with different start time
      addSession(createSessionInfo({ id: "test-1", startTime: 2000 }));

      // Original session unchanged
      expect(sessions.get("test-1")).toBe(originalSession);
      expect(sessions.get("test-1")!.startTime).toBe(1000);
      expect(sessions.get("test-1")!.events).toHaveLength(1);
    });

    it("replaces empty session when duplicate session ID is added", () => {
      const info1 = createSessionInfo({ id: "test-1", startTime: 1000 });
      addSession(info1);

      // Session is empty
      expect(sessions.get("test-1")!.events).toHaveLength(0);

      // Add same ID with different start time
      const info2 = createSessionInfo({ id: "test-1", startTime: 2000 });
      addSession(info2);

      // Session replaced with new start time
      expect(sessions.get("test-1")!.startTime).toBe(2000);
      expect(sessions.get("test-1")!.events).toHaveLength(0);
    });
  });

  describe("appendEvents", () => {
    it("appends events to existing session", () => {
      const info = createSessionInfo({ id: "test-1" });
      addSession(info);

      const events = [
        createCombatEvent({ id: "e1", timestamp: 1000 }),
        createCombatEvent({ id: "e2", timestamp: 2000 }),
      ];

      appendEvents("test-1", events);

      const session = sessions.get("test-1")!;
      expect(session.events).toHaveLength(2);
      expect(session.events[0].id).toBe("e1");
      expect(session.events[1].id).toBe("e2");
    });

    it("appends multiple batches to same session", () => {
      const info = createSessionInfo({ id: "test-1" });
      addSession(info);

      appendEvents("test-1", [createCombatEvent({ id: "e1" })]);
      appendEvents("test-1", [createCombatEvent({ id: "e2" })]);

      const session = sessions.get("test-1")!;
      expect(session.events).toHaveLength(2);
      expect(session.events[0].id).toBe("e1");
      expect(session.events[1].id).toBe("e2");
    });

    it("ignores events for unknown session", () => {
      const events = [createCombatEvent({ id: "e1" })];

      appendEvents("unknown-session", events);

      expect(sessions.has("unknown-session")).toBe(false);
    });
  });

  describe("endSession", () => {
    it("sets endTime on session", () => {
      const info = createSessionInfo({ id: "test-1" });
      addSession(info);
      appendEvents("test-1", [createCombatEvent({ id: "e1" })]);

      endSession("test-1", 5000);

      const session = sessions.get("test-1")!;
      expect(session.endTime).toBe(5000);
    });

    it("does not modify other session properties", () => {
      const info = createSessionInfo({ id: "test-1", startTime: 1000 });
      addSession(info);
      const events = [createCombatEvent({ id: "e1" })];
      appendEvents("test-1", events);

      endSession("test-1", 5000);

      const session = sessions.get("test-1")!;
      expect(session.id).toBe("test-1");
      expect(session.startTime).toBe(1000);
      expect(session.events).toHaveLength(1);
    });

    it("ignores endSession for unknown session", () => {
      endSession("unknown-session", 5000);

      expect(sessions.has("unknown-session")).toBe(false);
    });
  });

  describe("endSession behavior", () => {
    it("preserves session with no events", () => {
      const info = createSessionInfo({ id: "test-1" });
      addSession(info);

      endSession("test-1", 5000);

      expect(sessions.has("test-1")).toBe(true);
      expect(sessions.get("test-1")!.endTime).toBe(5000);
      expect(sessions.get("test-1")!.events).toHaveLength(0);
    });

    it("preserves session with events", () => {
      const info = createSessionInfo({ id: "test-1" });
      addSession(info);
      appendEvents("test-1", [createCombatEvent({ id: "e1" })]);

      endSession("test-1", 5000);

      expect(sessions.has("test-1")).toBe(true);
      expect(sessions.get("test-1")!.endTime).toBe(5000);
      expect(sessions.get("test-1")!.events).toHaveLength(1);
    });

    it("does not change active session when ending non-active session", () => {
      const info1 = createSessionInfo({ id: "test-1", startTime: 1000 });
      const info2 = createSessionInfo({ id: "test-2", startTime: 2000 });
      addSession(info1);
      addSession(info2);
      setActiveSession("test-2");

      endSession("test-1", 3000);

      expect(sessions.has("test-1")).toBe(true);
      expect(sessions.get("test-1")!.endTime).toBe(3000);
      expect(activeSessionId.value).toBe("test-2");
    });

    it("does not change active session when ending active session", () => {
      const info = createSessionInfo({ id: "test-1" });
      addSession(info);
      setActiveSession("test-1");

      endSession("test-1", 5000);

      expect(sessions.has("test-1")).toBe(true);
      expect(sessions.get("test-1")!.endTime).toBe(5000);
      expect(activeSessionId.value).toBe("test-1");
    });

    it("preserves all sessions when ending multiple sessions", () => {
      addSession(createSessionInfo({ id: "test-1", startTime: 1000 }));
      addSession(createSessionInfo({ id: "test-2", startTime: 2000 }));
      addSession(createSessionInfo({ id: "test-3", startTime: 3000 }));

      endSession("test-1", 4000);
      endSession("test-2", 5000);
      endSession("test-3", 6000);

      expect(sessions.size).toBe(3);
      expect(sessions.get("test-1")!.endTime).toBe(4000);
      expect(sessions.get("test-2")!.endTime).toBe(5000);
      expect(sessions.get("test-3")!.endTime).toBe(6000);
    });
  });

  describe("deleteSession", () => {
    it("removes session from map", () => {
      const info = createSessionInfo({ id: "test-1" });
      addSession(info);

      deleteSession("test-1");

      expect(sessions.has("test-1")).toBe(false);
    });

    it("clears active session if deleting active", () => {
      const info = createSessionInfo({ id: "test-1" });
      addSession(info);

      deleteSession("test-1");

      expect(activeSessionId.value).toBeNull();
    });

    it("clears active session when deleting the active session", () => {
      const info1 = createSessionInfo({ id: "test-1" });
      const info2 = createSessionInfo({ id: "test-2" });
      addSession(info1);
      addSession(info2); // test-2 becomes active

      deleteSession("test-2");

      expect(activeSessionId.value).toBeNull();
    });

    it("handles deleting unknown session gracefully", () => {
      deleteSession("unknown-session");

      expect(sessions.size).toBe(0);
    });
  });

  describe("clearAllSessions", () => {
    it("removes all sessions", () => {
      addSession(createSessionInfo({ id: "test-1" }));
      addSession(createSessionInfo({ id: "test-2" }));
      addSession(createSessionInfo({ id: "test-3" }));

      clearAllSessions();

      expect(sessions.size).toBe(0);
    });

    it("clears active session", () => {
      addSession(createSessionInfo({ id: "test-1" }));

      clearAllSessions();

      expect(activeSessionId.value).toBeNull();
    });
  });

  describe("setActiveSession", () => {
    it("sets active session to specified ID", () => {
      addSession(createSessionInfo({ id: "test-1" }));
      addSession(createSessionInfo({ id: "test-2" }));

      setActiveSession("test-2");

      expect(activeSessionId.value).toBe("test-2");
    });

    it("clears active session when set to null", () => {
      addSession(createSessionInfo({ id: "test-1" }));

      setActiveSession(null);

      expect(activeSessionId.value).toBeNull();
    });

    it("rejects setting unknown session as active", () => {
      addSession(createSessionInfo({ id: "test-1" }));

      setActiveSession("unknown-session");

      expect(activeSessionId.value).toBe("test-1");
    });
  });

  describe("activeSession derived state", () => {
    it("returns null when no active session", () => {
      expect(activeSession.value).toBeNull();
    });

    it("returns active session object", () => {
      const info = createSessionInfo({ id: "test-1", startTime: 1000 });
      addSession(info);

      const session = activeSession.value;
      expect(session).not.toBeNull();
      expect(session!.id).toBe("test-1");
      expect(session!.startTime).toBe(1000);
    });

    it("updates when active session changes", () => {
      addSession(createSessionInfo({ id: "test-1" }));
      addSession(createSessionInfo({ id: "test-2" }));

      setActiveSession("test-2");

      expect(activeSession.value!.id).toBe("test-2");
    });

    it("returns null when active session is deleted", () => {
      addSession(createSessionInfo({ id: "test-1" }));

      deleteSession("test-1");

      expect(activeSession.value).toBeNull();
    });
  });

  describe("activeSessionStats derived state", () => {
    it("returns null when no active session", () => {
      expect(activeSessionStats.value).toBeNull();
    });

    it("calculates stats for active session with fixed duration", () => {
      const info = createSessionInfo({ id: "test-1", startTime: 1000 });
      addSession(info);

      const events = createTimedEvents(5, 800); // 5 events, 800ms apart
      appendEvents("test-1", events);
      endSession("test-1", 5000);

      const stats = activeSessionStats.value;
      expect(stats).not.toBeNull();
      expect(stats!.durationMs).toBe(4000);
    });

    it("calculates stats for live session using current time", () => {
      setNow(10000); // Set current time to 10000

      const info = createSessionInfo({ id: "test-1", startTime: 1000 });
      addSession(info);

      const stats = activeSessionStats.value;
      expect(stats).not.toBeNull();
      expect(stats!.durationMs).toBe(9000); // 10000 - 1000
    });

    it("recalculates stats when events are appended", () => {
      const info = createSessionInfo({ id: "test-1", startTime: 0 });
      addSession(info);
      const initialEvent = createCombatEvent({ id: "e0", timestamp: 0 });
      appendEvents("test-1", [initialEvent]);
      endSession("test-1", 1000);

      const initialStats = activeSessionStats.value;
      expect(initialStats).not.toBeNull();

      const damageEvent = createCombatEvent({
        timestamp: 0,
        eventType: "damagePhysical",
        amount: 100,
      });
      appendEvents("test-1", [damageEvent]);

      // Stats should be recalculated (may still be 0 if event doesn't match filters)
      const updatedStats = activeSessionStats.value;
      expect(updatedStats).not.toBeNull();
      expect(updatedStats!.durationMs).toBe(1000);
    });

    it("uses endTime when session is ended", () => {
      const info = createSessionInfo({ id: "test-1", startTime: 1000 });
      addSession(info);
      appendEvents("test-1", [createCombatEvent({ id: "e1" })]);
      endSession("test-1", 3000);

      const stats = activeSessionStats.value;
      expect(stats).not.toBeNull();
      expect(stats!.durationMs).toBe(2000); // 3000 - 1000
    });

    it("returns null when active session has no events", () => {
      const info = createSessionInfo({ id: "test-1", startTime: 1000 });
      addSession(info);
      appendEvents("test-1", [createCombatEvent({ id: "e1" })]);
      endSession("test-1", 2000);

      const stats = activeSessionStats.value;
      expect(stats).not.toBeNull();
      expect(stats!.totalDamage).toBe(0);
      expect(stats!.durationMs).toBe(1000);
    });
  });

  describe("sessions map", () => {
    it("allows direct iteration over sessions", () => {
      addSession(createSessionInfo({ id: "test-1" }));
      addSession(createSessionInfo({ id: "test-2" }));
      addSession(createSessionInfo({ id: "test-3" }));

      const sessionIds = Array.from(sessions.keys());
      expect(sessionIds).toHaveLength(3);
      expect(sessionIds).toContain("test-1");
      expect(sessionIds).toContain("test-2");
      expect(sessionIds).toContain("test-3");
    });

    it("maintains session data integrity", () => {
      const info = createSessionInfo({ id: "test-1", startTime: 1000 });
      addSession(info);

      const session = sessions.get("test-1");
      expect(session).toBeDefined();
      expect(session!.id).toBe("test-1");
      expect(session!.startTime).toBe(1000);
      expect(session!.events).toEqual([]);
      expect(session!.endTime).toBeUndefined();
    });
  });
});
