using ErenshorLogs.Events;
using ErenshorLogs.Session;
using Newtonsoft.Json.Linq;

namespace ErenshorLogs.Protocol;

public sealed class ProtocolSessionState
{
  private readonly CombatSession _session;
  private readonly Dictionary<string, ActorRecord> _actors = new();
  private readonly Dictionary<string, AbilityRecord> _abilities = new();
  private readonly Dictionary<string, EffectRecord> _effects = new();
  private readonly List<JObject> _events = [];

  public ProtocolSessionState(CombatSession session)
  {
    _session = session;
  }

  public long LastEventSeq { get; private set; }
  public int RegistryRevision { get; private set; }
  public IReadOnlyList<JObject> Events => _events;

  public Registries Registries =>
    new()
    {
      Revision = RegistryRevision,
      Actors = _actors,
      Abilities = _abilities,
      Effects = _effects,
    };

  public JObject? Append(CombatEvent evt)
  {
    if (!IsProtocolCombatEvent(evt))
      return null;

    var protocolEvent = evt.EventType switch
    {
      EventType.DamagePhysical
      or EventType.DamageMagic
      or EventType.DamageMelee
      or EventType.DamageSkill
      or EventType.DamageSpell
      or EventType.DamageDot
      or EventType.DamageProc
      or EventType.DamagePet
      or EventType.DamageReflect
      or EventType.DamageEnvironmental => CreateDamageEvent(evt),
      EventType.HealSpell or EventType.HealHot or EventType.HealLifesteal or EventType.HealRegen =>
        CreateHealEvent(evt),
      EventType.ManaUse or EventType.ManaRestore or EventType.ManaRegen => CreateResourceEvent(evt),
      EventType.BuffApply
      or EventType.BuffRefresh
      or EventType.BuffFade
      or EventType.DebuffApply
      or EventType.DebuffRefresh
      or EventType.DebuffFade => CreateEffectEvent(evt),
      EventType.Death => CreateDeathEvent(evt),
      EventType.SpellInterrupt => CreateInterruptEvent(evt),
      EventType.Mechanic => CreateMechanicEvent(evt),
      _ => null,
    };

    if (protocolEvent == null)
      return null;

    _events.Add(protocolEvent);
    return protocolEvent;
  }

  public SessionSnapshotPayload CreateSnapshot()
  {
    return new SessionSnapshotPayload
    {
      SessionId = _session.Id,
      State = _session.IsActive ? "active" : "ended",
      Mode = _session.IsManual ? "manual" : "automatic",
      StartedAtUtcMs = _session.StartTime,
      EndedAtUtcMs = _session.EndTime,
      EndReason = _session.EndReason,
      DurationMs = _session.IsActive ? null : _session.Duration,
      Producer = new ProducerInfo
      {
        Name = "ErenshorLogsMod",
        ModVersion = _session.ModVersion,
        GameVersion = _session.GameVersion,
      },
      RegistryRevision = RegistryRevision,
      LastEventSeq = LastEventSeq,
      EventCount = _events.Count,
      Completeness = "complete",
      Registries = Registries,
      Diagnostics = new SessionDiagnostics
      {
        HookWarnings = [],
        AttributionFailures = 0,
        DroppedEvents = 0,
        DroppedFrames = 0,
        SerializationErrors = 0,
      },
    };
  }

  public EventsPayload CreateEventsPayload(IReadOnlyList<JObject> events)
  {
    if (events.Count == 0)
      throw new ArgumentException("Event payload cannot be empty", nameof(events));

    return new EventsPayload
    {
      SessionId = _session.Id,
      RegistryRevision = RegistryRevision,
      EventSeqStart = events[0].Value<long>("eventSeq"),
      EventSeqEnd = events[^1].Value<long>("eventSeq"),
      Events = events,
    };
  }

  private JObject CreateDamageEvent(CombatEvent evt)
  {
    LastEventSeq += 1;

    var result = new JObject
    {
      ["eventSeq"] = LastEventSeq,
      ["offsetMs"] = Math.Max(0, evt.Timestamp - _session.StartTime),
      ["kind"] = "damage",
      ["action"] = GetDamageAction(evt.EventType),
      ["data"] = new JObject
      {
        ["amount"] = evt.Amount ?? 0,
        ["damageType"] = ToProtocolDamageType(evt.DamageType),
        ["outcome"] = CreateDamageOutcome(evt.Flags),
      },
    };

    AddOptional(result, "sourceActorId", RegisterActor(evt.Source));
    AddOptional(result, "creditActorId", RegisterActor(evt.Source));
    AddOptional(result, "targetActorId", RegisterActor(evt.Target));
    AddOptional(result, "abilityId", RegisterAbility(evt.Ability));
    AddOptional(result, "effectId", RegisterEffect(evt.Effect, evt.EventType));
    AddOptional(result, "attribution", ToProtocolAttribution(evt));

    var data = (JObject)result["data"]!;
    AddOptional(data, "rawAmount", evt.RawAmount);
    AddOptional(data, "mitigatedAmount", evt.Mitigated);

    if (evt.DebugInfo != null)
    {
      result["debug"] = JObject.FromObject(
        new AttributionDebug
        {
          SourceMethod = evt.DebugInfo.SourceMethod,
          Parameters = evt.DebugInfo.Parameters,
          Context =
            evt.DebugInfo.Context == null ? null : JObject.FromObject(evt.DebugInfo.Context),
        }
      );
    }

    return result;
  }

