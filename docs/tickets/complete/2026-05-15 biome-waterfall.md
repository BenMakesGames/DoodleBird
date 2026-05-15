# Biome: Waterfall

## Context
`Biome.Waterfall` exists with sky/ground colors and an **empty** `PossibleEncounters` pool. This ticket runs a design session with the user to author **1-3 encounters** for waterfall, then lands the corresponding `Encounter` enum values, `EncounterExtensions.Info` entries, and `BiomeExtensions` pool entry.

No seeds suggested in design doc. Waterfall appears in template 3 (`river → river → waterfall → jungle`) — sits between river and jungle, transitional. Possible themes: spray / hidden cave behind the falls / rainbow visions / cliff-diving / amphibious denizens.

## Prerequisites
- [Biome & Encounter Info Pattern](./biome-and-encounter-info-pattern.md)

## Design session

**To be filled during collaborative session with the user.** For each encounter:

1. **Name** (`Encounter` enum value, PascalCase).
2. **Display name** (user-facing, space-cased).
3. **Options** — variable count. For each option:
   - Label (short string).
   - `OptionKind` (`Engage` / `Ignore` / `Retreat`).
   - **Outcomes** — non-empty list. For each outcome:
     - Text (short flavor string).
     - Effect kind: `FlavorOutcome`, `SubstituteOutcome`, or `EndAdventureOutcome`.
     - For `SubstituteOutcome`: target `Encounter`.

Capture the agreed design in `docs/biomes/waterfall.md`.

## Authoring checklist

- [ ] New `Encounter` enum values added.
- [ ] New `EncounterExtensions.Info` entries with options + non-empty outcomes.
- [ ] `BiomeExtensions.Info[Biome.Waterfall]` rebuilt with authored encounters appended.
- [ ] Design doc at `docs/biomes/waterfall.md` written.

## Acceptance Criteria
- [ ] 1-3 encounters authored for waterfall, each with at least 2 options.
- [ ] Every option has a non-empty `Outcomes[]` and a designed kind.
- [ ] `Biome.Waterfall.GetInfo().PossibleEncounters` contains every authored encounter.
- [ ] `EncounterExtensions` static-ctor sanity check passes.
- [ ] `docs/biomes/waterfall.md` describes design rationale.

## Test Plan
- [ ] `dotnet build` passes with no warnings.
- [ ] If T4 landed: template 3 can roll once river + waterfall + jungle are all authored.
- [ ] If T5/T6 landed: each authored option renders; timer / click / outcome behavior verified.
- [ ] Outcome text fits 128 px width with 6×8 font.

## Learnings

### Architectural decisions
- **Single pool encounter.** Smallest pool authored to date (river: 2, grasslands/cave/beach: 3, jungle: 4). Waterfall is a one-beat biome in template 3 (`river → river → waterfall → jungle`); the falls *are* the biome. Other ticket-suggested themes (rainbow visions, hidden cave behind the falls, cliff-diving, amphib denizens) were considered during the design session and rejected as load-bearing on a single template appearance. YAGNI / KISS.
- **Both options as `Engage`, no `Ignore`.** The bird is already in the river; descent is mandatory. Existing `Ignore` convention is "bird hops past / does not engage hazard" (Snake "Go around", FightSquirrel "Glide to surface", Sandcastle "Ignore") — that doesn't fit waterfall, where both options are active commits to *how* to descend. Captured in the design doc and as the trigger for the follow-up `drop-optionkind.md` ticket (see below).
- **No `Retreat` option.** Same physical reasoning — bird can't flap back upstream against a river about to plunge. Risky Engage (Ride) carries the EndAdventure tail; safe Engage (Glide) continues. Two-option shape, no third.
- **No substitute chains.** Like river / mountain, waterfall ships flat. No `SubstituteOutcome` swaps. Surface stays narrow.
- **Re-used existing color pair (LightBlue / Blue) from biome-info-pattern ticket.** Same palette as `River` — visually continuous when adventure walks river → river → waterfall in template 3. Flat-color rendering can't depict falling water; differentiation is purely the encounter. A future palette pass may differentiate.

### Problems encountered
- **`OptionKind` distinction surfaced as authoring confusion.** When the user tried to assign a kind to "Glide down", they pushed back: "what's the difference between Engage and Ignore?" Investigation confirmed: `Engage` and `Ignore` are mechanically identical today (both planned to call `ResolveCurrentStep` per `outcome-resolution.md`); only `Retreat` is special-cased (short-circuits to `EndAdventure`). User then questioned whether `Retreat` itself earns its keep — every existing Retreat option already has `Outcomes = [new EndAdventureOutcome("Flapped home!")]`, making the kind tag redundant with outcome data. Outcome of the discussion: spawned `docs/tickets/drop-optionkind.md` to collapse `OptionKind` entirely. Waterfall implementation kept `Kind = OptionKind.Engage,` lines for both options because drop-optionkind has not yet landed and the existing `EncounterOption.Kind` is `required`.

### Interesting tidbits
- **`OptionKind.Ignore` semantics depend on whether the bird *can* not-engage.** Across grasslands / beach / jungle / cave / mountain biomes, every `Ignore` option has a coherent "do nothing, hop past" framing. Waterfall broke that — the bird is in the water and committed to descent. This is the first biome where the design forced the question "is `Ignore` even meaningful here?", which then unlocked the broader question about whether `OptionKind` earns its keep at all.
- **Single-encounter biome, two-option encounter, satisfies all AC cleanly.** Earlier biome tickets (river, jungle, cave) had to flag "knowing AC violation" for single-option encounters where the second option needed a deferred mechanic. Waterfall has no such violation — the simple two-option shape was sufficient. First biome with no AC carve-outs since beach / grasslands.

### Workarounds / limitations
- `Kind = OptionKind.Engage` lines in `EncounterExtensions.Info` are temporary — they exist because `EncounterOption.Kind` is still `required`. Once `drop-optionkind.md` lands, those two lines (along with every other `Kind = …` line in the file) get stripped.

### Related areas affected
- Spawned `docs/tickets/drop-optionkind.md` — collapses `OptionKind` enum and rewrites pending tickets (`outcome-resolution.md`, `encounter-option-ui-and-timer.md`, `biome-umbra.md`, `rapids-swim-to-shore.md`, `biome-lagoon.md`, `biome-mountain-peak.md`) to assume the no-kind world.
- Saved memory `project_optionkind_semantics.md` — future design sessions should brief the user on the Engage-vs-Ignore-vs-Retreat semantics up front rather than making them re-derive it.

### Rejected alternatives
- **Multi-encounter pool (rainbow visions, hidden cave, amphib denizen).** Considered during design session per ticket's suggested themes. Rejected as not load-bearing on a single template appearance; YAGNI.
- **Substitute chain (e.g. Ride water down → hidden cave behind the falls).** Considered, rejected. Adds authoring surface for a single-template biome with no clear payoff.
- **`Glide down` as `OptionKind.Ignore`.** Initially proposed by analogy to FightSquirrel "Glide to surface". Rejected after discussion: bird can't *not* descend the falls, so "ignore" doesn't apply. Both options are active commits.
- **`Retreat` option.** Considered (flap back upstream). Rejected as physically incoherent — current pulls bird toward edge.
