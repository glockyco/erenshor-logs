using ErenshorLogs.Session;

namespace ErenshorLogs.Events;

public static class CombatEventDispatcher
{
  public static long PrepareForCapture(
    EventType eventType,
    ISessionManager? sessionManager,
    long timestamp
  )
  {
    sessionManager?.OnCombatEvent(eventType, timestamp);
    return timestamp;
  }

  public static void Dispatch(CombatEvent evt, IEventEmitter emitter)
  {
    emitter.Emit(evt);
  }
}
