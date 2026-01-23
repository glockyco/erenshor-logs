<script lang="ts">
  import { FlaskConical } from "@lucide/svelte";
  import WelcomeCard from "./WelcomeCard.svelte";
  import { sessions, addSession, setActiveSession } from "$lib/state/sessions.svelte";
  import { importSessions } from "$lib/services";

  let isLoading = $state(false);
  let error = $state<string | null>(null);

  const DEMO_STATS = {
    sessions: 4,
    events: 1491,
    duration: "~5 min",
  };

  const loadDemo = async () => {
    isLoading = true;
    error = null;

    try {
      const response = await fetch("/demo/sessions.json");
      if (!response.ok) throw new Error("Failed to load demo data");

      const text = await response.text();
      const result = importSessions(text);

      if (!result.success) {
        throw new Error(result.error);
      }

      // Import all demo sessions
      for (const session of result.sessions) {
        if (!sessions.has(session.id)) {
          addSession({ id: session.id, startTime: session.startTime });
          sessions.set(session.id, session);
        }
      }

      // Set first session as active
      if (result.sessions.length > 0) {
        setActiveSession(result.sessions[0].id);
      }
    } catch (err) {
      error = err instanceof Error ? err.message : "Failed to load demo";
    } finally {
      isLoading = false;
    }
  };
</script>

<WelcomeCard title="Try Demo Data" icon={FlaskConical}>
  <p class="text-center text-sm text-stone-400">Explore with sample combat logs</p>

  <!-- Demo Stats -->
  <div class="rounded-md bg-stone-900/50 p-3 text-center">
    <div class="grid grid-cols-3 gap-2 text-xs">
      <div>
        <div class="font-mono text-lg font-bold text-cyan-400">
          {DEMO_STATS.sessions}
        </div>
        <div class="text-stone-500">Sessions</div>
      </div>
      <div>
        <div class="font-mono text-lg font-bold text-cyan-400">
          {DEMO_STATS.events.toLocaleString()}
        </div>
        <div class="text-stone-500">Events</div>
      </div>
      <div>
        <div class="text-lg font-bold text-cyan-400">{DEMO_STATS.duration}</div>
        <div class="text-stone-500">Combat</div>
      </div>
    </div>
  </div>

  {#if error}
    <div
      class="rounded-lg border border-rose-500/30 bg-rose-500/10 px-3 py-2 text-xs text-rose-400"
    >
      {error}
    </div>
  {/if}

  <!-- Load Button -->
  <button
    onclick={loadDemo}
    disabled={isLoading}
    class="w-full rounded-lg bg-cyan-500 px-6 py-3 font-semibold text-stone-900 transition-all hover:bg-cyan-400 active:scale-95 disabled:cursor-not-allowed disabled:opacity-50"
  >
    {isLoading ? "Loading..." : "Load Demo Data"}
  </button>
</WelcomeCard>
