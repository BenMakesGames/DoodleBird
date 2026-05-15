# Biome: Grasslands

## Context
`Biome.Grasslands` exists with sky/ground colors and an **empty** `PossibleEncounters` pool. This ticket runs a design session with the user to author **1-3 encounters** for grasslands, then lands the corresponding `Encounter` enum values, `EncounterExtensions.Info` entries, and `BiomeExtensions` pool entry.

Seed suggestions from `docs/adventures.md`: `hollow log`, `snake`. Final encounter set is decided during the design session.

## Prerequisites
- [Biome & Encounter Info Pattern](./biome-and-encounter-info-pattern.md) — needs `Encounter` enum, `EncounterInfo`, `EncounterOption`, `OptionKind`, and the `Outcome` hierarchy in place.

## Design session

**To be filled during collaborative session with the user.** For each encounter:

1. **Name** (`Encounter` enum value, PascalCase).
2. **Display name** (user-facing, space-cased).
3. **Options** — variable count. For each option:
   - Label (short string).
   - `OptionKind` (`Engage` / `Ignore` / `Retreat`).
   - **Outcomes** — non-empty list. For each outcome:
     - Text (short flavor string).
     - Effect kind: `FlavorOutcome` (resolve step), `SubstituteOutcome` (swap current encounter to another `Encounter`), or `EndAdventureOutcome` (cut adventure short).
     - For `SubstituteOutcome`: target `Encounter` (may be a new enum value introduced here, or an existing one from another biome).

Capture the agreed design in `docs/biomes/grasslands.md` for future reference. The file should describe encounter intent, option flavor, and outcome distribution rationale — not just transcribe the code.

## Authoring checklist

After the design session, drop into the codebase:

- [ ] New `Encounter` enum values added to `PetDoodle.Data/Encounter.cs` for any encounters introduced by this ticket. (Shared encounters already in the enum — e.g. `HollowLog` if a sibling biome ticket landed first — are reused, not redeclared.)
- [ ] New entries in `EncounterExtensions.Info` for each new encounter — full `DisplayName` + `Options[]` with kinds and non-empty `Outcomes[]`.
- [ ] `BiomeExtensions.Info[Biome.Grasslands]` rebuilt with the new `PossibleEncounters` array including this biome's authored encounters (preserving any encounters already added by a sibling ticket that shares grasslands — there should be none unless explicitly designed).
- [ ] Design doc at `docs/biomes/grasslands.md` written.

## Acceptance Criteria
- [ ] 1-3 encounters authored for grasslands, each with at least 2 options.
- [ ] Every authored option has a non-empty `Outcomes[]` array.
- [ ] Every authored option has a designed kind (`Engage` / `Ignore` / `Retreat`).
- [ ] `Biome.Grasslands.GetInfo().PossibleEncounters` contains every authored encounter.
- [ ] `Encounter.<Name>.GetInfo()` returns valid `EncounterInfo` for every authored encounter.
- [ ] `EncounterExtensions` static-ctor sanity check still passes.
- [ ] `docs/biomes/grasslands.md` describes the design rationale.

## Test Plan
- [ ] `dotnet build` passes with no warnings.
- [ ] If T4 is landed: launch the game, idle the bird. If grasslands is the only authored biome, only adventure templates fully covered by authored biomes will roll. Confirm at least one such template can run end-to-end through the placeholder `"Continue"` link in `Adventuring`.
- [ ] If T5/T6 are landed: launch, idle, run an adventure containing a grasslands step. Verify each authored option renders, the timer expires correctly, click-to-fire works, and at least one outcome of each kind authored fires (drive multiple rolls).
- [ ] Spot-check each authored outcome's `Text` renders legibly in the 128×32 viewport with the 6×8 font (no overflow off the right edge for the typical text position).
