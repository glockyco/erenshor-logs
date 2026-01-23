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
  base: "rounded-lg border border-stone-700 bg-stone-800",

  /** Interactive card (clickable, hoverable) */
  interactive:
    "cursor-pointer hover:border-amber-600/50 hover:bg-stone-800/80 transition-all duration-200",

  /** Active/selected card state */
  active: "border-2 border-amber-600 bg-stone-800 shadow-lg",

  /** Card with elevated shadow */
  elevated: "shadow-lg",
} as const;

/**
 * Table Styles
 *
 * Consistent table cell and row styling.
 */
export const tableStyles = {
  /** Sticky table header */
  header: "sticky top-0 bg-stone-900/95 backdrop-blur border-b border-stone-800",

  /** Table header cell */
  headerCell: "px-4 py-3 text-xs uppercase tracking-wider text-stone-400 font-semibold",

  /** Standard table cell */
  cell: "px-4 py-3 text-sm",

  /** Table row */
  row: "border-b border-stone-800 last:border-0 hover:bg-stone-800/30 transition-colors",

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
  successGhost: "text-lime-500 hover:text-lime-400 hover:bg-lime-500/10 transition-colors",
} as const;
