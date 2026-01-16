using ErenshorLogs.Events;

namespace ErenshorLogs.Registry;

/// <summary>
/// Registry for mapping game Characters to stable ActorRef instances.
/// Provides consistent actor identification within a session.
/// </summary>
public interface IActorRegistry
{
  /// <summary>
  /// Gets or creates an ActorRef for a Character.
  /// Returns the same ActorRef for the same Character within a session.
  /// </summary>
  /// <param name="character">The game Character.</param>
  /// <returns>The ActorRef, or null if character is null/destroyed.</returns>
  ActorRef? GetOrCreate(Character character);

  /// <summary>
  /// Looks up a previously registered actor by stable ID.
  /// </summary>
  /// <param name="id">The stable actor ID (e.g., "player:0", "npc:5").</param>
  /// <returns>The ActorRef, or null if not found.</returns>
  ActorRef? GetById(string id);

  /// <summary>
  /// Clears all registrations. Call when starting a new session.
  /// </summary>
  void Clear();

  /// <summary>
  /// Number of registered actors.
  /// </summary>
  int Count { get; }
}
