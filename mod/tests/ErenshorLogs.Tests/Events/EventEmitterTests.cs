using ErenshorLogs.Events;
using Xunit;

namespace ErenshorLogs.Tests.Events;

public class EventEmitterTests
{
  private static CombatEvent CreateTestEvent() =>
    new()
    {
      Id = Guid.NewGuid().ToString(),
      Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
      EventType = EventType.DamageMelee,
    };

  [Fact]
  public void Emit_CallsAllSubscribers()
  {
    var emitter = new EventEmitter();
    var received1 = new List<CombatEvent>();
    var received2 = new List<CombatEvent>();

    emitter.Subscribe(e => received1.Add(e));
    emitter.Subscribe(e => received2.Add(e));

    var evt = CreateTestEvent();
    emitter.Emit(evt);

    Assert.Single(received1);
    Assert.Single(received2);
    Assert.Same(evt, received1[0]);
    Assert.Same(evt, received2[0]);
  }

  [Fact]
  public void Subscribe_ReturnsDisposable_ThatUnsubscribes()
  {
    var emitter = new EventEmitter();
    var received = new List<CombatEvent>();

    var subscription = emitter.Subscribe(e => received.Add(e));
    emitter.Emit(CreateTestEvent());
    Assert.Single(received);

    subscription.Dispose();
    emitter.Emit(CreateTestEvent());
    Assert.Single(received); // Still 1, not called again
  }

  [Fact]
  public void Emit_ContinuesAfterListenerException()
  {
    var emitter = new EventEmitter();
    var received = new List<CombatEvent>();

    emitter.Subscribe(_ => throw new InvalidOperationException("Test exception"));
    emitter.Subscribe(e => received.Add(e));

    emitter.Emit(CreateTestEvent());

    Assert.Single(received); // Second listener still called
  }

  [Fact]
  public void ListenerCount_TracksSubscriptions()
  {
    var emitter = new EventEmitter();
    Assert.Equal(0, emitter.ListenerCount);

    var sub1 = emitter.Subscribe(_ => { });
    Assert.Equal(1, emitter.ListenerCount);

    var sub2 = emitter.Subscribe(_ => { });
    Assert.Equal(2, emitter.ListenerCount);

    sub1.Dispose();
    Assert.Equal(1, emitter.ListenerCount);

    sub2.Dispose();
    Assert.Equal(0, emitter.ListenerCount);
  }

  [Fact]
  public void EventCount_IncrementsOnEmit()
  {
    var emitter = new EventEmitter();
    Assert.Equal(0, emitter.EventCount);

    emitter.Emit(CreateTestEvent());
    Assert.Equal(1, emitter.EventCount);

    emitter.Emit(CreateTestEvent());
    emitter.Emit(CreateTestEvent());
    Assert.Equal(3, emitter.EventCount);
  }

  [Fact]
  public void Emit_WithNoSubscribers_DoesNotThrow()
  {
    var emitter = new EventEmitter();
    var exception = Record.Exception(() => emitter.Emit(CreateTestEvent()));
    Assert.Null(exception);
  }

  [Fact]
  public void Dispose_CalledMultipleTimes_DoesNotThrow()
  {
    var emitter = new EventEmitter();
    var subscription = emitter.Subscribe(_ => { });

    subscription.Dispose();
    var exception = Record.Exception(() => subscription.Dispose());

    Assert.Null(exception);
    Assert.Equal(0, emitter.ListenerCount);
  }
}
