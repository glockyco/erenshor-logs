<script lang="ts">
  import { Download, Upload, Trash2 } from "@lucide/svelte";
  import SessionCardConnected from "./SessionCard.connected.svelte";
  import FileDropzone from "./FileDropzone.svelte";
  import {
    sessions,
    clearAllSessions,
    addSession,
    setActiveSession,
  } from "$lib/state/sessions.svelte";
  import { exportSessions } from "$lib/services";

  interface Props {
    onSessionSelect?: () => void;
  }

  let { onSessionSelect }: Props = $props();

  const sessionsList = $derived(Array.from(sessions.values()));
  const sortedSessions = $derived([...sessionsList].sort((a, b) => b.startTime - a.startTime));

  let showImport = $state(false);
  let importError = $state<string | null>(null);
  let importSuccess = $state<string | null>(null);

  const handleExportAll = () => {
    if (sessionsList.length === 0) return;
    exportSessions(sessionsList);
  };

  const handleImport = (importedSessions: import("$lib/types").Session[]) => {
    // Filter out empty sessions before processing
    const validSessions = importedSessions.filter((s) => s.events.length > 0);
    const emptyCount = importedSessions.length - validSessions.length;

    if (emptyCount > 0) {
      console.log(
        `Filtered out ${emptyCount} empty session${emptyCount === 1 ? "" : "s"} during import`
      );
    }

    let addedCount = 0;
    let skippedCount = 0;

    for (const session of validSessions) {
      if (sessions.has(session.id)) {
        skippedCount++;
        continue;
      }
      addSession({ id: session.id, startTime: session.startTime });
      // Update with full session data (addSession only creates minimal, we need to update)
      sessions.set(session.id, session);
      addedCount++;
    }

    // Build success message with all relevant counts
    if (addedCount > 0) {
      let message = `Imported ${addedCount} session${addedCount === 1 ? "" : "s"}`;
      const notes = [];
      if (skippedCount > 0) {
        notes.push(`${skippedCount} skipped - already exists`);
      }
      if (emptyCount > 0) {
        notes.push(`${emptyCount} empty session${emptyCount === 1 ? "" : "s"} filtered`);
      }
      if (notes.length > 0) {
        message += ` (${notes.join(", ")})`;
      }

      // Set the first imported session as active
      setActiveSession(validSessions[0].id);
      importSuccess = message;
      importError = null;
      showImport = false;

      // Clear success message after 3 seconds
      setTimeout(() => {
        importSuccess = null;
      }, 3000);
    } else {
      // No sessions were added - determine why
      if (emptyCount === importedSessions.length) {
        importError = "All sessions are empty (0 events)";
      } else {
        importError = "All sessions already exist";
      }
    }
  };

  const handleImportError = (error: string) => {
    importError = error;
    importSuccess = null;
  };

  const toggleImport = () => {
    showImport = !showImport;
    if (!showImport) {
      importError = null;
      importSuccess = null;
    }
  };

  const handleSessionClick = () => {
    // Auto-close drawer when session is selected on mobile
    onSessionSelect?.();
  };
</script>

<div class="space-y-3">
  <div class="mb-4 flex flex-wrap items-center justify-between gap-2">
    <h2 class="text-sm font-semibold uppercase tracking-wider text-amber-600">
      Sessions ({sessionsList.length})
    </h2>
    <div class="flex flex-wrap gap-2">
      {#if sessionsList.length > 0}
        <button
          type="button"
          onclick={handleExportAll}
          class="flex items-center gap-1 text-xs text-stone-400 hover:text-cyan-400 transition cursor-pointer"
          aria-label="Export all sessions"
        >
          <Download class="h-3 w-3" />
          Export All
        </button>
        <button
          type="button"
          onclick={clearAllSessions}
          class="flex items-center gap-1 text-xs text-stone-400 hover:text-rose-400 transition cursor-pointer"
          aria-label="Clear all sessions"
        >
          <Trash2 class="h-3 w-3" />
          Clear All
        </button>
      {/if}
      <button
        type="button"
        onclick={toggleImport}
        class={`flex items-center gap-1 text-xs transition cursor-pointer ${
          showImport ? "text-cyan-400" : "text-stone-400 hover:text-cyan-400"
        }`}
        aria-label="Import sessions"
      >
        <Upload class="h-3 w-3" />
        Import
      </button>
    </div>
  </div>

  <!-- Import Messages -->
  {#if importSuccess}
    <div
      class="rounded-lg bg-green-500/10 border border-green-500/30 px-3 py-2 text-xs text-green-400"
    >
      {importSuccess}
    </div>
  {/if}
  {#if importError}
    <div
      class="rounded-lg bg-rose-500/10 border border-rose-500/30 px-3 py-2 text-xs text-rose-400"
    >
      {importError}
    </div>
  {/if}

  <!-- Import Dropzone -->
  {#if showImport}
    <FileDropzone onimport={handleImport} onerror={handleImportError} />
  {/if}

  {#if sessionsList.length === 0}
    <div class="py-12 text-center">
      <p class="text-sm text-stone-500">No sessions recorded</p>
      <p class="mt-1 text-xs text-stone-600">Combat data will appear here</p>
    </div>
  {:else}
    {#each sortedSessions as session (session.id)}
      <SessionCardConnected {session} onSelect={handleSessionClick} />
    {/each}
  {/if}
</div>
