<script lang="ts">
  import { cn } from "$lib/utils";

  /**
   * LoadingScreen - Minimal loading indicator for hydration and async operations
   *
   * A reusable loading component that displays a pulsing amber dot, optionally
   * with a message. Used during app hydration (~50ms) and other async operations.
   *
   * Matches the Classic MMO design system with amber accents and stone backgrounds.
   * Always displays a pulsing indicator to indicate waiting/loading state.
   */

  interface Props {
    /**
     * Optional message to display below the loading indicator.
     * Typically omitted for brief operations like hydration (~50ms).
     * Use for longer async operations where users need context.
     *
     * @default undefined (no message)
     */
    message?: string;

    /**
     * Size of the loading indicator dot.
     *
     * - `sm`: 8px - for inline loading, minimal footprint
     * - `md`: 12px - default, visible but not intrusive (good for full-page)
     * - `lg`: 16px - prominent, for important loading states
     *
     * @default "md"
     */
    size?: "sm" | "md" | "lg";

    /**
     * Whether to center the loader with full-height container.
     * When true: Full viewport height, centered content
     * When false: Inline display, takes only necessary space
     *
     * @default true
     */
    centered?: boolean;

    /**
     * Additional CSS classes to merge with component classes.
     * Uses `cn()` utility for proper class merging with Tailwind.
     *
     * @default undefined
     */
    class?: string;
  }

  let { message, size = "md", centered = true, class: className }: Props = $props();

  // Tailwind size classes for the pulsing dot
  const sizeClasses = {
    sm: "h-2 w-2", // 8px
    md: "h-3 w-3", // 12px
    lg: "h-4 w-4", // 16px
  };
</script>

<!-- Hydration loading state or async operation indicator -->
<div
  class={cn(
    "flex flex-col items-center gap-4",
    centered && "min-h-[600px] justify-center px-4 py-8",
    className
  )}
  role="status"
  aria-live="polite"
  aria-label={message || "Loading application"}
>
  <!-- Pulsing amber indicator dot -->
  <!-- Amber is used throughout the app for "waiting" states (connection, live sessions) -->
  <span class={cn("rounded-full bg-amber-500 animate-pulse", sizeClasses[size])} aria-hidden="true"
  ></span>

  <!-- Optional loading message for user context -->
  {#if message}
    <span class="text-sm text-stone-400">
      {message}
    </span>
  {/if}
</div>
