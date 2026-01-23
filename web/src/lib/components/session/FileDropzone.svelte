<script lang="ts">
  import { Upload } from "@lucide/svelte";
  import { importSessions, readFileAsText } from "$lib/services";
  import type { ImportResult } from "$lib/services";

  interface Props {
    onimport?: (sessions: import("$lib/types").Session[]) => void;
    onerror?: (error: string) => void;
  }

  let { onimport, onerror }: Props = $props();

  let isDragging = $state(false);
  let isProcessing = $state(false);
  let dragCounter = 0;

  const handleDragEnter = (e: DragEvent) => {
    e.preventDefault();
    e.stopPropagation();
    dragCounter++;
    if (e.dataTransfer?.types.includes("Files")) {
      isDragging = true;
    }
  };

  const handleDragOver = (e: DragEvent) => {
    e.preventDefault();
    e.stopPropagation();
  };

  const handleDragLeave = (e: DragEvent) => {
    e.preventDefault();
    e.stopPropagation();
    dragCounter--;
    if (dragCounter === 0) {
      isDragging = false;
    }
  };

  const handleDrop = async (e: DragEvent) => {
    e.preventDefault();
    e.stopPropagation();
    isDragging = false;
    dragCounter = 0;

    const files = e.dataTransfer?.files;
    if (files && files.length > 0) {
      await processFile(files[0]);
    }
  };

  const handleFileInput = async (e: Event) => {
    const input = e.target as HTMLInputElement;
    const files = input.files;
    if (files && files.length > 0) {
      await processFile(files[0]);
    }
    // Reset input so the same file can be selected again
    input.value = "";
  };

  const processFile = async (file: File) => {
    // Validate file type
    if (!file.name.endsWith(".json")) {
      onerror?.("Please select a .json file");
      return;
    }

    isProcessing = true;

    try {
      const text = await readFileAsText(file);
      const result: ImportResult = importSessions(text);

      if (result.success) {
        onimport?.(result.sessions);
      } else {
        onerror?.(result.error);
      }
    } catch (err) {
      onerror?.(err instanceof Error ? err.message : "Failed to read file");
    } finally {
      isProcessing = false;
    }
  };
</script>

<div
  class={`relative rounded-lg border-2 border-dashed p-8 text-center transition-all ${
    isDragging
      ? "border-cyan-500 bg-cyan-500/10"
      : "border-stone-700 bg-stone-800/50 hover:border-stone-600 hover:bg-stone-800/70"
  } ${isProcessing ? "pointer-events-none opacity-50" : ""}`}
  ondragenter={handleDragEnter}
  ondragover={handleDragOver}
  ondragleave={handleDragLeave}
  ondrop={handleDrop}
  role="button"
  tabindex="0"
  aria-label="Drop zone for importing session files"
>
  <Upload class={`mx-auto h-12 w-12 ${isDragging ? "text-cyan-400" : "text-stone-500"}`} />

  <p class="mt-4 text-sm font-medium text-stone-300">
    {#if isProcessing}
      Processing...
    {:else if isDragging}
      Drop to import
    {:else}
      Drop session file here
    {/if}
  </p>

  <p class="mt-1 text-xs text-stone-500">or click to browse</p>

  <input
    type="file"
    accept=".json"
    onchange={handleFileInput}
    class="absolute inset-0 cursor-pointer opacity-0"
    disabled={isProcessing}
    aria-label="Select session file to import"
  />
</div>
