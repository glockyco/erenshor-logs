using ErenshorLogs.Events;
using ErenshorLogs.Registry;
using Xunit;

namespace ErenshorLogs.Tests.Registry;

public class ActorRegistryTests
{
  /// <summary>
  /// Simple mock character for testing. In production this would be the game's Character type.
  /// </summary>
  private sealed class MockCharacter
  {
    public int InstanceId { get; init; }
    public string Name { get; init; } = "Unknown";
    public ActorType Type { get; init; } = ActorType.Npc;
    public string? ClassName { get; init; }
    public int? Level { get; init; }
    public int? MasterInstanceId { get; init; }
    public MockCharacter? Master { get; init; }
  }

  private static ActorRegistry<MockCharacter> CreateRegistry(Action<string>? logError = null)
  {
    return new ActorRegistry<MockCharacter>(
      c => c.InstanceId,
      c => c.Type,
      c => new ActorData
      {
        Name = c.Name,
        ClassName = c.ClassName,
        Level = c.Level,
        MasterInstanceId = c.MasterInstanceId,
      },
      logError
    );
  }

  #region GetOrCreate Tests

  [Fact]
  public void GetOrCreate_WithNull_ReturnsNull()
  {
    var registry = CreateRegistry();

    var result = registry.GetOrCreate(null);

    Assert.Null(result);
  }

  [Fact]
  public void GetOrCreate_WithNewCharacter_ReturnsActorRef()
  {
    var registry = CreateRegistry();
    var character = new MockCharacter
    {
      InstanceId = 100,
      Name = "Goblin",
      Type = ActorType.Npc,
      Level = 5,
    };

    var result = registry.GetOrCreate(character);

    Assert.NotNull(result);
    Assert.Equal("Goblin", result.Name);
    Assert.Equal(ActorType.Npc, result.Type);
    Assert.Equal(5, result.Level);
  }

  [Fact]
  public void GetOrCreate_SameCharacterTwice_ReturnsSameActorRef()
  {
    var registry = CreateRegistry();
    var character = new MockCharacter { InstanceId = 100, Name = "Goblin" };

    var first = registry.GetOrCreate(character);
    var second = registry.GetOrCreate(character);

    Assert.Same(first, second);
  }

  [Fact]
  public void GetOrCreate_DifferentCharacters_ReturnsDifferentActorRefs()
  {
    var registry = CreateRegistry();
    var goblin = new MockCharacter { InstanceId = 100, Name = "Goblin" };
    var orc = new MockCharacter { InstanceId = 101, Name = "Orc" };

    var goblinRef = registry.GetOrCreate(goblin);
    var orcRef = registry.GetOrCreate(orc);

    Assert.NotSame(goblinRef, orcRef);
    Assert.NotEqual(goblinRef!.Id, orcRef!.Id);
  }

  #endregion

  #region Player ID Tests

  [Fact]
  public void GetOrCreate_Player_AlwaysReturnsPlayerZeroId()
  {
    var registry = CreateRegistry();
    var player = new MockCharacter
    {
      InstanceId = 999,
      Name = "Valdris",
      Type = ActorType.Player,
      ClassName = "Duelist",
      Level = 35,
    };

    var result = registry.GetOrCreate(player);

    Assert.NotNull(result);
    Assert.Equal("player:0", result.Id);
    Assert.Equal(ActorType.Player, result.Type);
    Assert.Equal("Duelist", result.Class);
  }

  [Fact]
  public void GetOrCreate_PlayerRegisteredTwice_StillReturnsPlayerZero()
  {
    var registry = CreateRegistry();
    var player = new MockCharacter
    {
      InstanceId = 999,
      Type = ActorType.Player,
      Name = "Valdris",
    };

    // Register player, then clear, then register again
    var first = registry.GetOrCreate(player);
    registry.Clear();
    var second = registry.GetOrCreate(player);

    Assert.Equal("player:0", first!.Id);
    Assert.Equal("player:0", second!.Id);
  }

  #endregion

  #region ID Generation Tests

