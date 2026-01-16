namespace ErenshorLogs.Registry;

/// <summary>
/// Extracts actor data from a game Character object.
/// </summary>
public interface IActorDataExtractor
{
  /// <summary>
  /// Extracts data from a Character for creating an ActorRef.
  /// </summary>
  /// <param name="character">The game Character object.</param>
  /// <returns>Extracted actor data.</returns>
  ActorData Extract(Character character);
}
