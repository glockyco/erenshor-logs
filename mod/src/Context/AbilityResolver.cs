using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ErenshorLogs.Events;

namespace ErenshorLogs.Context;

/// <summary>
/// Resolves ability attribution from context or infers from damage parameters.
/// Centralizes attribution logic to maintain consistency across all damage patches.
/// </summary>
public static class AbilityResolver
{
  /// <summary>
  /// Resolves ability from current context stack.
  /// Returns null if no context available.
  /// </summary>
  public static AbilityRef? FromContext()
  {
    var context = CombatContext.CurrentAbility();
    if (context == null)
      return null;

    return new AbilityRef
    {
      Name = context.Name,
      Type = context.Type,
      StableKey = context.StableKey,
      ProcSource = context.ProcSource,
    };
  }

  /// <summary>
  /// Infers ability type from damage parameters when context is unavailable.
  /// Used as fallback for melee auto-attacks and other unattributed damage.
  /// </summary>
  public static AbilityRef InferFromDamageType(GameData.DamageType damageType)
  {
    // Physical damage without context = melee auto-attack
    if (damageType == GameData.DamageType.Physical)
    {
      return new AbilityRef
      {
        Name = "Melee Attack",
        Type = AbilityType.Auto,
        StableKey = null,
      };
    }

    // Magic damage without context = unknown (rare, possibly unhooked spell)
    return new AbilityRef
    {
      Name = "Unknown",
      Type = AbilityType.Unknown,
      StableKey = null,
    };
  }

  /// <summary>
  /// Resolves ability with smart fallback.
  /// Tries context first, falls back to inference from damage type.
  /// </summary>
  public static AbilityRef ResolveWithFallback(GameData.DamageType damageType)
  {
    return FromContext() ?? InferFromDamageType(damageType);
  }

  /// <summary>
  /// Creates a hardcoded ability reference for special cases.
  /// Used by environmental damage, system events, etc.
  /// </summary>
  public static AbilityRef CreateFixed(string name, AbilityType type)
  {
    return new AbilityRef
    {
      Name = name,
      Type = type,
      StableKey = null,
    };
  }

  /// <summary>
  /// Captures detailed debug information for ability attribution troubleshooting.
  /// Only captures if enabled via config settings or if attribution failed (Unknown type).
  /// Logs to BepInEx at Debug level.
  /// </summary>
  /// <param name="sourceMethod">The method where this event originated (e.g., "Character.DamageMe").</param>
  /// <param name="parameters">Key parameter values that help understand the context.</param>
  /// <param name="ability">The ability that was attributed (or Unknown).</param>
  /// <param name="captureForUnknown">Whether to capture debug info for Unknown attributions.</param>
  /// <param name="captureForAll">Whether to capture debug info for all attributions.</param>
  /// <param name="log">Optional logging callback for outputting debug info.</param>
  /// <returns>Debug info object if capture was enabled, otherwise null.</returns>
  public static AttributionDebugInfo? CaptureDebugInfoIfEnabled(
    string sourceMethod,
    Dictionary<string, string> parameters,
    AbilityRef ability,
    bool captureForUnknown,
    bool captureForAll,
    Action<string>? log = null
  )
  {
    // Check if we should capture based on config
    bool shouldCapture =
      captureForAll || (captureForUnknown && ability.Type == AbilityType.Unknown);

    if (!shouldCapture)
      return null;

    // Capture stack trace (skip 2 frames: this method + caller)
    var stackTrace = new StackTrace(skipFrames: 2, fNeedFileInfo: false);
    var frames = stackTrace.GetFrames()?.Take(7).Select(f => FormatStackFrame(f)).ToArray();

    // Capture context snapshot
    var context = CombatContext.CurrentAbility();
    var contextSnapshot = new ContextSnapshot
    {
      StackDepth = CombatContext.Depth(),
      TopContextName = context?.Name,
      TopContextType = context?.Type,
    };

    var debugInfo = new AttributionDebugInfo
    {
      SourceMethod = sourceMethod,
      Parameters = parameters,
      StackTrace = frames,
      Context = contextSnapshot,
    };

    // Log to BepInEx at Debug level
    if (log != null)
    {
      LogDebugInfo(debugInfo, ability, log);
    }

    return debugInfo;
  }

  /// <summary>
  /// Formats a stack frame for display in debug output.
  /// </summary>
  private static string FormatStackFrame(StackFrame frame)
  {
    var method = frame.GetMethod();
    if (method == null)
      return "<unknown>";

    var className = method.DeclaringType?.Name ?? "<unknown>";
    var methodName = method.Name;

    return $"{className}.{methodName}";
  }

  /// <summary>
  /// Logs debug information to BepInEx at Debug level.
  /// </summary>
  private static void LogDebugInfo(
    AttributionDebugInfo info,
    AbilityRef ability,
    Action<string> log
  )
  {
    log($"Attribution Debug: {ability.Name} ({ability.Type})");
    log($"  Source: {info.SourceMethod}");

    if (info.Parameters != null && info.Parameters.Count > 0)
    {
      log("  Parameters:");
      foreach (var (key, value) in info.Parameters)
      {
        log($"    {key}: {value}");
      }
    }

    if (info.Context != null)
    {
      log(
        $"  Context: depth={info.Context.StackDepth}, "
          + $"top={info.Context.TopContextName ?? "null"}"
          + (info.Context.TopContextType.HasValue ? $" ({info.Context.TopContextType})" : "")
      );
    }

    if (info.StackTrace != null && info.StackTrace.Length > 0)
    {
      log("  Stack trace:");
      foreach (var frame in info.StackTrace)
      {
        log($"    {frame}");
      }
    }
  }
}
