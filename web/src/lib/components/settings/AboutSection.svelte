<script lang="ts">
  import { Button } from "$lib/components/ui";
  import { Download, Copy, Check } from "@lucide/svelte";
  import { VERSION } from "$lib/version";

  let copied = $state(false);

  function copyVersion() {
    navigator.clipboard.writeText(VERSION);
    copied = true;
    setTimeout(() => {
      copied = false;
    }, 2000);
  }
</script>

<section class="space-y-4">
  <div class="flex items-center gap-2">
    <span class="font-mono text-xs text-stone-500">{VERSION}</span>
    <Button
      size="icon"
      variant="ghost"
      onclick={copyVersion}
      aria-label={copied ? "Version copied!" : "Copy version to clipboard"}
      class="h-7 w-7 text-stone-500 hover:text-stone-300"
    >
      {#if copied}
        <Check class="h-3.5 w-3.5 text-amber-500" aria-hidden="true" />
      {:else}
        <Copy class="h-3.5 w-3.5" aria-hidden="true" />
      {/if}
    </Button>
  </div>

  <div>
    <a
      href="/mods/ErenshorLogs.dll"
      download="ErenshorLogs.dll"
      data-sveltekit-reload
      rel="external"
      class="inline-flex items-center gap-2 rounded-lg bg-stone-700 px-4 py-2 text-sm font-semibold text-stone-200 transition-all hover:bg-stone-600 active:scale-95"
    >
      <Download class="h-4 w-4" aria-hidden="true" />
      <span>Download ErenshorLogs.dll</span>
    </a>
  </div>
</section>
