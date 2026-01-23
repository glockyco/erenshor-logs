namespace ErenshorLogs.Context;

/// <summary>
/// Thread-local context stack for tracking ability execution flow.
/// Allows damage/heal hooks to attribute events to the correct ability.
/// </summary>
/// <remarks>
/// Uses a context stack pattern to handle nested ability calls (e.g., skills that proc spells).
/// Each ability pushes context before execution and pops after completion.
/// Thread-local to support concurrent combat processing without synchronization overhead.
/// </remarks>
public static class CombatContext
{
  [ThreadStatic]
  private static Stack<AbilityContext>? _abilityStack;

  /// <summary>
  /// Gets the thread-local ability stack, creating it if needed.
  /// </summary>
  private static Stack<AbilityContext> AbilityStack =>
    _abilityStack ??= new Stack<AbilityContext>();

  /// <summary>
  /// Push ability context onto the stack before execution.
  /// MUST be paired with Pop() in a try/finally block to ensure cleanup.
  /// </summary>
  /// <param name="context">The ability context to push.</param>
  public static void PushAbility(AbilityContext context)
  {
    AbilityStack.Push(context);
  }

  /// <summary>
  /// Pop ability context from the stack after execution.
  /// MUST be called in a finally block to ensure cleanup even if exceptions occur.
  /// </summary>
  public static void PopAbility()
  {
    if (AbilityStack.Count > 0)
    {
      AbilityStack.Pop();
    }
  }

  /// <summary>
  /// Get the current ability context without removing it from the stack.
  /// Returns the most recently pushed context (top of stack).
  /// </summary>
  /// <returns>
  /// The current ability context, or null if no ability is currently executing.
  /// </returns>
  public static AbilityContext? CurrentAbility()
  {
    return AbilityStack.Count > 0 ? AbilityStack.Peek() : null;
  }

  /// <summary>
  /// Clear all context from the stack.
  /// Useful for testing or recovery from error states.
  /// </summary>
  public static void Clear()
  {
    AbilityStack.Clear();
  }

  /// <summary>
  /// Get the current stack depth.
  /// Primarily for testing and diagnostics.
  /// </summary>
  internal static int Depth() => AbilityStack.Count;
}
