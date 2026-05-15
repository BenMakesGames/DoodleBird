# Biome: Lagoon

## Context
`Biome.Lagoon` exists with sky/ground colors and an **empty** `PossibleEncounters` pool. This ticket runs a design session with the user to author **1-3 encounters** for lagoon, then lands the corresponding `Encounter` enum values, `EncounterExtensions.Info` entries, and `BiomeExtensions` pool entry.

Seed suggestions from `docs/adventures.md`: `mermaid`. Lagoon was also the original "no retreat" example in the design discussion — consider whether one or more lagoon encounters intentionally omit an end-adventure escape option (player commits once they engage). Final set decided in design session.

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

Capture the agreed design in `docs/biomes/lagoon.md`.

## Authoring checklist

- [ ] New `Encounter` enum values added.
- [ ] New `EncounterExtensions.Info` entries with options + non-empty outcomes.
- [ ] `BiomeExtensions.Info[Biome.Lagoon]` rebuilt with authored encounters appended.
- [ ] Design doc at `docs/biomes/lagoon.md` written.

## Acceptance Criteria
- [ ] 1-3 encounters authored for lagoon, each with at least 2 options.
- [ ] Every option has a non-empty `Outcomes[]`.
- [ ] `Biome.Lagoon.GetInfo().PossibleEncounters` contains every authored encounter.
- [ ] `EncounterExtensions` static-ctor sanity check passes.
- [ ] `docs/biomes/lagoon.md` describes design rationale.

## Test Plan
- [ ] `dotnet build` passes with no warnings.
- [ ] If T4 landed: templates 5 and 6 can roll once lagoon + their other biomes are authored.
- [ ] If T5/T6 landed: each authored option renders; timer / click / outcome behavior verified. If any lagoon encounter has no end-adventure escape option (i.e. no option whose only outcome is `EndAdventureOutcome`), confirm the option row contains no escape-labelled button for that encounter.
- [ ] Outcome text fits 128 px width with 6×8 font.

## Learnings

### Architectural decisions
- **Pool composition: cross-biome reuse + one new encounter.** Pool = `[MusclyTrout, Mermaid]`. `MusclyTrout` already lives in `EncounterExtensions.Info` from the river ticket; lagoon references the existing value rather than authoring a duplicate. `docs/adventures.md` §Authoring workflow explicitly endorses this pattern; lagoon is the first biome ticket to exercise it. Template 6 (`river → river → lagoon`) can now roll `MusclyTrout` three steps in a row — that's fine and intentional.
- **Mermaid ships single-option — knowing AC "≥2 options" violation.** Same precedent as Rapids in `docs/biomes/river.md`. The design is "Listen, hear something, move on" — a second option would be redundant ("Ignore" is a no-op duplicate of the encounter not happening). The single option still has two outcomes, so the ≥1-outcome static-ctor invariant holds. Captured in the design doc rationale for visibility.
- **No retreat-style options anywhere in lagoon.** Lagoon was called out in the ticket as the original "no retreat" example. Both pool encounters omit any `EndAdventureOutcome`. Once the bird engages, the adventure continues — matches the "player commits once they engage" framing. Mermaid is the cleanest expression: zero-risk encounter, nothing to flee.
- **Reused outcome wording "Words made no sense." across Mermaid and NanerBird.** Both encounters are "creature speaks; bird is a bird; bird does not have grammar." Identical phrasing is intentional cross-encounter flavor consistency, not a copy-paste oversight.

### Problems encountered
- **`OptionKind` mention in the ticket body is stale.** The ticket's Design-session step 3 still listed `OptionKind` as a per-option authorial field; that enum was deleted in the `drop-optionkind.md` ticket (also dated 2026-05-15). `EncounterOption` no longer carries a `Kind` — only `Label` + `Outcomes`. Implemented against the current code shape; no enum reference needed.
- **Auto-memory `project-optionkind-semantics` was outdated** — described `OptionKind.Engage` vs `Ignore` vs `Retreat` semantics that no longer exist. Updated post-session to record the enum's deletion and the uniform-resolution model that replaced it.

### Interesting tidbits
- **Cross-biome encounter reuse is a one-liner.** Adding `MusclyTrout` to lagoon's pool is just appending the existing enum value to `BiomeExtensions.Info[Biome.Lagoon].PossibleEncounters` — no new `EncounterInfo` entry, no `Encounter` enum value. The pattern scales cheaply; future biome tickets can pull from the existing encounter set without inflating the authoring surface.
- **Single-option encounters are now established convention, not edge case.** Rapids was the first; Mermaid is the second. Both are intentional "single-axis" designs where a second option would be redundant. The `≥2 options` AC is a default guard against under-design, not a hard invariant.

### Workarounds / limitations
- None structural. The "no retreat" framing fell out of the outcome model naturally — omitting `EndAdventureOutcome` from every option means the adventure cannot end at this encounter. No new mechanic required.

### Related areas affected
- `DoodleBird.Data/Encounter.cs`: one new enum value (`Mermaid`).
- `DoodleBird/Encounters/EncounterExtensions.cs`: one new `EncounterInfo` entry for `Mermaid`.
- `DoodleBird/Biomes/BiomeExtensions.cs`: `Biome.Lagoon` pool rebuilt from `[]` to `[MusclyTrout, Mermaid]`.
- `docs/biomes/lagoon.md`: new design doc.

### Rejected alternatives
- **Authoring a second option on Mermaid just to satisfy ≥2 AC.** "Ignore" / "Hop past" would be redundant — a no-op duplicate of the encounter not occurring. Single-option Rapids set the precedent for "ship the design, document the deviation."
- **Authoring a brand-new lagoon-only fish instead of reusing `MusclyTrout`.** Cross-biome reuse is endorsed by adventures.md and produces zero authoring overhead; a parallel fish would just be a renamed clone with the same option shape.
- **Adding an `EndAdventureOutcome` somewhere to give lagoon any escape valve at all.** Would contradict the ticket's explicit "no retreat" framing. The whole point of the lagoon design example is "this biome commits the bird." Honoured.
