namespace ErenshorLogs.Events;

/// <summary>
/// Central event bus for combat event dispatch.
/// </summary>
public sealed class EventEmitter : IEventEmitter
{
  private readonly Action<string>? _logError;
  private readonly object _lock = new();
  private readonly List<Action<CombatEvent>> _listeners = [];
  private long _eventCount;

  /// <summary>
  /// Creates a new event emitter.
  /// </summary>
  /// <param name="logError">Optional callback for error logging.</param>
  public EventEmitter(Action<string>? logError = null)
  {
    _logError = logError;
  }

  /// <inheritdoc />
  public int ListenerCount
  {
    get
    {
      lock (_lock)
      {
        return _listeners.Count;
      }
    }
  }

  /// <inheritdoc />
  public long EventCount => Interlocked.Read(ref _eventCount);

  /// <inheritdoc />
  public void Emit(CombatEvent evt)
  {
    Interlocked.Increment(ref _eventCount);

    List<Action<CombatEvent>> snapshot;
    lock (_lock)
    {
      snapshot = [.. _listeners];
    }

    foreach (var listener in snapshot)
    {
      try
      {
        listener(evt);
      }
      catch (Exception ex)
      {
        _logError?.Invoke($"Event listener threw exception: {ex}");
      }
    }
  }

  /// <inheritdoc />
  public IDisposable Subscribe(Action<CombatEvent> handler)
  {
    lock (_lock)
    {
      _listeners.Add(handler);
    }

    return new Subscription(this, handler);
  }

  private void Unsubscribe(Action<CombatEvent> handler)
  {
    lock (_lock)
    {
      _listeners.Remove(handler);
    }
  }

  private sealed class Subscription(EventEmitter emitter, Action<CombatEvent> handler) : IDisposable
  {
    private bool _disposed;

    public void Dispose()
    {
      if (_disposed)
        return;
      _disposed = true;
      emitter.Unsubscribe(handler);
    }
  }
}
