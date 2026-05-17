namespace ErenshorLogs.Server;

public static class WebSocketEndpoint
{
  public static string CreateBindLocation(int port, bool allowLanConnections)
  {
    var host = allowLanConnections ? "0.0.0.0" : "127.0.0.1";
    return $"ws://{host}:{port}";
  }
}
