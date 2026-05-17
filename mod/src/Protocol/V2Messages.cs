using Newtonsoft.Json.Linq;

namespace ErenshorLogs.Protocol;

public sealed record LiveEnvelope
{
  public string Protocol { get; init; } = "erenshor.logs.live";
  public string ProtocolVersion { get; init; } = "2.0.0";
  public string SchemaVersion { get; init; } = "2.0.0";
  public required string Kind { get; init; }
  public required long FrameSeq { get; init; }
  public string? SessionId { get; init; }
  public required long SentAtMs { get; init; }
  public required object Payload { get; init; }
}

public sealed record HelloPayload
{
  public required ProducerInfo Producer { get; init; }
  public string? ActiveSessionId { get; init; }
  public required IReadOnlyList<string> Capabilities { get; init; }
  public IReadOnlyList<string>? RequiredCapabilities { get; init; }
}

public sealed record RegistryDeltaPayload
{
  public required int Revision { get; init; }
  public IReadOnlyDictionary<string, ActorRecord>? Actors { get; init; }
  public IReadOnlyDictionary<string, AbilityRecord>? Abilities { get; init; }
  public IReadOnlyDictionary<string, EffectRecord>? Effects { get; init; }
}

public sealed record EventsPayload
{
  public required string SessionId { get; init; }
  public required int RegistryRevision { get; init; }
  public required long EventSeqStart { get; init; }
  public required long EventSeqEnd { get; init; }
  public required IReadOnlyList<JObject> Events { get; init; }
}

public sealed record ErrorPayload
{
  public required string Code { get; init; }
  public required string Severity { get; init; }
  public required string Message { get; init; }
  public required bool Recoverable { get; init; }
  public string? SessionId { get; init; }
  public long? EventSeq { get; init; }
  public IReadOnlyDictionary<string, object>? Details { get; init; }
}

public sealed record ServerStatsPayload
{
  public required int ConnectedClients { get; init; }
  public required long EventsCaptured { get; init; }
  public required long EventsSent { get; init; }
  public required int RegistryRevision { get; init; }
  public required int QueuedFrames { get; init; }
  public required int DroppedEvents { get; init; }
  public required int AttributionFailures { get; init; }
  public required int HookWarnings { get; init; }
  public long? BytesSent { get; init; }
}

public sealed record CombatLogFile
{
  public required string Format { get; init; }
  public required string SchemaVersion { get; init; }
  public required long ExportedAtMs { get; init; }
  public required ProducerInfo Producer { get; init; }
  public required IReadOnlyList<CombatLogSession> Sessions { get; init; }
}

public sealed record CombatLogSession
{
  public required SessionSnapshotPayload Snapshot { get; init; }
  public required IReadOnlyList<JObject> Events { get; init; }
  public SessionEndedPayload? Ended { get; init; }
  public DerivedData? Derived { get; init; }
}

public sealed record ProducerInfo
{
  public required string Name { get; init; }
  public string? ModVersion { get; init; }
  public string? WebVersion { get; init; }
  public string? GameVersion { get; init; }
  public string? BuildCommit { get; init; }
}

public sealed record SessionSnapshotPayload
{
  public required string SessionId { get; init; }
  public required string State { get; init; }
  public required string Mode { get; init; }
  public required long StartedAtUtcMs { get; init; }
  public long? EndedAtUtcMs { get; init; }
  public string? EndReason { get; init; }
  public long? DurationMs { get; init; }
  public required ProducerInfo Producer { get; init; }
  public string? PlayerActorId { get; init; }
  public required int RegistryRevision { get; init; }
  public required long LastEventSeq { get; init; }
  public required int EventCount { get; init; }
  public required string Completeness { get; init; }
  public LossCounters? Loss { get; init; }
  public required Registries Registries { get; init; }
  public SessionDiagnostics? Diagnostics { get; init; }
}

public sealed record LossCounters
{
  public required int EventsDropped { get; init; }
  public required int FramesDropped { get; init; }
  public string? Reason { get; init; }
}

public sealed record Registries
{
  public required int Revision { get; init; }
  public required IReadOnlyDictionary<string, ActorRecord> Actors { get; init; }
  public required IReadOnlyDictionary<string, AbilityRecord> Abilities { get; init; }
  public required IReadOnlyDictionary<string, EffectRecord> Effects { get; init; }
}

public sealed record ActorRecord
{
  public required string Id { get; init; }
  public required string Name { get; init; }
  public required string Kind { get; init; }
  public string? Class { get; init; }
  public int? Level { get; init; }
  public string? OwnerActorId { get; init; }
  public string? Faction { get; init; }
  public bool? IsPlayerControlled { get; init; }
  public int? RaidGroup { get; init; }
  public string? RaidRole { get; init; }
  public long? FirstSeenEventSeq { get; init; }
}

