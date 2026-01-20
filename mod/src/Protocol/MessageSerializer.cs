using System.Text.Json;
using ErenshorLogs.Json;

namespace ErenshorLogs.Protocol;

/// <summary>
/// JSON serialization for WebSocket protocol messages.
/// Uses System.Text.Json with camelCase naming for JavaScript compatibility.
/// </summary>
public static class MessageSerializer
{
  /// <summary>
  /// Serializes a message to compact JSON.
  /// </summary>
  public static string Serialize<T>(T message) =>
    JsonSerializer.Serialize(message, JsonContext.Options);

  /// <summary>
  /// Deserializes JSON to a message object.
  /// </summary>
  public static T? Deserialize<T>(string json) =>
    JsonSerializer.Deserialize<T>(json, JsonContext.Options);
}
