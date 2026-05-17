using System.Collections.Concurrent;
using BepInEx.Logging;
using ErenshorLogs.Config;
using Fleck;

namespace ErenshorLogs.Server;

/// <summary>
/// Fleck-based WebSocket server implementation.
/// Handles client connections and broadcasts messages to all connected clients.
/// </summary>
public class WebSocketServer : IWebSocketServer
{
  private readonly ModConfig _config;
  private readonly ManualLogSource _logger;
  private readonly ConcurrentDictionary<Guid, IWebSocketConnection> _clients = new();

  private Fleck.WebSocketServer? _server;
  private bool _disposed;

  /// <inheritdoc />
  public int ClientCount => _clients.Count;

  /// <inheritdoc />
  public event Action? ClientConnected;

  /// <summary>
  /// Creates a new WebSocketServer.
  /// </summary>
  /// <param name="config">Mod configuration for port settings.</param>
  /// <param name="logger">BepInEx logger for diagnostics.</param>
  public WebSocketServer(ModConfig config, ManualLogSource logger)
  {
    _config = config;
    _logger = logger;

    ConfigureFleckLogging();
  }

  /// <inheritdoc />
  public void Start()
  {
    var port = _config.Port.Value;
    var allowLanConnections = _config.AllowLanConnections.Value;
    var location = WebSocketEndpoint.CreateBindLocation(port, allowLanConnections);

    try
    {
      _server = new Fleck.WebSocketServer(location);
      _server.Start(ConfigureSocket);
      _logger.LogInfo($"WebSocket server started on {location}");
      LogConnectionUrls(port, allowLanConnections);
    }
    catch (Exception ex)
    {
      _logger.LogError($"Failed to start WebSocket server on port {port}: {ex.Message}");
      _logger.LogDebug(ex.ToString());
    }
  }

  /// <summary>
  /// Logs all available connection URLs (localhost + network IPs).
  /// </summary>
  private void LogConnectionUrls(int port, bool allowLanConnections)
  {
    _logger.LogInfo($"→ Local:   ws://localhost:{port}");

    if (!allowLanConnections)
    {
      _logger.LogInfo("LAN access disabled. Enable AllowLanConnections to bind all interfaces.");
      return;
    }

    var networkIPs = NetworkUtils.GetLocalIPv4Addresses();

    if (networkIPs.Count > 0)
    {
      foreach (var ip in networkIPs)
      {
        var interfaceName = NetworkUtils.GetInterfaceName(ip);
        var interfaceLabel = interfaceName != null ? $" ({interfaceName})" : "";
        _logger.LogInfo($"→ Network: ws://{ip}:{port}{interfaceLabel}");
      }
    }
    else
    {
      _logger.LogWarning("No network adapters found. Server only accessible via localhost.");
    }
  }

  /// <inheritdoc />
  public void Stop()
  {
    if (_server == null)
      return;

    foreach (var client in _clients.Values)
    {
      try
      {
        client.Close();
      }
      catch
      {
        // Ignore errors during shutdown
      }
    }

    _clients.Clear();
    _server.Dispose();
    _server = null;

    _logger.LogInfo("WebSocket server stopped");
  }

  /// <inheritdoc />
  public void Broadcast(string message)
  {
    foreach (var (id, client) in _clients)
    {
      try
      {
        if (client.IsAvailable)
        {
          client.Send(message);
        }
        else
        {
          _clients.TryRemove(id, out _);
        }
      }
      catch (Exception ex)
      {
        _logger.LogWarning($"Failed to send to client {id}: {ex.Message}");
        _clients.TryRemove(id, out _);
      }
    }
  }

  /// <inheritdoc />
  public void Dispose()
  {
    if (_disposed)
      return;

    Stop();
    _disposed = true;
  }

  private void ConfigureSocket(IWebSocketConnection socket)
  {
    socket.OnOpen = () => OnClientConnected(socket);
    socket.OnClose = () => OnClientDisconnected(socket);
    socket.OnError = ex => OnClientError(socket, ex);
    socket.OnMessage = message => OnClientMessage(socket, message);
  }

  private void OnClientConnected(IWebSocketConnection socket)
  {
    _clients[socket.ConnectionInfo.Id] = socket;
    _logger.LogInfo(
      $"Client connected: {socket.ConnectionInfo.ClientIpAddress} (total: {ClientCount})"
    );

    // Fire event so Plugin can send handshake
    ClientConnected?.Invoke();
  }

  private void OnClientDisconnected(IWebSocketConnection socket)
  {
    _clients.TryRemove(socket.ConnectionInfo.Id, out _);
    _logger.LogInfo(
      $"Client disconnected: {socket.ConnectionInfo.ClientIpAddress} (total: {ClientCount})"
    );
  }

  private void OnClientError(IWebSocketConnection socket, Exception ex)
  {
    _logger.LogWarning($"Client error ({socket.ConnectionInfo.ClientIpAddress}): {ex.Message}");
    _clients.TryRemove(socket.ConnectionInfo.Id, out _);
  }

  private void OnClientMessage(IWebSocketConnection socket, string message)
  {
    // Inbound message handling not implemented in MVP.
    // For now, just log that we received something.
    _logger.LogDebug($"Received message from {socket.ConnectionInfo.ClientIpAddress}: {message}");
  }

  private void ConfigureFleckLogging()
  {
    // Configure Fleck to log through BepInEx
    FleckLog.LogAction = (level, message, ex) =>
    {
      // Only log warnings and errors from Fleck to avoid spam
      switch (level)
      {
        case Fleck.LogLevel.Warn:
          _logger.LogWarning($"[Fleck] {message}");
          break;
        case Fleck.LogLevel.Error:
          _logger.LogError($"[Fleck] {message}");
          if (ex != null)
            _logger.LogDebug(ex.ToString());
          break;
      }
    };
  }
}
