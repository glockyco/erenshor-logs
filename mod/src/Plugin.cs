using BepInEx;
using BepInEx.Logging;
using ErenshorLogs.Events;
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

  private void Awake()
  {
    Log = Logger;

    _services = ConfigureServices();
    _harmony = new Harmony(PluginInfo.GUID);

    // Patches will be added in future issues
    // Example: DamagePatch.Emitter = _services.GetRequiredService<IEventEmitter>();

    Log.LogInfo($"{PluginInfo.Name} v{PluginInfo.Version} loaded");
  }

  private ServiceProvider ConfigureServices()
  {
    var services = new ServiceCollection();

    services.AddSingleton<IEventEmitter>(new EventEmitter(msg => Logger.LogError(msg)));

    return services.BuildServiceProvider();
  }

  private void OnDestroy()
  {
    _harmony?.UnpatchSelf();
    _services?.Dispose();
  }
}