public sealed record AbilityRecord
{
  public required string Id { get; init; }
  public required string Name { get; init; }
  public required string Kind { get; init; }
  public string? StableKey { get; init; }
  public string? DamageType { get; init; }
  public string? ProcSource { get; init; }
  public string? ParentAbilityId { get; init; }
}

public sealed record EffectRecord
{
  public required string Id { get; init; }
  public required string Name { get; init; }
  public required string Kind { get; init; }
  public string? StableKey { get; init; }
  public string? SourceAbilityId { get; init; }
  public long? DefaultDurationMs { get; init; }
  public int? MaxStacks { get; init; }
}

public sealed record SessionEndedPayload
{
  public required string SessionId { get; init; }
  public required long EndedAtUtcMs { get; init; }
  public required long EndedAtEventSeq { get; init; }
  public required string Reason { get; init; }
  public required long DurationMs { get; init; }
  public SessionDiagnostics? Diagnostics { get; init; }
}

public sealed record SessionDiagnostics
{
  public required IReadOnlyList<string> HookWarnings { get; init; }
  public required int AttributionFailures { get; init; }
  public required int DroppedEvents { get; init; }
  public required int DroppedFrames { get; init; }
  public required int SerializationErrors { get; init; }
}

public abstract record CombatEventBase
{
  public required long EventSeq { get; init; }
  public required long OffsetMs { get; init; }
  public required string Kind { get; init; }
  public required string Action { get; init; }
  public string? SourceActorId { get; init; }
  public string? CreditActorId { get; init; }
  public string? TargetActorId { get; init; }
  public string? AbilityId { get; init; }
  public string? EffectId { get; init; }
  public long? CauseEventSeq { get; init; }
  public string? Attribution { get; init; }
  public AttributionDebug? Debug { get; init; }
}

public sealed record DamageEventRecord : CombatEventBase
{
  public required DamageData Data { get; init; }
}

public sealed record HealEventRecord : CombatEventBase
{
  public required HealData Data { get; init; }
}

public sealed record ResourceEventRecord : CombatEventBase
{
  public required ResourceData Data { get; init; }
}

public sealed record EffectEventRecord : CombatEventBase
{
  public required EffectData Data { get; init; }
}

public sealed record DeathEventRecord : CombatEventBase
{
  public required DeathData Data { get; init; }
}

public sealed record InterruptEventRecord : CombatEventBase
{
  public required InterruptData Data { get; init; }
}

public sealed record MechanicEventRecord : CombatEventBase
{
  public required MechanicData Data { get; init; }
}

public sealed record DamageData
{
  public required long Amount { get; init; }
  public long? RawAmount { get; init; }
  public long? MitigatedAmount { get; init; }
  public long? OverkillAmount { get; init; }
  public required string DamageType { get; init; }
  public required JObject Outcome { get; init; }
}

public sealed record HealData
{
  public required long Amount { get; init; }
  public long? RawAmount { get; init; }
  public long? OverhealAmount { get; init; }
  public bool? Critical { get; init; }
}

public sealed record ResourceData
{
  public required string Resource { get; init; }
  public required long Delta { get; init; }
  public long? Current { get; init; }
  public long? Max { get; init; }
}

public sealed record EffectData
{
  public int? Stacks { get; init; }
  public long? DurationMs { get; init; }
  public long? RemainingMs { get; init; }
  public string? Reason { get; init; }
}

public sealed record DeathData
{
  public long? KillingBlowEventSeq { get; init; }
}

public sealed record InterruptData
{
  public string? InterruptedAbilityId { get; init; }
}

public sealed record MechanicData
{
  public required string Name { get; init; }
  public object? Value { get; init; }
  public object? PreviousValue { get; init; }
  public string? AffectedStat { get; init; }
  public int? Amount { get; init; }
}

public sealed record AttributionDebug
{
  public required string SourceMethod { get; init; }
  public IReadOnlyDictionary<string, string>? Parameters { get; init; }
  public JObject? Context { get; init; }
}

public sealed record DerivedData
{
  public required string AlgorithmVersion { get; init; }
  public required long ComputedAtMs { get; init; }
  public required long ComputedFromEventSeq { get; init; }
  public required DerivedSummary Summary { get; init; }
}

public sealed record DerivedSummary
{
  public required long TotalDamage { get; init; }
  public required long TotalHealing { get; init; }
  public required long TotalDamageTaken { get; init; }
  public required long TotalHealingReceived { get; init; }
  public required long DurationMs { get; init; }
}
