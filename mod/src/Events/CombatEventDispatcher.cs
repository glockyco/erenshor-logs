using ErenshorLogs.Session;

namespace ErenshorLogs.Events;

public static class KillingBlowTracker
{
  private static readonly object Lock = new();
  private static readonly Dictionary<string, long> LatestDamageByTarget = new();

  public static void RecordDamage(string targetId, long eventSeq)
  {
    if (string.IsNullOrEmpty(targetId))
      return;

    lock (Lock)
      LatestDamageByTarget[targetId] = eventSeq;
  }

  public static long? GetLatestDamageEventSeq(string? targetId)
  {
    if (string.IsNullOrEmpty(targetId))
      return null;

    lock (Lock)
      return LatestDamageByTarget.TryGetValue(targetId, out var seq) ? seq : null;
  }

  public static void Clear()
  {
    lock (Lock)
      LatestDamageByTarget.Clear();
  }
}

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
    if (
      evt.EventType
        is EventType.DamagePhysical
          or EventType.DamageMagic
          or EventType.DamageMelee
          or EventType.DamageSkill
          or EventType.DamageSpell
          or EventType.DamageDot
          or EventType.DamageProc
          or EventType.DamagePet
          or EventType.DamageReflect
          or EventType.DamageEnvironmental
      && evt.Target?.Id != null
    )
      KillingBlowTracker.RecordDamage(evt.Target.Id, emitter.EventCount + 1);

    emitter.Emit(evt);
  }
}
