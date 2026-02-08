---
stepsCompleted: [1, 2, 3, 4, 5]
inputDocuments: []
date: 2026-02-01
author: Johann
---

# Product Brief: erenshor-logs

## Executive Summary

erenshor-logs is a combat logging and analysis platform for Erenshor, a single-player MMO-inspired RPG. It consists of a BepInEx mod that hooks into the game via Harmony patches to capture granular combat event data, and a static web application (hosted on Cloudflare Pages) that processes and visualizes that data for in-depth theorycrafting and damage optimization.

The project fills a complete gap in the Erenshor ecosystem. The game's built-in combat log is barebones with no analysis functionality, and the only existing third-party tool is a basic DPS meter without ability attribution. erenshor-logs is the first and only tool capable of attributing damage and healing to specific abilities, enabling the kind of detailed combat analysis that theorycrafters need.

The long-term vision is full combat replay - a tick-by-tick timeline of every combat-relevant event (damage, healing, buffs, DoTs, interrupts, regeneration, positioning) that can be scrubbed through like a recording, complete with unit frames and a 2D positional view of all participants.

---

## Core Vision

### Problem Statement

Erenshor players interested in damage optimization and theorycrafting have no reliable tools for combat analysis. The in-game combat log provides raw event output with no breakdowns, filtering, or analysis. The training dummy shows only aggregate DPS over a 1-minute interval. The only third-party alternative is a basic DPS meter that still lacks ability attribution. There is no way to determine which abilities contribute most to damage output, analyze performance across real encounters (bosses, dungeons), or optimize SimPlayer builds with data-driven decisions.

### Problem Impact

Without proper combat analysis tools, theorycrafting in Erenshor is reduced to guesswork. Players cannot identify which abilities to prioritize, whether gear changes improve performance, or how their SimPlayer companions are contributing to encounters. The tight-knit Erenshor community that cares about optimization is left without the foundational data layer that games like World of Warcraft take for granted through tools like WarcraftLogs.

### Why Existing Solutions Fall Short

- **In-game combat log**: Raw text output with no ability attribution, filtering, aggregation, or analysis
- **In-game DPS tracker**: Unreliable, shows only total damage and DPS for a single combat window, no ability breakdown
- **Third-party DPS meter**: Slightly more accurate but still limited to aggregate DPS and total damage numbers, no ability attribution or analysis features
- **No solution exists** that provides per-ability damage breakdowns, encounter-level analysis, or any form of combat replay

### Proposed Solution

A two-component platform:

