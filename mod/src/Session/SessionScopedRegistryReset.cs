namespace ErenshorLogs.Session;

public static class SessionScopedRegistryReset
{
  public static void Wire(ISessionManager sessionManager, Action clearRegistry)
  {
    sessionManager.SessionStarted += _ => clearRegistry();
  }
}
