using BepInEx;
using BepInEx.Logging;
using ErenshorLogs.Broadcast;
using ErenshorLogs.Config;
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
  }

  private void ConfigureDamagePatches()
  {
    var services = _services!;
    var emitter = services.GetRequiredService<IEventEmitter>();
    var eventBuilder = services.GetRequiredService<ICombatEventBuilder>();

    // Create and store relevance checker
    _relevanceChecker = new CombatRelevanceCheckerAdapter();

    DamageMePatch.Emitter = emitter;
    DamageMePatch.EventBuilder = eventBuilder;
    DamageMePatch.LogDebug = msg => Logger.LogDebug(msg);
    DamageMePatch.RelevanceChecker = _relevanceChecker;

    MagicDamageMePatch.Emitter = emitter;
    MagicDamageMePatch.EventBuilder = eventBuilder;
    MagicDamageMePatch.LogDebug = msg => Logger.LogDebug(msg);
    MagicDamageMePatch.RelevanceChecker = _relevanceChecker;

    BleedDamageMePatch.Emitter = emitter;
    BleedDamageMePatch.EventBuilder = eventBuilder;
    BleedDamageMePatch.LogDebug = msg => Logger.LogDebug(msg);
    BleedDamageMePatch.RelevanceChecker = _relevanceChecker;

    EnvironmentalDamageMePatch.Emitter = emitter;
    EnvironmentalDamageMePatch.EventBuilder = eventBuilder;
    EnvironmentalDamageMePatch.LogDebug = msg => Logger.LogDebug(msg);
    EnvironmentalDamageMePatch.RelevanceChecker = _relevanceChecker;
  }

  private void ConfigureSessionPatch()
  {
    var services = _services!;
    var sessionManager = services.GetRequiredService<ISessionManager>();

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

    // Send handshake to clients when they connect
    // (This will be called on first Update() if clients are already connected)
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
