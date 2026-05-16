# Biome: Umbra (+ BiomeShiftOutcome)

## Context
**Current behavior**: The `Outcome` sealed-record hierarchy has three kinds — `FlavorOutcome`, `SubstituteOutcome` (swap the current step's encounter, same biome), `EndAdventureOutcome` (end the run). No mechanism exists to *replace the remaining steps* of an in-flight adventure with a sub-adventure in a different biome. The `Biome` enum has 9 values; `Umbra` is not among them. The grasslands `Mushrooms` encounter has two outcomes on its "Eat one" option; a third "trippy" outcome was deferred pending this ticket. The cave `GlowingMushrooms` encounter ships single-option (Ignore only) — its "Eat" option was deferred pending this ticket because Glowing Mushrooms are *always* trippy when eaten.

**New behavior**: A new sealed `BiomeShiftOutcome` joins the hierarchy. When applied, it **clears the adventure's remaining steps and replaces them with a freshly-rolled sub-adventure in a different biome** (default: roll N encounters uniformly from the target biome's `PossibleEncounters`). A new `Biome.Umbra` enum value is added with its `BiomeInfo` entry (display name, sky/ground, `PossibleEncounters`) and **at least one authored Umbra encounter** so the shift can resolve into real content. The grasslands `Mushrooms` "Eat one" option gets a third outcome appended (a `BiomeShiftOutcome` targeting Umbra with short trippy flavor text). The cave `GlowingMushrooms` encounter gets an "Eat" option appended (a single-outcome Engage whose only outcome is a `BiomeShiftOutcome` targeting Umbra — always-trippy semantics).

## Prerequisites
- [Biome: Grasslands](./biome-grasslands.md) — authors the `Mushrooms` encounter that this ticket extends with a third outcome. If grasslands lands `Mushrooms` under a different name during its design session, this ticket targets that name instead.
- [Biome: Cave](./biome-cave.md) — authors the `GlowingMushrooms` encounter (single-option Ignore today). This ticket appends an "Eat" option whose only outcome is a `BiomeShiftOutcome` to Umbra.
- [Biome: Jungle](./complete/2026-05-15%20biome-jungle.md) — authors the `NanerBird` substitute-only encounter (reached from `NanerTree.Climb`). Its `Listen` option ships single-outcome today. This ticket appends a second outcome (`BiomeShiftOutcome` → Umbra) so `Listen` becomes a 50/50 between safe flavor and Umbra shift.
- [Outcome Resolution](./outcome-resolution.md) — establishes the `Adventuring.ApplyOutcome` switch over the sealed-record hierarchy. **This ticket adds a `BiomeShiftOutcome` arm to that switch.** Without T6 the new record exists but is never applied; with T6's exhaustive-switch + `UnreachableException` default arm, a `BiomeShiftOutcome` rolled before its arm is wired would crash at runtime. See Open Decision 1 if you want to land this ticket without T6.

## Scope
### In scope
- `PetDoodle.Data/Biome.cs`: add `Umbra` enum value.
- `PetDoodle/Biomes/BiomeExtensions.cs`: add a `BiomeInfo` entry for `Biome.Umbra` (display name, sky/ground colors, `PossibleEncounters`).
- `PetDoodle/Encounters/Outcome.cs`: new sealed `BiomeShiftOutcome` derived record carrying flavor `Text` + enough info to construct the sub-adventure. See Open Decision 2 for shape.
- `PetDoodle.Data/Encounter.cs`: 1+ new `Encounter` enum values for Umbra encounters (final count decided in the design session — aim for at least 1, up to 3).
- `PetDoodle/Encounters/EncounterExtensions.cs`: `EncounterInfo` entries for the new Umbra encounters with full `Options` + non-empty `Outcomes`.
- `PetDoodle/Biomes/BiomeExtensions.cs`: rebuild `Biome.Umbra`'s entry with the authored encounters appended to `PossibleEncounters`.
- `PetDoodle/Encounters/EncounterExtensions.cs`: append a `BiomeShiftOutcome` to the existing `Mushrooms` "Eat one" option's `Outcomes` array (rebuild the `EncounterInfo` record for `Mushrooms`).
- `PetDoodle/Encounters/EncounterExtensions.cs`: append a new "Eat" option to the existing cave `GlowingMushrooms` `EncounterInfo` — single Engage option whose only outcome is a `BiomeShiftOutcome` targeting `Biome.Umbra`. Glowing Mushrooms are *always* trippy when eaten, so unlike grasslands `Mushrooms` (1-in-3), this Eat is deterministic-shift.
- `PetDoodle/Encounters/EncounterExtensions.cs`: append a second outcome to the jungle `NanerBird` "Listen" option's `Outcomes` array — a `BiomeShiftOutcome` targeting `Biome.Umbra`. Result: `Listen` goes from single-outcome flavor ("Words made no sense.") to a 50/50 between flavor and Umbra shift.
- `PetDoodle/GameStates/Adventuring.cs`: add a `BiomeShiftOutcome` arm to the `ApplyOutcome` switch (introduced by T6). Implementation: clear `RemainingSteps`, append a freshly-rolled sub-adventure for the target biome, save, then call `EnterCurrentStep` to load the first new step into the option UI. (Same internal helpers as `SubstituteCurrentEncounter` — save + `EnterCurrentStep` — just with a multi-step replacement instead of a single-step swap.)
- `docs/biomes/umbra.md`: design doc capturing Umbra's aesthetic, encounter intents, and any flavor notes — mirrors the per-biome design-doc pattern.

