import type { AttributionDebug, CombatEventRecord, Session } from "$lib/types";

export interface UnknownSignature {
  sourceMethod: string;
  damageType?: string;
  contextState: "empty" | "wrong" | "partial";
  eventCount: number;
  totalDamage: number;
  totalHealing: number;
  sampleEvent: CombatEventRecord;
  commonParameters: Record<string, string>;
  uniqueStackTraces: string[][];
}

export interface AttributionSummary {
  total: number;
  attributed: number;
  unknown: number;
  inferred: number;
}

export interface DebugAnalysis {
  summary: AttributionSummary;
  signatures: UnknownSignature[];
}

export function analyzeUnknownEvents(session: Session): DebugAnalysis {
  const eventsWithDebug = session.events.filter((event) => event.debug !== undefined);
  const unknownEvents = eventsWithDebug.filter((event) => event.attribution === "unknown");
  const signatureMap = new Map<string, UnknownSignature>();

  for (const event of unknownEvents) {
    const key = createSignatureKey(event.debug!);
    const existing = signatureMap.get(key);

    if (existing) updateSignature(existing, event);
    else signatureMap.set(key, createSignature(event));
  }

  const inferredEvents = session.events.filter((event) => event.attribution === "inferred");
  const signatures = Array.from(signatureMap.values()).sort(
    (a, b) => calculateImpact(b) - calculateImpact(a)
  );

  return {
    summary: {
      total: session.events.length,
      attributed: session.events.length - unknownEvents.length - inferredEvents.length,
      unknown: unknownEvents.length,
      inferred: inferredEvents.length,
    },
    signatures,
  };
}

function createSignatureKey(debugInfo: AttributionDebug): string {
  const contextState = classifyContextState(debugInfo.context);
  const damageType = debugInfo.parameters?.damageType ?? "none";
  return `${debugInfo.sourceMethod}|${damageType}|${contextState}`;
}

function classifyContextState(
  context?: AttributionDebug["context"]
): "empty" | "wrong" | "partial" {
  if (!context || context.stackDepth === 0) return "empty";
  if (context.topContextType === "unknown") return "partial";
  return "wrong";
}

function createSignature(event: CombatEventRecord): UnknownSignature {
  const debugInfo = event.debug!;

  return {
    sourceMethod: debugInfo.sourceMethod,
    damageType: debugInfo.parameters?.damageType,
    contextState: classifyContextState(debugInfo.context),
    eventCount: 1,
    totalDamage: event.kind === "damage" ? event.data.amount : 0,
    totalHealing: event.kind === "heal" ? event.data.amount : 0,
    sampleEvent: event,
    commonParameters: debugInfo.parameters ?? {},
    uniqueStackTraces: [],
  };
}

function updateSignature(signature: UnknownSignature, event: CombatEventRecord): void {
  signature.eventCount += 1;
  if (event.kind === "damage") signature.totalDamage += event.data.amount;
  if (event.kind === "heal") signature.totalHealing += event.data.amount;
}

function calculateImpact(signature: UnknownSignature): number {
  return signature.eventCount * 10 + signature.totalDamage / 100 + signature.totalHealing / 100;
}

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
