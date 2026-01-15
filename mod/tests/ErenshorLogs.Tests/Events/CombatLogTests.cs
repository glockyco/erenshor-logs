using System.Text.Json;
using ErenshorLogs.Events;
using ErenshorLogs.Json;
using Xunit;

namespace ErenshorLogs.Tests.Events;

public class CombatLogTests
{
  [Fact]
  public void CombatLog_Serializes_CompleteStructure()
  {
    var log = new CombatLog
    {
      Version = "1.0.0",
      Session = new SessionMetadata
      {
        Id = "550e8400-e29b-41d4-a716-446655440000",
        StartTime = 1704067200000,
        EndTime = 1704067260000,
        Duration = 60000,
        Player = new PlayerInfo
        {
          Name = "Valdris",
          Class = "Duelist",
          Level = 35,
        },
        GameVersion = "1.2.3",
        ModVersion = "0.1.0",
      },
      Summary = new SessionSummary
      {
        TotalDamageDealt = 150000,
        TotalDamageReceived = 25000,
        TotalHealing = 30000,
        Dps = 2500.0,
        Hps = 500.0,
        Deaths = 0,
        Kills = 5,
        CritRate = 0.15,
        HighestHit = 5000,
        DamageByType = new() { ["Physical"] = 100000, ["Magic"] = 50000 },
        TopAbilities =
        [
          new AbilitySummary
          {
            Name = "Backstab",
            Damage = 45000,
            Hits = 30,
          },
        ],
      },
      Events =
      [
        new CombatEvent
        {
          Id = "evt-1",
          Timestamp = 1704067200000,
          EventType = EventType.CombatStart,
        },
      ],
    };

    var json = JsonSerializer.Serialize(log, JsonContext.Options);

    Assert.Contains("\"version\":\"1.0.0\"", json);
    Assert.Contains("\"session\":", json);
    Assert.Contains("\"summary\":", json);
    Assert.Contains("\"events\":", json);
    Assert.Contains("\"totalDamageDealt\":150000", json);
  }
}
