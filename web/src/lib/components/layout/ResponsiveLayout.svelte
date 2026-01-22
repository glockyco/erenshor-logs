<script lang="ts">
  import type { Snippet } from "svelte";
  import { clsx } from "clsx";

  interface Props {
    sidebar: Snippet;
    main: Snippet;
  }

  let { sidebar, main }: Props = $props();

  let activeTab = $state<"sessions" | "detail">("sessions");
</script>

<!-- Mobile/Tablet: Tab navigation -->
<div class="lg:hidden">
  <div class="flex border-b border-slate-800" role="tablist" aria-label="Content sections">
    <button
      role="tab"
      id="tab-sessions"
      aria-selected={activeTab === "sessions"}
      aria-controls="panel-sessions"
      class={clsx(
        "flex-1 px-4 py-3 text-sm font-semibold uppercase tracking-wider transition",
        activeTab === "sessions"
          ? "border-b-2 border-cyan-400 text-cyan-400"
          : "text-slate-400 hover:text-slate-300"
      )}
      onclick={() => (activeTab = "sessions")}
    >
      Sessions
    </button>
    <button
      role="tab"
      id="tab-detail"
      aria-selected={activeTab === "detail"}
      aria-controls="panel-detail"
      class={clsx(
        "flex-1 px-4 py-3 text-sm font-semibold uppercase tracking-wider transition",
        activeTab === "detail"
          ? "border-b-2 border-cyan-400 text-cyan-400"
          : "text-slate-400 hover:text-slate-300"
      )}
      onclick={() => (activeTab = "detail")}
    >
      Detail
    </button>
  </div>

  {#if activeTab === "sessions"}
    <div
      role="tabpanel"
      id="panel-sessions"
      aria-labelledby="tab-sessions"
      tabindex="0"
      class="p-4"
    >
      {@render sidebar()}
    </div>
  {:else}
    <div role="tabpanel" id="panel-detail" aria-labelledby="tab-detail" tabindex="0" class="p-4">
      {@render main()}
    </div>
  {/if}
</div>

<!-- Desktop: Two-column layout -->
<div class="hidden lg:grid lg:grid-cols-[320px_1fr] lg:gap-6 lg:p-6">
  <aside class="max-h-[calc(100vh-80px)] overflow-y-auto">
    {@render sidebar()}
  </aside>
  <main class="max-h-[calc(100vh-80px)] overflow-y-auto">
    {@render main()}
  </main>
</div>
