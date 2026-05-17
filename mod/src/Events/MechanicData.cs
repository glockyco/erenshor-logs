namespace ErenshorLogs.Events;

public sealed record MechanicData
{
  public required string Name { get; init; }
  public string? Action { get; init; }
  public object? Value { get; init; }
  public object? PreviousValue { get; init; }
  public string? AffectedStat { get; init; }
  public int? Amount { get; init; }
}