  private JObject CreateHealEvent(CombatEvent evt)
  {
    var data = new JObject { ["amount"] = evt.Amount ?? 0 };
    AddOptional(data, "rawAmount", evt.RawAmount);
    AddOptional(data, "overhealAmount", evt.OverhealAmount);
    if (evt.Flags?.Critical == true)
      data["critical"] = true;

    return CreateEventRecord(evt, "heal", GetHealAction(evt), data);
  }

  private JObject CreateResourceEvent(CombatEvent evt)
  {
    var data = new JObject
    {
      ["resource"] = evt.ResourceType ?? "mana",
      ["delta"] = evt.ResourceDelta ?? 0,
    };
    AddOptional(data, "current", evt.ResourceCurrent);
    AddOptional(data, "max", evt.ResourceMax);

    return CreateEventRecord(evt, "resource", GetResourceAction(evt), data);
  }

  private JObject CreateEffectEvent(CombatEvent evt)
  {
    var data = new JObject();
    AddOptional(data, "stacks", evt.EffectStacks);
    AddOptional(data, "durationMs", evt.EffectDurationMs);
    AddOptional(data, "reason", evt.EffectReason);

    return CreateEventRecord(evt, "effect", GetEffectAction(evt), data);
  }

  private JObject CreateDeathEvent(CombatEvent evt)
  {
    var data = new JObject();
    AddOptional(data, "killingBlowEventSeq", evt.KillingBlowEventSeq);

    return CreateEventRecord(evt, "death", "die", data);
  }

  private JObject CreateInterruptEvent(CombatEvent evt)
  {
    return CreateEventRecord(evt, "interrupt", "interrupt", new JObject());
  }

  private JObject CreateMechanicEvent(CombatEvent evt)
  {
    var mechanic =
      evt.Mechanic
      ?? new ErenshorLogs.Events.MechanicData { Name = evt.Ability.Name, Action = "phase" };
    var data = new JObject { ["name"] = mechanic.Name };
    AddOptional(data, "value", mechanic.Value);
    AddOptional(data, "previousValue", mechanic.PreviousValue);
    AddOptional(data, "affectedStat", mechanic.AffectedStat);
    AddOptional(data, "amount", mechanic.Amount);

    return CreateEventRecord(evt, "mechanic", mechanic.Action ?? "phase", data);
  }

  private JObject CreateEventRecord(CombatEvent evt, string kind, string action, JObject data)
  {
    LastEventSeq += 1;

    var result = new JObject
    {
      ["eventSeq"] = LastEventSeq,
      ["offsetMs"] = Math.Max(0, evt.Timestamp - _session.StartTime),
      ["kind"] = kind,
      ["action"] = action,
      ["data"] = data,
    };

    AddOptional(result, "sourceActorId", RegisterActor(evt.Source));
    AddOptional(result, "creditActorId", RegisterActor(evt.Source));
    AddOptional(result, "targetActorId", RegisterActor(evt.Target));
    AddOptional(result, "abilityId", RegisterAbility(evt.Ability));
    AddOptional(result, "effectId", RegisterEffect(evt.Effect, evt.EventType));
    AddOptional(result, "attribution", ToProtocolAttribution(evt));
    AddDebug(result, evt.DebugInfo);

    return result;
  }

  private string? RegisterActor(ActorRef? actor)
  {
    if (actor == null)
      return null;

    if (_actors.ContainsKey(actor.Id))
      return actor.Id;

    _actors.Add(
      actor.Id,
      new ActorRecord
      {
        Id = actor.Id,
        Name = actor.Name,
        Kind = ToProtocolActorKind(actor.Type),
        Class = actor.Class,
        Level = actor.Level,
        OwnerActorId = actor.MasterId,
        Faction = actor.Type == ActorType.Npc ? "hostile" : "friendly",
        IsPlayerControlled = actor.Type is ActorType.Player or ActorType.SimPlayer,
        FirstSeenEventSeq = LastEventSeq,
      }
    );
    RegistryRevision += 1;
    return actor.Id;
  }

  private string RegisterAbility(AbilityRef ability)
  {
    var id =
      ability.StableKey ?? $"ability:{ability.Type.ToString().ToLowerInvariant()}:{ability.Name}";
    if (_abilities.ContainsKey(id))
      return id;

    _abilities.Add(
      id,
      new AbilityRecord
      {
        Id = id,
        Name = ability.Name,
        Kind = ToProtocolAbilityKind(ability.Type),
        StableKey = ability.StableKey,
        DamageType = null,
        ProcSource = ability.ProcSource?.ToString().ToLowerInvariant(),
      }
    );
    RegistryRevision += 1;
    return id;
  }

