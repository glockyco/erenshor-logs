/**
 * Design Tokens - Cyberpunk Analyst Theme
 *
 * Semantic design tokens that enforce consistency across the application.
 * These tokens define spacing, typography, and visual effects with meaningful
 * names that communicate intent rather than arbitrary values.
 */

/**
 * Spacing Scale
 *
 * Semantic spacing tokens for consistent layout and component spacing.
 * Use these instead of arbitrary Tailwind classes to maintain consistency.
 */
export const spacing = {
  /** Page-level sections: large breathing room between major areas */
  section: "p-8 space-y-8",

  /** Card interiors: comfortable padding for card content */
  card: "p-6 space-y-6",

  /** Smaller panels and compact cards */
  panel: "p-4 space-y-4",

  /** Vertical lists and stacked elements */
  stack: "space-y-3",

  /** Horizontal inline elements (buttons, badges, etc) */
  inline: "gap-3",

  /** Compact spacing for tightly grouped elements */
  tight: "gap-2",
} as const;

/**
 * Typography Scale
 *
 * Standardized typography tokens for all text hierarchies.
 * Numbers use JetBrains Mono, UI text uses Inter.
 */
export const typography = {
  /** Hero numbers: Massive display values (DPS in hero sections) */
  display: "text-5xl font-mono font-bold",

  /** Hero stats: Large prominent numbers */
  hero: "text-4xl font-mono font-bold",

  /** Section titles and page headers */
  h1: "text-3xl font-bold",

  /** Large numbers: Session totals */
  large: "text-2xl font-mono font-bold",

  /** Subsection headers */
  h2: "text-xl font-semibold uppercase tracking-wider",

  /** Card titles and section labels */
  h3: "text-sm font-semibold uppercase tracking-wider",

  /** Body text: Actor names, regular content */
  body: "text-base font-semibold",

  /** Stat labels and form labels */
  label: "text-sm uppercase tracking-wider text-slate-400",

  /** Small text: Secondary information */
  small: "text-sm",

  /** Metadata: Timestamps, session IDs, tiny details */
  metadata: "text-xs font-mono text-slate-500",
} as const;

/**
 * Visual Effects
 *
 * Cyberpunk aesthetic effects: glows, transitions, borders.
 */
export const effects = {
  /** Cyan glow effect for active/interactive elements */
  glow: "shadow-[0_0_20px_rgb(34_211_238_/_0.6)]",

  /** Stronger glow for primary actions and focus states */
  glowStrong: "shadow-[0_0_30px_rgb(34_211_238_/_0.8)]",

  /** Border with hover transition for interactive cards */
  border: "border border-slate-700 hover:border-cyan-500/60 transition-all",

  /** Standard transition timing for all animations */
  transition: "transition-all duration-200 ease-out",
} as const;

/**
 * Color Tokens
 *
 * Actor type colors for consistent visual coding.
 */
export const colors = {
  actor: {
    player: "rgb(34 211 238)", // cyan-400
    simPlayer: "rgb(52 211 153)", // emerald-400
    npc: "rgb(251 113 133)", // rose-400
    pet: "rgb(167 139 250)", // violet-400
  },
} as const;
