using ErenshorLogs.Events;

namespace ErenshorLogs.Registry;

/// <summary>
/// Production adapter that implements IActorRegistry using the generic ActorRegistry
/// with game Character types. This is the class registered in the DI container.
/// </summary>
public sealed class ActorRegistryAdapter : IActorRegistry
{
  private readonly ActorRegistry<Character> _inner;

  /// <summary>
  /// Creates a new ActorRegistryAdapter with the specified dependencies.
  /// </summary>
  /// <param name="typeResolver">Resolver for determining actor types.</param>
  /// <param name="dataExtractor">Extractor for actor data.</param>
  /// <param name="logError">Optional callback for error logging.</param>
  public ActorRegistryAdapter(
    IActorTypeResolver typeResolver,
    IActorDataExtractor dataExtractor,
    Action<string>? logError = null
  )
  {
    _inner = new ActorRegistry<Character>(
      c => c.GetInstanceID(),
      typeResolver.Resolve,
      dataExtractor.Extract,
      logError
    );
  }

  /// <inheritdoc />
  public ActorRef? GetOrCreate(Character character) => _inner.GetOrCreate(character);

  /// <inheritdoc />
  public ActorRef? GetById(string id) => _inner.GetById(id);

  /// <inheritdoc />
  public void Clear() => _inner.Clear();

  /// <inheritdoc />
  public int Count => _inner.Count;
}