  [Fact]
  public void GetOrCreate_Npc_GeneratesSequentialId()
  {
    var registry = CreateRegistry();
    var npc1 = new MockCharacter
    {
      InstanceId = 1,
      Name = "Goblin",
      Type = ActorType.Npc,
    };
    var npc2 = new MockCharacter
    {
      InstanceId = 2,
      Name = "Orc",
      Type = ActorType.Npc,
    };
    var npc3 = new MockCharacter
    {
      InstanceId = 3,
      Name = "Troll",
      Type = ActorType.Npc,
    };

    var ref1 = registry.GetOrCreate(npc1);
    var ref2 = registry.GetOrCreate(npc2);
    var ref3 = registry.GetOrCreate(npc3);

    Assert.Equal("npc:1", ref1!.Id);
    Assert.Equal("npc:2", ref2!.Id);
    Assert.Equal("npc:3", ref3!.Id);
  }

  [Fact]
  public void GetOrCreate_SimPlayer_GeneratesSimPlayerPrefixedId()
  {
    var registry = CreateRegistry();
    var sim = new MockCharacter
    {
      InstanceId = 200,
      Name = "Aeryn",
      Type = ActorType.SimPlayer,
      ClassName = "Paladin",
      Level = 30,
    };

    var result = registry.GetOrCreate(sim);

    Assert.NotNull(result);
    Assert.Equal("sim_player:1", result.Id);
    Assert.Equal(ActorType.SimPlayer, result.Type);
    Assert.Equal("Paladin", result.Class);
    Assert.Equal(30, result.Level);
  }

  [Fact]
  public void GetOrCreate_MixedTypes_SharesIdCounter()
  {
    var registry = CreateRegistry();
    var npc = new MockCharacter
    {
      InstanceId = 1,
      Type = ActorType.Npc,
      Name = "Goblin",
    };
    var sim = new MockCharacter
    {
      InstanceId = 2,
      Type = ActorType.SimPlayer,
      Name = "Aeryn",
    };
    var pet = new MockCharacter
    {
      InstanceId = 3,
      Type = ActorType.Pet,
      Name = "Wolf",
    };

    var npcRef = registry.GetOrCreate(npc);
    var simRef = registry.GetOrCreate(sim);
    var petRef = registry.GetOrCreate(pet);

    // IDs are sequential across all types (except player)
    Assert.Equal("npc:1", npcRef!.Id);
    Assert.Equal("sim_player:2", simRef!.Id);
    Assert.Equal("pet:3", petRef!.Id);
  }

  #endregion

  #region Pet and Master Tests

  [Fact]
  public void GetOrCreate_Pet_WithMasterRegisteredFirst_SetsMasterId()
  {
    var registry = CreateRegistry();
    var player = new MockCharacter
    {
      InstanceId = 100,
      Type = ActorType.Player,
      Name = "Valdris",
    };
    var pet = new MockCharacter
    {
      InstanceId = 200,
      Type = ActorType.Pet,
      Name = "Wolf",
      MasterInstanceId = 100,
    };

    // Register master first
    registry.GetOrCreate(player);
    var petRef = registry.GetOrCreate(pet);

    Assert.NotNull(petRef);
    Assert.Equal("player:0", petRef.MasterId);
    Assert.Equal(ActorType.Pet, petRef.Type);
  }

  [Fact]
  public void GetOrCreate_Pet_WithSimPlayerMaster_SetsMasterIdCorrectly()
  {
    var registry = CreateRegistry();
    var simPlayer = new MockCharacter
    {
      InstanceId = 100,
      Type = ActorType.SimPlayer,
      Name = "Aeryn",
      ClassName = "Druid",
    };
    var pet = new MockCharacter
    {
      InstanceId = 200,
      Type = ActorType.Pet,
      Name = "Bear",
      MasterInstanceId = 100,
    };

    registry.GetOrCreate(simPlayer);
    var petRef = registry.GetOrCreate(pet);

    Assert.Equal("sim_player:1", petRef!.MasterId);
  }

  [Fact]
  public void GetOrCreate_Pet_WithMasterObjectRegistersMasterFirst()
  {
    var loggedWarnings = new List<string>();
    var simPlayer = new MockCharacter
    {
      InstanceId = 100,
      Type = ActorType.SimPlayer,
      Name = "Leliril",
      ClassName = "Druid",
    };
    var pet = new MockCharacter
    {
      InstanceId = 200,
      Type = ActorType.Pet,
      Name = "Leliril's pet",
      MasterInstanceId = 100,
      Master = simPlayer,
    };
    var registry = new ActorRegistry<MockCharacter>(
      c => c.InstanceId,
      c => c.Type,
      c => new ActorData
      {
        Name = c.Name,
        ClassName = c.ClassName,
        Level = c.Level,
        MasterInstanceId = c.MasterInstanceId,
      },
      msg => loggedWarnings.Add(msg),
      c => c.Master
    );

    var petRef = registry.GetOrCreate(pet);

    Assert.Equal("sim_player:1", petRef!.MasterId);
    Assert.Equal(2, registry.Count);
    Assert.Empty(loggedWarnings);
  }

