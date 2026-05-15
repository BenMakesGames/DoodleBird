# Biome: River

## Context
`Biome.River` exists with sky/ground colors and an **empty** `PossibleEncounters` pool. This ticket runs a design session with the user to author **1-3 encounters** for river, then lands the corresponding `Encounter` enum values, `EncounterExtensions.Info` entries, and `BiomeExtensions` pool entry.

Seed suggestions from `docs/adventures.md`: `hollow log` (shared with grasslands), `muscly trout`, `rapids`. Final set decided in design session.

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

Capture the agreed design in `docs/biomes/river.md`.

## Authoring checklist

- [ ] New `Encounter` enum values added (or shared encounters reused).
- [ ] New `EncounterExtensions.Info` entries with options + non-empty outcomes.
- [ ] `BiomeExtensions.Info[Biome.River]` rebuilt with authored encounters appended to `PossibleEncounters`.
- [ ] Design doc at `docs/biomes/river.md` written.

## Acceptance Criteria
- [ ] 1-3 encounters authored for river, each with at least 2 options.
- [ ] Every option has a non-empty `Outcomes[]`.
- [ ] Every option has a designed kind.
- [ ] `Biome.River.GetInfo().PossibleEncounters` contains every authored encounter.
- [ ] `EncounterExtensions` static-ctor sanity check passes.
- [ ] `docs/biomes/river.md` describes design rationale.

## Test Plan
- [ ] `dotnet build` passes with no warnings.
- [ ] If T4 landed: a river-containing template can roll once river is authored alongside any sibling biomes in that template.
- [ ] If T5/T6 landed: each authored option renders, timer / click / outcome behavior verified.
- [ ] Outcome text fits 128 px width with 6×8 font.
