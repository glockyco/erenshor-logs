namespace ErenshorLogs.Diagnostics;

public interface IDiagnosticReporter
{
  DiagnosticCounters Counters { get; }
  IReadOnlyList<DiagnosticRecord> RecentDiagnostics { get; }
  void ReportProjectionError(Exception exception, string? sessionId, string eventType, string path);
  void ReportSerializationError(Exception exception, string operation, string? sessionId = null);
  void ReportClientSendError(Exception exception);
  void ReportDroppedFrame(string operation, string message, string? sessionId = null);
  void ReportRecoverable(
    string code,
    string severity,
    string impact,
    string component,
    string operation,
    string message,
    string? sessionId = null,
    IReadOnlyDictionary<string, object>? details = null
  );
  void ReportFatal(string code, string component, string operation, string message);
  IReadOnlyList<DiagnosticRecord> DrainPendingDiagnostics(int maxCount);
}

public sealed class DiagnosticReporter(Action<string>? log = null) : IDiagnosticReporter
{
  private const int MaxRecentDiagnostics = 32;
  private const int MaxDedupeBuckets = 64;
  private const int MaxDetailKeys = 16;
  private const int MaxDetailValueLength = 120;
  private const int MaxMessageLength = 160;

  private readonly Action<string>? _log = log;
  private readonly List<DiagnosticRecord> _recent = [];
  private readonly List<string> _pendingKeys = [];
  private readonly Dictionary<string, DiagnosticBucket> _buckets = new(StringComparer.Ordinal);
  private long _nextId;

  public DiagnosticCounters Counters { get; } = new();
  public IReadOnlyList<DiagnosticRecord> RecentDiagnostics => _recent;

  public void ReportProjectionError(
    Exception exception,
    string? sessionId,
    string eventType,
    string path
  )
  {
    Counters.ProjectionErrors += 1;
    Counters.DroppedEvents += 1;

    Report(
      code: "projection.failed",
      severity: "error",
      impact: "eventDropped",
      component: "mod.protocol",
      operation: "projectEvent",
      message: "A combat event could not be converted to protocol v3.",
      sessionId,
      details: new Dictionary<string, object>
      {
        ["eventType"] = eventType,
        ["exceptionType"] = exception.GetType().Name,
        ["path"] = path,
      }
    );
  }

  public void ReportSerializationError(
    Exception exception,
    string operation,
    string? sessionId = null
  )
  {
    Counters.SerializationErrors += 1;
    Counters.DroppedFrames += 1;

    Report(
      code: "serialization.failed",
      severity: "error",
      impact: "frameSkipped",
      component: "mod.protocol",
      operation,
      message: "A live protocol frame could not be serialized.",
      sessionId,
      details: new Dictionary<string, object> { ["exceptionType"] = exception.GetType().Name }
    );
  }

  public void ReportClientSendError(Exception exception)
  {
    Counters.ClientSendErrors += 1;

    Report(
      code: "client.send.failed",
      severity: "warning",
      impact: "none",
      component: "mod.websocket",
      operation: "send",
      message: "A WebSocket client did not accept a live frame.",
      sessionId: null,
      details: new Dictionary<string, object> { ["exceptionType"] = exception.GetType().Name }
    );
  }

  public void ReportDroppedFrame(string operation, string message, string? sessionId = null)
  {
    Counters.DroppedFrames += 1;
    Report(
      code: "frame.dropped",
      severity: "warning",
      impact: "frameSkipped",
      component: "mod.protocol",
      operation,
      message,
      sessionId
    );
  }

  public void ReportRecoverable(
    string code,
    string severity,
    string impact,
    string component,
    string operation,
    string message,
    string? sessionId = null,
    IReadOnlyDictionary<string, object>? details = null
  ) => Report(code, severity, impact, component, operation, message, sessionId, details);

  public void ReportFatal(string code, string component, string operation, string message) =>
    Report(code, "fatal", "modFatal", component, operation, message, sessionId: null);

  public IReadOnlyList<DiagnosticRecord> DrainPendingDiagnostics(int maxCount)
  {
    if (maxCount <= 0 || _pendingKeys.Count == 0)
      return [];

    var count = Math.Min(maxCount, _pendingKeys.Count);
    var drained = new List<DiagnosticRecord>(count);
    for (var index = 0; index < count; index += 1)
    {
      var key = _pendingKeys[0];
      _pendingKeys.RemoveAt(0);
      drained.Add(_buckets[key].Record);
    }

    Counters.DiagnosticsEmitted += drained.Count;
    return drained;
  }