  [Fact]
  public void GetOrCreate_Pet_WithMasterNotRegistered_LogsWarningAndOmitsMasterId()
  {
    var loggedWarnings = new List<string>();
    var registry = CreateRegistry(msg => loggedWarnings.Add(msg));

    var pet = new MockCharacter
    {
      InstanceId = 200,
      Type = ActorType.Pet,
      Name = "Wolf",
      MasterInstanceId = 100, // Master not registered
    };

    var petRef = registry.GetOrCreate(pet);

    Assert.NotNull(petRef);
    Assert.Null(petRef.MasterId);
    Assert.Single(loggedWarnings);
    Assert.Contains("Wolf", loggedWarnings[0]);
    Assert.Contains("100", loggedWarnings[0]);
  }

  [Fact]
  public void GetOrCreate_Pet_WithoutMaster_HasNullMasterId()
  {
    var registry = CreateRegistry();
    var pet = new MockCharacter
    {
      InstanceId = 200,
      Type = ActorType.Pet,
      Name = "Wolf",
      MasterInstanceId = null, // No master
    };

    var petRef = registry.GetOrCreate(pet);

    Assert.NotNull(petRef);
    Assert.Null(petRef.MasterId);
  }

  #endregion

  #region Class Inclusion Tests

  [Fact]
  public void GetOrCreate_Player_IncludesClass()
  {
    var registry = CreateRegistry();
    var player = new MockCharacter
    {
      InstanceId = 1,
      Type = ActorType.Player,
      Name = "Valdris",
      ClassName = "Duelist",
    };

    var result = registry.GetOrCreate(player);

    Assert.Equal("Duelist", result!.Class);
  }

  [Fact]
  public void GetOrCreate_SimPlayer_IncludesClass()
  {
    var registry = CreateRegistry();
    var sim = new MockCharacter
    {
      InstanceId = 1,
      Type = ActorType.SimPlayer,
      Name = "Aeryn",
      ClassName = "Paladin",
    };

    var result = registry.GetOrCreate(sim);

    Assert.Equal("Paladin", result!.Class);
  }

  [Fact]
  public void GetOrCreate_Npc_ExcludesClass()
  {
    var registry = CreateRegistry();
    var npc = new MockCharacter
    {
      InstanceId = 1,
      Type = ActorType.Npc,
      Name = "Goblin",
      ClassName = "SomeInternalClass", // NPCs might have class data but we don't expose it
    };

    var result = registry.GetOrCreate(npc);

    Assert.Null(result!.Class);
  }

  [Fact]
  public void GetOrCreate_Pet_ExcludesClass()
  {
    var registry = CreateRegistry();
    var pet = new MockCharacter
    {
      InstanceId = 1,
      Type = ActorType.Pet,
      Name = "Wolf",
      ClassName = "SomeInternalClass",
    };

    var result = registry.GetOrCreate(pet);

    Assert.Null(result!.Class);
  }

  #endregion

  #region GetById Tests

  [Fact]
  public void GetById_ExistingId_ReturnsActorRef()
  {
    var registry = CreateRegistry();
    var character = new MockCharacter
    {
      InstanceId = 100,
      Name = "Goblin",
      Type = ActorType.Npc,
    };
    var created = registry.GetOrCreate(character);

    var result = registry.GetById(created!.Id);

    Assert.Same(created, result);
  }

  [Fact]
  public void GetById_NonExistentId_ReturnsNull()
  {
    var registry = CreateRegistry();

    var result = registry.GetById("npc:999");

    Assert.Null(result);
  }

  [Fact]
  public void GetById_AfterClear_ReturnsNull()
  {
    var registry = CreateRegistry();
    var character = new MockCharacter
    {
      InstanceId = 100,
      Name = "Goblin",
      Type = ActorType.Npc,
    };
    var created = registry.GetOrCreate(character);
    var stableId = created!.Id;

    registry.Clear();
    var result = registry.GetById(stableId);

    Assert.Null(result);
  }

  #endregion

  #region Clear Tests

