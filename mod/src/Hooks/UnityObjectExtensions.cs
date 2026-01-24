namespace ErenshorLogs.Hooks;

/// <summary>
/// Extension methods for safely working with Unity objects that may be destroyed.
/// Unity objects can become "destroyed" (native pointer null) while the C# wrapper persists,
/// causing NullReferenceException when accessing properties like transform.
/// </summary>
public static class UnityObjectExtensions
{
  /// <summary>
  /// Checks if a Unity Character object is valid (not null and not destroyed).
  /// Uses Unity's special null-checking that handles destroyed objects.
  /// </summary>
  /// <param name="character">The character to validate.</param>
  /// <returns>True if the character is valid and not destroyed; false otherwise.</returns>
  public static bool IsValid(this Character? character)
  {
    // Unity's implicit bool operator returns false for both null and destroyed objects
    // This is Unity-specific behavior - a destroyed object is not the same as C# null
    return character != null && character;
  }

  /// <summary>
  /// Safely gets the transform name of a Character, returning empty string if invalid.
  /// Prevents NullReferenceException when accessing destroyed Unity objects.
  /// </summary>
  /// <param name="character">The character whose transform name to retrieve.</param>
  /// <returns>The transform name, or empty string if the character is invalid or destroyed.</returns>
  public static string GetSafeTransformName(this Character? character)
  {
    if (!character.IsValid())
      return string.Empty;

    try
    {
      // Access transform.name safely - may still throw if object destroyed between checks
      // (rare race condition in Unity lifecycle)
      // Null-forgiving operator is safe here because we validated with IsValid()
      return character!.transform?.name ?? string.Empty;
    }
    catch
    {
      // Object destroyed between IsValid() check and property access
      return string.Empty;
    }
  }

  /// <summary>
  /// Safely gets the instance ID of a Character.
  /// Returns null if the character is invalid or destroyed.
  /// </summary>
  /// <param name="character">The character whose instance ID to retrieve.</param>
  /// <returns>The instance ID, or null if the character is invalid or destroyed.</returns>
  public static int? GetSafeInstanceID(this Character? character)
  {
    if (!character.IsValid())
      return null;

    try
    {
      // Null-forgiving operator is safe here because we validated with IsValid()
      return character!.GetInstanceID();
    }
    catch
    {
      // Rare race condition: object destroyed between checks
      return null;
    }
  }
}
