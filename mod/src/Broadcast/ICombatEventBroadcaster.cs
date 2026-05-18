using ErenshorLogs.Hooks;

namespace ErenshorLogs.Broadcast;

/// <summary>
/// Manages periodic broadcasting of combat events to WebSocket clients.
/// </summary>
public interface ICombatEventBroadcaster : IDisposable
{
  /// <summary>
  /// Called each frame to handle timing and broadcast batched events.
  /// </summary>
  /// <param name="deltaTime">Time elapsed since last tick in seconds.</param>
  void Tick(float deltaTime);

  /// <summary>
  /// Sends handshake message to a newly connected client.
  /// </summary>
  void SendHandshakeToNewClient(Server.IWebSocketClient client);

  void SetPatchManifestResult(PatchManifestResult result);
}
