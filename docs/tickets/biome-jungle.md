# Biome: Jungle

## Context
`Biome.Jungle` exists with sky/ground colors and an **empty** `PossibleEncounters` pool. This ticket runs a design session with the user to author **1-3 encounters** for jungle, then lands the corresponding `Encounter` enum values, `EncounterExtensions.Info` entries, and `BiomeExtensions` pool entry.

No seeds suggested in design doc — encounters are open. The user may want dense undergrowth / canopy / predator / fae themes; settle direction during the design session.

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

Capture the agreed design in `docs/biomes/jungle.md`.

## Authoring checklist

- [ ] New `Encounter` enum values added.
- [ ] New `EncounterExtensions.Info` entries with options + non-empty outcomes.
- [ ] `BiomeExtensions.Info[Biome.Jungle]` rebuilt with authored encounters appended.
- [ ] Design doc at `docs/biomes/jungle.md` written.

## Acceptance Criteria
- [ ] 1-3 encounters authored for jungle, each with at least 2 options.
- [ ] Every option has a non-empty `Outcomes[]` and a designed kind.
- [ ] `Biome.Jungle.GetInfo().PossibleEncounters` contains every authored encounter.
- [ ] `EncounterExtensions` static-ctor sanity check passes.
- [ ] `docs/biomes/jungle.md` describes design rationale.

## Test Plan
- [ ] `dotnet build` passes with no warnings.
- [ ] If T4 landed: a jungle-containing template can roll once jungle + all other biomes in that template are authored.
- [ ] If T5/T6 landed: each authored option renders correctly; timer / click / outcome behavior verified.
- [ ] Outcome text fits 128 px width with 6×8 font.
