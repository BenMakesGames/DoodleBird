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

- [x] New `Encounter` enum values added.
- [x] New `EncounterExtensions.Info` entries with options + non-empty outcomes.
- [x] `BiomeExtensions.Info[Biome.Mountain]` rebuilt with authored encounters appended.
- [x] Design doc at `docs/biomes/mountain.md` written.

## Acceptance Criteria
- [x] 1-3 encounters authored for mountain, each with at least 2 options.
- [x] Every option has a non-empty `Outcomes[]` and a designed kind.
- [x] `Biome.Mountain.GetInfo().PossibleEncounters` contains every authored encounter.
- [x] `EncounterExtensions` static-ctor sanity check passes.
- [x] `docs/biomes/mountain.md` describes design rationale.

## Test Plan
- [x] `dotnet build` passes with no warnings.
- [ ] If T4 landed: a mountain-containing template can roll (template 2: grasslands → mountain → mountain peak — gated also on grasslands + mountain peak).
- [ ] If T5/T6 landed: each authored option renders; timer / click / outcome behavior verified.
- [x] Outcome text fits 128 px width with 6×8 font. (Longest: "Smashed flat. Home." at 19 chars — under grasslands' 20-char "Knocked silly. Home." baseline.)

## Learnings

### Architectural decisions
- **Fight-or-run authoring stance for mountain.** All 3 pool encounters are 2-option (one risk + Retreat). No safe Engage, no flavor-only speed-bump. Reads as the first overtly perilous biome in the early template set (template 2: grasslands → mountain → mountain peak).
- **Golem + Climb mirror the same shape.** Engage with `Flavor success + EndAdv fail` (50/50), plus Retreat. Resolves cleanly with no third "safe disengage" path. Earlier design pass considered Ignore-as-sneak on Golem; rejected because user wanted mountain options to feel committed.
- **Griffin is the exception — `Ignore` (sneak) replaces Engage.** Griffin is authored as un-fightable: the only options are "sneak past" (Ignore: success-or-EndAdv) and Retreat. Kind reflects intent ("don't engage the apex predator"), even though the option carries end-adventure risk. Mirrors grasslands' Snake "Go around" Ignore-kind, with the added risk tail.
- **No `SubstituteOutcome` chains.** Considered ("Climb dislodges Golem") but rejected — would have widened scope and the flat 2-option shape was already pulling its weight. River set the precedent of "flat biome ships are valid."
- **Retreat outcomes are always `EndAdventureOutcome`.** Matches grasslands authoring convention — kind + outcome data both say "this option cancels the adventure." Resolver-agnostic; pit-of-success contract.

### Problems encountered
- **First design pass was reformulated mid-session.** User said "fight & run — no sneak option," which initially read as a global rule. Griffin then clarified as the exception (un-fightable, sneak-only). Resolution: ask per-encounter rather than apply one stance globally. The doc captures the distinction so future readers don't see Griffin's `Ignore` kind as inconsistent.

### Interesting tidbits
- **`OptionKind.Ignore` is semantically broader than "no-risk pass."** Grasslands' Snake "Go around" Ignore = pure flavor; grasslands' FightSquirrel "Glide to surface" Ignore = pure flavor; mountain's Griffin "Sneak past" Ignore = success-or-EndAdv. The kind communicates *intent* (not engaging the threat), not risk profile. Author flexibility, not author confusion.
- **Mountain has zero substitute-only encounters.** All 3 authored encounters are in the pool, none are substitute-only. Same shape as river.

### Workarounds / limitations
- Test Plan items 2 & 3 (T4 template roll, T5/T6 UI render) cannot be verified yet — those tickets haven't landed. Build + static-ctor sanity check are the only automated verifications available; manual UI walkthrough waits on T5/T6.

### Related areas affected
- `PetDoodle.Data/Encounter.cs` — 3 new enum values appended (LimestoneGolem, SteepClimb, Griffin).
- `PetDoodle/Encounters/EncounterExtensions.cs` — 3 new dictionary entries.
- `PetDoodle/Biomes/BiomeExtensions.cs` — `Biome.Mountain` `PossibleEncounters` rebuilt from `[]` to the 3 authored encounters.
- Template 2 (grasslands → mountain → mountain peak) is now half-gated: mountain peak still has empty pool, so the template remains filtered out until `biome-mountain-peak.md` lands.

### Rejected alternatives
- **3-option Golem (Peck + Topple + Retreat).** Considered; user picked the cleaner 2-option Peck+Retreat shape. Topple's "won't budge / crashes down" outcomes would have been flavor-only, undermining the fight-or-run stance.
- **3-option Climb (Climb + Go around + Retreat).** Considered; user picked 2-option. "Go around" would have been a safe Ignore — clashed with the mountain authoring stance of "every option commits."
- **`SubstituteOutcome` from Climb to Golem.** Considered ("Dislodged a rock! → Golem"); rejected to keep the biome flat. Substitute chains belong to biomes where the trap-style design adds variety (grasslands) — mountain's risk is already in the outcome list.
- **Griffin combat option.** Considered (Peck + Intimidate + Retreat); rejected because user wanted Griffin to read as un-fightable. The `Ignore`/sneak shape carries the narrative load.
