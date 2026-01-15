using System.Text.Json;
using System.Text.RegularExpressions;

namespace ErenshorLogs.Json;

/// <summary>
/// JSON naming policy that converts PascalCase to snake_case.
/// Used for enum serialization to match LOG_FORMAT.md specification.
/// </summary>
public sealed class SnakeCaseNamingPolicy : JsonNamingPolicy
{
  public static SnakeCaseNamingPolicy Instance { get; } = new();

  private static readonly Regex PascalCasePattern = new("([a-z0-9])([A-Z])", RegexOptions.Compiled);

  public override string ConvertName(string name)
  {
    if (string.IsNullOrEmpty(name))
    {
      return name;
    }

    return PascalCasePattern.Replace(name, "$1_$2").ToLowerInvariant();
  }
}
