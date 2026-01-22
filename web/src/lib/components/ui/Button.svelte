<script lang="ts">
  import { clsx } from "clsx";

  interface Props {
    variant?: "primary" | "default" | "ghost" | "danger";
    size?: "sm" | "md" | "lg";
    disabled?: boolean;
    onclick?: () => void;
    children: import("svelte").Snippet;
  }

  let { variant = "default", size = "md", disabled = false, onclick, children }: Props = $props();

  const classes = $derived(
    clsx(
      // Base styles
      "inline-flex items-center justify-center font-semibold rounded-lg transition-all",
      "focus:outline-none focus:ring-2 focus:ring-cyan-400 focus:ring-offset-2 focus:ring-offset-slate-950",

      // Size variants
      size === "sm" && "px-3 py-1.5 text-sm",
      size === "md" && "px-4 py-2 text-base",
      size === "lg" && "px-6 py-3 text-lg",

      // Color variants
      variant === "default" && "bg-slate-700 text-slate-300 hover:bg-slate-600",
      variant === "primary" &&
        "bg-cyan-500 text-slate-950 hover:bg-cyan-400 hover:shadow-[0_0_20px_rgb(34_211_238_/_0.6)]",
      variant === "ghost" && "bg-transparent text-cyan-400 hover:bg-cyan-500/10",
      variant === "danger" && "bg-red-500 text-white hover:bg-red-600",

      // Disabled state
      disabled && "opacity-50 cursor-not-allowed pointer-events-none"
    )
  );
</script>

<button class={classes} {disabled} {onclick}>
  {@render children()}
</button>
