// Locale-aware formatting utilities
// Uses Intl APIs to respect user's locale settings for number/date formatting

/**
 * Format number with locale-aware thousands separators.
 * en-US: 12,458 | de-DE: 12.458
 */
export function formatNumber(value: number): string {
  return new Intl.NumberFormat().format(value);
}

/**
 * Format duration as human-readable string.
 * Uses abbreviated format common in gaming contexts.
 * 323000 → "5m 23s" | 4354000 → "1h 12m 34s" | 45000 → "45s"
 */
export function formatDuration(ms: number): string {
  const totalSeconds = Math.floor(ms / 1000);
  const seconds = totalSeconds % 60;
  const totalMinutes = Math.floor(totalSeconds / 60);
  const minutes = totalMinutes % 60;
  const hours = Math.floor(totalMinutes / 60);

  if (hours > 0) {
    return `${hours}h ${minutes}m ${seconds}s`;
  }
  if (minutes > 0) {
    return `${minutes}m ${seconds}s`;
  }
  return `${seconds}s`;
}

/**
 * Format timestamp as locale-aware date/time string.
 * Respects user's 12h/24h preference.
 * en-US: "Jan 20, 2:34 PM" | de-DE: "20. Jan., 14:34"
 */
export function formatTime(timestamp: number): string {
  return new Intl.DateTimeFormat(undefined, {
    month: "short",
    day: "numeric",
    hour: "numeric",
    minute: "2-digit",
  }).format(timestamp);
}

/**
 * Format DPS/HPS with 1 decimal place, locale-aware.
 * en-US: 156.3 | de-DE: 156,3
 */
export function formatDps(dps: number): string {
  return new Intl.NumberFormat(undefined, {
    minimumFractionDigits: 1,
    maximumFractionDigits: 1,
  }).format(dps);
}

/**
 * Format a ratio as percentage with 1 decimal place, locale-aware.
 * Input should be a decimal (0.4523 → "45.2%").
 * en-US: "45.2%" | de-DE: "45,2 %"
 */
export function formatPercent(value: number): string {
  return new Intl.NumberFormat(undefined, {
    style: "percent",
    minimumFractionDigits: 1,
    maximumFractionDigits: 1,
  }).format(value);
}
