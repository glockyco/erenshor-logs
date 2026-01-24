<script lang="ts">
  import { Button } from "$lib/components/ui";
  import { Text } from "$lib/components/ui/typography";
  import { Download, Copy, Check } from "@lucide/svelte";
  import SettingRow from "./SettingRow.svelte";
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
  <!-- Version Info -->
  <SettingRow label="Version">
    <div class="flex items-center gap-2">
      <Text variant="muted" as="span" class="font-mono text-sm">{VERSION}</Text>
      <Button
        size="icon"
        variant="ghost"
        onclick={copyVersion}
        aria-label={copied ? "Version copied!" : "Copy version to clipboard"}
        class="h-8 w-8 text-stone-400 hover:text-stone-200"
      >
        {#if copied}
          <Check class="h-4 w-4 text-amber-500" aria-hidden="true" />
        {:else}
          <Copy class="h-4 w-4" aria-hidden="true" />
        {/if}
      </Button>
    </div>
  </SettingRow>

  <div role="separator" class="border-t border-stone-700"></div>

  <!-- Download Mod -->
  <SettingRow label="Download Mod" helpText="Get the latest BepInEx plugin for Erenshor">
    <a
      href="/mods/ErenshorLogs.dll"
      download="ErenshorLogs.dll"
      data-sveltekit-reload
      rel="external"
      class="inline-flex items-center gap-2 rounded-lg bg-amber-500 px-4 py-2 text-sm font-semibold text-stone-900 transition-all hover:bg-amber-400 active:scale-95"
    >
      <Download class="h-4 w-4" aria-hidden="true" />
      <span>Download ErenshorLogs.dll</span>
    </a>
  </SettingRow>
</section>
