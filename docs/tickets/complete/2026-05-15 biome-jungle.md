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
- [x] `dotnet build` passes with no warnings.
- [ ] If T4 landed: a jungle-containing template can roll once jungle + all other biomes in that template are authored.
- [ ] If T5/T6 landed: each authored option renders correctly; timer / click / outcome behavior verified.
- [x] Outcome text fits 128 px width with 6×8 font.

## Learnings

### Architectural decisions
- **Five total encounters (4 pool + 1 substitute-only).** User spec at `Desktop/encounters.md` drove the roster. Pool: CarnivorousPlant, Quicksand, NanerTree, LongAbandonedVillage. Substitute-only: NanerBird (chained from NanerTree.Climb). **Knowing AC deviation:** ticket said "1-3 encounters"; user wanted 4-in-pool. Captured here so the next reader doesn't re-litigate.
- **Quicksand ships single-option, single-encounter authored once for two uses.** Same data-model carve-out as river `Rapids` / cave `GlowingMushrooms` (knowing AC violation; option's `Outcomes` still non-empty). Quicksand is *both* in the pool *and* a substitute target from `LongAbandonedVillage.Explore`. Per `docs/adventures.md` §Outcomes, substitute targets don't have to be pool-excluded — confirmed pattern works.
- **First cross-biome substitute.** `LongAbandonedVillage.Explore` substitutes into grasslands `Snake`. Previously every substitute chain was within-biome (HollowLog → GiantToad/Mushrooms, LoneTree → FightSquirrel, Sandcastle → StartledCrab, NanerTree → NanerBird). The data model already supports cross-biome substitutes — `SubstituteOutcome.NewEncounter` is just an `Encounter`, no biome constraint — and jungle is the first to exercise it. Saves authoring a "jungle snake" duplicate; the grasslands snake encounter is reused intact.
- **NanerBird's Listen ships single-outcome.** Same deferral pattern as grasslands `Mushrooms` "Eat one" and cave `GlowingMushrooms`: the intended outcome is a `BiomeShiftOutcome` to Umbra, which doesn't exist yet. `biome-umbra.md` is the follow-up ticket that lands the shift mechanic; this ticket adds NanerBird to the deferral list. Single-outcome options are valid by convention (`docs/adventures.md` §Encounters: "Single-outcome options are valid when the design wants deterministic feedback") so the data model is happy.
- **`LongAbandonedVillage` enum name preserved; display name shortened.** Spec said "Long-Abandoned Village" (22 chars) which overflows the ~21-char 6×8-font budget. Display name landed as `"Abandoned Village"` (17 chars) — first display-name-vs-enum-name divergence in the codebase. Enum name stays `LongAbandonedVillage` because code has no viewport budget; design doc captures the reason.
- **Long-Abandoned Village's `Explore` has 4 outcomes (25/25/25/25), not 3 (33/33/33).** Spec said "33/33/33" but listed four branches (machinery, quicksand, snake, nothing). Confirmed literal-list interpretation with user. This is the highest-variance option in the codebase today (previous max: HollowLog `Crawl through` at 3 outcomes).

### Problems encountered
- **NanerBird's Listen outcome referenced an undelivered mechanic.** User spec at `Desktop/encounters.md` wrote "transported to umbra (see completed grasslands and cave tickets for how to handle umbra encounter)". `BiomeShiftOutcome` is not yet implemented — `biome-umbra.md` is still in `docs/tickets/`, not `docs/tickets/complete/`. Caught at ticket-read time by grepping for `BiomeShiftOutcome` (only doc hits, no code). Resolved by deferring — same pattern as grasslands Mushrooms and cave GlowingMushrooms. Asked user to confirm defer-vs-block; they picked defer.

### Interesting tidbits
- **Cross-biome substitute keeps the encounter roster compact.** When the design wanted a snake in the abandoned village, the cheap path was to author a new `JungleSnake` enum + EncounterInfo. The cheaper path was to reuse the existing `Snake` from grasslands. The data model allows it (substitute target is just an `Encounter`), the runtime allows it (no biome-check on the target), and `docs/adventures.md` §Outcomes explicitly endorses it. Net: less authoring, broader chain shape.
- **Pool / substitute-target overlap is a valid shape.** Quicksand sits in `Biome.Jungle.PossibleEncounters` *and* is a `SubstituteOutcome` target from `LongAbandonedVillage.Explore`. Both reachable, both authored once. Beach's `StartledCrab` is the inverse — substitute-only, not in the pool. Both patterns ship in the same codebase; per-encounter design intent picks.
- **The encounter roster now has its first 4-outcome option** (`Explore` on `LongAbandonedVillage`). The data model handles it transparently — `Outcome[]` is just an array, RNG.Next picks uniformly. The 4-outcome shape is a useful expressiveness signal: when a designer wants high-variance "explore the unknown" feel, the model already supports it.

### Workarounds / limitations
- **Display name had to be shortened to fit the 6×8 / 128 px budget.** Not a structural limitation — the rendering layer (post-T6) will be the real arbiter of "does it fit". Picked the conservative shortening today; can revisit when actual outcome text rendering lands.

### Related areas affected
- `PetDoodle.Data/Encounter.cs` — five new enum values (`CarnivorousPlant`, `Quicksand`, `NanerTree`, `NanerBird`, `LongAbandonedVillage`).
- `PetDoodle/Encounters/EncounterExtensions.cs` — five new dictionary entries.
- `PetDoodle/Biomes/BiomeExtensions.cs` — `Biome.Jungle` `PossibleEncounters` rebuilt from `[]` to four entries.
- `docs/biomes/jungle.md` — new design doc.
- `docs/tickets/biome-umbra.md` — gains a third deferred outcome to wire up when it lands: `NanerBird.Listen`'s second outcome (BiomeShiftOutcome → Umbra). Adds to the existing Mushrooms / GlowingMushrooms deferred list.

### Rejected alternatives
- **Author a `JungleSnake` enum + EncounterInfo for the Village substitute.** Rejected in favour of cross-biome substitute into grasslands `Snake`. Saved a duplicate authoring pass and exercised the cross-biome pattern for the first time.
- **Keep "Long-Abandoned Village" as the display name.** Rejected: overflows the 6×8 / ~21-char budget at 22 chars. Code name `LongAbandonedVillage` preserves the spec's word.
- **Block jungle until `biome-umbra` lands** (so NanerBird's Listen can ship with the full Umbra outcome). Rejected: same deferral pattern as grasslands Mushrooms / cave GlowingMushrooms, which already shipped. Defer + capture in design doc + add to biome-umbra's append list. User confirmed.
- **Three pool encounters (drop one).** Rejected: user spec listed four. The "1-3" range in the ticket AC is a default, not a hard cap; jungle's higher template-appearance count (3 of 6 templates) justifies the wider roster.
- **Quicksand as substitute-only.** Rejected: user picked pool + substitute (both). Substitute targets don't have to be pool-excluded; Quicksand is both directly rollable and chain-reachable.
