<script lang="ts">
  type TabValue = "damageDealt" | "damageTaken" | "healingDone" | "healingReceived";

  interface Props {
    activeTab: TabValue;
    onTabChange: (tab: TabValue) => void;
  }

  let { activeTab, onTabChange }: Props = $props();

  const tabs = [
    { value: "damageDealt" as const, label: "Damage Dealt" },
    { value: "damageTaken" as const, label: "Damage Taken" },
    { value: "healingDone" as const, label: "Healing Done" },
    { value: "healingReceived" as const, label: "Healing Received" },
  ];
</script>

<div class="border-b border-stone-700">
  <nav class="flex -mb-px">
    {#each tabs as tab (tab.value)}
      <button
        type="button"
        class={`flex-1 px-4 py-3 text-xs font-semibold uppercase tracking-wider transition-colors cursor-pointer ${
          activeTab === tab.value
            ? "border-b-2 border-amber-500 text-amber-500"
            : "border-b-2 border-transparent text-stone-400 hover:text-stone-200"
        }`}
        onclick={() => onTabChange(tab.value)}
      >
        {tab.label}
      </button>
    {/each}
  </nav>
</div>
