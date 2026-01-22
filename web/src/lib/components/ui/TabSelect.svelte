<script lang="ts">
  import { ChevronDown } from "@lucide/svelte";

  interface Tab {
    value: string;
    label: string;
  }

  interface Props {
    tabs: Tab[];
    active: string;
    onTabChange: (value: string) => void;
  }

  let { tabs, active, onTabChange }: Props = $props();

  let isOpen = $state(false);

  const activeTab = $derived(tabs.find((t) => t.value === active));

  function handleSelect(value: string) {
    onTabChange(value);
    isOpen = false;
  }

  function handleKeydown(event: KeyboardEvent) {
    if (event.key === "Escape") {
      isOpen = false;
    }
  }

  // Close dropdown when clicking outside
  function handleClickOutside(event: MouseEvent) {
    const target = event.target as HTMLElement;
    if (!target.closest("[data-tab-select]")) {
      isOpen = false;
    }
  }

  $effect(() => {
    if (isOpen) {
      document.addEventListener("click", handleClickOutside);
      return () => document.removeEventListener("click", handleClickOutside);
    }
  });
</script>

<div class="relative" data-tab-select>
  <!-- Trigger Button -->
  <button
    type="button"
    class="flex items-center justify-between w-full px-4 py-2.5 text-sm font-medium rounded-lg border transition-colors text-cyan-400"
    class:bg-slate-800={!isOpen}
    class:border-slate-700={!isOpen}
    class:bg-slate-700={isOpen}
    class:border-cyan-400={isOpen}
    onclick={() => (isOpen = !isOpen)}
    onkeydown={handleKeydown}
    aria-expanded={isOpen}
    aria-haspopup="listbox"
  >
    <span>{activeTab?.label ?? "Select..."}</span>
    <ChevronDown class={`ml-2 h-4 w-4 transition-transform ${isOpen ? "rotate-180" : ""}`} />
  </button>

  <!-- Dropdown Menu -->
  {#if isOpen}
    <div
      class="absolute z-50 mt-2 w-full rounded-lg border border-slate-700 bg-slate-800 shadow-xl"
      role="listbox"
    >
      {#each tabs as tab (tab.value)}
        <button
          type="button"
          role="option"
          aria-selected={active === tab.value}
          class="flex w-full items-center px-4 py-2.5 text-sm font-medium transition-colors first:rounded-t-lg last:rounded-b-lg"
          class:bg-slate-700={active === tab.value}
          class:text-cyan-400={active === tab.value}
          class:text-slate-300={active !== tab.value}
          class:hover:bg-slate-700={active !== tab.value}
          class:hover:text-slate-200={active !== tab.value}
          onclick={() => handleSelect(tab.value)}
        >
          {tab.label}
          {#if active === tab.value}
            <span class="ml-auto text-cyan-400">✓</span>
          {/if}
        </button>
      {/each}
    </div>
  {/if}
</div>
