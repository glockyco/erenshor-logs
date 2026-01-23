using BepInEx;
using BepInEx.Logging;
using ErenshorLogs.Broadcast;
using ErenshorLogs.Config;
using ErenshorLogs.Context;
using ErenshorLogs.Events;
using ErenshorLogs.Hooks;
using ErenshorLogs.Registry;
using ErenshorLogs.Server;
using ErenshorLogs.Session;
using HarmonyLib;
using Microsoft.Extensions.DependencyInjection;

namespace ErenshorLogs;

/// <summary>
/// Main plugin entry point for Erenshor Logs.
/// </summary>
[BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
public sealed class Plugin : BaseUnityPlugin
{
  internal static ManualLogSource Log { get; private set; } = null!;

  private ServiceProvider? _services;
  private Harmony? _harmony;
  private ICombatRelevanceChecker? _relevanceChecker;
  private ISessionManager? _sessionManager;
  private IWebSocketServer? _server;
  private ICombatEventBroadcaster? _broadcaster;

  private void Awake()
  {
    Log = Logger;

    _services = ConfigureServices();
    _harmony = new Harmony(PluginInfo.GUID);

    ConfigureDamagePatches();
    ConfigureSessionPatch();
    ConfigureWebSocket();
    _harmony.PatchAll();

    Log.LogInfo($"{PluginInfo.Name} v{PluginInfo.Version} loaded");
  }

  private void Update()
  {
    // Tick the broadcaster for periodic event batching
    if (_broadcaster != null)
    {
      _broadcaster.Tick(UnityEngine.Time.deltaTime);
    }

    // Check for pending session timeout
    if (_sessionManager != null)
    {
      _sessionManager.CheckPendingSessionTimeout(UnityEngine.Time.time);
    }
  }

  private void ConfigureDamagePatches()
  {
    var services = _services!;
    var emitter = services.GetRequiredService<IEventEmitter>();
    var eventBuilder = services.GetRequiredService<ICombatEventBuilder>();
    var sessionManager = services.GetRequiredService<ISessionManager>();
    var config = services.GetRequiredService<ModConfig>();

    // Create and store relevance checker
    _relevanceChecker = new CombatRelevanceCheckerAdapter();

    // Create and wire up EffectTracker for DoT/HoT attribution
    var effectTracker = new EffectTracker();

    DamageMePatch.Emitter = emitter;
    DamageMePatch.EventBuilder = eventBuilder;
    DamageMePatch.LogDebug = msg => Logger.LogDebug(msg);
    DamageMePatch.RelevanceChecker = _relevanceChecker;
    DamageMePatch.SessionManager = sessionManager;
    DamageMePatch.CaptureDebugForUnknown = config.CaptureDebugForUnknown.Value;
    DamageMePatch.CaptureDebugForAll = config.CaptureDebugForAll.Value;
    DamageMePatch.EffectTracker = effectTracker;

    MagicDamageMePatch.Emitter = emitter;
    MagicDamageMePatch.EventBuilder = eventBuilder;
    MagicDamageMePatch.LogDebug = msg => Logger.LogDebug(msg);
    MagicDamageMePatch.RelevanceChecker = _relevanceChecker;
    MagicDamageMePatch.SessionManager = sessionManager;
    MagicDamageMePatch.CaptureDebugForUnknown = config.CaptureDebugForUnknown.Value;
    MagicDamageMePatch.CaptureDebugForAll = config.CaptureDebugForAll.Value;

    BleedDamageMePatch.Emitter = emitter;
    BleedDamageMePatch.EventBuilder = eventBuilder;
    BleedDamageMePatch.LogDebug = msg => Logger.LogDebug(msg);
    BleedDamageMePatch.RelevanceChecker = _relevanceChecker;
    BleedDamageMePatch.SessionManager = sessionManager;
    BleedDamageMePatch.CaptureDebugForUnknown = config.CaptureDebugForUnknown.Value;
    BleedDamageMePatch.CaptureDebugForAll = config.CaptureDebugForAll.Value;
    BleedDamageMePatch.EffectTracker = effectTracker;

    EnvironmentalDamageMePatch.Emitter = emitter;
    EnvironmentalDamageMePatch.EventBuilder = eventBuilder;
    EnvironmentalDamageMePatch.LogDebug = msg => Logger.LogDebug(msg);
    EnvironmentalDamageMePatch.RelevanceChecker = _relevanceChecker;
    EnvironmentalDamageMePatch.SessionManager = sessionManager;
    EnvironmentalDamageMePatch.CaptureDebugForUnknown = config.CaptureDebugForUnknown.Value;
    EnvironmentalDamageMePatch.CaptureDebugForAll = config.CaptureDebugForAll.Value;

    // Wire EffectTracker to effect lifecycle hooks
    AddStatusEffectPatch.Tracker = effectTracker;
    RemoveStatusEffectPatch.Tracker = effectTracker;
  }

  private void ConfigureSessionPatch()
  {
    var services = _services!;
    var sessionManager = services.GetRequiredService<ISessionManager>();

    // Store reference for Update() timeout checks
    _sessionManager = sessionManager;

    CheckForTrueCombatPatch.SessionManager = sessionManager;

    // Clear relevance cache when combat sessions end
    sessionManager.SessionEnded += _ => _relevanceChecker?.ClearCache();
  }

  private void ConfigureWebSocket()
  {
    var services = _services!;
    var config = services.GetRequiredService<ModConfig>();
    var eventEmitter = services.GetRequiredService<IEventEmitter>();
    var sessionManager = services.GetRequiredService<ISessionManager>();

    // Create WebSocket server
    _server = new WebSocketServer(config, Logger);
    _server.Start();

    // Create broadcaster
    _broadcaster = new CombatEventBroadcaster(
      eventEmitter,
      sessionManager,
      _server,
      config,
      PluginInfo.Version,
      log: msg => Logger.LogDebug(msg)
    );

    // Send handshake when clients connect
    _server.ClientConnected += () =>
    {
      Logger.LogDebug("Client connected, sending handshake");
      _broadcaster.SendHandshakeToNewClient();
    };
  }

  private ServiceProvider ConfigureServices()
  {
    var services = new ServiceCollection();

    // Configuration
    services.AddSingleton(new ModConfig(Config));

    // Event system
    services.AddSingleton<IEventEmitter>(new EventEmitter(msg => Logger.LogError(msg)));

    // Actor registry
    services.AddSingleton<IActorTypeResolver, ActorTypeResolver>();
    services.AddSingleton<IActorDataExtractor, ActorDataExtractor>();
    services.AddSingleton<IActorRegistry>(sp => new ActorRegistryAdapter(
      sp.GetRequiredService<IActorTypeResolver>(),
      sp.GetRequiredService<IActorDataExtractor>(),
      msg => Logger.LogError(msg)
    ));

    // Combat event building
    services.AddSingleton<ICombatEventBuilder>(sp => new CombatEventBuilderAdapter(
      sp.GetRequiredService<IActorRegistry>()
    ));

    // Session management
    services.AddSingleton<IGameVersionProvider, GameVersionProvider>();
    services.AddSingleton<ISessionManager>(sp => new SessionManager(
      sp.GetRequiredService<IEventEmitter>(),
      sp.GetRequiredService<IGameVersionProvider>(),
      PluginInfo.Version,
      log: (level, msg) =>
      {
        switch (level)
        {
          case Logging.LogLevel.Debug:
            Logger.LogDebug(msg);
            break;
          case Logging.LogLevel.Info:
            Logger.LogInfo(msg);
            break;
          case Logging.LogLevel.Warning:
            Logger.LogWarning(msg);
            break;
          case Logging.LogLevel.Error:
            Logger.LogError(msg);
            break;
        }
      }
    ));

    return services.BuildServiceProvider();
  }

  private void OnDestroy()
  {
    _broadcaster?.Dispose();
    _server?.Dispose();
    _harmony?.UnpatchSelf();
    _services?.Dispose();
  }
}
