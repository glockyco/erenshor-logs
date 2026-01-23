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
}
