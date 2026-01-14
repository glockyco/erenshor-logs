---
name: debugging
description: Debug issues in the mod or web app. Use when encountering errors, crashes, or unexpected behavior.
---

# Debugging Guide

Techniques for debugging the mod and web application.

## Mod Debugging

### BepInEx Logs

Primary log file: `<Erenshor>/BepInEx/LogOutput.log`

This contains:
- Plugin load messages
- Harmony patch results
- Any `Plugin.Log.*` calls
- Exceptions and stack traces

```csharp
// Add logging in your code
Plugin.Log.LogInfo("Event captured");
Plugin.Log.LogWarning("Missing context");
Plugin.Log.LogError("Failed to process event");
Plugin.Log.LogDebug("Detailed debug info");  // Only shown if debug enabled
```

### Enable Debug Logging

In `BepInEx/config/BepInEx.cfg`:

```ini
[Logging.Console]
Enabled = true
LogLevels = All

[Logging.Disk]
LogLevels = All
```

### Common Mod Issues

**Plugin doesn't load**:
- Check `LogOutput.log` for errors during load
- Verify DLL is in `BepInEx/plugins/`
- Check for missing dependencies (Assembly-CSharp, UnityEngine)

**Harmony patch fails**:
- Method signature mismatch - check parameter types
- Method not found - verify method exists in game version
- Look for `HarmonyException` in logs

**Null reference in hook**:
- Game object not initialized yet
- Target destroyed before hook ran
- Add null checks: `if (_attacker?.MyStats == null) return;`

**Events not captured**:
- Hook not registered - check Harmony.PatchAll() ran
- Wrong method hooked - verify in decompiled source
- Context lost - check Prefix/Postfix pairing

### Unity Console (GUI Mode)

Launch game normally (not through Steam) to see Unity's console window.
Shows runtime errors that may not appear in BepInEx logs.

## Web App Debugging

### Browser DevTools

**Console tab**: JavaScript errors, console.log output
**Network tab**: WebSocket messages, failed requests
**Application tab**: localStorage (persisted settings)

### Svelte DevTools

Install the Svelte DevTools browser extension to:
- Inspect component hierarchy
- View and modify store values
- Track reactive updates

### Common Web Issues

**Import fails silently**:
- Check Console for JSON parse errors
- Verify file is valid JSON or gzipped JSON
- Check file matches expected schema

**WebSocket won't connect**:
- Verify mod is running and server started
- Check correct port (default 8765)
- Browser may block non-HTTPS WebSocket - use localhost

**Charts not rendering**:
- Check canvas element exists before creating chart
- Verify data format matches chart expectations
- Check for JavaScript errors in console

**Performance issues**:
- Too many events rendered - implement virtual scrolling
- Derived stores recalculating excessively - add debounce
- Charts updating too frequently - batch updates

### Debug Logging in Web

```typescript
// Conditional logging
const DEBUG = import.meta.env.DEV;

function log(...args: unknown[]) {
  if (DEBUG) console.log('[CombatLog]', ...args);
}
```

## Testing Workflow

### Mod Testing

1. Make code change
2. Build: `dotnet build`
3. Copy DLL to plugins (or configure output path)
4. Launch game
5. Trigger the relevant combat action
6. Check logs and in-game UI
7. Export log if needed for inspection

### Web Testing

1. Make code change
2. Dev server auto-reloads
3. Import a test log file or connect to live game
4. Verify in browser
5. Check Console for errors

### End-to-End Testing

1. Start game with mod
2. Start web app: `pnpm dev`
3. Connect web app to game via WebSocket
4. Perform combat in game
5. Verify events appear in web app
6. Export from game, import in web app
7. Compare live vs imported data

## Log File Locations

| Component | Location |
|-----------|----------|
| BepInEx log | `<Erenshor>/BepInEx/LogOutput.log` |
| BepInEx config | `<Erenshor>/BepInEx/config/` |
| Exported logs | `<Erenshor>/CombatLogs/` (default) |
| Web localStorage | Browser DevTools → Application |
