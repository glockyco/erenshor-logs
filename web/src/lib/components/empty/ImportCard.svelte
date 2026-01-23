<script lang="ts">
  import { Upload } from "@lucide/svelte";
  import { cn } from "$lib/utils";
  import WelcomeCard from "./WelcomeCard.svelte";
  import FileDropzone from "../session/FileDropzone.svelte";
  import { sessions, addSession, setActiveSession } from "$lib/state/sessions.svelte";
  import type { Session } from "$lib/types";

  let importStatus = $state<{ type: "success" | "error"; message: string } | null>(null);

  const handleImport = (importedSessions: Session[]) => {
    let addedCount = 0;
    let skippedCount = 0;

    for (const session of importedSessions) {
      if (sessions.has(session.id)) {
        skippedCount++;
        continue;
      }
      addSession({ id: session.id, startTime: session.startTime });
      sessions.set(session.id, session);
      addedCount++;
    }

    if (addedCount > 0) {
      setActiveSession(importedSessions[0].id);
      importStatus = {
        type: "success",
        message: `Imported ${addedCount} session${addedCount === 1 ? "" : "s"}${skippedCount > 0 ? ` (${skippedCount} skipped)` : ""}`,
      };
      setTimeout(() => {
        importStatus = null;
      }, 3000);
    } else {
      importStatus = { type: "error", message: "All sessions already exist" };
    }
  };

  const handleImportError = (error: string) => {
    importStatus = { type: "error", message: error };
  };
</script>

<WelcomeCard title="Import Sessions" icon={Upload}>
  <p class="text-center text-sm text-stone-400">Load your saved combat logs</p>

  {#if importStatus}
    <div
      class={cn(
        "rounded-lg border px-3 py-2 text-xs",
        importStatus.type === "success"
          ? "border-green-500/30 bg-green-500/10 text-green-400"
          : "border-rose-500/30 bg-rose-500/10 text-rose-400"
      )}
    >
      {importStatus.message}
    </div>
  {/if}

  <FileDropzone onimport={handleImport} onerror={handleImportError} />
</WelcomeCard>
