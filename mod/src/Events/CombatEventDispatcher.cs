using ErenshorLogs.Session;

namespace ErenshorLogs.Events;

public static class CombatEventDispatcher
{
  public static void Dispatch(
    CombatEvent evt,
    IEventEmitter emitter,
    ISessionManager? sessionManager
  )
  {
    sessionManager?.OnCombatEvent(evt.EventType, evt.Timestamp);
    emitter.Emit(evt);
  }
}
