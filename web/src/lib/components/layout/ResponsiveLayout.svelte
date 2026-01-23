<script lang="ts">
  import type { Snippet } from "svelte";
  import { clsx } from "clsx";

  interface Props {
    sidebar: Snippet;
    main: Snippet;
  }

  let { sidebar, main }: Props = $props();

  let activeTab = $state<"sessions" | "detail">("sessions");

  // Tab element references for focus management
  let sessionsTabRef: HTMLButtonElement | undefined;
  let detailTabRef: HTMLButtonElement | undefined;

  const tabs = ["sessions", "detail"] as const;

  function focusTab(tab: "sessions" | "detail") {
    if (tab === "sessions") sessionsTabRef?.focus();
    else detailTabRef?.focus();
  }

  function handleTabKeydown(event: KeyboardEvent) {
    const currentIndex = tabs.indexOf(activeTab);
    let newIndex = currentIndex;

    switch (event.key) {
      case "ArrowLeft":
      case "ArrowUp":
        event.preventDefault();
        newIndex = currentIndex === 0 ? tabs.length - 1 : currentIndex - 1;
        break;
      case "ArrowRight":
      case "ArrowDown":
        event.preventDefault();
        newIndex = currentIndex === tabs.length - 1 ? 0 : currentIndex + 1;
        break;
      case "Home":
        event.preventDefault();
        newIndex = 0;
        break;
      case "End":
        event.preventDefault();
        newIndex = tabs.length - 1;
        break;
      default:
        return;
    }

    activeTab = tabs[newIndex];
    focusTab(tabs[newIndex]);
  }
</script>

<!-- Mobile/Tablet: Tab navigation -->
<div class="lg:hidden">
  <!-- svelte-ignore a11y_interactive_supports_focus -->
  <div
    class="flex border-b border-slate-800"
    role="tablist"
    aria-label="Content sections"
    onkeydown={handleTabKeydown}
  >
    <button
      bind:this={sessionsTabRef}
      role="tab"
      id="tab-sessions"
      aria-selected={activeTab === "sessions"}
      aria-controls="panel-sessions"
      tabindex={activeTab === "sessions" ? 0 : -1}
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
      bind:this={detailTabRef}
      role="tab"
      id="tab-detail"
      aria-selected={activeTab === "detail"}
      aria-controls="panel-detail"
      tabindex={activeTab === "detail" ? 0 : -1}
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
  <aside class="self-start max-h-[calc(100vh-80px)] overflow-y-auto pr-3">
    {@render sidebar()}
  </aside>
  <main class="max-h-[calc(100vh-80px)] overflow-y-auto">
    {@render main()}
  </main>
</div>
