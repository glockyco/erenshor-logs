<script lang="ts">
  import type { HTMLAttributes } from "svelte/elements";
  import { cn, type WithElementRef } from "$lib/utils.js";

  type Props = WithElementRef<HTMLAttributes<HTMLDivElement>> & {
    glow?: boolean;
  };

  let {
    ref = $bindable(null),
    class: className,
    glow = false,
    children,
    ...restProps
  }: Props = $props();
</script>

<div
  bind:this={ref}
  data-slot="card"
  class={cn(
    "flex flex-col gap-6 rounded-lg border border-slate-700 bg-slate-900 py-6 text-slate-200",
    glow && "shadow-[0_0_20px_rgb(34_211_238_/_0.6)]",
    className
  )}
  {...restProps}
>
  {@render children?.()}
</div>
