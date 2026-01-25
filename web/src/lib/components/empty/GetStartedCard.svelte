<script lang="ts">
  import { Download } from "@lucide/svelte";
  import WelcomeCard from "./WelcomeCard.svelte";

  const steps = ["Download and install the mod", "Launch Erenshor", "Enter combat to see data"];
</script>

<WelcomeCard title="Get Started" icon={Download}>
  <!-- Download Button -->
  <a
    href="/mods/ErenshorLogs.dll"
    download="ErenshorLogs.dll"
    data-sveltekit-reload
    rel="external"
    class="flex items-center justify-center gap-2 rounded-lg bg-amber-500 px-6 py-3 font-semibold text-stone-900 transition-all hover:bg-amber-400 active:scale-95"
  >
    <Download class="h-5 w-5" aria-hidden="true" />
    <span>Download Mod</span>
  </a>

  <!-- Steps -->
  <div class="space-y-2 text-sm text-stone-300">
    {#each steps as step, i (i)}
      <div class="flex items-start gap-3">
        <div
          class="flex h-6 w-6 flex-shrink-0 items-center justify-center rounded-full bg-amber-500/10 text-sm font-bold text-amber-500"
        >
          {i + 1}
        </div>
        <p>{step}</p>
      </div>
    {/each}
  </div>

  <!-- Link to detailed installation guide -->
  <p class="mt-4 text-center text-xs text-stone-400">
    Need help?
    <a href="#installation-guide" class="text-amber-400 underline hover:text-amber-300">
      See installation guide below ↓
    </a>
  </p>
</WelcomeCard>
