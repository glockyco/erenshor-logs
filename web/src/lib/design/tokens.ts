/**
 * Design Tokens - Classic MMO Theme
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
 * Numbers use JetBrains Mono, headings use Cinzel (fantasy serif), UI text uses Inter.
 */
export const typography = {
  /** Hero numbers: Massive display values (DPS in hero sections) */
  display: "text-5xl font-mono font-bold",

  /** Hero stats: Large prominent numbers */
  hero: "text-4xl font-mono font-bold",

  /** Section titles and page headers - Fantasy serif for MMO feel */
  h1: "text-3xl font-fantasy font-bold",

  /** Large numbers: Session totals */
  large: "text-2xl font-mono font-bold",

  /** Subsection headers - Fantasy serif for emphasis */
  h2: "text-xl font-fantasy font-semibold uppercase tracking-wider",

  /** Card titles and section labels */
  h3: "text-sm font-semibold uppercase tracking-wider",

  /** Body text: Actor names, regular content */
  body: "text-base font-semibold",

  /** Stat labels and form labels */
  label: "text-sm uppercase tracking-wider text-stone-400",

  /** Small text: Secondary information */
  small: "text-sm",

  /** Metadata: Timestamps, session IDs, tiny details */
  metadata: "text-xs font-mono text-stone-500",
} as const;

/**
 * Visual Effects
 *
 * Classic MMO aesthetic effects: subtle shadows, smooth transitions, stone borders.
 */
export const effects = {
  /** Subtle shadow for cards and elevated elements */
  shadow: "shadow-lg",

  /** Stronger shadow for active/focused elements */
  shadowStrong: "shadow-xl",

  /** Border with hover transition for interactive cards */
  border: "border border-stone-700 hover:border-amber-600/50 transition-all",

  /** Active/selected border for cards */
  borderActive: "border-2 border-amber-600",

  /** Standard transition timing for all animations */
  transition: "transition-all duration-200 ease-out",
} as const;

/**
 * Color Tokens
 *
 * Actor type colors for consistent visual coding.
 * Classic MMO palette: warm amber, vibrant lime, soft rose, mystical violet.
 */
export const colors = {
  actor: {
    player: "rgb(245 158 11)", // amber-500
    simPlayer: "rgb(132 204 22)", // lime-500
    npc: "rgb(251 113 133)", // rose-400
    pet: "rgb(167 139 250)", // violet-400
  },
  primary: {
    /** Primary accent color - warm amber gold */
    main: "rgb(217 119 6)", // amber-600
    light: "rgb(245 158 11)", // amber-500
    dark: "rgb(180 83 9)", // amber-700
  },
  background: {
    /** Main background - deep stone */
    main: "rgb(28 25 23)", // stone-900
    card: "rgb(41 37 36)", // stone-800
    elevated: "rgb(68 64 60)", // stone-700
  },
  text: {
    /** Primary text - light stone */
    primary: "rgb(245 245 244)", // stone-100
    secondary: "rgb(168 162 158)", // stone-400
    muted: "rgb(120 113 108)", // stone-500
  },
} as const;
