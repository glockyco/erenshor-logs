using ErenshorLogs.Events;

namespace ErenshorLogs.Registry;

/// <summary>
/// Determines the ActorType for a game Character.
/// </summary>
public interface IActorTypeResolver
{
  /// <summary>
  /// Resolves the actor type for a Character.
  /// </summary>
  /// <param name="character">The game Character object.</param>
  /// <returns>The determined ActorType.</returns>
  ActorType Resolve(Character character);
}
