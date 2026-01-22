using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ErenshorLogs.Json;

/// <summary>
/// Provides consistent JSON serialization settings for all JSON output.
/// Uses Newtonsoft.Json which is compatible with Unity's Mono runtime.
/// </summary>
public static class JsonSettings
{
  /// <summary>
  /// Default JSON serializer settings.
  /// Properties and enums use camelCase, nulls are omitted.
  /// </summary>
  public static JsonSerializerSettings Default { get; } =
    new()
    {
      ContractResolver = new DefaultContractResolver
      {
        NamingStrategy = new CamelCaseNamingStrategy(),
      },
      Converters =
      {
        new Newtonsoft.Json.Converters.StringEnumConverter(new CamelCaseNamingStrategy()),
      },
      NullValueHandling = NullValueHandling.Ignore,
      Formatting = Formatting.None,
    };

  /// <summary>
  /// Pretty-printed JSON settings for file export.
  /// </summary>
  public static JsonSerializerSettings Indented { get; } =
    new()
    {
      ContractResolver = new DefaultContractResolver
      {
        NamingStrategy = new CamelCaseNamingStrategy(),
      },
      Converters =
      {
        new Newtonsoft.Json.Converters.StringEnumConverter(new CamelCaseNamingStrategy()),
      },
      NullValueHandling = NullValueHandling.Ignore,
      Formatting = Formatting.Indented,
    };
}