### Out of scope
- Adding `Umbra` to the existing 6 adventure templates in `AdventureGenerator`. Umbra is reachable **only** via `BiomeShiftOutcome` for this iteration — a "secret" / discovered biome.
- Recursive biome-shifts (e.g. Umbra → some other biome). Authored only if a design need surfaces; not required for this ticket.
- Per-encounter art / sprites for Umbra encounters. Like all biomes, Umbra renders sky + ground band + encounter-name text only.
- Generalising `BiomeShiftOutcome` into a stat-effect-bearing outcome (e.g. "lose hunger and shift biome"). Stat systems don't exist; YAGNI.
- Stat / inventory / mood effects from eating the trippy mushroom beyond the biome shift itself.

## Relevant Docs & Anchors
- `docs/adventures.md` §Outcomes — current taxonomy this ticket extends. §Conventions — sealed-record hierarchy + compiler-flagged exhaustiveness via switch expressions.
- `docs/tickets/biome-grasslands.md` — parent ticket authoring `Mushrooms`. Read its final shape (design doc at `docs/biomes/grasslands.md`) when picking the exact option label and outcome-array index to append to.
- `docs/tickets/outcome-resolution.md` — T6, the home of the `ApplyOutcome` switch this ticket extends. Read the existing arms before adding a new one.
- `docs/tickets/complete/2026-05-15 biome-and-encounter-info-pattern.md` — `Outcome` hierarchy origin; explains why these records live in `PetDoodle` (not `PetDoodle.Data`) and reference `Encounter` / `Biome` from the data project.
- Code anchors:
  - `Outcome` (`PetDoodle/Encounters/Outcome.cs`) — extend here.
  - `Biome` (`PetDoodle.Data/Biome.cs`) — append `Umbra`.
  - `BiomeExtensions.Info` static-ctor (`PetDoodle/Biomes/BiomeExtensions.cs`) — add the Umbra entry; count-sanity-check will fire if the dictionary entry is missed.
  - `EncounterExtensions.Info` static-ctor (`PetDoodle/Encounters/EncounterExtensions.cs`) — Umbra encounter entries land here; static-ctor non-empty-Outcomes sanity check still applies.
  - `Adventuring.ApplyOutcome` (post-T6) — add `BiomeShiftOutcome` arm.
  - `Adventuring.SubstituteCurrentEncounter` / `EnterCurrentStep` (post-T6) — model the new helper on these (save + rebuild option UI).
  - `AdventureGenerator.TryRoll` (`PetDoodle/Adventures/AdventureGenerator.cs`) — reference for "uniform pick from `biome.GetInfo().PossibleEncounters`"; mirror that idiom inside the new helper.

