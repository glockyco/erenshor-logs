<script lang="ts">
  import { clsx } from "clsx";

  interface Props {
    label: string;
    value: string;
    sublabel?: string;
    sublabelVariant?: "default" | "positive" | "negative";
  }

  let { label, value, sublabel, sublabelVariant = "default" }: Props = $props();

  const sublabelClass = $derived(
    clsx(
      "text-sm",
      sublabelVariant === "default" && "text-slate-400",
      sublabelVariant === "positive" && "text-emerald-400",
      sublabelVariant === "negative" && "text-rose-400"
    )
  );
</script>

<div>
  <div class="text-sm text-slate-400 uppercase tracking-wider">{label}</div>
  <div class="text-2xl font-mono font-bold text-cyan-400">{value}</div>
  {#if sublabel}
    <div class={sublabelClass}>{sublabel}</div>
  {/if}
</div>
