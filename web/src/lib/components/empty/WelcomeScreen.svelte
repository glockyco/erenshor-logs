<script lang="ts">
  import WelcomeHeader from "./WelcomeHeader.svelte";
  import GetStartedCard from "./GetStartedCard.svelte";
  import ImportCard from "./ImportCard.svelte";
  import DemoCard from "./DemoCard.svelte";
</script>

<div class="flex min-h-[600px] items-center justify-center px-4 py-8">
  <div class="w-full max-w-6xl">
    <WelcomeHeader />

    <!-- Three-column grid on desktop, single column on mobile -->
    <div class="grid grid-cols-1 gap-6 lg:grid-cols-3">
      <GetStartedCard />
      <ImportCard />
      <DemoCard />
    </div>

    <!-- Status indicator (waiting for live data) -->
    <div class="mt-8 flex items-center justify-center gap-2 text-sm text-stone-500">
      <span class="h-2 w-2 animate-pulse rounded-full bg-amber-500" aria-hidden="true"></span>
      <span>Waiting for live combat data...</span>
    </div>
  </div>
</div>
