<script lang="ts">
  import { clsx } from "clsx";

  interface Props {
    variant?: "default" | "player" | "simPlayer" | "npc" | "pet";
    children: import("svelte").Snippet;
  }

  let { variant = "default", children }: Props = $props();

  const classes = $derived(
    clsx(
      "inline-flex items-center px-2 py-0.5 text-xs font-medium rounded-full",
      variant === "default" && "bg-slate-700 text-slate-300",
      variant === "player" && "bg-cyan-500/20 text-cyan-400",
      variant === "simPlayer" && "bg-emerald-500/20 text-emerald-400",
      variant === "npc" && "bg-rose-500/20 text-rose-400",
      variant === "pet" && "bg-violet-500/20 text-violet-400"
    )
  );
</script>

<span class={classes}>
  {@render children()}
</span>