## Constraints & Gotchas
- **`PetDoodle.Data` is zero-deps.** `BiomeShiftOutcome` lives in `PetDoodle` (alongside the rest of `Outcome`), not in the data project — it references the `Outcome` base record which is already in `PetDoodle/Encounters/`. `Biome` (referenced by `BiomeShiftOutcome`) is a plain enum in the data project — safe to reference from a `PetDoodle` record.
- **Exhaustive switch on sealed-record hierarchy is *runtime*-enforced, not compile-time.** T6 places a `default: throw new UnreachableException()` arm. Adding a new sealed derived record without a matching arm = silent compile + loud runtime crash on first roll. **The new arm must land in the same commit as the new record.** If splitting the ticket, see Open Decision 1.
- **Template filter is unaffected.** `AdventureGenerator.TryRoll` filters templates by `biome.GetInfo().PossibleEncounters.Length > 0`. Umbra is not in any template, so the filter doesn't gate on Umbra's pool. **However**, the `BiomeShiftOutcome` apply path *does* gate on Umbra's pool — if you ship `BiomeShiftOutcome` with a target biome that has an empty pool, the `Random.Shared.Next` call inside the helper will throw (or pick from an empty array, depending on implementation). Acceptance criterion below: Umbra must have ≥1 authored encounter at the close of this ticket.
- **`Biome` count-sanity check.** `BiomeExtensions` static ctor asserts `Info.Count == Enum.GetValues<Biome>().Length`. Adding `Umbra` to the enum without the dictionary entry (or vice-versa) fails fast at app startup. Pit-of-success — don't bypass.
- **Outcome text width.** 128 px viewport, 6×8 font ≈ 21 chars per line. User's suggested flavor `"Trippy! Wobbled into the Umbra..."` is ~33 chars — overflows. Shorten in the design session; see Open Decision 3.
- **Save boundary.** The new `BiomeShiftOutcome` apply helper mutates `RemainingSteps` (clear + repopulate). That mutation must pair with a `SaveService.Save` call, same as `ResolveCurrentStep` / `SubstituteCurrentEncounter` / `EndAdventure`. Pit-of-success: helper-method-only mutation; don't sprinkle.
- **`WarningsAsErrors=Nullable`.** `BiomeShiftOutcome.TargetBiome` is a value-type enum (non-null). If Open Decision 2 selects an explicit `AdventureStep[]` payload, those array elements are non-null records. No nullability surprises if the shape is enum + int.
- **Sub-adventure is generated at *apply* time, not at outcome construction.** The pre-rolled-adventure / anti-save-scum invariant in `docs/adventures.md` says the **outer** adventure is pre-rolled; a `BiomeShiftOutcome` is a transition event, and the sub-adventure it spawns is rolled when the shift fires. Player can't save-scum the *roll* of the shift outcome itself (T6 already establishes that outcome rolls aren't persisted until applied), and the sub-adventure becomes part of the persisted state the moment it's spawned — same anti-scum guarantee as the original adventure.

## Open Decisions
1. **Land with T6 prereq vs. data-only scope.** Default: **prereq on T6 + ship the resolver arm in this ticket.** Alternative: scope this ticket to *data shape only* (record + enum + Umbra encounters + Mushrooms outcome), and have the implementer add a `case BiomeShiftOutcome: throw new NotImplementedException()` placeholder arm pre-T6. Rejected by default because authoring a `BiomeShiftOutcome` instance that can roll before its resolver arm exists is a footgun — the moment a Mushrooms "Eat one" rolls onto the trippy outcome, the game crashes. Better to gate on T6.
2. **`BiomeShiftOutcome` payload shape.** Three options:
   - **(a) `BiomeShiftOutcome(string Text, Biome TargetBiome, int StepCount)`** — apply-time rolls `StepCount` encounters uniformly from `TargetBiome.GetInfo().PossibleEncounters`. Simplest; mirrors `AdventureGenerator` per-biome encounter-pick idiom. Default.
   - **(b) `BiomeShiftOutcome(string Text, AdventureStep[] NewSteps)`** — fully authored sub-adventure baked into the outcome record. Most control, least re-rollable, lets authors craft specific Umbra step sequences. Heavier authoring surface.
   - **(c) Hybrid `BiomeShiftOutcome(string Text, Biome TargetBiome, int MinSteps, int MaxSteps)`** — random length within bounds. YAGNI.
   - **Default: (a).** YAGNI / KISS; (b) is a follow-up the moment a designer asks for hand-crafted Umbra sequences. **Suggested `StepCount` value for the Mushrooms shift: 2 or 3** — long enough to feel like a detour, short enough not to drag.
