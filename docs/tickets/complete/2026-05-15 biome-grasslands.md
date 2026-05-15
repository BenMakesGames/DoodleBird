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

## Learnings

### Architectural decisions

- **6 encounters authored, not 1-3.** Ticket scoped 1-3 grasslands encounters, but design landed on 3 pool encounters (HollowLog, Snake, LoneTree) plus 3 substitute targets (Mushrooms, GiantToad, FightSquirrel). `SubstituteOutcome.NewEncounter` requires its target to have a fully-authored `EncounterInfo` entry — `GetInfo()` uses dictionary indexer, throws `KeyNotFoundException` on miss. Cannot ship a `SubstituteOutcome` that points at an unauthored enum value. Either the substitute target gets full options/outcomes, or the design swaps `SubstituteOutcome` for `FlavorOutcome`. We chose the former for richer branching.
- **Substitute targets stay out of `PossibleEncounters`.** Mushrooms, GiantToad, FightSquirrel are reachable only via substitute chains, never via random pool draw. `BiomeInfo.PossibleEncounters` and the dictionary of authored encounters are independent concerns — the pool controls top-of-step rolls; the dictionary controls runtime resolvability. This separation is intentional: keeps the random-roll surface narrow while letting individual paths chain into specifics.
- **Retreat option uses `EndAdventureOutcome` on every outcome.** `docs/adventures.md` says a `Retreat` option "cancels the entire adventure" — but the data model has no `RetreatOutcome`. We model retreat semantics by giving Retreat options outcome lists where every entry is an `EndAdventureOutcome`. Double-locks the contract: the kind says "this option ends the adventure," and the outcome data says the same thing mechanically. The future resolver (T6) can use either signal.
- **`EndAdventureOutcome` shows up on Engage options too.** GiantToad "Peck at it" and FightSquirrel "Peck" both have one Flavor outcome + one EndAdventure outcome. That's how we model "this is a real fight and you might lose." `EndAdventureOutcome` ≠ `Retreat`: Retreat *intends* to end the adventure; an Engage option *can* end it as a bad outcome roll. The hierarchy carries enough expressive range without needing a "BadFightOutcome" or similar.
- **Single-outcome options are valid for deterministic feedback.** Snake's "Go around" and "Intimidate" each have one outcome — the user explicitly wanted "always succeeds" semantics. The data model requires non-empty `Outcomes[]` but does not require length > 1. RNG over a length-1 array still picks deterministically. Good fit when the design wants no variance.
- **Mushrooms "Trippy" outcome deferred to biome-umbra ticket.** Original design wanted a third outcome on "Eat one" that replaces the remaining adventure steps with an Umbra-biome sub-adventure. That mechanic needs (1) a new `Outcome` derived record, (2) a new `Biome.Umbra` enum + `BiomeInfo` entry + Umbra encounter authoring, and (3) resolver support that doesn't exist yet (T6 not landed). All three live in the new `docs/tickets/biome-umbra.md` ticket (spawned in parallel by a subagent during this session). Mushrooms ships today with 2 flavor outcomes; the third gets appended when biome-umbra lands.

### Problems encountered

- **Initial scope question: substitute target authoring.** When the user proposed `Mushrooms` and `GiantToad` as substitute targets from HollowLog, the immediate question was whether the targets need full encounter authoring or could remain as labels-only. Reading `EncounterExtensions.GetInfo()` confirmed the dictionary indexer is the entry point and would throw on missing keys. Surfaced this to the user before authoring; ticket scope expanded from 3 → 6 encounters with explicit user agreement.
- **Trippy-mushroom mechanic needed a new outcome kind.** Caught during the design session that "replaces remaining encounters with Umbra biome" doesn't fit any of `FlavorOutcome` / `SubstituteOutcome` / `EndAdventureOutcome`. Could have invented a `ReplaceAdventureOutcome` here but that would have dragged in: a new Biome enum value, new BiomeInfo entry, new Umbra encounters, and resolver-side wiring (T6 not landed). Pushed back, agreed to defer, spawned a sibling ticket to capture all three pieces coherently.

### Interesting tidbits