  private void Report(
    string code,
    string severity,
    string impact,
    string component,
    string operation,
    string message,
    string? sessionId,
    IReadOnlyDictionary<string, object>? details = null
  )
  {
    var boundedDetails = BoundDetails(details);
    var key = CreateKey(code, component, operation, sessionId, boundedDetails);
    var nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    if (_buckets.TryGetValue(key, out var existing))
    {
      Counters.DiagnosticsSuppressed += 1;
      var updated = existing.Record with
      {
        LastSeenAtMs = nowMs,
        Count = existing.Record.Count + 1,
        SuppressedCount = existing.Record.SuppressedCount + 1,
      };
      existing.Record = updated;
      return;
    }

    TrimDedupeBucketsIfNeeded();

    var record = new DiagnosticRecord
    {
      Id = $"d-{++_nextId}",
      Code = code,
      Severity = severity,
      Impact = impact,
      Component = component,
      Operation = operation,
      Message = BoundString(message, MaxMessageLength),
      SessionId = sessionId,
      FirstSeenAtMs = nowMs,
      LastSeenAtMs = nowMs,
      Count = 1,
      SuppressedCount = 0,
      Details = boundedDetails,
    };

    _buckets.Add(key, new DiagnosticBucket(key, record));
    _pendingKeys.Add(key);
    _recent.Add(record);
    if (_recent.Count > MaxRecentDiagnostics)
      _recent.RemoveAt(0);

    _log?.Invoke($"{code}: {record.Message}");
  }

  private void TrimDedupeBucketsIfNeeded()
  {
    if (_buckets.Count < MaxDedupeBuckets)
      return;

    var key = _buckets.Keys.First();
    _buckets.Remove(key);
    _pendingKeys.Remove(key);
  }

  private static string CreateKey(
    string code,
    string component,
    string operation,
    string? sessionId,
    IReadOnlyDictionary<string, object>? details
  )
  {
    var detailKey =
      details == null
        ? string.Empty
        : string.Join("|", details.Select(kv => $"{kv.Key}={kv.Value}"));
    return string.Join("|", code, component, operation, sessionId ?? string.Empty, detailKey);
  }

  private static IReadOnlyDictionary<string, object>? BoundDetails(
    IReadOnlyDictionary<string, object>? details
  )
  {
    if (details == null || details.Count == 0)
      return null;

    var bounded = new Dictionary<string, object>(StringComparer.Ordinal);
    foreach (var (key, value) in details.Take(MaxDetailKeys))
    {
      bounded[key] = value is string text ? BoundString(text, MaxDetailValueLength) : value;
    }

    return bounded;
  }

  private static string BoundString(string value, int maxLength) =>
    value.Length <= maxLength ? value : value[..maxLength];

  private sealed class DiagnosticBucket(string key, DiagnosticRecord record)
  {
    public string Key { get; } = key;
    public DiagnosticRecord Record { get; set; } = record;
  }
}

public sealed class DiagnosticCounters
{
  public long CapturedEvents { get; set; }
  public long ProjectedEvents { get; set; }
  public long SentEvents { get; set; }
  public long SentFrames { get; set; }
  public long DroppedEvents { get; set; }
  public long DroppedFrames { get; set; }
  public long ProjectionErrors { get; set; }
  public long SerializationErrors { get; set; }
  public long ClientSendErrors { get; set; }
  public long HookWarnings { get; set; }
  public long AttributionFailures { get; set; }
  public long DiagnosticsEmitted { get; set; }
  public long DiagnosticsSuppressed { get; set; }
}

public sealed record DiagnosticRecord
{
  public required string Id { get; init; }
  public required string Code { get; init; }
  public required string Severity { get; init; }
  public required string Impact { get; init; }
  public required string Component { get; init; }
  public required string Operation { get; init; }
  public required string Message { get; init; }
  public string? SessionId { get; init; }
  public long? FrameId { get; init; }
  public required long FirstSeenAtMs { get; init; }
  public required long LastSeenAtMs { get; init; }
  public required long Count { get; init; }
  public required long SuppressedCount { get; init; }
  public IReadOnlyDictionary<string, object>? Details { get; init; }
}
