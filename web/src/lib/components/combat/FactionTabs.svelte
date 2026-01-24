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

  const handleSelectChange = (e: Event) => {
    const target = e.target as HTMLSelectElement;
    onTabChange(target.value as TabValue);
  };
</script>

<!-- Mobile: Select dropdown -->
<div class="border-b border-stone-700 md:hidden">
  <select
    value={activeTab}
    onchange={handleSelectChange}
    class="w-full bg-stone-800 px-4 py-3 text-base font-semibold uppercase tracking-wider text-amber-500 border-none outline-none cursor-pointer"
  >
    {#each tabs as tab (tab.value)}
      <option value={tab.value}>{tab.label}</option>
    {/each}
  </select>
</div>

<!-- Desktop: Tabs -->
<div class="border-b border-stone-700 hidden md:block">
  <nav class="flex -mb-px">
    {#each tabs as tab (tab.value)}
      <button
        type="button"
        class={`px-4 py-3 text-sm font-semibold uppercase tracking-wider transition-colors cursor-pointer ${
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
