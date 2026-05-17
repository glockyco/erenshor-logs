using ErenshorLogs.Events;
using ErenshorLogs.Session;
using Xunit;

namespace ErenshorLogs.Tests.Session;

public sealed class SessionScopedRegistryResetTests
{
  [Fact]
  public void Wire_ClearsActorRegistryWhenSessionStarts()
  {
    var sessionManager = new FakeSessionManager();
    var clearCount = 0;

    SessionScopedRegistryReset.Wire(sessionManager, () => clearCount += 1);
    sessionManager.Start(new CombatSession("playtest-23258843", "2026.5.17.14"));

    Assert.Equal(1, clearCount);
  }

  private sealed class FakeSessionManager : ISessionManager
  {
    public CombatSession? CurrentSession { get; private set; }
    public event Action<CombatSession>? SessionStarted;
    public event Action<CombatSession>? SessionEnded;

    public void OnCombatEvent(EventType eventType, long eventTimestamp) { }

    public void CheckInactivityTimeout(float currentTime) { }

    public void StartManualSession() { }

    public void EndManualSession() => SessionEnded?.Invoke(new CombatSession("test", "test"));

    public void Start(CombatSession session)
    {
      CurrentSession = session;
      SessionStarted?.Invoke(session);
    }
  }
}
