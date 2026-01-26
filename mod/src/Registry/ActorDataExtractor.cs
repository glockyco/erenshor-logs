namespace ErenshorLogs.Registry;

/// <summary>
/// Production implementation that extracts data from game Character objects.
/// </summary>
public sealed class ActorDataExtractor : IActorDataExtractor
{
  public ActorData Extract(Character character)
  {
    var stats = character.MyStats;

    return new ActorData
    {
      Name = stats?.MyName ?? character.name, // Fallback to GameObject.name
      ClassName = stats?.CharacterClass?.ClassName,
      Level = stats?.Level,
      MasterInstanceId = character.Master != null ? character.Master.GetInstanceID() : null,
    };
  }
}