- **`Random.Shared.Next(IList<T>)`** is the `BenMakesGames.RandomHelpers` extension `AdventureGenerator` already uses for biome-template and encounter picks. The future outcome resolver (T6) will use the same shape for outcome picks within an option — uniform-random selection. Single-outcome options become trivially deterministic.
- **`SubstituteOutcome` chains aren't recursive in practice** — Mushrooms / GiantToad / FightSquirrel are terminal substitute targets in this biome (neither of their options substitutes into yet another encounter). Nothing in the data model prevents chained substitutes; just no current author intent for them. A future encounter could chain through three substitutions if desired.
- **The 128×32 viewport / 6×8 font budget bites at ~17 chars** for text drawn next to the bird (bird occupies left ~16 px + 4 px gap). Most authored strings fit; "Knocked silly. Home." (20 chars) and "Glide to surface" (16 chars label) are the tightest. Will be revisited when T6 lands and we can see real text placement in `Adventuring.ShowingOutcome` phase.

### Workarounds / limitations

- **Retreat semantics encoded via outcome list, not via kind handling.** The data model's outcome hierarchy doesn't include a `RetreatOutcome` record. We compensated by making every outcome in a Retreat option an `EndAdventureOutcome`. Works fine, but it means the resolver (T6) must either check `OptionKind.Retreat` and short-circuit, OR rely on every Retreat option's outcomes being end-adventure-shaped. The latter is fragile — a future author could add a Flavor outcome to a Retreat option by accident. A `Debug.Assert(option.Kind != OptionKind.Retreat || option.Outcomes.All(o => o is EndAdventureOutcome))` in the static-ctor sanity check would lock this down. Consider for a follow-up.
- **All `dotnet build` warnings cleared** *from our code*. A solution-level `NETSDK1194` warning ("--output not supported when building a solution") fires regardless of code changes — orthogonal to this ticket.

### Related areas affected

- `PetDoodle.Data/Encounter.cs` populated for the first time (6 values added).
- `PetDoodle/Encounters/EncounterExtensions.cs` populated for the first time. Static-ctor sanity check now does real work (was a no-op before).
- `PetDoodle/Biomes/BiomeExtensions.cs` `[Biome.Grasslands]` entry rebuilt with non-empty `PossibleEncounters`.
- `docs/biomes/grasslands.md` created (new directory `docs/biomes/`).
- `docs/tickets/biome-umbra.md` created in parallel by a subagent — captures the deferred Mushrooms "Trippy" outcome along with the new `BiomeShiftOutcome` record, the `Biome.Umbra` enum + content, and T6 resolver wiring.
- `AdventureGenerator` was previously emitting `null` from `TryRoll()` because no biome had a populated encounter pool. With Grasslands now populated, template #1 (grasslands → river → jungle → cave) and template #5 (grasslands → beach → lagoon) still filter out (other biomes empty), but template #2 (grasslands → mountain → mountain peak) does too. Until at least one *other* biome ticket lands, `TryRoll()` continues to return `null`. Not a defect — the filter is designed exactly for this.

### Rejected alternatives

- **Drop substitute outcomes, use pure flavor everywhere.** Would have kept scope at 3 encounters but flattened the design — Hollow Log's "trap" archetype (the docs' worked example) disappears. Trade-off discussed with user; substitute branching kept.
- **Substitute to an existing in-pool encounter (e.g. log crawl → Snake)** instead of authoring new substitute targets. Considered for scope reduction. Rejected because it ties the trap dynamic to whatever happens to be in the pool, which weakens the surprise — players would recognise Snake from regular rolls and the substitute would lose impact.
- **Author `BiomeShiftOutcome` and `Biome.Umbra` inline in this ticket.** Tempting but would have: doubled scope, required Umbra encounter design in the same session, and added an `Outcome` record whose resolver doesn't exist yet (dead code until T6). Spawning the sibling ticket keeps both bodies of work coherent.
- **Add a static-ctor assertion that Retreat options use only `EndAdventureOutcome`.** Considered — would have locked down the Retreat-via-outcome convention. Deferred to a follow-up since the convention is new and may evolve as T6 lands.
