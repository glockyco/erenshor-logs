using BepInEx.Bootstrap;
using ErenshorLogs.Logging;
using UnityEngine;

namespace ErenshorLogs.Config;

internal static class BepInExManagerHiding
{
  public static void Enforce(ModLog log)
  {
    var managerObject = Chainloader.ManagerObject;
    if (managerObject == null)
    {
      return;
    }

    if (!EnsureHiddenFlags(managerObject.hideFlags, out var updatedFlags))
    {
      return;
    }

    managerObject.hideFlags = updatedFlags;
    log.Debug("Enabled BepInEx manager object hiding");
  }

  internal static bool EnsureHiddenFlags(HideFlags currentFlags, out HideFlags updatedFlags)
  {
    updatedFlags = currentFlags | HideFlags.HideAndDontSave;
    return updatedFlags != currentFlags;
  }
}
