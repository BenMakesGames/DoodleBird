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

## Learnings

### Architectural decisions

- **2 encounters authored, both pool-only.** Design session landed at `MusclyTrout` + `Rapids` — no shared `HollowLog` (rejected: log's grasslands substitute chain doesn't fit river), no substitute-only encounters. River ships flat — every authored encounter is in the pool, no `SubstituteOutcome` chains today. Smaller surface than grasslands (3 pool + 3 substitute-only).
- **Rapids ships single-option — knowing AC deviation.** Acceptance Criterion says "each with at least 2 options." Rapids has one option (`Avoid rocks`) because the intended second option (`Swim to shore`) needs a biome-shift mechanic that doesn't exist yet. User explicitly approved the deviation; a sibling ticket `docs/tickets/rapids-swim-to-shore.md` was spawned to land the second option post-T6 (depends on `outcome-resolution` + `biome-umbra`). The single option still has 2 outcomes, so the ≥1-outcome invariant is intact.
- **`MusclyTrout` modelled on grasslands `Snake`.** Engage with two flavor outcomes + Ignore with one flavor outcome — "speed-bump" shape. Teaches that not every encounter is dangerous. Snake's precedent of single-outcome `Ignore` options is reused for `Swam past`.
- **`Rapids` modelled on grasslands `GiantToad` / `FightSquirrel` risk shape.** Single Engage option with one Flavor + one EndAdventure outcome = "real fight, you might lose" framing. Difference: no second `Retreat` option today — `Retreat` semantics overlap conceptually with the deferred `Swim to shore`, so leaving `Retreat` off keeps the design coherent when the second option lands.
- **No reuse of grasslands `HollowLog`.** `docs/adventures.md` seeded `hollow log` as a candidate for river (a floating log). Rejected during design session: reusing the encounter pulls in its grasslands substitute chain (Mushrooms, GiantToad) which are biome-fiction-bound to grasslands, not river. Cross-biome shared encounters are *allowed* by the data model but should share *intent*, not just labels — a river log would be a different encounter from a grasslands log.
- **River sky/ground inherited as-is from the biome-info-pattern ticket** (`LightBlue` / `Blue`). Not re-designed. Identical to `Waterfall` today — a future biome ticket may differentiate (palette has limited blue spread).

### Problems encountered

- **`Swim to shore` mechanic doesn't exist yet.** User's spec for Rapids' second option ("replace remaining adventure with [jungle, beach]") needs a `BiomeShiftOutcome`-shaped outcome that clears `RemainingSteps` and repopulates with an authored step list — payload shape (b) per `docs/tickets/biome-umbra.md`'s Open Decision 2, not the default shape (a) which uses `(TargetBiome, StepCount)`. Plus T6's resolver isn't landed. Same class of problem the grasslands implementer hit with `Mushrooms` "Trippy" — and resolved the same way: defer to a follow-up ticket. Spawned `docs/tickets/rapids-swim-to-shore.md` to capture the work + the cross-ticket payload-shape tension as its own Open Decision.
- **Outcome text length spot-check.** Tightest string in this ticket: "Hit a rock. Limped home." at 24 chars — same magnitude as grasslands' "Knocked silly. Home." (20 chars). Both borderline against the 128 px / 6×8 font budget (~17–21 chars next to the bird). Real fit can't be verified until T6 lands and outcome text actually renders.
- **Mid-ticket scope question on bird sprite frame.** User noticed the unused `Bird` sprite frame 1 (sitting pose) and proposed using it for water biomes. Out of scope for biome-river (encounter authoring only) and conflicts with `docs/adventures.md` §Presentation which currently reserves frame 1 for idle/sleep states. Spawned a sibling ticket `docs/tickets/biome-bird-sprite-frame.md` to handle the `BiomeInfo.BirdFrame` property + `Adventuring.Draw` wiring + docs update.

### Interesting tidbits

- **Encounter dictionary insertion order doesn't matter for `FrozenDictionary` lookups**, but it does matter for static-ctor sanity-check stack-trace readability if an assertion fires. New encounters appended at the end of the `EncounterExtensions.Info` literal for easy git-blame.
- **`AdventureGenerator.TryRoll` template filter** gates on `biome.GetInfo().PossibleEncounters.Length > 0`. With river populated, template 3 (river/river/waterfall/jungle) and template 6 (river/river/lagoon) still filter out because waterfall, jungle, lagoon pools remain empty. Template 1 (grasslands/river/jungle/cave) still filters out for the same reason. The filter is doing exactly what it's designed to do.
- **Subagents are great for parallel ticket creation.** Two `/create-ticket` subagents ran in the background while the main implementation continued: one for the post-T6 `Swim to shore` option, one for the water-biome bird-sprite-frame mechanic. Keeps follow-up scope captured without blocking the main work.

### Workarounds / limitations

- **Rapids' single-option AC deviation.** Documented in `docs/biomes/river.md` and in this Learnings block. Not a workaround for a framework limitation — a deliberate scope-management choice. Will be cleaned up by `docs/tickets/rapids-swim-to-shore.md`.

### Related areas affected

- `PetDoodle.Data/Encounter.cs` gained 2 new values (`MusclyTrout`, `Rapids`).
- `PetDoodle/Encounters/EncounterExtensions.cs` gained 2 new dictionary entries.
- `PetDoodle/Biomes/BiomeExtensions.cs`'s `[Biome.River]` entry rebuilt with non-empty `PossibleEncounters`.
- `docs/biomes/river.md` created.
- `docs/tickets/rapids-swim-to-shore.md` created in parallel by a subagent — captures the deferred `Swim to shore` second option for Rapids.
- `docs/tickets/biome-bird-sprite-frame.md` created in parallel by a subagent — captures the water-biome bird-sprite-frame work.

### Rejected alternatives

- **Reuse grasslands `HollowLog` in river pool.** Suggested by `docs/adventures.md`. Rejected because the encounter's existing substitute chain (Mushrooms, GiantToad) is grasslands-flavored, and reusing the encounter pulls those substitutes into river by reference. Cross-biome reuse is fine when the *encounter intent* is biome-neutral; here it isn't.
- **Author `BiomeShiftOutcome` in this ticket for Rapids' second option.** Tempting but: (a) `biome-umbra.md` already plans the record with a different payload shape, (b) T6 resolver not landed → dead code, (c) doubles ticket scope with mechanic-design that should go in its own ticket. Deferred to `rapids-swim-to-shore.md` which captures the payload-shape tension as an explicit Open Decision.
- **Ship Rapids with `Flee` (Retreat) as the second option to satisfy ≥2 AC.** Considered. Rejected because `Retreat` semantically overlaps with the deferred `Swim to shore` — would lock in a design that the follow-up ticket then has to undo. Better to ship single-option and flag the AC deviation than to ship a placeholder Retreat.
- **Roll the bird-sprite-frame work into this ticket.** Out of scope (encounter authoring vs. rendering); also touches docs/adventures.md §Presentation which is shared infrastructure. Spawned as its own ticket to keep biome-river focused.
