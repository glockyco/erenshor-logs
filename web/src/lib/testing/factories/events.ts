import type {
  AbilityRecord,
  CombatEventRecord,
  DamageEvent,
  EffectEvent,
  EffectRecord,
  HealEvent,
} from "$lib/types";

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
