using ErenshorLogs.Hooks;

namespace ErenshorLogs.Registry;

/// <summary>
/// Production implementation that extracts data from game Character objects.
/// </summary>
public sealed class ActorDataExtractor : IActorDataExtractor
{
  public ActorData Extract(Character character)
  {
    // Early validation - should never happen as we validate before calling, but be safe
    if (!character.IsValid())
    {
      return new ActorData
      {
        Name = "Unknown (Destroyed)",
        ClassName = null,
        Level = null,
        MasterInstanceId = null,
      };
    }

    var stats = character.MyStats;

    return new ActorData
    {
      // Prefer MyStats.MyName, fall back to transform name (safe), finally use "Unknown"
      Name = stats?.MyName ?? character.GetSafeTransformName() ?? "Unknown",
      // Safe navigation for optional properties
      ClassName = stats?.CharacterClass?.ClassName,
      Level = stats?.Level,
      // Use safe instance ID getter for master
      MasterInstanceId = character.Master.GetSafeInstanceID(),
    };
  }
}
