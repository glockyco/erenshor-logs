using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace ErenshorLogs;

/// <summary>
/// Main plugin entry point for Erenshor Logs.
/// </summary>
[BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
public sealed class Plugin : BaseUnityPlugin
{
  internal static ManualLogSource Log { get; private set; } = null!;

  private Harmony? _harmony;

  private void Awake()
  {
    Log = Logger;
    Log.LogInfo($"{PluginInfo.Name} v{PluginInfo.Version} loaded");

    _harmony = new Harmony(PluginInfo.GUID);
    // Patches will be added in future issues
  }

  private void OnDestroy()
  {
    _harmony?.UnpatchSelf();
  }
}
