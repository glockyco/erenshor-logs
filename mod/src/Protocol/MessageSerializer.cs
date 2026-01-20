using ErenshorLogs.Json;
using Newtonsoft.Json;

namespace ErenshorLogs.Protocol;

/// <summary>
/// JSON serialization for WebSocket protocol messages.
/// Uses Newtonsoft.Json with camelCase naming for JavaScript compatibility.
/// </summary>
public static class MessageSerializer
{
  /// <summary>
  /// Serializes a message to compact JSON.
  /// </summary>
  public static string Serialize<T>(T message) =>
    JsonConvert.SerializeObject(message, JsonSettings.Default);

  /// <summary>
  /// Deserializes JSON to a message object.
  /// </summary>
  public static T? Deserialize<T>(string json) =>
    JsonConvert.DeserializeObject<T>(json, JsonSettings.Default);
}
