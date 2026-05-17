using BepInEx.Configuration;
using ErenshorLogs.Events;
using UnityEngine;

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
  /// Bind the WebSocket server to all network interfaces instead of loopback.
  /// </summary>
  public ConfigEntry<bool> AllowLanConnections { get; }

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
  /// Enable automatic session detection based on combat events.
  /// When false, sessions only start/stop via manual hotkeys.
  /// </summary>
  public ConfigEntry<bool> AutoSessionDetection { get; }

  /// <summary>
  /// Inactivity timeout in seconds before automatically ending sessions.
  /// Only applies when AutoSessionDetection is enabled.
  /// Sessions end when this duration passes with no combat events.
  /// </summary>
  public ConfigEntry<float> SessionInactivityTimeout { get; }

  /// <summary>
  /// Event types that can START a new session (comma-separated).
  /// Only applies when AutoSessionDetection is enabled.
  /// Uses camelCase event type names.
  /// </summary>
  public ConfigEntry<string> SessionStartEvents { get; }

  /// <summary>
  /// Event types that EXTEND an active session (reset inactivity timer).
  /// Uses camelCase event type names.
  /// </summary>
  public ConfigEntry<string> SessionKeepAliveEvents { get; }

  /// <summary>
  /// Hotkey to manually start a combat session.
  /// Ends any existing session first (manual or automatic).
  /// Set to the same key as ManualSessionStopKey for toggle behavior.
  /// </summary>
  public ConfigEntry<KeyCode> ManualSessionStartKey { get; }

  /// <summary>
  /// Hotkey to manually stop a combat session.
  /// Only ends manually-started sessions (not automatic sessions).
  /// Set to the same key as ManualSessionStartKey for toggle behavior.
  /// </summary>
  public ConfigEntry<KeyCode> ManualSessionStopKey { get; }

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

    AllowLanConnections = config.Bind(
      "Server",
      "AllowLanConnections",
      false,
      "Allow other devices on the LAN to connect. Disabled by default so the WebSocket server only listens on loopback."
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

    // Build dynamic event type list from enum
    var availableEventTypes = System
      .Enum.GetValues(typeof(EventType))
      .Cast<EventType>()
      .Where(e => e != EventType.CombatStart && e != EventType.CombatEnd)
      .Select(e => ToCamelCase(e.ToString()))
      .OrderBy(e => e)
      .ToArray();

    var eventTypeList = string.Join(", ", availableEventTypes);

    // Session configuration
    AutoSessionDetection = config.Bind(
      "Session",
      "AutoSessionDetection",
      true,
      "Enable automatic session detection. When false, use hotkeys to control sessions."
    );

    SessionInactivityTimeout = config.Bind(
      "Session",
      "SessionInactivityTimeout",
      5.0f,
      "Seconds of inactivity before auto-ending sessions (only for automatic sessions)"
    );

    SessionStartEvents = config.Bind(
      "Session",
      "SessionStartEvents",
      "damagePhysical,damageMagic,damageDot,damageSkill,damageSpell,damageMelee,damageProc,damagePet,damageReflect",
      "Event types that can START a new session (comma-separated).\n"
        + $"Available: {eventTypeList}"
    );

    SessionKeepAliveEvents = config.Bind(
      "Session",
      "SessionKeepAliveEvents",
      "damagePhysical,damageMagic,damageDot,damageSkill,damageSpell,damageMelee,damageProc,damagePet,damageReflect",
      "Event types that EXTEND an active session (reset inactivity timer).\n"
        + $"Available: {eventTypeList}"
    );

    ManualSessionStartKey = config.Bind(
      "Session",
      "ManualSessionStartKey",
      KeyCode.F9,
      "Hotkey to start a manual session (ends existing session first)"
    );

    ManualSessionStopKey = config.Bind(
      "Session",
      "ManualSessionStopKey",
      KeyCode.F10,
      "Hotkey to stop a manual session (set to same as Start for toggle mode)"
    );
  }

  private static string ToCamelCase(string pascalCase)
  {
    if (string.IsNullOrEmpty(pascalCase))
      return pascalCase;
    return char.ToLowerInvariant(pascalCase[0]) + pascalCase.Substring(1);
  }
}
