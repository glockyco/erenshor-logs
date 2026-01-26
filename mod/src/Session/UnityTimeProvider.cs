using UnityEngine;

namespace ErenshorLogs.Session;

/// <summary>
/// Provides time from Unity's Time.time.
/// </summary>
public sealed class UnityTimeProvider : ITimeProvider
{
  /// <inheritdoc />
  public float CurrentTime => Time.time;
}
