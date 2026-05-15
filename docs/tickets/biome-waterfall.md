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
