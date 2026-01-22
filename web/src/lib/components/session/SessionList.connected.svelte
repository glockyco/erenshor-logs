<script lang="ts">
  import {
    sessions,
    activeSessionId,
    setActiveSession,
    deleteSession,
    clearAllSessions,
  } from "$lib/state";
  import SessionList from "./SessionList.svelte";

  const sortedSessions = $derived(
    Array.from(sessions.values()).sort((a, b) => b.startTime - a.startTime)
  );

  function handleDelete(sessionId: string) {
    if (window.confirm("Delete this session? This cannot be undone.")) {
      deleteSession(sessionId);
    }
  }

  function handleClearAll() {
    if (window.confirm("Delete all sessions? This cannot be undone.")) {
      clearAllSessions();
    }
  }
</script>

<SessionList
  sessions={sortedSessions}
  activeSessionId={activeSessionId.value}
  onSessionSelect={setActiveSession}
  onSessionDelete={handleDelete}
  onClearAll={handleClearAll}
/>
