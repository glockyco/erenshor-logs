using BepInEx.Configuration;

namespace ErenshorLogs.Config;

/// <summary>
/// BepInEx configuration for the Erenshor Logs mod.
/// </summary>
public class ModConfig
{
  /// <summary>
  /// WebSocket server port. Clients connect to ws://localhost:{port}
  /// </summary>
  public ConfigEntry<int> Port { get; }

  /// <summary>
  /// Interval in milliseconds between event broadcasts to clients.
  /// </summary>
  public ConfigEntry<int> BroadcastInterval { get; }

  /// <summary>
  /// Capture detailed debug info for Unknown ability attributions.
  /// Includes method name, parameters, stack trace, and context state.
  /// Always logged at Debug level to BepInEx log.
  /// </summary>
  public ConfigEntry<bool> CaptureDebugForUnknown { get; }

  /// <summary>
  /// Capture detailed debug info for ALL ability attributions.
  /// Useful for debugging misattributions or verifying context flow.
  /// WARNING: Adds significant data overhead.
  /// </summary>
  public ConfigEntry<bool> CaptureDebugForAll { get; }

  /// <summary>
  /// Creates a new ModConfig using the provided BepInEx config file.
  /// </summary>
  /// <param name="config">BepInEx configuration file.</param>
  public ModConfig(ConfigFile config)
  {
    Port = config.Bind(
      "Server",
      "Port",
      38729,
      "WebSocket server port. Clients connect to ws://localhost:{port}"
    );

    BroadcastInterval = config.Bind(
      "Server",
      "BroadcastInterval",
      100,
      "Interval in milliseconds between event broadcasts to clients"
    );

    CaptureDebugForUnknown = config.Bind(
      "Debugging",
      "CaptureDebugForUnknown",
      true,
      "Capture detailed debug info for Unknown ability attributions. "
        + "Includes method name, parameters, stack trace, and context state. "
        + "Always logged at Debug level to BepInEx log."
    );

    CaptureDebugForAll = config.Bind(
      "Debugging",
      "CaptureDebugForAll",
      false,
      "Capture detailed debug info for ALL ability attributions. "
        + "Useful for debugging misattributions or verifying context flow. "
        + "WARNING: Adds significant data overhead."
    );
  }
}
