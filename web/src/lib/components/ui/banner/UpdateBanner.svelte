<script lang="ts">
  import { Download, X } from "@lucide/svelte";
  import { Button } from "$lib/components/ui";
  import { fade } from "svelte/transition";
  import { cn } from "$lib/utils";

  /**
   * UpdateBanner - Dismissable notification for mod updates
   *
   * A styled alert banner that informs users when a newer version of the mod
   * is available. Uses the amber accent pattern consistent with the Classic MMO
   * design system. Includes a download link and dismiss button.
   *
   * This is a presentational component - it receives all data via props and
   * has no direct state dependencies.
   */

  interface Props {
    /**
     * Callback invoked when the user dismisses the banner.
     * The parent component should handle state persistence.
     */
    ondismiss: () => void;

    /**
     * URL for the mod download link.
     * @default "/mods/ErenshorLogs.dll"
     */
    downloadHref?: string;

    /**
     * Additional CSS classes to merge with component classes.
     * Uses `cn()` utility for proper class merging with Tailwind.
     */
    class?: string;
  }

  let { ondismiss, downloadHref = "/mods/ErenshorLogs.dll", class: className }: Props = $props();
</script>

<!-- Update notification banner -->
<div
  role="alert"
  class={cn(
    "flex items-center gap-4 rounded-lg border border-amber-600/50 bg-amber-900/30 p-4 md:ml-0",
    className
  )}
  transition:fade={{ duration: 200 }}
>
  <!-- Download icon -->
  <Download class="h-5 w-5 flex-shrink-0 text-amber-400" aria-hidden="true" />

  <!-- Message and download link -->
  <div class="flex-1 text-sm text-stone-200">
    A newer version of the mod is available.
    <a href={downloadHref} class="ml-1 font-semibold text-amber-400 underline hover:text-amber-300">
      Download update
    </a>
  </div>

  <!-- Dismiss button -->
  <Button
    variant="ghost"
    size="icon"
    onclick={ondismiss}
    aria-label="Dismiss update notification"
    class="flex-shrink-0"
  >
    <X class="h-4 w-4" aria-hidden="true" />
  </Button>
</div>
