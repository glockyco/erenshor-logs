// Icon mapping for actor types and player classes

import {
  WandSparkles,
  Swords,
  Leaf,
  Shield,
  BowArrow,
  Axe,
  User,
  Skull,
  Cat,
} from "@lucide/svelte";
import type { Component } from "svelte";
import type { ActorType } from "$lib/types";

type IconComponent = Component;

/**
 * Get the appropriate icon component for an actor based on type and class.
 *
 * @param actorType - The actor's type (player, simPlayer, npc, pet)
 * @param actorClass - Optional class name for players (Arcanist, Duelist, Druid, Paladin, Stormcaller, Reaver)
 * @returns Lucide icon component
 *
 * @example
 * ```ts
 * const icon = getActorIcon("player", "Duelist"); // Returns Swords icon
 * const icon = getActorIcon("npc"); // Returns Skull icon
 * const icon = getActorIcon("pet"); // Returns Cat icon
 * ```
 */
export function getActorIcon(actorType: ActorType, actorClass?: string): IconComponent {
  // Player classes (both player and simPlayer use class-based icons)
  if (actorType === "player" || actorType === "simPlayer") {
    switch (actorClass) {
      case "Arcanist":
        return WandSparkles; // Arcane magic caster
      case "Duelist":
        return Swords; // Dual-wield melee DPS
      case "Druid":
        return Leaf; // Nature-based class
      case "Paladin":
        return Shield; // Tank/defender
      case "Stormcaller":
        return BowArrow; // Ranged bow user with storm magic
      case "Reaver":
        return Axe; // 2H weapon DPS/tank
      default:
        // No class specified or unknown class - use generic user icon
        return User;
    }
  }

  // NPCs (enemies)
  if (actorType === "npc") {
    return Skull;
  }

  // Pets (summoned creatures)
  if (actorType === "pet") {
    return Cat;
  }

  // Fallback for any unknown type
  return User;
}
