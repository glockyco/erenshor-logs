namespace ErenshorLogs.Hooks;

using ErenshorLogs.Events;

/// <summary>
/// Maps game DamageType enum values to our DamageType enum.
/// </summary>
public static class DamageTypeMapper
{
  /// <summary>
  /// Converts the game's DamageType to our DamageType enum.
  /// </summary>
  /// <param name="gameType">The game's GameData.DamageType value.</param>
  /// <returns>The corresponding DamageType.</returns>
  public static DamageType FromGame(GameData.DamageType gameType)
  {
    return gameType switch
    {
      GameData.DamageType.Physical => DamageType.Physical,
      GameData.DamageType.Magic => DamageType.Magic,
      GameData.DamageType.Elemental => DamageType.Elemental,
      GameData.DamageType.Void => DamageType.Void,
      GameData.DamageType.Poison => DamageType.Poison,
      _ => DamageType.Unknown,
    };
  }
}
