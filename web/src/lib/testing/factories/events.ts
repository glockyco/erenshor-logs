import type {
  AbilityRecord,
  CombatEventRecord,
  DamageEvent,
  DeathEvent,
  EffectEvent,
  EffectRecord,
  HealEvent,
  InterruptEvent,
  MechanicEvent,
  ResourceEvent,
} from "$lib/types/schemas";

export function createAbilityRecord(overrides: Partial<AbilityRecord> = {}): AbilityRecord {
  return {
    id: "ability-1",
    name: "Ability",
    kind: "skill",
    ...overrides,
  };
}

export function createEffectRecord(overrides: Partial<EffectRecord> = {}): EffectRecord {
  return {
    id: "effect-1",
    name: "Effect",
    kind: "buff",
    defaultDurationMs: 10000,
    maxStacks: 1,
    ...overrides,
  };
}

export function createDamageEvent(overrides: Partial<DamageEvent> = {}): DamageEvent {
  return {
    eventSeq: 1,
    offsetMs: 0,
    kind: "damage",
    action: "hit",
    sourceActorId: "player-1",
    targetActorId: "npc-1",
    abilityId: "ability-1",
    data: {
      amount: 1000,
      damageType: "physical",
      outcome: { result: "landed" },
    },
    ...overrides,
  };
}

export function createHealEvent(overrides: Partial<HealEvent> = {}): HealEvent {
  return {
    eventSeq: 1,
    offsetMs: 0,
    kind: "heal",
    action: "direct",
    sourceActorId: "player-1",
    targetActorId: "sim-1",
    abilityId: "heal-1",
    data: {
      amount: 500,
    },
    ...overrides,
  };
}

export function createResourceEvent(overrides: Partial<ResourceEvent> = {}): ResourceEvent {
  return {
    eventSeq: 1,
    offsetMs: 0,
    kind: "resource",
    action: "drain",
    sourceActorId: "npc-1",
    targetActorId: "player-1",
    abilityId: "ability-1",
    data: {
      resource: "mana",
      delta: -300,
    },
    ...overrides,
  };
}

export function createDeathEvent(overrides: Partial<DeathEvent> = {}): DeathEvent {
  return {
    eventSeq: 1,
    offsetMs: 0,
    kind: "death",
    action: "die",
    sourceActorId: "npc-1",
    targetActorId: "player-1",
    abilityId: "ability-1",
    data: {},
    ...overrides,
  };
}

export function createInterruptEvent(overrides: Partial<InterruptEvent> = {}): InterruptEvent {
  return {
    eventSeq: 1,
    offsetMs: 0,
    kind: "interrupt",
    action: "interrupt",
    sourceActorId: "player-1",
    targetActorId: "npc-1",
    abilityId: "ability-1",
    data: {
      interruptedAbilityId: "spell-1",
    },
    ...overrides,
  };
}

export function createMechanicEvent(overrides: Partial<MechanicEvent> = {}): MechanicEvent {
  return {
    eventSeq: 1,
    offsetMs: 0,
    kind: "mechanic",
    action: "invulnerability",
    sourceActorId: "npc-1",
    targetActorId: "npc-1",
    abilityId: "ability-1",
    data: {
      name: "Sprinkles wards",
      value: true,
    },
    ...overrides,
  };
}

export function createCombatEvent(overrides: Partial<CombatEventRecord> = {}): CombatEventRecord {
  return createDamageEvent(overrides as Partial<DamageEvent>);
}

export function createCriticalDamageEvent(overrides: Partial<DamageEvent> = {}): DamageEvent {
  return createDamageEvent({
    data: {
      amount: 2000,
      damageType: "physical",
      outcome: { result: "landed", critical: true },
    },
    ...overrides,
  });
}

export function createBuffEvent(overrides: Partial<EffectEvent> = {}): EffectEvent {
  return {
    eventSeq: 1,
    offsetMs: 0,
    kind: "effect",
    action: "apply",
    sourceActorId: "player-1",
    targetActorId: "player-1",
    abilityId: "ability-1",
    effectId: "effect-1",
    data: {
      stacks: 1,
      durationMs: 30000,
    },
    ...overrides,
  };
}

export function createTimedEvents(count: number, intervalMs: number): CombatEventRecord[] {
  return Array.from({ length: count }, (_, index) =>
    createCombatEvent({ eventSeq: index + 1, offsetMs: index * intervalMs })
  );
}
