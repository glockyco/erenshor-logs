using ErenshorLogs.Hooks;
using Xunit;

namespace ErenshorLogs.Tests.Hooks;

public class CombatRelevanceCheckerTests
{
  private sealed class MockCharacter
  {
    public int InstanceId { get; init; }
    public string Name { get; init; } = "";
    public MockNpc? Npc { get; init; }
    public MockCharacter? Master { get; init; }
  }

  private sealed class MockNpc
  {
    public bool SimPlayer { get; init; }
    public bool InGroup { get; init; }
    public object? MyRaidSlot { get; init; }
    public List<AggroSlot> AggroTable { get; } = [];
  }

  private sealed class AggroSlot
  {
    public MockCharacter? Player { get; init; }
  }

  [Fact]
  public void IsRelevantCombat_WhenSimPlayerIsInGroupMembersButNpcFlagIsFalse_ReturnsTrue()
  {
    var sim = new MockCharacter
    {
      InstanceId = 1,
      Name = "Draven",
      Npc = new MockNpc { SimPlayer = true, InGroup = false },
    };
    var boss = new MockCharacter
    {
      InstanceId = 2,
      Name = "Raid Boss",
      Npc = new MockNpc(),
    };

    var checker = CreateChecker(groupMembers: [sim]);

    Assert.True(checker.IsRelevantCombat(sim, boss));
  }

  [Fact]
  public void IsRelevantCombat_WhenPetMasterIsInGroupMembersButNpcFlagIsFalse_ReturnsTrue()
  {
    var sim = new MockCharacter
    {
      InstanceId = 1,
      Name = "Draven",
      Npc = new MockNpc { SimPlayer = true, InGroup = false },
    };
    var pet = new MockCharacter
    {
      InstanceId = 2,
      Name = "Draven's Pet",
      Npc = new MockNpc(),
      Master = sim,
    };
    var boss = new MockCharacter
    {
      InstanceId = 3,
      Name = "Raid Boss",
      Npc = new MockNpc(),
    };

    var checker = CreateChecker(groupMembers: [sim]);

    Assert.True(checker.IsRelevantCombat(pet, boss));
  }

  [Fact]
  public void IsRelevantCombat_WhenNpcAggroContainsGroupMemberWithStaleFlag_ReturnsTrue()
  {
    var sim = new MockCharacter
    {
      InstanceId = 1,
      Name = "Draven",
      Npc = new MockNpc { SimPlayer = true, InGroup = false },
    };
    var bossNpc = new MockNpc();
    bossNpc.AggroTable.Add(new AggroSlot { Player = sim });
    var boss = new MockCharacter
    {
      InstanceId = 2,
      Name = "Raid Boss",
      Npc = bossNpc,
    };
    var add = new MockCharacter
    {
      InstanceId = 3,
      Name = "Raid Add",
      Npc = new MockNpc(),
    };

    var checker = CreateChecker(groupMembers: [sim]);

    Assert.True(checker.IsRelevantCombat(boss, add));
  }

  [Fact]
  public void IsRelevantCombat_WhenSimPlayerHasRaidSlot_ReturnsTrue()
  {
    var sim = new MockCharacter
    {
      InstanceId = 1,
      Name = "Raid Cleric",
      Npc = new MockNpc { SimPlayer = true, MyRaidSlot = new object() },
    };
    var boss = new MockCharacter
    {
      InstanceId = 2,
      Name = "Raid Boss",
      Npc = new MockNpc(),
    };
    var checker = CreateChecker();

    Assert.True(checker.IsRelevantCombat(sim, boss));
  }

  [Fact]
  public void IsRelevantCombat_WhenTargetIsRaidTarget_ReturnsTrue()
  {
    var target = new MockCharacter
    {
      InstanceId = 3,
      Name = "Raid Target",
      Npc = new MockNpc(),
    };
    var other = new MockCharacter
    {
      InstanceId = 4,
      Name = "Other Actor",
      Npc = new MockNpc(),
    };
    var checker = CreateChecker(raidTargets: [target]);

    Assert.True(checker.IsRelevantCombat(target, other));
  }

  [Fact]
  public void IsRelevantCombat_WhenTargetIsLooseAdd_ReturnsTrue()
  {
    var add = new MockCharacter
    {
      InstanceId = 5,
      Name = "Loose Add",
      Npc = new MockNpc(),
    };
    var other = new MockCharacter
    {
      InstanceId = 6,
      Name = "Other Actor",
      Npc = new MockNpc(),
    };
    var checker = CreateChecker(looseAdds: [add]);

    Assert.True(checker.IsRelevantCombat(other, add));
  }

  private static CombatRelevanceChecker<MockCharacter, MockNpc> CreateChecker(
    IReadOnlyList<MockCharacter>? groupMembers = null,
    IReadOnlyList<MockCharacter>? raidTargets = null,
    IReadOnlyList<MockCharacter>? looseAdds = null
  )
  {
    return new CombatRelevanceChecker<MockCharacter, MockNpc>(
      getInstanceId: c => c.InstanceId,
      getTransformName: c => c.Name,
      getMyNpc: c => c.Npc,
      isSimPlayer: npc => npc.SimPlayer,
      isInGroup: npc => npc.InGroup || npc.MyRaidSlot != null,
      getMaster: c => c.Master,
      getAttackingPlayer: () => [],
      getGroupMatesInCombat: () => [],
      getGroupTargets: () => null,
      getGroupMembers: () => groupMembers?.ToArray() ?? [],
      getRaidTargets: () => raidTargets?.ToArray() ?? [],
      getLooseAdds: () => looseAdds?.ToArray() ?? [],
      getAggroTablePlayers: npc =>
        npc.AggroTable.Select(slot => slot.Player).OfType<MockCharacter>().ToArray()
    );
  }
}
