/**
 * Component Style Utilities
 *
 * Reusable style patterns for common component types.
 * These combine design tokens into complete component styles.
 */

/**
 * Card Styles
 *
 * Standard card appearance with interactive variants.
 */
export const cardStyles = {
  /** Base card appearance */
  base: "rounded-lg border border-slate-700 bg-slate-900",

  /** Interactive card (clickable, hoverable) */
  interactive:
    "cursor-pointer hover:border-cyan-500/60 hover:bg-slate-800/50 transition-all duration-200",

  /** Active/selected card state */
  active: "border-cyan-400 bg-slate-800 shadow-[0_0_20px_rgb(34_211_238_/_0.3)]",

  /** Card with glow effect */
  glow: "shadow-[0_0_20px_rgb(34_211_238_/_0.6)]",
} as const;

/**
 * Table Styles
 *
 * Consistent table cell and row styling.
 */
export const tableStyles = {
  /** Sticky table header */
  header: "sticky top-0 bg-slate-900/95 backdrop-blur border-b border-slate-800",

  /** Table header cell */
  headerCell: "px-4 py-3 text-xs uppercase tracking-wider text-slate-400 font-semibold",

  /** Standard table cell */
  cell: "px-4 py-3 text-sm",

  /** Table row */
  row: "border-b border-slate-800 last:border-0 hover:bg-slate-800/30 transition-colors",

  /** Monospace cell for numbers */
  numberCell: "px-4 py-3 text-sm font-mono text-right",
} as const;

/**
 * Button Styles
 *
 * Extended button utilities beyond shadcn variants.
 */
export const buttonStyles = {
  /** Icon-only button */
  icon: "inline-flex items-center justify-center rounded-lg p-2 transition-colors",

  /** Danger ghost button */
  dangerGhost: "text-rose-400 hover:text-rose-300 hover:bg-rose-500/10 transition-colors",

  /** Success ghost button */
  successGhost: "text-emerald-400 hover:text-emerald-300 hover:bg-emerald-500/10 transition-colors",
} as const;
