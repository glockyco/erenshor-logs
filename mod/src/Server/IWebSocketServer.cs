namespace ErenshorLogs.Server;

/// <summary>
/// Connected WebSocket client that can receive targeted messages.
/// </summary>
public interface IWebSocketClient
{
  Guid Id { get; }
  string ClientIpAddress { get; }
  void Send(string message);
}

/// <summary>
/// WebSocket server for broadcasting combat events to connected clients.
/// </summary>
public interface IWebSocketServer : IDisposable
{
  /// <summary>
  /// Starts the WebSocket server and begins accepting connections.
  /// </summary>
  void Start();

  /// <summary>
  /// Stops the server and disconnects all clients.
  /// </summary>
  void Stop();

  /// <summary>
  /// Broadcasts a message to all connected clients.
  /// </summary>
  void Broadcast(string message);

  /// <summary>
  /// Number of currently connected clients.
  /// </summary>
  int ClientCount { get; }

  /// <summary>
  /// Fired when a new client connects to the server.
  /// </summary>
  event Action<IWebSocketClient>? ClientConnected;
}
