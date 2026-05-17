using ErenshorLogs.Events;

namespace ErenshorLogs.Registry;

/// <summary>
/// Generic, thread-safe registry that maps characters to stable ActorRef instances.
/// Caches actor data at registration time for safety and consistency.
/// </summary>
/// <typeparam name="TCharacter">The character type (Character in production, mock type in tests).</typeparam>
public sealed class ActorRegistry<TCharacter>
  where TCharacter : class
{
  private readonly Func<TCharacter, int> _getInstanceId;
  private readonly Func<TCharacter, ActorType> _resolveType;
  private readonly Func<TCharacter, ActorData> _extractData;
  private readonly Action<string>? _logWarning;
  private readonly Func<TCharacter, TCharacter?>? _getMaster;

  private readonly object _lock = new();
  private readonly Dictionary<int, ActorRef> _byInstanceId = new();
  private readonly Dictionary<string, ActorRef> _byStableId = new();
  private int _nextId = 1; // Start at 1; player is always 0

  /// <summary>
  /// Creates a new ActorRegistry with the specified delegates for character access.
  /// </summary>
  /// <param name="getInstanceId">Function to get the unique instance ID from a character.</param>
  /// <param name="resolveType">Function to determine the ActorType of a character.</param>
  /// <param name="extractData">Function to extract ActorData from a character.</param>
  /// <param name="logWarning">Optional callback for warning logging.</param>
  /// <param name="getMaster">Optional function to get a pet's master character.</param>
  public ActorRegistry(
    Func<TCharacter, int> getInstanceId,
    Func<TCharacter, ActorType> resolveType,
    Func<TCharacter, ActorData> extractData,
    Action<string>? logWarning = null,
    Func<TCharacter, TCharacter?>? getMaster = null
  )
  {
    _getInstanceId = getInstanceId;
    _resolveType = resolveType;
    _extractData = extractData;
    _logWarning = logWarning;
    _getMaster = getMaster;
  }

  /// <summary>
  /// Number of registered actors.
  /// </summary>
  public int Count
  {
    get
    {
      lock (_lock)
      {
        return _byInstanceId.Count;
      }
    }
  }

  /// <summary>
  /// Gets or creates an ActorRef for a character.
  /// Returns the same ActorRef for the same character within a session.
  /// </summary>
  /// <param name="character">The character to register.</param>
  /// <returns>The ActorRef, or null if character is null.</returns>
  public ActorRef? GetOrCreate(TCharacter? character)
  {
    if (character == null)
      return null;

    var instanceId = _getInstanceId(character);

    lock (_lock)
    {
      // Return cached if already registered
      if (_byInstanceId.TryGetValue(instanceId, out var existing))
        return existing;

      // Resolve type and extract data
      var actorType = _resolveType(character);
      var data = _extractData(character);

      // Handle pet master relationship
      string? masterId = null;
      if (actorType == ActorType.Pet && data.MasterInstanceId.HasValue)
      {
        var masterRef = GetRegisteredOrCreateMaster(character, data.MasterInstanceId.Value);
        if (masterRef != null)
        {
          masterId = masterRef.Id;
        }
        else
        {
          // Master not registered yet and no master object was available.
          _logWarning?.Invoke(
            $"Pet '{data.Name}' registered before master (instance {data.MasterInstanceId})"
          );
        }
      }

      // Generate stable ID after resolving master so pet-first registration
      // still assigns the master its normal ID before assigning the pet ID.
      var stableId = GenerateStableId(actorType);

      // Only include class for Player and SimPlayer
      var includeClass = actorType is ActorType.Player or ActorType.SimPlayer;

      var actorRef = new ActorRef
      {
        Id = stableId,
        Name = data.Name,
        Type = actorType,
        Class = includeClass ? data.ClassName : null,
        Level = data.Level,
        MasterId = masterId,
      };

      // Store in both lookup dictionaries
      _byInstanceId[instanceId] = actorRef;
      _byStableId[stableId] = actorRef;

      return actorRef;
    }
  }

  /// <summary>
  /// Looks up a previously registered actor by stable ID.
  /// </summary>
  /// <param name="id">The stable actor ID (e.g., "player:0", "npc:5").</param>
  /// <returns>The ActorRef, or null if not found.</returns>
  public ActorRef? GetById(string id)
  {
    lock (_lock)
    {
      return _byStableId.TryGetValue(id, out var actorRef) ? actorRef : null;
    }
  }

  /// <summary>
  /// Clears all registrations. Call when starting a new session.
  /// </summary>
  public void Clear()
  {
    lock (_lock)
    {
      _byInstanceId.Clear();
      _byStableId.Clear();
      _nextId = 1;
    }
  }

  private ActorRef? GetRegisteredOrCreateMaster(TCharacter character, int masterInstanceId)
  {
    if (_byInstanceId.TryGetValue(masterInstanceId, out var masterRef))
    {
      return masterRef;
    }

    var master = _getMaster?.Invoke(character);
    if (master == null || _getInstanceId(master) != masterInstanceId)
    {
      return null;
    }

    return GetOrCreate(master);
  }

  private string GenerateStableId(ActorType type)
  {
    // Player always gets ID 0
    if (type == ActorType.Player)
      return "player:0";

    var prefix = type switch
    {
      ActorType.SimPlayer => "sim_player",
      ActorType.Pet => "pet",
      ActorType.Npc => "npc",
      _ => "unknown",
    };

    return $"{prefix}:{_nextId++}";
  }
}
