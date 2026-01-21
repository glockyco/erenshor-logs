// Combat event types mirroring mod/src/Events/
// Enums serialize to snake_case (e.g., DamagePhysical -> "damage_physical")

export type EventType =
  | "damage_physical"
  | "damage_magic"
  | "damage_melee"
  | "damage_skill"
  | "damage_spell"
  | "damage_dot"
  | "damage_proc"
  | "damage_pet"
  | "damage_reflect"
  | "damage_environmental"
  | "heal_spell"
  | "heal_hot"
  | "heal_lifesteal"
  | "heal_regen"
  | "mana_use"
  | "mana_restore"
  | "mana_regen"
  | "spell_interrupt"
  | "buff_apply"
  | "buff_refresh"
  | "buff_fade"
  | "debuff_apply"
  | "debuff_refresh"
  | "debuff_fade"
  | "death"
  | "combat_start"
  | "combat_end";

export type ActorType = "player" | "sim_player" | "npc" | "pet";

export type DamageType = "unknown" | "physical" | "magic" | "elemental" | "void" | "poison";

export type AbilityType = "skill" | "spell" | "auto" | "dot" | "hot";

export type ProcSource = "weapon" | "wand" | "bow" | "buff" | "skill";

export interface ActorRef {
  id: string;
  name: string;
  type: ActorType;
  class?: string;
  level?: number;
  masterId?: string; // For pets - references owner's actor ID
}

export interface AbilityRef {
  name: string;
  type: AbilityType;
  stableKey?: string; // Game's stable identifier for linking
  procSource?: ProcSource; // What triggered this ability, if it was proc'd
}

export interface EffectRef {
  name: string;
  duration?: number; // Seconds
  stacks?: number;
}

export interface EventFlags {
  critical?: boolean;
  overkill?: boolean;
  fromPlayer?: boolean;
  pet?: boolean;
  resonating?: boolean;
  attributionFailed?: boolean;
  missed?: boolean;
  resisted?: boolean;
  absorbed?: boolean;
}

export interface CombatEvent {
  id: string; // UUID
  timestamp: number; // Unix timestamp in milliseconds
  eventType: EventType;
  source?: ActorRef;
  target?: ActorRef;
  ability?: AbilityRef;
  amount?: number; // Final amount after mitigation
  rawAmount?: number; // Before mitigation
  mitigated?: number; // Amount reduced by armor/resists
  damageType?: DamageType;
  effect?: EffectRef;
  flags?: EventFlags;
}

export interface PlayerInfo {
  name: string;
  class: string; // Arcanist, Paladin, Duelist, Druid, Stormcaller
  level: number; // 1-35
}
