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
    let addedCount = 0;
    let skippedCount = 0;

    for (const session of importedSessions) {
      if (sessions.has(session.id)) {
        skippedCount++;
        continue;
      }
      addSession({ id: session.id, startTime: session.startTime });
      // Update with full session data (addSession only creates minimal, we need to update)
      sessions.set(session.id, session);
      addedCount++;
    }

    if (addedCount > 0) {
      // Set the first imported session as active
      setActiveSession(importedSessions[0].id);
      importSuccess = `Imported ${addedCount} session${addedCount === 1 ? "" : "s"}${skippedCount > 0 ? ` (${skippedCount} skipped - already exists)` : ""}`;
      importError = null;
      showImport = false;

      // Clear success message after 3 seconds
      setTimeout(() => {
        importSuccess = null;
      }, 3000);
    } else {
      importError = "All sessions already exist";
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
          <span class="hidden sm:inline">Export All</span>
          <span class="sm:hidden">Export</span>
        </button>
        <button
          type="button"
          onclick={clearAllSessions}
          class="flex items-center gap-1 text-xs text-stone-400 hover:text-rose-400 transition cursor-pointer"
          aria-label="Clear all sessions"
        >
          <Trash2 class="h-3 w-3" />
          <span class="hidden sm:inline">Clear All</span>
          <span class="sm:hidden">Clear</span>
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
      <SessionCardConnected {session} />
    {/each}
  {/if}
</div>
