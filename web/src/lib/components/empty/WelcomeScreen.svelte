<script lang="ts">
  import { Swords } from "@lucide/svelte";
  import FileDropzone from "../session/FileDropzone.svelte";
  import { sessions, addSession, setActiveSession } from "$lib/state/sessions.svelte";
  import type { Session } from "$lib/types";

  let importError = $state<string | null>(null);
  let importSuccess = $state<string | null>(null);

  const handleImport = (importedSessions: Session[]) => {
    let addedCount = 0;
    let skippedCount = 0;

    for (const session of importedSessions) {
      if (sessions.has(session.id)) {
        skippedCount++;
        continue;
      }
      addSession({ id: session.id, startTime: session.startTime });
      // Update with full session data
      sessions.set(session.id, session);
      addedCount++;
    }

    if (addedCount > 0) {
      // Set the first imported session as active
      setActiveSession(importedSessions[0].id);
      importSuccess = `Imported ${addedCount} session${addedCount === 1 ? "" : "s"}${skippedCount > 0 ? ` (${skippedCount} skipped)` : ""}`;
      importError = null;

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
</script>

<div class="flex min-h-[600px] items-center justify-center px-4">
  <div class="max-w-md rounded-lg border-2 border-amber-600/50 bg-stone-800 p-8 shadow-lg">
    <!-- Icon -->
    <div class="mb-6 flex justify-center">
      <Swords class="h-20 w-20 text-amber-500" aria-hidden="true" />
    </div>

    <!-- Title -->
    <h2 class="mb-3 text-center font-fantasy text-2xl font-bold text-amber-500">
      Welcome to Erenshor Logs
    </h2>

    <!-- Subtitle -->
    <p class="mb-6 text-center text-stone-400">
      The dashboard will appear here<br />once you enter combat in-game.
    </p>

    <!-- Steps -->
    <div class="mb-6 space-y-3 rounded-md bg-stone-900/50 p-4">
      <div class="flex items-start gap-3">
        <div
          class="flex h-6 w-6 flex-shrink-0 items-center justify-center rounded-full bg-amber-500/10 text-sm font-bold text-amber-500"
        >
          1
        </div>
        <p class="text-sm text-stone-300">Launch Erenshor with mod installed</p>
      </div>

      <div class="flex items-start gap-3">
        <div
          class="flex h-6 w-6 flex-shrink-0 items-center justify-center rounded-full bg-amber-500/10 text-sm font-bold text-amber-500"
        >
          2
        </div>
        <p class="text-sm text-stone-300">Enter combat with any enemy</p>
      </div>

      <div class="flex items-start gap-3">
        <div
          class="flex h-6 w-6 flex-shrink-0 items-center justify-center rounded-full bg-amber-500/10 text-sm font-bold text-amber-500"
        >
          3
        </div>
        <p class="text-sm text-stone-300">See real-time DPS analysis appear here</p>
      </div>
    </div>

    <!-- Status indicator with pulsing dot -->
    <div class="flex items-center justify-center gap-2 text-sm text-stone-500">
      <span class="h-2 w-2 animate-pulse rounded-full bg-amber-500"></span>
      <span>Waiting for combat data...</span>
    </div>

    <!-- Import section -->
    <div class="mt-6 border-t border-stone-700 pt-6">
      <p class="mb-3 text-center text-sm text-stone-500">Or import saved sessions:</p>

      {#if importSuccess}
        <div
          class="mb-3 rounded-lg bg-green-500/10 border border-green-500/30 px-3 py-2 text-xs text-green-400"
        >
          {importSuccess}
        </div>
      {/if}
      {#if importError}
        <div
          class="mb-3 rounded-lg bg-rose-500/10 border border-rose-500/30 px-3 py-2 text-xs text-rose-400"
        >
          {importError}
        </div>
      {/if}

      <FileDropzone onimport={handleImport} onerror={handleImportError} />
    </div>
  </div>
</div>