1. **BepInEx Mod (C#)**: Hooks into Erenshor's combat system via Harmony patches to capture every combat-relevant event with full ability attribution. Streams data to the web app via WebSocket during gameplay. The mod leverages Erenshor's single-player, Mono/Unity architecture for unrestricted access to game internals - data the game itself doesn't track (like which ability caused which hit or heal).

2. **Static Web Application (SvelteKit on Cloudflare Pages)**: Receives, stores, and analyzes combat data entirely in-browser. Provides real-time session monitoring during gameplay and comprehensive post-encounter analysis for theorycrafting. The architecture must scale within the constraints of static hosting - no server-side databases, all processing client-side.

The current MVP has proven the core concept works: the mod successfully captures and attributes combat events that no other tool can. The next phase focuses on rebuilding the data pipeline (WebSocket protocol, client-side storage) on a more scalable foundation and expanding analysis capabilities toward the full combat replay vision.

### Key Differentiators

- **Only tool with ability attribution**: No other mod or in-game feature can tell you which ability caused which damage or heal event. erenshor-logs reverse-engineers this from game internals.
- **Uncontested space**: Zero competition for combat analysis in Erenshor. This is the definitive tool from day one.
- **Full combat replay vision**: Beyond static charts, the long-term goal is a scrubbable timeline with unit frames and 2D positional replay - a level of analysis depth unprecedented for an indie game of this scale.
- **Community-embedded development**: Direct, ongoing dialogue with the target user base ensures the tool evolves based on real player needs rather than assumptions.
- **No platform restrictions**: Erenshor's single-player, Mono/C#/Unity architecture means full access to game state with no anti-cheat, server-validation, or API-restriction concerns.

## Target Users

### Primary Users

#### Persona 1: "The Theorycafter" - Marcus
**Profile:** Endgame player with one or two max-level characters, active on the Erenshor Discord. Spends significant time optimizing builds, testing on the training dummy, and sharing findings with the community. Likely contributes to or references the wiki's BiS gear page.

**Current Experience:** Runs 1-minute training dummy tests repeatedly, eyeballing aggregate DPS numbers. Swaps gear and proficiencies between runs to compare, but has no way to see per-ability breakdowns. Field testing is largely useless for optimization because fights end too quickly for meaningful data, and burst-heavy classes skew results. Shares findings as screenshots or manual write-ups on Discord.

**Needs:** Per-ability damage breakdowns, DPS over time graphs, ability-by-ability comparisons across gear/proficiency configurations. Wants to answer "which abilities contribute most to my damage?" and "is this gear swap actually an improvement?" Also interested in SimPlayer performance to optimize group composition - especially relevant with the upcoming 6th class limiting group slots to 5 out of 6 available classes.

**Success Moment:** Identifies that a specific ability rotation produces 15% more DPS than their previous approach, backed by data rather than gut feeling. Shares a screenshot of the ability breakdown on Discord that settles a community debate about class balance.

#### Persona 2: "The Curious Player" - Lena
**Profile:** Mid-game or recently endgame player, active on Discord or Steam forums. Has noticed their DPS feels low compared to others or that certain SimPlayers seem to massively out-damage them. Already has BepInEx installed for other mods. Wants answers, not necessarily deep optimization.

**Current Experience:** Asks "why is my DPS so low?" on Discord. Gets told their class scales later, or that a specific spell unlocked at a higher level carries certain classes. Has no way to verify this themselves. The in-game DPS tracker gives a number but no context for understanding it.

**Needs:** Aggregate damage breakdowns that show which abilities are doing the heavy lifting - both for their own character and their SimPlayers. Wants to understand relative class/ability contribution without necessarily diving into tick-by-tick analysis. Clear, readable summaries over detailed data tables.

**Success Moment:** Opens the web app after a dungeon run and immediately sees that their SimPlayer Paladin is doing 40% of the group's healing and competitive DPS, answering the question "should I keep this SimPlayer in my group?" with data.

### Secondary Users

#### Community Contributors
Players who write guides, maintain wiki pages (like the BiS gear recommendations), or regularly help others on Discord. They benefit from erenshor-logs as a source of verified data to back up their recommendations. Screenshots of breakdowns (and richer sharing options in future releases) serve as evidence for guide claims. Not a separate persona to design for - they're power-user theorycrafters who happen to share their findings.

#### Experimenters
Players running unconventional compositions (all-paladin groups, niche builds) for fun. They use the same tools as theorycrafters but with a different motivation - curiosity and entertainment rather than strict optimization. The upcoming 6th class and 5-slot group limit will likely increase interest in composition experimentation.

### User Journey

**Discovery:** Word of mouth on Discord or Steam forums. Someone shares a screenshot of an ability breakdown. "How did you get that?" leads to erenshor-logs.

**Onboarding:** Install BepInEx mod (often already installed for other mods), launch Erenshor, open the web app in a browser. WebSocket connection establishes automatically. Player enters combat and data starts flowing.

**Core Usage - Theorycrafters:** Run training dummy tests or dungeon encounters, then switch to the web app for post-encounter analysis. Compare ability breakdowns across sessions. Share screenshots of results on Discord.

**Core Usage - Curious Players:** Glance at the web app after a play session to see aggregate damage numbers and ability breakdowns. Occasional deeper dives when something surprises them.

**Success Moment:** First time seeing a per-ability damage breakdown for an encounter. The data that was previously invisible is now laid out clearly.

**Long-term:** Becomes a standard part of the Erenshor optimization workflow. Players reference session data in Discord discussions. Richer sharing options (generated images, session exports) become the expected evidence format for build recommendations.

## Success Metrics

### User Success

- **First impression**: A new user runs a combat encounter and sees a per-ability damage breakdown for the first time. The data that was previously invisible is immediately clear and answers questions they couldn't answer before.
- **Sustained value**: Theorycrafters rely on per-session analysis as their go-to reference for build decisions. The persistent storage foundation enables future cross-session comparison (deferred to a later release).
- **Actionable insights**: Users can make specific, data-backed recommendations ("prioritize ability A over B", "your spell X damage is low - check your proficiencies") rather than relying on gut feeling or anecdotal evidence.
- **Context-rich analysis**: Combat data is displayed alongside key character context (e.g., class, level, equipment, proficiencies) so users can understand performance in the context of their build rather than as isolated numbers.

### Project Success

This is a passion-driven open-source side project. Success is measured differently than a commercial product:

- **Architecture quality**: The rebuilt data pipeline is clean, robust, and maintainable. The WebSocket protocol transmits data efficiently (no redundant retransmission of static entity data). Client-side storage supports aggregation and analysis queries, not just raw JSON blobs.
- **Technical growth**: The project serves as a vehicle for learning new tools, practicing modern development patterns, and building a portfolio of well-engineered work.
- **Community utility**: The tool is genuinely useful to the Erenshor community. Players reference it in Discord discussions. Screenshots of analysis results back up build recommendations, with richer sharing options planned for future releases.
- **Sustainable constraints**: The static-hosting-only architecture remains viable as features grow. No server costs, no user data liability, no attack surface beyond a static site.

### Key Performance Indicators

Given the project's nature, these are practical indicators rather than business KPIs:

| Indicator | What it measures | How to assess |
|-----------|-----------------|---------------|
| Ability breakdown accuracy | Core value delivery | Damage attribution matches actual game behavior across all classes |
| Session data persistence | Storage architecture viability | Users can store, retrieve, and query across multiple sessions without hitting localStorage limits or performance issues |
| Storage scalability | Foundation for future cross-session work | Sessions persist reliably without hitting storage limits, enabling future cross-session analysis |
| WebSocket payload efficiency | Pipeline architecture quality | Entity metadata transmitted once per session, not repeated per event |
| Time to first insight | Onboarding friction | New user sees meaningful data within minutes of installing the mod and opening the web app |
| Community adoption | Real-world utility | Active users in the Erenshor community who reference erenshor-logs data in discussions |

## Scope

### Core Capabilities (Next Release)

**1. Rebuilt Data Foundation**
- Mod-side persistent structured storage for combat data - enables future in-game UI and historical queries. SQLite is the leading candidate based on initial research.
- Efficient bidirectional protocol - browser requests data on-demand, mod pushes events
- Entity metadata transmitted once per session, referenced by ID thereafter
- Browser-side persistent structured storage for offline analysis without mod running. SQLite via OPFS is the leading candidate based on initial research; alternatives (e.g., IndexedDB) to be evaluated during technical design.

**2. Enhanced Combat Event Capture**
- All current event types (damage, healing, buffs/debuffs with ability attribution)
- Character context: equipment loadout, proficiencies, ascensions, level, class (requires new Harmony hooks and data structures beyond the current ActorRef model)
- Group composition and active auras at session start
- Session metadata: zone, encounter type, duration

**3. Analysis Features**
- Per-ability damage/healing breakdowns (existing, improved)
- Aggregate statistics across a session
- Character build context displayed alongside combat data
- **Live updates during combat** (retained from current implementation)

**4. Dual Frontend Support (Foundation)**
- The mod-side persistent storage and bidirectional protocol serve as the shared data layer: any client (web app, future in-game UI) can query the same data through the same protocol
- Web app remains the primary analysis interface for this release
- In-game UI deferred - acknowledged as non-trivial Unity/BepInEx work

### Out of Scope (Future Releases)

**In-Game UI Implementation**
- Architecture supports it, but building actual in-game screens is deferred
- Unity/BepInEx UI work is non-trivial and warrants its own focused effort

**Full Combat Replay**
- Timeline scrubber with tick-by-tick playback
- Unit frames showing health/mana over time
- 2D positional visualization
- Requires position tracking (not yet implemented)

**Cross-Session Analysis**
- Aggregate statistics across multiple sessions
- Trend analysis over time
- Build comparison tools

**JSON File Import/Export**
- The prototype's AGENTS.md references JSON export as a secondary data path
- Deferred for the rewrite; persistent storage on both sides replaces the immediate need
- May be revisited in a future release for offline sharing or archival

**Advanced Sharing & Community Features**
- Session comparison links
- Community leaderboards (potential future exception - requires cheat validation)
- Integration with external tools

**Map Integration**
- Displaying combat encounters on the interactive map
- Requires position tracking infrastructure

### Constraints

- **Static hosting only**: All processing client-side (browser) or mod-side (game)
- **User-owned data**: No data stored on external servers
- **No ongoing costs**: Cloudflare Pages free tier
- **Offline capability**: Browser app functions without mod using saved data
- **Clean break from prototype**: This is a rewrite, not an incremental upgrade. No migration of data from the existing localStorage-based prototype is planned. Users start fresh.

### Future Vision

The long-term vision is a complete combat analysis platform:

1. **Full replay capability** - Scrub through encounters with unit frames and positional data
2. **In-game integration** - Real-time DPS meter and analysis without leaving the game
3. **Build optimization tools** - Compare configurations, track personal records
4. **Community features** - Share sessions, leaderboards (with integrity validation), wiki integration
5. **Map integration** - Visualize encounters on the interactive Erenshor map
