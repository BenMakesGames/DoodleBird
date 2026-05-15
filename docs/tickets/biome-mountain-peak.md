# Biome: Mountain Peak

## Context
`Biome.MountainPeak` exists with sky/ground colors and an **empty** `PossibleEncounters` pool. This ticket runs a design session with the user to author **1-3 encounters** for mountain peak, then lands the corresponding `Encounter` enum values, `EncounterExtensions.Info` entries, and `BiomeExtensions` pool entry.

No seeds suggested in design doc. Mountain peak is the terminal biome of template 2 (`grasslands → mountain → mountain peak`) — feels like a climactic biome. Possible themes: summit views / extreme weather / divine encounters / rare flora / dizzying heights.

## Prerequisites
- [Biome & Encounter Info Pattern](./biome-and-encounter-info-pattern.md)

## Design session

**To be filled during collaborative session with the user.** For each encounter:

1. **Name** (`Encounter` enum value, PascalCase).
2. **Display name** (user-facing, space-cased).
3. **Options** — variable count. For each option:
   - Label (short string).
   - **Outcomes** — non-empty list. For each outcome:
     - Text (short flavor string).
     - Effect kind: `FlavorOutcome`, `SubstituteOutcome`, or `EndAdventureOutcome`.
     - For `SubstituteOutcome`: target `Encounter`.

Capture the agreed design in `docs/biomes/mountain-peak.md`.

## Authoring checklist

- [ ] New `Encounter` enum values added.
- [ ] New `EncounterExtensions.Info` entries with options + non-empty outcomes.
- [ ] `BiomeExtensions.Info[Biome.MountainPeak]` rebuilt with authored encounters appended.
- [ ] Design doc at `docs/biomes/mountain-peak.md` written.

## Acceptance Criteria
- [ ] 1-3 encounters authored for mountain peak, each with at least 2 options.
- [ ] Every option has a non-empty `Outcomes[]`.
- [ ] `Biome.MountainPeak.GetInfo().PossibleEncounters` contains every authored encounter.
- [ ] `EncounterExtensions` static-ctor sanity check passes.
- [ ] `docs/biomes/mountain-peak.md` describes design rationale.

## Test Plan
- [ ] `dotnet build` passes with no warnings.
- [ ] If T4 landed: template 2 can roll once grasslands + mountain + mountain peak are all authored.
- [ ] If T5/T6 landed: each authored option renders; timer / click / outcome behavior verified.
- [ ] Outcome text fits 128 px width with 6×8 font.
