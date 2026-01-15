using System.Text.Json;
using System.Text.Json.Serialization;

namespace ErenshorLogs.Json;

/// <summary>
/// Provides consistent JSON serialization options for the combat log format.
/// </summary>
public static class JsonContext
{
  /// <summary>
  /// Default JSON serializer options for combat log data.
  /// Uses camelCase for properties and snake_case for enums.
  /// </summary>
  public static JsonSerializerOptions Options { get; } = CreateOptions();

  private static JsonSerializerOptions CreateOptions()
  {
    var options = new JsonSerializerOptions
    {
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
      WriteIndented = false,
      DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    options.Converters.Add(new JsonStringEnumConverter(SnakeCaseNamingPolicy.Instance));

    return options;
  }
}
