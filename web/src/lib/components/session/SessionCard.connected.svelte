<script lang="ts">
  import SessionCard from "./SessionCard.svelte";
  import { activeSessionId, setActiveSession, deleteSession } from "$lib/state/sessions.svelte";
  import { exportSession } from "$lib/services";
  import type { Session } from "$lib/types";

  interface Props {
    session: Session;
  }

  let { session }: Props = $props();

  const handleClick = () => {
    setActiveSession(session.id);
  };

  const handleDelete = () => {
    deleteSession(session.id);
  };

  const handleExport = () => {
    exportSession(session);
  };

  const isActive = $derived(activeSessionId.value === session.id);
</script>

<SessionCard
  {session}
  {isActive}
  onclick={handleClick}
  ondelete={handleDelete}
  onexport={handleExport}
/>
