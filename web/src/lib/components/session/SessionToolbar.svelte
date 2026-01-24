<script lang="ts">
  import { Download, Upload, Trash2 } from "@lucide/svelte";
  import FileDropzone from "./FileDropzone.svelte";
  import {
    sessions,
    clearAllSessions,
    addSession,
    setActiveSession,
  } from "$lib/state/sessions.svelte";
  import { exportSessions } from "$lib/services";

  const sessionsList = $derived(Array.from(sessions.values()));

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
</script>

<div class="space-y-3">
  <!-- Action Buttons -->
  <div class="flex gap-2">
    {#if sessionsList.length > 0}
      <button
        type="button"
        onclick={handleExportAll}
        class="flex-1 flex items-center justify-center gap-1.5 px-3 py-2 text-xs text-stone-400 hover:text-cyan-400 bg-stone-800/50 hover:bg-stone-700/50 rounded-md transition cursor-pointer"
        aria-label="Export all sessions"
      >
        <Download class="h-3.5 w-3.5" />
        <span>Export</span>
      </button>
      <button
        type="button"
        onclick={clearAllSessions}
        class="flex-1 flex items-center justify-center gap-1.5 px-3 py-2 text-xs text-stone-400 hover:text-rose-400 bg-stone-800/50 hover:bg-stone-700/50 rounded-md transition cursor-pointer"
        aria-label="Clear all sessions"
      >
        <Trash2 class="h-3.5 w-3.5" />
        <span>Clear</span>
      </button>
    {/if}
    <button
      type="button"
      onclick={toggleImport}
      class={`flex-1 flex items-center justify-center gap-1.5 px-3 py-2 text-xs rounded-md transition cursor-pointer ${
        showImport
          ? "text-cyan-400 bg-cyan-500/10"
          : "text-stone-400 hover:text-cyan-400 bg-stone-800/50 hover:bg-stone-700/50"
      }`}
      aria-label="Import sessions"
    >
      <Upload class="h-3.5 w-3.5" />
      <span>Import</span>
    </button>
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
</div>