  [Fact]
  public void Clear_RemovesAllActors()
  {
    var registry = CreateRegistry();
    registry.GetOrCreate(new MockCharacter { InstanceId = 1, Name = "A" });
    registry.GetOrCreate(new MockCharacter { InstanceId = 2, Name = "B" });
    registry.GetOrCreate(new MockCharacter { InstanceId = 3, Name = "C" });

    registry.Clear();

    Assert.Equal(0, registry.Count);
  }

  [Fact]
  public void Clear_ResetsIdCounter()
  {
    var registry = CreateRegistry();
    registry.GetOrCreate(
      new MockCharacter
      {
        InstanceId = 1,
        Name = "Goblin",
        Type = ActorType.Npc,
      }
    );
    registry.GetOrCreate(
      new MockCharacter
      {
        InstanceId = 2,
        Name = "Orc",
        Type = ActorType.Npc,
      }
    );

    registry.Clear();

    var newNpc = new MockCharacter
    {
      InstanceId = 3,
      Name = "Troll",
      Type = ActorType.Npc,
    };
    var result = registry.GetOrCreate(newNpc);

    // After clear, ID counter resets to 1
    Assert.Equal("npc:1", result!.Id);
  }

  [Fact]
  public void Clear_AllowsReregistrationOfSameCharacter()
  {
    var registry = CreateRegistry();
    var character = new MockCharacter
    {
      InstanceId = 100,
      Name = "Goblin",
      Type = ActorType.Npc,
    };

    var first = registry.GetOrCreate(character);
    registry.Clear();
    var second = registry.GetOrCreate(character);

    // Same stable ID after clear (counter reset)
    Assert.Equal(first!.Id, second!.Id);
    // But different object instances (re-created)
    Assert.NotSame(first, second);
  }

  #endregion

  #region Count Tests

  [Fact]
  public void Count_InitiallyZero()
  {
    var registry = CreateRegistry();

    Assert.Equal(0, registry.Count);
  }

  [Fact]
  public void Count_IncrementsOnRegistration()
  {
    var registry = CreateRegistry();

    registry.GetOrCreate(new MockCharacter { InstanceId = 1, Name = "A" });
    Assert.Equal(1, registry.Count);

    registry.GetOrCreate(new MockCharacter { InstanceId = 2, Name = "B" });
    Assert.Equal(2, registry.Count);
  }

  [Fact]
  public void Count_DoesNotIncrementForSameCharacter()
  {
    var registry = CreateRegistry();
    var character = new MockCharacter { InstanceId = 100, Name = "Goblin" };

    registry.GetOrCreate(character);
    registry.GetOrCreate(character);
    registry.GetOrCreate(character);

    Assert.Equal(1, registry.Count);
  }

  #endregion

  #region Thread Safety Tests

  [Fact]
  public void ConcurrentRegistrations_NoDuplicateIds()
  {
    var registry = CreateRegistry();
    var characters = Enumerable
      .Range(1, 100)
      .Select(i => new MockCharacter
      {
        InstanceId = i,
        Name = $"NPC{i}",
        Type = ActorType.Npc,
      })
      .ToList();

    // Register all characters concurrently
    var results = new System.Collections.Concurrent.ConcurrentBag<ActorRef>();
    Parallel.ForEach(
      characters,
      character =>
      {
        var actorRef = registry.GetOrCreate(character);
        if (actorRef != null)
          results.Add(actorRef);
      }
    );

    // All should be registered
    Assert.Equal(100, results.Count);
    Assert.Equal(100, registry.Count);

    // All IDs should be unique
    var ids = results.Select(r => r.Id).ToHashSet();
    Assert.Equal(100, ids.Count);
  }

  [Fact]
  public void ConcurrentGetOrCreate_SameCharacter_ReturnsSameInstance()
  {
    var registry = CreateRegistry();
    var character = new MockCharacter
    {
      InstanceId = 100,
      Name = "Goblin",
      Type = ActorType.Npc,
    };

    var results = new System.Collections.Concurrent.ConcurrentBag<ActorRef>();

    // Call GetOrCreate many times concurrently for the same character
    Parallel.For(
      0,
      100,
      _ =>
      {
        var actorRef = registry.GetOrCreate(character);
        if (actorRef != null)
          results.Add(actorRef);
      }
    );

    // All should return the same instance
    Assert.Equal(100, results.Count);
    Assert.Single(results.Distinct());
    Assert.Equal(1, registry.Count);
  }

  #endregion
}
