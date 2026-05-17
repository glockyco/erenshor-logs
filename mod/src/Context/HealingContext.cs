using ErenshorLogs.Events;

namespace ErenshorLogs.Context;

public sealed record HealingContextFrame(
  Character? Source,
  AbilityRef Ability,
  EventType EventType,
  AttributionMethod Attribution
);

public static class HealingContext
{
  [ThreadStatic]
  private static Stack<HealingContextFrame>? _stack;

  private static Stack<HealingContextFrame> Stack => _stack ??= new Stack<HealingContextFrame>();

  public static IDisposable Push(
    Character? source,
    AbilityRef ability,
    EventType eventType,
    AttributionMethod attribution
  )
  {
    Stack.Push(new HealingContextFrame(source, ability, eventType, attribution));
    return new PopScope();
  }

  public static HealingContextFrame? Current() => Stack.Count == 0 ? null : Stack.Peek();

  public static void Clear() => Stack.Clear();

  private sealed class PopScope : IDisposable
  {
    private bool _disposed;

    public void Dispose()
    {
      if (_disposed)
        return;

      _disposed = true;
      if (Stack.Count > 0)
        Stack.Pop();
    }
  }
}
