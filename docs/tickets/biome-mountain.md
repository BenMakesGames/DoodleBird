# Biome: Mountain

## Context
`Biome.Mountain` exists with sky/ground colors and an **empty** `PossibleEncounters` pool. This ticket runs a design session with the user to author **1-3 encounters** for mountain, then lands the corresponding `Encounter` enum values, `EncounterExtensions.Info` entries, and `BiomeExtensions` pool entry.

Seed suggestions from `docs/adventures.md`: `limestone golem`, `steep climb`, `griffin`. Final set decided in design session.

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

Capture the agreed design in `docs/biomes/mountain.md`.

## Authoring checklist

- [ ] New `Encounter` enum values added.
- [ ] New `EncounterExtensions.Info` entries with options + non-empty outcomes.
- [ ] `BiomeExtensions.Info[Biome.Mountain]` rebuilt with authored encounters appended.
- [ ] Design doc at `docs/biomes/mountain.md` written.

## Acceptance Criteria
- [ ] 1-3 encounters authored for mountain, each with at least 2 options.
- [ ] Every option has a non-empty `Outcomes[]` and a designed kind.
- [ ] `Biome.Mountain.GetInfo().PossibleEncounters` contains every authored encounter.
- [ ] `EncounterExtensions` static-ctor sanity check passes.
- [ ] `docs/biomes/mountain.md` describes design rationale.

## Test Plan
- [ ] `dotnet build` passes with no warnings.
- [ ] If T4 landed: a mountain-containing template can roll (template 2: grasslands → mountain → mountain peak — gated also on grasslands + mountain peak).
- [ ] If T5/T6 landed: each authored option renders; timer / click / outcome behavior verified.
- [ ] Outcome text fits 128 px width with 6×8 font.
