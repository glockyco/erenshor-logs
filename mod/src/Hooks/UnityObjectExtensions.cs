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
  /// Uses Unity's overloaded equality operator which properly detects destroyed objects.
  /// </summary>
  /// <param name="character">The character to validate.</param>
  /// <returns>True if the character is valid and not destroyed; false otherwise.</returns>
  /// <remarks>
  /// We explicitly cast to UnityEngine.Object and use the != operator rather than
  /// relying on implicit bool conversion. Unity's == and != operators are overloaded
  /// to return true for comparisons with null when the native object is destroyed,
  /// even if the C# wrapper reference is non-null. This approach is more reliable
  /// across assembly boundaries in the BepInEx/Harmony context.
  /// </remarks>
  public static bool IsValid(this Character? character)
  {
    // Unity's == and != operators handle both C# null AND destroyed objects.
    // Cast to UnityEngine.Object to ensure we use Unity's overloaded operator, not C#'s default.
    // The null-forgiving operator is safe here - we're explicitly checking for null.
    return (UnityEngine.Object)character! != null;
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
