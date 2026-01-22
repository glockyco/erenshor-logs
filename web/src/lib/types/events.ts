// Combat event types - re-exported from Zod schemas
// See schemas.ts for the canonical definitions

export type {
  EventType,
  ActorType,
  DamageType,
  AbilityType,
  ProcSource,
  ActorRef,
  AbilityRef,
  EffectRef,
  EventFlags,
  CombatEvent,
} from "./schemas";

export {
  EventTypeSchema,
  ActorTypeSchema,
  DamageTypeSchema,
  AbilityTypeSchema,
  ProcSourceSchema,
  ActorRefSchema,
  AbilityRefSchema,
  EffectRefSchema,
  EventFlagsSchema,
  CombatEventSchema,
} from "./schemas";
