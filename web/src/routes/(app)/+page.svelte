<script lang="ts">
  import { onMount } from "svelte";
  import { List } from "@lucide/svelte";
  import Header from "$lib/components/layout/Header.svelte";
  import WelcomeScreen from "$lib/components/empty/WelcomeScreen.svelte";
  import SessionList from "$lib/components/session/SessionList.svelte";
  import SessionListMobile from "$lib/components/session/SessionListMobile.svelte";
  import SessionToolbar from "$lib/components/session/SessionToolbar.svelte";
  import SidebarToggle from "$lib/components/session/SidebarToggle.svelte";
  import StatsPanel from "$lib/components/combat/StatsPanel.svelte";
  import ActorTable from "$lib/components/combat/ActorTable.svelte";
  import DebugPanel from "$lib/components/debug/DebugPanel.svelte";
  import * as Drawer from "$lib/components/ui/drawer";
  import { sessions, activeSession, activeSessionStats } from "$lib/state/sessions.svelte";
  import { sidebarCollapsed, initUiPersistence } from "$lib/state/ui.svelte";

  const hasSessions = $derived(sessions.size > 0);
  const session = $derived(activeSession.value);
  const stats = $derived(activeSessionStats.value);
  const isLive = $derived(session ? !session.endTime : false);
  const duration = $derived(
    session
      ? session.endTime
        ? session.endTime - session.startTime
        : Date.now() - session.startTime
      : 0
  );

  // Sidebar width for desktop layout offset
  const sidebarWidth = $derived(sidebarCollapsed.value ? 80 : 280);

  // Mobile drawer state
  let drawerOpen = $state(false);

  // Initialize UI persistence on mount
  onMount(() => {
    return initUiPersistence();
  });

  // Keyboard shortcut: Cmd+B / Ctrl+B to toggle sidebar
  function handleKeydown(e: KeyboardEvent) {
    if ((e.metaKey || e.ctrlKey) && e.key === "b") {
      e.preventDefault();
      sidebarCollapsed.toggle();
    }
  }
</script>

<svelte:window onkeydown={handleKeydown} />

{#if !hasSessions}
  <div class="p-8">
    <div class="max-w-7xl mx-auto space-y-8">
      <Header />
      <WelcomeScreen />
    </div>
  </div>
{:else}
  <!-- Desktop: Fixed collapsible sidebar (hidden on mobile) -->
  <aside
    aria-label="Session list"
    class="fixed left-0 top-0 z-20 hidden h-full flex-col border-r border-stone-700 bg-stone-900 md:flex"
    style="width: {sidebarWidth}px"
  >
    <!-- Header with toggle -->
    <div class="flex h-14 items-center justify-between border-b border-stone-700 px-3">
      {#if !sidebarCollapsed.value}
        <h2 class="text-sm font-semibold uppercase tracking-wider text-amber-500">
          Sessions ({sessions.size})
        </h2>
      {/if}
      <SidebarToggle isCollapsed={sidebarCollapsed.value} ontoggle={sidebarCollapsed.toggle} />
    </div>

    <!-- Toolbar area (only when expanded) -->
    {#if !sidebarCollapsed.value}
      <div class="border-b border-stone-700 px-3 py-2">
        <SessionToolbar />
      </div>
    {/if}

    <!-- Session list content -->
    <div class="flex-1 overflow-y-auto p-3">
      <SessionList collapsed={sidebarCollapsed.value} />
    </div>
  </aside>

  <!-- Main content (offset on desktop to account for sidebar) -->
  <div class="min-h-screen main-content" style="--sidebar-offset: {sidebarWidth}px">
    <div class="p-8">
      <div class="max-w-7xl mx-auto space-y-8">
        <Header />
        <StatsPanel {stats} {isLive} {duration} />
        <ActorTable {stats} />
        {#if session}
          <DebugPanel {session} {duration} />
        {/if}
      </div>
    </div>
  </div>

  <!-- Mobile: FAB to open drawer (hidden on desktop) -->
  <button
    type="button"
    class="fixed bottom-4 right-4 z-20 flex h-14 w-14 items-center justify-center rounded-full bg-amber-600 text-stone-900 shadow-lg hover:bg-amber-500 transition-colors md:hidden"
    onclick={() => (drawerOpen = true)}
    aria-label="View sessions"
  >
    <List class="h-6 w-6" />
  </button>

  <!-- Mobile: Drawer with session list -->
  <Drawer.Root bind:open={drawerOpen}>
    <Drawer.Portal>
      <Drawer.Overlay />
      <Drawer.Content class="max-h-[85vh]">
        <Drawer.Header>
          <Drawer.Title>Sessions ({sessions.size})</Drawer.Title>
        </Drawer.Header>

        <!-- Toolbar -->
        <div class="px-4 pb-3">
          <SessionToolbar />
        </div>

        <!-- Session list -->
        <div class="flex-1 overflow-y-auto px-4 pb-4">
          <SessionListMobile onSessionSelect={() => (drawerOpen = false)} />
        </div>
      </Drawer.Content>
    </Drawer.Portal>
  </Drawer.Root>
{/if}

<style>
  @media (min-width: 768px) {
    .main-content {
      padding-left: var(--sidebar-offset);
    }
  }
</style>
