namespace ErenshorLogs.Events;

/// <summary>
/// Central event bus for combat event dispatch.
/// </summary>
public interface IEventEmitter
{
  /// <summary>Emit an event to all subscribers.</summary>
  void Emit(CombatEvent evt);

  /// <summary>Subscribe to events. Dispose the result to unsubscribe.</summary>
  IDisposable Subscribe(Action<CombatEvent> handler);

  /// <summary>Number of active subscribers.</summary>
  int ListenerCount { get; }

  /// <summary>Total events emitted.</summary>
  long EventCount { get; }
}