3. **Mushrooms trippy outcome text.** Must fit ~21 chars in the 6×8 font (and may render alongside other UI — implementer eyeballs the practical limit). Suggested shortlist:
   - `"Trippy! To the Umbra"` (20)
   - `"Wobble! Umbra calls."` (20)
   - `"Trippy! Wobble..."` (17)
   - `"Reality bends..."` (16)
   - **Default: `"Trippy! To the Umbra"`** — preserves the user's intent in fewer chars. Implementer / user iterates in design session.
4. **`Biome.Umbra` colors.** `DawnBringers16` palette is 16 entries (`docs/colors.md`). Trippy / liminal vibe wants high-contrast, off-natural. Suggested defaults:
   - **Sky: `DarkPurple`. Ground: `Black`** (deep void aesthetic), or
   - **Sky: `Purple`. Ground: `DarkPurple`** (saturated unreal), or
   - **Sky: `Black`. Ground: `Purple`** (negative-space inversion).
   - **Default: Sky `DarkPurple`, Ground `Black`.** Defer to design session; final pick captured in `docs/biomes/umbra.md`. (Confirm exact palette names against `BenMakesGames.MonoGame.Palettes.DawnBringers16` — the entries used elsewhere are `LightBlue`, `DarkGreen`, `Black`, `Brown`, `LightGray`, `DarkGray`, `White`, `Blue`, `Green`, `Yellow`, `DarkBlue`. Substitute `Purple` / `DarkPurple` with whichever closest names exist.)