  private string? RegisterEffect(EffectRef? effect, EventType eventType)
  {
    if (effect == null)
      return null;

    var id = $"effect:{effect.Name}";
    if (_effects.ContainsKey(id))
      return id;

    _effects.Add(
      id,
      new EffectRecord
      {
        Id = id,
        Name = effect.Name,
        Kind = GetEffectKind(eventType),
        DefaultDurationMs = effect.Duration * 1000,
        MaxStacks = effect.Stacks,
      }
    );
    RegistryRevision += 1;
    return id;
  }

  private static bool IsProtocolCombatEvent(CombatEvent evt)
  {
    return evt.EventType is not (EventType.CombatStart or EventType.CombatEnd);
  }

  private static string GetDamageAction(EventType eventType) =>
    eventType switch
    {
      EventType.DamageDot => "tick",
      EventType.DamageReflect => "reflect",
      _ => "hit",
    };

  private static string GetHealAction(CombatEvent evt) =>
    evt.EventType switch
    {
      EventType.HealHot => "tick",
      EventType.HealLifesteal => "lifesteal",
      EventType.HealRegen => "regen",
      EventType.HealSpell when evt.Mechanic?.Action == "scripted" => "scripted",
      _ => "direct",
    };

  private static string GetResourceAction(CombatEvent evt) =>
    evt.EventType switch
    {
      EventType.ManaRestore => "restore",
      EventType.ManaRegen => "regen",
      EventType.ManaUse
        when evt.ResourceDelta < 0
          && evt.Ability.StableKey?.StartsWith("mechanic:", StringComparison.Ordinal) == true =>
        "drain",
      _ => "spend",
    };

  private static string GetEffectAction(CombatEvent evt)
  {
    if (!string.IsNullOrWhiteSpace(evt.EffectAction))
      return evt.EffectAction;

    return evt.EventType switch
    {
      EventType.BuffRefresh or EventType.DebuffRefresh => "refresh",
      EventType.BuffFade or EventType.DebuffFade => "fade",
      _ => "apply",
    };
  }

  private static JObject CreateDamageOutcome(EventFlags? flags)
  {
    var result =
      flags?.Missed == true ? "missed"
      : flags?.Resisted == true ? "resisted"
      : flags?.Absorbed == true ? "absorbed"
      : "landed";

    var outcome = new JObject { ["result"] = result };
    if (flags?.Critical == true)
      outcome["critical"] = true;
    return outcome;
  }

  private static string? ToProtocolAttribution(CombatEvent evt)
  {
    var attribution =
      evt.Attribution ?? (evt.Flags?.AttributionFailed == true ? AttributionMethod.Unknown : null);

    return attribution switch
    {
      AttributionMethod.Verified => "verified",
      AttributionMethod.Context => "context",
      AttributionMethod.EffectTracker => "effectTracker",
      AttributionMethod.Inferred => "inferred",
      AttributionMethod.Unknown => "unknown",
      _ => null,
    };
  }

  private static string GetEffectKind(EventType eventType)
  {
    return eventType switch
    {
      EventType.BuffApply or EventType.BuffRefresh or EventType.BuffFade => "buff",
      EventType.DebuffApply or EventType.DebuffRefresh or EventType.DebuffFade => "debuff",
      _ => "unknown",
    };
  }

  private static string ToProtocolActorKind(ActorType type) =>
    type switch
    {
      ActorType.Player => "player",
      ActorType.SimPlayer => "simPlayer",
      ActorType.Npc => "npc",
      ActorType.Pet => "pet",
      _ => "unknown",
    };

  private static string ToProtocolAbilityKind(AbilityType type) =>
    type switch
    {
      AbilityType.Skill => "skill",
      AbilityType.Spell => "spell",
      AbilityType.Auto => "auto",
      AbilityType.Dot => "dot",
      AbilityType.Hot => "hot",
      AbilityType.Environmental => "environmental",
      AbilityType.AreaEffect => "areaEffect",
      _ => "unknown",
    };

  private static string ToProtocolDamageType(DamageType? type) =>
    type switch
    {
      DamageType.Physical => "physical",
      DamageType.Magic => "magic",
      DamageType.Elemental => "elemental",
      DamageType.Void => "void",
      DamageType.Poison => "poison",
      _ => "unknown",
    };

  private static void AddDebug(JObject result, AttributionDebugInfo? debugInfo)
  {
    if (debugInfo == null)
      return;

    result["debug"] = JObject.FromObject(
      new AttributionDebug
      {
        SourceMethod = debugInfo.SourceMethod,
        Parameters = debugInfo.Parameters,
        Context = debugInfo.Context == null ? null : JObject.FromObject(debugInfo.Context),
      }
    );
  }

  private static void AddOptional(JObject target, string property, string? value)
  {
    if (!string.IsNullOrEmpty(value))
      target[property] = value;
  }

  private static void AddOptional(JObject target, string property, long? value)
  {
    if (value.HasValue)
      target[property] = value.Value;
  }

  private static void AddOptional(JObject target, string property, int? value)
  {
    if (value.HasValue)
      target[property] = value.Value;
  }

  private static void AddOptional(JObject target, string property, object? value)
  {
    if (value != null)
      target[property] = JToken.FromObject(value);
  }
}
