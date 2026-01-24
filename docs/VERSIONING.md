# Versioning System

This document describes the automatic git-based versioning system used in the Erenshor Logs project.

## Overview

Erenshor Logs uses **automatic versioning** based on git commit metadata. Versions are generated at build time and follow a **System.Version compatible CalVer format**: `YYYY.M.D.REVISION`.

**Key principles:**
- **No manual version bumps** - Just commit and the version updates automatically
- **Unified versions** - Mod and web app always have identical versions (same git commit)
- **Build-time generation** - Versions are computed during build from git metadata
- **Clean release enforcement** - Release builds fail if working tree has uncommitted changes
- **BepInEx compatibility** - Format works with System.Version class requirements

## Version Format

### Production Build
```
YYYY.M.D.REVISION
```

**Example:** `2026.1.24.72108205`

- `YYYY.M.D` - Date of the git commit (UTC, no leading zeros)
- `REVISION` - Commit hash converted from hex to decimal (e.g., `44c48ad` → `72108205`)

**Why this format?**
- **System.Version compatible**: BepInEx uses System.Version which only accepts integers
- **CalVer semantics**: Date-based versions auto-increment with each commit
- **No leading zeros**: `2026.1.9` not `2026.01.09` (required for integer parsing)
- **Full traceability**: Revision field encodes commit hash as decimal integer
- **Unique per commit**: Two commits on same day have different revision numbers

### Dirty Debug Build
```
YYYY.M.D+COMMITHASH-dirty-YYYYMMDD-HHMMSS
```

**Example:** `2026.1.24.72108205-dirty-20260124-204219`

- Same as production, plus:
- `-dirty` - Indicates uncommitted changes exist
- `YYYYMMDD-HHMMSS` - Timestamp when version was generated

### Fallback (No Git)
```
0.0.0-YYYYMMDD-HHMMSS
```

**Example:** `0.0.0-20260124-204219`

- Used when git is not available or not in a git repository
- `0.0.0` indicates fallback mode
- `YYYYMMDD-HHMMSS` - Timestamp when version was generated

## Where Versions Appear

### Mod (BepInEx Plugin)

**Location:** BepInEx log on startup

**Example output:**
```
[Info   : ErenshorLogs] ErenshorLogs v2026.1.24.72108205 loaded
```

**Implementation:**
- `GetGitVersion` MSBuild target in `ErenshorLogs.csproj`
- Generates `PluginInfo.g.cs` with version constant
- Runs before compilation via `BeforeTargets="GeneratePluginInfo"`

### Web Application

**Location 1: Settings Drawer**
- Open Settings (gear icon in top-right)
- Version displayed at bottom with copy button
- Monospace font (`font-mono text-xs`), muted styling (`text-stone-500`)

**Location 2: Browser Console**
- Logged on app initialization
- Format: `[Erenshor Logs vX.Y.Z] Initialized`

**Implementation:**
- `scripts/generate-version.js` generates `web/src/lib/version.ts`
- Runs via `predev` and `prebuild` npm scripts
- Generated file is gitignored

### CLI

**Command:** `uv run erenshor version`

**Output (clean tree):**
```
2026.1.24.72108205
```
(displayed in green)

**Output (dirty tree):**
```
2026.1.24.72108205-dirty-20260124-204219
⚠️  Working tree has uncommitted changes
```
(displayed in yellow)

**Output (no git):**
```
0.0.0-20260124-204219
⚠️  Git not available, using fallback version
```
(displayed in red)

## Build Enforcement

### Debug Builds

**Behavior:** Allow uncommitted changes, append timestamp

**Mod:**
```bash
cd mod
dotnet build -c Debug
```

**Web:**
```bash
cd web
pnpm dev      # Development server
pnpm build    # Production build
```

### Release Builds

**Behavior:** Fail with error if working tree is dirty

**Mod:**
```bash
cd mod
dotnet build -c Release
```

**Error message if dirty:**
```
error : ❌ Cannot build Release with uncommitted changes. Commit or stash your changes first. Run 'git status' to see uncommitted changes.
```

**Success requires:**
- All changes committed
- Working tree clean (`git status` shows no modifications)

## Implementation Details

### Mod (C#)

**File:** `mod/ErenshorLogs.csproj`

