import type { CombatEvent, AttributionDebugInfo, ContextSnapshot } from "$lib/types";

/**
 * Signature identifying a unique pattern of Unknown attribution.
 * Groups events by source method, damage type, and context state.
 */
export interface UnknownSignature {
  /** Source method where the event originated (e.g., "Character.DamageMe") */
  sourceMethod: string;

  /** Damage type if present in parameters */
  damageType?: string;

  /** Classification of context state (empty/wrong/partial) */
  contextState: "empty" | "wrong" | "partial";

  /** Number of events matching this signature */
  eventCount: number;

  /** Total damage from these events */
  totalDamage: number;

  /** Total healing from these events */
  totalHealing: number;

  /** Sample event for reference */
  sampleEvent: CombatEvent;

  /** Most common parameter values across all events */
  commonParameters: Record<string, string>;

  /** Unique stack traces seen for this signature */
  uniqueStackTraces: string[][];
}

/**
 * Summary of attribution health for a session.
 */
export interface AttributionSummary {
  /** Total number of events */
  total: number;

  /** Events with proper attribution */
  attributed: number;

  /** Events with Unknown attribution */
  unknown: number;

  /** Events with inferred attribution (e.g., melee auto-attacks) */
  inferred: number;
}

/**
 * Result of analyzing Unknown events in a session.
 */
export interface DebugAnalysis {
  summary: AttributionSummary;
  signatures: UnknownSignature[];
}

/**
 * Analyzes combat events to identify and group Unknown attribution patterns.
 * Creates actionable signatures that help identify missing hooks and context issues.
 */
export function analyzeUnknownEvents(events: CombatEvent[]): DebugAnalysis {
  // Filter to events with debug info (Unknown or debug-all enabled)
  const eventsWithDebug = events.filter((e) => e.debugInfo !== undefined);

  // Filter to Unknown events specifically
  const unknownEvents = eventsWithDebug.filter((e) => e.ability.type === "unknown");

  // Group by signature
  const signatureMap = new Map<string, UnknownSignature>();

  for (const event of unknownEvents) {
    const key = createSignatureKey(event.debugInfo!);

    if (!signatureMap.has(key)) {
      signatureMap.set(key, createSignature(event));
    } else {
      updateSignature(signatureMap.get(key)!, event);
    }
  }

  // Sort by impact (frequency * magnitude)
  const signatures = Array.from(signatureMap.values()).sort(
    (a, b) => calculateImpact(b) - calculateImpact(a)
  );

  // Calculate inferred events (auto-attacks without context)
  const inferredEvents = events.filter(
    (e) =>
      e.ability.type === "auto" &&
      e.debugInfo?.context?.topContextName === undefined &&
      e.debugInfo?.context?.stackDepth === 0
  );

  return {
    summary: {
      total: events.length,
      attributed: events.length - unknownEvents.length - inferredEvents.length,
      unknown: unknownEvents.length,
      inferred: inferredEvents.length,
    },
    signatures,
  };
}

/**
 * Creates a unique key for grouping signatures.
 * Groups by source method, damage type, and context state.
 */
function createSignatureKey(debugInfo: AttributionDebugInfo): string {
  const contextState = classifyContextState(debugInfo.context);
  const damageType = debugInfo.parameters?.damageType ?? "none";
  return `${debugInfo.sourceMethod}|${damageType}|${contextState}`;
}

/**
 * Classifies the context state to help diagnose attribution failures.
 */
function classifyContextState(context?: ContextSnapshot): "empty" | "wrong" | "partial" {
  if (!context || context.stackDepth === 0) {
    return "empty"; // No context available at all
  }

  if (context.topContextType === "unknown") {
    return "partial"; // Context exists but is also unknown
  }

  return "wrong"; // Has context but still marked as unknown (timing issue?)
}

/**
 * Creates a new signature from the first event matching this pattern.
 */
function createSignature(event: CombatEvent): UnknownSignature {
  const debugInfo = event.debugInfo!;

  return {
    sourceMethod: debugInfo.sourceMethod,
    damageType: debugInfo.parameters?.damageType,
    contextState: classifyContextState(debugInfo.context),
    eventCount: 1,
    totalDamage: event.amount ?? 0,
    totalHealing: 0, // TODO: Add healing support
    sampleEvent: event,
    commonParameters: debugInfo.parameters ?? {},
    uniqueStackTraces: debugInfo.stackTrace ? [debugInfo.stackTrace] : [],
  };
}

/**
 * Updates an existing signature with data from another matching event.
 */
function updateSignature(signature: UnknownSignature, event: CombatEvent): void {
  signature.eventCount++;
  signature.totalDamage += event.amount ?? 0;

  // Track unique stack traces
  const stackTrace = event.debugInfo?.stackTrace;
  if (stackTrace) {
    const traceString = stackTrace.join("|");
    const exists = signature.uniqueStackTraces.some(
      (existing) => existing.join("|") === traceString
    );
    if (!exists) {
      signature.uniqueStackTraces.push(stackTrace);
    }
  }

  // Update common parameters (keep most frequent values)
  // For now, just keep the first event's parameters
  // Could be enhanced to track frequency of each value
}

/**
 * Calculates impact score for sorting signatures by importance.
 * Higher score = more important to fix.
 */
function calculateImpact(signature: UnknownSignature): number {
  // Weight both frequency and magnitude
  // Frequency is weighted heavily to surface common issues
  return signature.eventCount * 10 + signature.totalDamage / 100;
}

/**
 * Exports debug signatures to CSV format for analysis in spreadsheets.
 */
export function exportSignaturesToCSV(signatures: UnknownSignature[]): string {
  const headers = [
    "Source Method",
    "Damage Type",
    "Context State",
    "Event Count",
    "Total Damage",
    "Sample Stack Trace",
  ];

  const rows = signatures.map((sig) => [
    sig.sourceMethod,
    sig.damageType ?? "none",
    sig.contextState,
    sig.eventCount.toString(),
    sig.totalDamage.toString(),
    sig.uniqueStackTraces[0]?.join(" > ") ?? "",
  ]);

  return [headers, ...rows].map((row) => row.map((cell) => `"${cell}"`).join(",")).join("\n");
}

/**
 * Exports debug signatures to JSON format for sharing or programmatic analysis.
 */
export function exportSignaturesToJSON(
  signatures: UnknownSignature[],
  summary: AttributionSummary
): string {
  return JSON.stringify(
    {
      summary,
      signatures: signatures.map((sig) => ({
        sourceMethod: sig.sourceMethod,
        damageType: sig.damageType,
        contextState: sig.contextState,
        eventCount: sig.eventCount,
        totalDamage: sig.totalDamage,
        commonParameters: sig.commonParameters,
        stackTraces: sig.uniqueStackTraces,
      })),
    },
    null,
    2
  );
}
