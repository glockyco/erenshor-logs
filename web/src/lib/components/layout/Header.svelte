<script lang="ts">
  import ConnectionStatus from "$lib/components/status/ConnectionStatus.connected.svelte";
</script>

<header class="sticky top-0 z-50 border-b border-slate-800 bg-slate-950/90 backdrop-blur-sm">
  <div class="container mx-auto flex items-center justify-between px-4 py-4">
    <h1 class="text-2xl font-bold text-cyan-400 [text-shadow:0_0_20px_rgb(34_211_238_/_0.5)]">
      Erenshor Logs
    </h1>
    <ConnectionStatus />
  </div>
</header>