**MSBuild Target:**
```xml
<Target Name="GetGitVersion" BeforeTargets="GeneratePluginInfo">
  <!-- Get version from git in semver-compliant CalVer format (YYYY.M.D+COMMITHASH) -->
  <!-- Transform YYYY-MM-DD+hash to YYYY.M.D+hash (remove leading zeros for semver) -->
  <Exec Command="git log -1 --format=&quot;%cs+%h&quot; | sed 's/-/./g' | sed 's/\.0\([0-9]\)/.\1/g'"
        ConsoleToMSBuild="true"
        IgnoreExitCode="true">
    <Output TaskParameter="ConsoleOutput" PropertyName="GitVersionRaw" />
    <Output TaskParameter="ExitCode" PropertyName="GitExitCode" />
  </Exec>

  <!-- For Release builds: check if working tree is dirty and fail -->
  <Exec Command="git status --porcelain"
        ConsoleToMSBuild="true"
        Condition="'$(Configuration)' == 'Release'">
    <Output TaskParameter="ConsoleOutput" PropertyName="GitStatusRelease" />
  </Exec>

  <Error Text="❌ Cannot build Release with uncommitted changes..."
         Condition="'$(Configuration)' == 'Release' AND '$(GitStatusRelease)' != ''" />

  <!-- For Debug builds: append -dirty-timestamp if uncommitted changes exist -->
  <!-- ... -->
</Target>

<Target Name="GeneratePluginInfo" BeforeTargets="CoreCompile">
  <!-- Writes version to intermediate output path -->
  <WriteLinesToFile File="$(IntermediateOutputPath)PluginInfo.g.cs" ... />
</Target>
```

**Generated file:** `mod/obj/Debug|Release/netstandard2.1/PluginInfo.g.cs` (in build output, not tracked)

### Web (Node.js)

**File:** `scripts/generate-version.js`

**Usage:**
```bash
node scripts/generate-version.js
```

**Npm hooks:**
```json
{
  "scripts": {
    "version:generate": "node ../scripts/generate-version.js",
    "predev": "pnpm version:generate",
    "prebuild": "pnpm version:generate"
  }
}
```

**Generated file:** `web/src/lib/version.ts` (gitignored)

**Example output:**
```typescript
export const VERSION = '2026.1.24.72108205';
```

### CLI (Python)

**File:** `cli/src/erenshor_dev/commands/version.py`

**Implementation:**
- Uses `subprocess` to run `git log`
- Same logic as web version generator
- Color output via `rich` library

## Troubleshooting

### Release Build Fails: "Cannot build Release with uncommitted changes"

**Cause:** You have uncommitted changes in your working directory.

**Solution:** Commit or stash your changes, or use a Debug build instead:
```bash
# Option 1: Commit changes
git add .
git commit -m "your message"

# Option 2: Stash changes
git stash

# Option 3: Build in Debug mode instead
cd mod && dotnet build -c Debug
```

### Version Shows "0.0.0-..."

**Cause:** Git is not available or you're not in a git repository.

**Solution:**
- Ensure git is installed: `git --version`
- Ensure you're in a git repo: `git status`
- Don't download source as ZIP - use `git clone`

### Mod and Web Versions Don't Match

**Cause:** The components were built from different commits.

**Solution:** Rebuild both from the same commit:
```bash
# After git pull or checkout
cd mod && dotnet build
cd ../web && pnpm build
```

### Web Build Error: "Cannot find module '$lib/version'"

**Cause:** Version file wasn't generated.

**Solution:** Run the version generator manually:
```bash
node scripts/generate-version.js
# Then check that web/src/lib/version.ts exists
```

## Development Workflow

### Normal Development

Debug builds allow uncommitted changes and append `-dirty-{timestamp}`:

```bash
# Make changes
vim mod/SomeFile.cs

# Build (dirty tree is fine)
cd mod && dotnet build -c Debug
# Version: 2026.1.24.72108205-dirty-20260124-210530
```

### Creating a Release

Release builds require a clean working tree:

```bash
# Commit all changes
git add .
git commit -m "feat: add new feature"

# Verify clean tree
git status  # Should show "nothing to commit, working tree clean"

# Build release (will fail if tree is dirty)
cd mod && dotnet build -c Release
```

### Checking the Current Version

**CLI:**
```bash
uv run erenshor version
```

**Mod log:**
Check BepInEx log for: `Erenshor Logs v{VERSION} loaded`

**Web app:**
- Open Settings drawer (gear icon)
- Or check browser console: `[Erenshor Logs v{VERSION}] Initialized`





## References

- **Mod version generation**: `mod/ErenshorLogs.csproj` (GetGitVersion target)
- **Web version generation**: `scripts/generate-version.js`
- **CLI version command**: `cli/src/erenshor_dev/commands/version.py`
- **Version display (web)**: `web/src/lib/components/settings/AboutSection.svelte`
- **Version logging (web)**: `web/src/routes/(app)/+layout.svelte`
