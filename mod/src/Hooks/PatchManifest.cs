using ErenshorLogs.Diagnostics;
using HarmonyLib;

namespace ErenshorLogs.Hooks;

public sealed record PatchManifestEntry(string Id, bool Required, Action Apply);

public sealed record PatchStatus(string Id, bool Required, string Status);

public sealed record PatchManifestResult(IReadOnlyList<PatchStatus> Statuses, string HealthStatus);

public static class PatchManifest
{
  public static PatchManifestResult Apply(
    IReadOnlyList<PatchManifestEntry> entries,
    IDiagnosticReporter diagnostics
  )
  {
    var statuses = new List<PatchStatus>(entries.Count);
    var hasRequiredFailure = false;
    var hasOptionalFailure = false;

    foreach (var entry in entries)
    {
      try
      {
        entry.Apply();
        statuses.Add(new PatchStatus(entry.Id, entry.Required, "active"));
      }
      catch (Exception ex)
      {
        statuses.Add(new PatchStatus(entry.Id, entry.Required, "failed"));
        diagnostics.Counters.HookWarnings += 1;
        if (entry.Required)
        {
          hasRequiredFailure = true;
          diagnostics.ReportRecoverable(
            code: "patch.failed",
            severity: "fatal",
            impact: "modFatal",
            component: "mod.hooks",
            operation: "patch",
            message: "A required combat capture patch failed to apply.",
            details: CreatePatchDetails(entry, ex)
          );
        }
        else
        {
          hasOptionalFailure = true;
          diagnostics.ReportRecoverable(
            code: "patch.failed",
            severity: "warning",
            impact: "captureDegraded",
            component: "mod.hooks",
            operation: "patch",
            message: "An optional combat capture patch failed to apply.",
            details: CreatePatchDetails(entry, ex)
          );
        }
      }
    }

    var healthStatus =
      hasRequiredFailure ? "fatal"
      : hasOptionalFailure ? "degraded"
      : "healthy";
    return new PatchManifestResult(statuses, healthStatus);
  }

  public static IReadOnlyList<PatchManifestEntry> CreateDefault(Harmony harmony)
  {
    return typeof(PatchManifest)
      .Assembly.GetTypes()
      .Where(IsHarmonyPatchType)
      .OrderBy(type => type.FullName, StringComparer.Ordinal)
      .Select(type => new PatchManifestEntry(
        GetPatchId(type),
        IsRequired(type),
        () => PatchType(harmony, type)
      ))
      .ToArray();
  }

  private static void PatchType(Harmony harmony, Type type) =>
    harmony.CreateClassProcessor(type).Patch();

  private static bool IsHarmonyPatchType(Type type) =>
    type.GetCustomAttributes(typeof(HarmonyPatch), inherit: false).Length > 0;

  private static string GetPatchId(Type type) => type.FullName ?? type.Name;

  private static bool IsRequired(Type type)
  {
    var name = type.FullName ?? type.Name;
    return !name.Contains("Siraethe", StringComparison.Ordinal)
      && !name.Contains("Mizuki", StringComparison.Ordinal)
      && !name.Contains("Sprinkles", StringComparison.Ordinal)
      && !name.Contains("DpsCheck", StringComparison.Ordinal)
      && !name.Contains("Faith", StringComparison.Ordinal)
      && !name.Contains("Grace", StringComparison.Ordinal)
      && !name.Contains("Fernalla", StringComparison.Ordinal)
      && !name.Contains("Lighthouse", StringComparison.Ordinal)
      && !name.Contains("Astra", StringComparison.Ordinal)
      && !name.Contains("Sableheart", StringComparison.Ordinal)
      && !name.Contains("DeathTouch", StringComparison.Ordinal);
  }

  private static IReadOnlyDictionary<string, object> CreatePatchDetails(
    PatchManifestEntry entry,
    Exception exception
  ) =>
    new Dictionary<string, object>
    {
      ["patchId"] = entry.Id,
      ["required"] = entry.Required,
      ["exceptionType"] = exception.GetType().Name,
    };
}