5. **Umbra encounter count + roster.** Design session output. 1 is the minimum (otherwise the shift can't resolve into anything); 2–3 gives the sub-adventure variety. Suggested seed list (use, swap, or discard):
   - `WhisperingSpore` — "Whispering Spore" — Engage ("Listen") / Ignore ("Hum over it") / Retreat ("Plug ears").
   - `EchoSelf` — "Echo Self" — Engage ("Greet") / Engage ("Mimic") / Retreat ("Look away"). Substitute-outcome candidate: greeting your echo summons a `MirrorBird` (another Umbra encounter).
   - `GlowcapGlade` — "Glowcap Glade" — Engage ("Nibble"; recursion risk: a `BiomeShiftOutcome` back to grasslands? See Out of scope — recursive shifts not authored unless design asks).
   - **Default: 2 Umbra encounters authored**, chosen during the design session. Roster captured in `docs/biomes/umbra.md`.
6. **Outcome record name.** `BiomeShiftOutcome` (default) reads as "the outcome shifts biomes". Alternatives: `SubAdventureOutcome`, `ReplaceAdventureOutcome`, `BiomeJumpOutcome`. **Default: `BiomeShiftOutcome`.** Symmetric with `SubstituteOutcome` (substitutes encounter) / `EndAdventureOutcome` (ends adventure) — verb-noun where the verb describes the mutation scope.
7. **Helper method name on `Adventuring`.** Apply-time helper to do the clear-and-roll. Suggested: `ShiftToBiome(Biome target, int stepCount)`. Alternative: `ReplaceAdventureWith(IList<AdventureStep>)` if Open Decision 2 picks shape (b). Mirrors `SubstituteCurrentEncounter`'s naming density.
8. **Should Umbra encounters be eligible for `SubstituteOutcome` from non-Umbra encounters?** Free-standing question, low stakes: `SubstituteOutcome.NewEncounter` does not check biome membership (per T3 Constraints / `docs/adventures.md` §Outcomes). Default: yes, Umbra encounters are valid substitute targets — but no non-Umbra encounter authored today is expected to use one. Out of scope unless the design session adds it.

## Acceptance Criteria
- [ ] `PetDoodle.Data/Biome.cs` contains `Umbra` as a new enum value.
- [ ] `BiomeExtensions.Info[Biome.Umbra]` is populated: non-empty `DisplayName` (default `"Umbra"`), sky + ground `Color` values from `DawnBringers16`, and `PossibleEncounters` containing every Umbra encounter authored by this ticket.
- [ ] `BiomeExtensions` static-ctor count-sanity check (`Info.Count == Enum.GetValues<Biome>().Length`) still passes — Umbra entry present.
- [ ] `PetDoodle/Encounters/Outcome.cs` contains `public sealed record BiomeShiftOutcome(...) : Outcome(Text)` whose payload is sufficient to construct the sub-adventure (default: `Text`, `TargetBiome`, `StepCount` per Open Decision 2a).
- [ ] At least 1 (up to 3) new `Encounter` enum values authored in `PetDoodle.Data/Encounter.cs` for Umbra encounters.
- [ ] Each new Umbra encounter has an `EncounterInfo` entry in `EncounterExtensions.Info` with ≥2 options; every option has a non-empty `Outcomes` array.
- [ ] `EncounterExtensions` static-ctor sanity check (every option's `Outcomes` non-empty) passes.
- [ ] The `Mushrooms` encounter's "Eat one" option (or whatever the grasslands ticket named the equivalent) has a third outcome of type `BiomeShiftOutcome` whose `TargetBiome == Biome.Umbra`, with flavor text fitting the 128-px viewport at 6×8 font (≤~21 chars).
- [ ] The cave `GlowingMushrooms` encounter gains an "Eat" option (Engage kind) whose `Outcomes` array contains a single `BiomeShiftOutcome` whose `TargetBiome == Biome.Umbra`. Result: cave Glowing Mushrooms goes from single-option (Ignore) to two-option (Eat, Ignore); Eat is a deterministic shift to Umbra.
- [ ] The jungle `NanerBird` encounter's "Listen" option has a second outcome of type `BiomeShiftOutcome` whose `TargetBiome == Biome.Umbra`, with flavor text fitting the 128-px viewport at 6×8 font (≤~21 chars). Result: `Listen` becomes a 50/50 between flavor ("Words made no sense.") and Umbra shift.
- [ ] `Adventuring.ApplyOutcome` (from T6) has an explicit arm for `BiomeShiftOutcome` that: clears `GameData.CurrentAdventure.RemainingSteps`, repopulates it with `StepCount` freshly-rolled `AdventureStep`s targeting `TargetBiome` (uniform pick from `TargetBiome.GetInfo().PossibleEncounters`), calls `SaveService.Save`, then calls `EnterCurrentStep` to enter the first new step. (Adjust if Open Decision 2 picks shape (b) / (c).)
- [ ] No new dependencies added to `PetDoodle.Data` (zero-deps rule).
- [ ] `docs/biomes/umbra.md` exists and describes the biome's aesthetic, the authored encounter set + intents, and the rationale for the chosen sky/ground colors.

## Implementation

### 1. Add `Biome.Umbra` enum value
Append `Umbra` to `PetDoodle.Data/Biome.cs`. Position at the end of the list (existing values are positional; adding at the end avoids shifting any other value's underlying int). The `BiomeExtensions` count-sanity check will fire on app startup until the dictionary entry is added — that's the safety net.

### 2. Add `BiomeShiftOutcome` to the `Outcome` hierarchy
In `PetDoodle/Encounters/Outcome.cs`, add a new `public sealed record BiomeShiftOutcome(string Text, Biome TargetBiome, int StepCount) : Outcome(Text);` (default shape per Open Decision 2a). The record inherits `Text` from the base, mirrors the existing `SubstituteOutcome` / `EndAdventureOutcome` density. **Guard at ctor: `StepCount` must be ≥1** — throw `ArgumentOutOfRangeException` if zero or negative. Pit-of-success: a `StepCount = 0` shift would clear `RemainingSteps` to empty and immediately end the adventure (functionally equivalent to `EndAdventureOutcome` but more surprising) — authors should use `EndAdventureOutcome` for that intent.

### 3. Author Umbra encounters (design session output)
In `PetDoodle.Data/Encounter.cs`, append the new enum values from the design session (e.g. `WhisperingSpore`, `EchoSelf`). In `PetDoodle/Encounters/EncounterExtensions.cs`, add corresponding `EncounterInfo` entries with `DisplayName` + `Options[]` (each option has `Label` + non-empty `Outcomes[]`). Mirror the shape of the encounters authored by the grasslands ticket — same `EncounterInfo` / `EncounterOption` shape, same `Outcome` derived types.

### 4. Add `Biome.Umbra` to `BiomeExtensions.Info`
In `BiomeExtensions.cs`'s static-ctor dictionary literal, add a `[Biome.Umbra]` entry with display name, sky color, ground color (Open Decision 4), and `PossibleEncounters` containing the Umbra encounters from step 3. Keep the static-ctor count-sanity-check call site unchanged — it'll pass automatically once enum + dictionary are in sync.

### 5. Append the trippy outcome to `Mushrooms`
Locate the `Mushrooms` (or grasslands-implementer-chosen-name) entry in `EncounterExtensions.Info`. Rebuild its `EncounterInfo` record (records are immutable; replace the dictionary entry) with the same options as before, except the "Eat one" option's `Outcomes` array gains a third element: `new BiomeShiftOutcome("Trippy! To the Umbra", Biome.Umbra, /* StepCount */ 2 or 3 per Open Decision 2)`. The other Mushrooms outcomes (already authored by grasslands) stay untouched. Net effect: `Random.Shared.Next(option.Outcomes)` now has 1-in-3 chance of rolling the trippy outcome.

### 5b. Append the "Eat" option to cave `GlowingMushrooms`
Locate the `GlowingMushrooms` entry in `EncounterExtensions.Info` (currently single-option). Rebuild its `EncounterInfo` record with the existing option plus a new "Eat" option:
```
new EncounterOption
{
    Label = "Eat",
    Outcomes = [new BiomeShiftOutcome("Trippy! To the Umbra", Biome.Umbra, /* StepCount */ 2 or 3)],
}
```
Unlike grasslands `Mushrooms` where the shift is one of three outcomes (1-in-3 roll), cave Glowing Mushrooms' Eat ships with **only** the shift outcome — eating glowing mushrooms is deterministically trippy. Update `docs/biomes/cave.md` to remove the deferral note and document the now-two-option encounter.

### 6. Add the `BiomeShiftOutcome` arm to `ApplyOutcome`
In `Adventuring.cs`, locate the `ApplyOutcome` switch (introduced by T6). Add an arm before the `default`:
```
case BiomeShiftOutcome b:
    ShiftToBiome(b.TargetBiome, b.StepCount);
    break;
```
Then add the helper method:
- `private void ShiftToBiome(Biome target, int stepCount)`:
  - Pull the target's `PossibleEncounters` via `target.GetInfo().PossibleEncounters`.
  - Build a fresh list of `stepCount` `AdventureStep`s: each step is `new AdventureStep(target, Random.Shared.Next(possibleEncounters))`.
  - Mutate the existing `GameData.CurrentAdventure!.RemainingSteps`: `Clear()` then `AddRange(newSteps)` (preserves the same `List<AdventureStep>` reference — safer than reassigning the property if any code anywhere caches it).
  - `SaveService.Save(GameData);`
  - `EnterCurrentStep();` — same helper T5/T6 use to rebuild the option `ButtonList`, reset the 5s option timer, pick a fresh random default, set `CurrentPhase = ChoosingOption`. Reuses all the existing rendering invariants for free.

Defensive: if `possibleEncounters.Length == 0`, throw a loud `InvalidOperationException` (this should be impossible given Acceptance Criterion: Umbra has ≥1 encounter, but the apply path is the canonical home of the assertion). Don't silently fall back to `EndAdventure`.

### 7. Design doc at `docs/biomes/umbra.md`
Mirror the per-biome design-doc pattern (see how `docs/biomes/grasslands.md` lands once the grasslands ticket completes). Capture: biome aesthetic / intent (trippy, unreal, accessible only via mushroom-shift), each authored encounter's flavor and rationale (why these options, why these outcomes), the chosen sky/ground colors and why, and any flavor notes for future expansion (e.g. "future Umbra encounters could include `BiomeShiftOutcome` back to grasslands for a 'come down' arc").

## Test Plan
- [ ] `dotnet build` passes with no warnings.
- [ ] Launch the game. Trigger a grasslands adventure that rolls `Mushrooms`. Click "Eat one" repeatedly across multiple runs — verify all three outcomes appear over enough rolls (sample size ~10–20).
- [ ] When the trippy outcome rolls: confirm the outcome text renders inside the 128 px viewport without overflowing the right edge.
- [ ] After the trippy outcome's auto-advance: confirm the screen transitions into an Umbra step — sky/ground colors match the design-doc choices, encounter name is one of the authored Umbra encounters, and the option `ButtonList` rebuilds with that encounter's options + a fresh 5.0 timer.
- [ ] Confirm the **remaining grasslands steps and any subsequent biome steps from the original adventure are gone** — `save.json` mid-Umbra-shift shows only the new Umbra steps in `RemainingSteps`, not the grasslands tail. This is the key behavioral difference from `SubstituteOutcome`.
- [ ] Resolve the Umbra sub-adventure to completion (click through each Umbra step's options). Confirm return to `Playing` once the new `RemainingSteps` list empties. `save.json` shows `CurrentAdventure: null`.
- [ ] Quit mid-Umbra. Relaunch. Game boots into `Adventuring` with the saved Umbra steps — no idle wait, no replay of the trippy roll, no return to grasslands.
- [ ] Spot-check the `Adventuring.ApplyOutcome` switch: temporarily add a new sealed derived record (`record TestOutcome(...) : Outcome(...)`) and confirm the `default: throw UnreachableException` arm still fires on it (i.e. adding `BiomeShiftOutcome` didn't accidentally fall through). Revert.
- [ ] Confirm `Biome.Umbra.GetInfo().PossibleEncounters.Length >= 1` at startup (debugger / breakpoint). Confirm `Biome.Umbra` is **not** referenced by any template in `AdventureGenerator.Templates` (i.e. Umbra is reachable only via shift).
- [ ] Eyeball each Umbra encounter's option labels and outcome texts for 128 px fit at 6×8 font.
- [ ] Inspect `save.json` shape after a shift: `CurrentAdventure.RemainingSteps` is a fresh list of `(Umbra, <encounter>)` pairs; the pre-shift steps are gone.

## Learnings

### Architectural decisions

- **Reused `ReplaceStepsOutcome` instead of authoring a parallel
  `BiomeShiftOutcome` record.** Between the ticket landing and
  implementation, the rapids-swim-to-shore ticket landed
  `ReplaceStepsOutcome(Text, IReadOnlyList<Biome>)` along with its
  `Adventuring.ApplyOutcome` arm and `ReplaceRemainingSteps` helper.
  That record is strictly more general than the ticket's proposed
  `BiomeShiftOutcome(Text, Biome, int StepCount)` — `[Biome.Umbra,
  Biome.Umbra]` gives the same effect as `(Biome.Umbra, 2)` would, and
  cross-biome lists like `[Jungle, Beach]` are also expressible.
  Two outcome records doing the same job would be a pit-of-failure
  (two arms in the switch, two authoring surfaces, identical apply
  semantics). Surfaced to user; user confirmed reuse.
- **Open Decision 1 (T6 prereq).** T6 (`Adventuring.ApplyOutcome`
  switch with `UnreachableException` default) had already landed, so
  the prereq was satisfied before this ticket started. No data-only
  fallback needed.
- **Open Decision 2 (payload shape).** Resolved by the reuse decision
  above: `ReplaceStepsOutcome`'s shape (b-ish — fully-specified biome
  sequence, encounter rolled at apply time) wins over the ticket's
  proposed (a) (`Biome + StepCount`). All three Umbra triggers ship
  `[Biome.Umbra, Biome.Umbra]` literally.
- **Open Decision 4 (Umbra colors).** Pink sky / DarkPurple ground —
  user-selected over the ticket's default DarkPurple/Black. Chosen for
  saturated-unreal contrast; Pink is otherwise unused as any biome's
  sky and DarkPurple is otherwise unused as any biome's ground, which
  reinforces "this is somewhere else." Rationale captured in
  `docs/biomes/umbra.md`.
- **Open Decision 5 (encounter roster).** User-supplied via
  `encounters.md` desktop file: Lost Spirit (Help / Ignore), Dark River
  (Fish / Search shore), Magic Library (Browse). Three pool encounters,
  not the ticket's default of two. `HungrySpirit` substitute target
  (referenced from `LostSpirit.Help`) was specified in a follow-up
  question per the design-session memory rule about asking for
  undefined substitute targets.
- **AC violation: Magic Library is single-option.** AC #6 requires
  "≥2 options" per encounter; Magic Library ships with one (Browse,
  50/50 flavor). User-driven — same shape of knowing AC violation as
  river `Rapids` / cave `GlowingMushrooms` (pre-this-ticket) / jungle
  `Quicksand`. The single option still has a non-empty `Outcomes[]`
  (two outcomes), so the data-model invariant holds. Captured in
  `docs/biomes/umbra.md` design rationale.
- **Umbra not in any `AdventureGenerator.Templates` entry.** Per ticket
  scope, Umbra is reachable only via `ReplaceStepsOutcome` from three
  trigger encounters (grasslands `Mushrooms.Eat one`, cave
  `GlowingMushrooms.Eat`, jungle `NanerBird.Listen`). First "secret"
  biome — discovered, not rolled.
- **Recursive shifts (Umbra → other biomes) deliberately not authored.**
  Out of scope per ticket. None of the three Umbra pool encounters
  ships a `ReplaceStepsOutcome`; the only end-adventure carrier in
  Umbra is the substitute-only `HungrySpirit`.

### Problems encountered

- **`ReplaceStepsOutcome` already existed mid-ticket.** Initial reading
  of the ticket showed `BiomeShiftOutcome` as a new record to author.
  After completing the design alignment with the user, attempting to
  add `BiomeShiftOutcome` to `Outcome.cs` failed because the file had
  been modified — `ReplaceStepsOutcome` was already there from the
  rapids-swim-to-shore commit landed in parallel. Re-reading caught
  this; surfaced the overlap to user; user pivoted to reuse. Lesson:
  re-read every code anchor file at implementation time, not just at
  Phase 1 orient — parallel work can change the substrate.

### Interesting tidbits

- The 6×8 font / 21-char outcome text budget is well-established
  across all biome design docs. Picking text was a constraint-driven
  exercise — `"Trippy! To the Umbra"` (20 chars) and `"Wobble! Umbra
  calls."` (20 chars) both sit right at the borderline.
- `[Umbra, Umbra]` as a literal pair in three different
  `ReplaceStepsOutcome` calls is a slight smell — if the StepCount-2
  default ever changes, all three call sites need editing. YAGNI says
  leave it; if more shift triggers land, a `[..Umbra * 2]` helper or
  named constant could clean it up. Not worth it for three call sites.

### Workarounds / limitations

- Test Plan items requiring runtime verification (game launch, mid-Umbra
  quit/relaunch, save.json inspection, eyeball text fit) cannot be
  exercised in this implementation pass — flagged for user verification.
  The build + test pass and the static-ctor sanity checks (Biome
  count + non-empty Outcomes) cover the data-model invariants.
- AC #4 ("`BiomeShiftOutcome` derived record exists") is technically
  unmet by the literal text — no `BiomeShiftOutcome` record was
  authored. The user-confirmed reuse of `ReplaceStepsOutcome` is the
  intended substitute; the *behavioral* AC (Umbra shift fires from
  three trigger encounters) is met.
- AC #11 ("`ApplyOutcome` arm for `BiomeShiftOutcome`") similarly N/A
  — the `ReplaceStepsOutcome` arm landed by the rapids ticket is what
  resolves the Umbra shifts.

### Related areas affected

- `docs/biomes/grasslands.md`, `docs/biomes/cave.md`,
  `docs/biomes/jungle.md` — all three updated to remove the
  Umbra-shift deferral notes and document the now-shipped outcomes.
- `docs/biomes/umbra.md` — new design doc covering aesthetic, encounter
  set, color rationale, and design rationale notes.

### Rejected alternatives

- **Authoring `BiomeShiftOutcome` anyway.** User briefly approved this
  via the AskUserQuestion dialog (initial misclick), then corrected
  to reuse `ReplaceStepsOutcome`. Rejected for redundancy — two
  outcome records doing the same job is a pit-of-failure under the
  CLAUDE.md "principle of least surprise" + "YAGNI / KISS" guidance.
- **Open Decision 2 (a) literal: `(Biome, StepCount)` shape.** Strictly
  less general than `ReplaceStepsOutcome(Text, IReadOnlyList<Biome>)`.
  No use case where (a)'s shape is preferable.
- **Open Decision 4 ground = Black.** Ticket default; passed over for
  Pink-sky/DarkPurple-ground per user choice. Black ground would have
  read more like a void / underworld; Pink/DarkPurple reads more
  candy-trippy / dreamlike — better matches Umbra's "the mushrooms
  hit" framing.
- **Larger Umbra roster (3 pool encounters → could have been 2).** User
  supplied 3 encounters via `encounters.md` rather than the ticket
  default of 2. Rejected default in favor of user-driven count.
  Trivially upgradable; no tradeoff.
