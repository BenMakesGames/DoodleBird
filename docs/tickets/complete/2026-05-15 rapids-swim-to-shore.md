# Rapids: Swim-to-shore second option

## Context
**Current behavior**: The `Rapids` encounter (river biome) ships from the parent river ticket with **one** option: `"Avoid rocks"` (Engage) with two outcomes — `FlavorOutcome("Avoided them.")` and `EndAdventureOutcome("Hit a rock. Limped home.")`. This is a deliberate, user-approved deviation from `docs/tickets/biome-river.md`'s own acceptance criterion ("each encounter has ≥2 options") because the intended second option needs a mechanic — replacing the rest of the adventure with a hand-authored two-step sub-adventure across different biomes — that does not exist yet. `docs/biomes/river.md` (written by the parent river ticket) records the gap and points here.

**New behavior**: `Rapids` gains a second option `"Swim to shore"` whose single outcome **clears `GameData.CurrentAdventure.RemainingSteps` and replaces it with two freshly-rolled `AdventureStep`s — one in `Biome.Jungle` and one in `Biome.Beach`** — each step's `Encounter` uniformly rolled from that biome's `PossibleEncounters` pool at apply time. Mechanically this is the "replace-the-remaining-adventure" effect the deferred Mushrooms "Trippy" outcome wants, but with an explicit per-step biome list authored at the option's outcome rather than a target-biome + step-count. After this ticket, the `Rapids` encounter satisfies the ≥2-options convention and the river design doc no longer carries a "TODO: add second option" note.

## Prerequisites
- [Outcome Resolution](./outcome-resolution.md) — T6 introduces `Adventuring.ApplyOutcome`, the exhaustive switch over the sealed `Outcome` hierarchy. This ticket adds (or extends) an arm there. Without T6, the new outcome type exists but is never applied; T6's `default: throw UnreachableException()` arm guarantees that a missing resolver arm crashes loudly the first time the outcome rolls.
- [Biome: Umbra](./biome-umbra.md) — establishes the "replace-the-remaining-steps" outcome record in `PetDoodle/Encounters/Outcome.cs`. See **Open Decision 1** for how this ticket interacts with whichever payload shape `biome-umbra` lands.
- [Biome: Jungle](./biome-jungle.md) — `Biome.Jungle.GetInfo().PossibleEncounters` must be non-empty when this ticket lands, otherwise the apply-time uniform pick for the jungle step has nothing to draw from.
- [Biome: Beach](./biome-beach.md) — same reason, for the beach step.
- (Implicit) [Biome: River](./biome-river.md) — authored `Rapids`. This ticket *rebuilds* the existing `Rapids` `EncounterInfo` to add the second option.

## Scope
### In scope
- `PetDoodle/Encounters/EncounterExtensions.cs`: rebuild the `Encounter.Rapids` `EncounterInfo` entry to add a second `EncounterOption` (`"Swim to shore"`) with a single outcome that performs the two-step biome shift.
- `PetDoodle/Encounters/Outcome.cs`: depending on Open Decision 1, *either* rely entirely on the `BiomeShiftOutcome` shape that `biome-umbra` lands (no new record), *or* add a sibling sealed record (working name `ReplaceStepsOutcome(string Text, AdventureStep[] NewSteps)`) for the "fully authored sub-adventure" shape. Exactly one of those — never both.
- `PetDoodle/GameStates/Adventuring.cs`: depending on Open Decision 1, *either* exercise the existing `BiomeShiftOutcome` resolver arm (if that arm already supports an authored-steps payload), *or* add a new arm for the new sibling record that does: clear `CurrentAdventure.RemainingSteps`, append the authored steps (with apply-time encounter rolls if the payload doesn't already pin encounters), `SaveService.Save`, `EnterCurrentStep`.
- `docs/biomes/river.md`: rewrite the Rapids section to describe both options + the cross-biome detour, and drop the "second option deferred" note.

### Out of scope
- Reworking `BiomeShiftOutcome`'s payload more broadly than what this ticket's option needs. If Open Decision 1 lands on "extend `BiomeShiftOutcome` to accept an authored-steps payload", do the minimum required for this outcome — don't generalise into `MinSteps/MaxSteps`, per-step-biome-roll, or recursive shifts (`biome-umbra` already calls those YAGNI).
- Touching the `Mushrooms` "Trippy" outcome from `biome-umbra`. That outcome's shape decision is owned by `biome-umbra`; this ticket only consumes whatever shape lands.
- Adding new `Encounter` enum values. Both target biomes' pools provide the encounters; this ticket just routes into them.
- Changing the existing `"Avoid rocks"` option or its outcomes.
- Authoring `Biome.Umbra`-style "secret-only" reachability for Jungle/Beach. Both are already pool biomes referenced by adventure templates; this option is just an additional reach path, not the only one.
- Tuning Rapids' option distribution (e.g. making `"Swim to shore"` rarer than `"Avoid rocks"`). The bird's default-option roll and player choice already drive the distribution; no per-option weights exist in the data model and none are added here.

## Relevant Docs & Anchors
- **Design docs**:
  - `docs/biomes/river.md` — the river design doc the parent river ticket creates today; the Rapids section currently documents the single-option gap and points here.
  - `docs/adventures.md` §Outcomes — outcome taxonomy this ticket extends (or consumes the `biome-umbra` extension of).
  - `docs/adventures.md` §Persistence — every `RemainingSteps` mutation must be paired with `SaveService.Save`.
- **Analogue tickets**:
  - `docs/tickets/biome-umbra.md` — the sibling ticket establishing the cross-biome step-replacement mechanic. Read it first; **its** Open Decision 2 (`BiomeShiftOutcome` payload shape) is the upstream pivot this ticket bends to.
  - `docs/tickets/outcome-resolution.md` — T6, home of `Adventuring.ApplyOutcome`. The new arm (if any) lives next to the existing `FlavorOutcome` / `SubstituteOutcome` / `EndAdventureOutcome` arms.
  - `docs/tickets/complete/2026-05-15 biome-grasslands.md` §Learnings — explains why substitute targets need full `EncounterInfo` (the dictionary indexer in `GetInfo()` throws `KeyNotFoundException` on miss). Same principle applies to *every encounter the rolled sub-adventure steps land on* — that's why Jungle and Beach must have non-empty pools as a prerequisite.
- **Code anchors**:
  - `Encounter.Rapids` entry in `EncounterExtensions.Info` static-ctor (`PetDoodle/Encounters/EncounterExtensions.cs`) — rebuild this record.
  - `Outcome` hierarchy in `PetDoodle/Encounters/Outcome.cs`.
  - `Adventuring.ApplyOutcome` (post-T6) — the switch arm this ticket adds to or reuses.
  - `Adventuring.SubstituteCurrentEncounter` / `Adventuring.EnterCurrentStep` (post-T6) — model the resolver helper on these (save + rebuild option UI).
  - `AdventureGenerator.TryRoll` (`PetDoodle/Adventures/AdventureGenerator.cs`) — reference for "uniform pick from `biome.GetInfo().PossibleEncounters`". Mirror that idiom inside whichever resolver arm runs.

## Constraints & Gotchas
- **`AdventureStep` is in `PetDoodle.Data`, which is zero-deps** (per `PetDoodle.Data/CLAUDE.md`). If Open Decision 1 lands on a record carrying `AdventureStep[]`, the record itself still lives in `PetDoodle/Encounters/Outcome.cs` (alongside the other outcomes) and references `AdventureStep` *from* the data project — that's allowed; the data project takes on no new deps.
- **Cross-ticket payload tension.** `biome-umbra`'s Open Decision 2 defaults to shape (a) `BiomeShiftOutcome(string Text, Biome TargetBiome, int StepCount)` — apply-time uniform roll across one target biome's pool, `StepCount` encounters. This ticket needs **two different target biomes** (jungle and beach) in a fixed order. Shape (a) cannot express that directly. See Open Decision 1 for resolutions.
- **`Adventuring.ApplyOutcome` exhaustiveness is runtime-enforced, not compile-time** (per `biome-umbra` Constraints). If a new sealed record is added, its arm must land in the same commit. If reusing `BiomeShiftOutcome` with an extended payload, no new record is added and no new arm is needed — but the existing arm's logic may need to branch on payload shape.
- **`EnterCurrentStep` reset semantics.** After clearing + repopulating `RemainingSteps`, the apply-time helper must call `EnterCurrentStep` to rebuild the option `ButtonList`, reset the 5s option timer, pick a fresh random default, and set `CurrentPhase = ChoosingOption`. Same idiom as `SubstituteCurrentEncounter`. Don't reimplement the rebuild; reuse the helper.
- **Save pairing.** Mutating `RemainingSteps` (clear + addrange) must be followed by `SaveService.Save(GameData)` before `EnterCurrentStep`. Mirrors the existing `ResolveCurrentStep` / `SubstituteCurrentEncounter` / `EndAdventure` save sites. Anti-save-scum invariant: once the player commits to the swim outcome, the new sub-adventure is persisted before they can quit.
- **Outcome text width.** 128 px viewport, 6×8 font ≈ 21 chars per line (cf. `docs/biomes/grasslands.md` design-rationale notes; `biome-umbra` Constraints lists the same budget). The `"Swim to shore"` option label is 13 chars — safe. The outcome text needs to fit too — see Open Decision 3.
- **Empty-pool defence.** If `Jungle` or `Beach` happens to be wired in with an empty `PossibleEncounters` at apply time (e.g. one of the prereq tickets shipped partial content), `Random.Shared.Next(possibleEncounters)` throws / picks from empty. The apply-time helper should throw a loud `InvalidOperationException` rather than silently fall back, same defensive stance `biome-umbra` Implementation step 6 takes. Pit-of-success — the prereq is the contract; the throw is the canary.
- **The two new steps' encounters are rolled at *apply* time, not at outcome construction.** Same anti-save-scum reasoning as the rest of the system: the outcome roll itself isn't persisted until the outcome is applied; once applied, the rolled sub-adventure becomes part of the persisted state. The player can't save-scum the sub-adventure's encounter picks any more than they can save-scum the original adventure's.

## Open Decisions
1. **Payload shape resolution with `biome-umbra`.** The fundamental tension: `biome-umbra` defaults to shape (a) `BiomeShiftOutcome(Text, TargetBiome, StepCount)` — single-biome, count-based, uniform roll. This ticket needs *two* target biomes (jungle, beach) in fixed order. Three options for the implementer (and, where relevant, the user — this is design-adjacent, not pure local taste):
   - **(A) Sibling record.** Land `biome-umbra` with shape (a) for the mushroom shift, then add a separate sealed `record ReplaceStepsOutcome(string Text, AdventureStep[] NewSteps) : Outcome(Text)` in this ticket, with its own `ApplyOutcome` arm. Two outcome records that look similar but encode different authoring shapes. Pros: each record's payload is exactly what its callers need; no payload-shape `if`s inside the resolver. Cons: two records for closely-related effects; future "what's the difference between `BiomeShiftOutcome` and `ReplaceStepsOutcome`?" question.
   - **(B) `biome-umbra` adopts shape (b) for everything.** Lobby for `biome-umbra` to land shape (b) `BiomeShiftOutcome(string Text, AdventureStep[] NewSteps)` instead of shape (a). Mushroom shift then authors `[(Umbra, encounter), (Umbra, encounter)]` explicitly (each step's `Encounter` picked at construction-time or as a stable seed); this ticket authors `[(Jungle, <rolled>), (Beach, <rolled>)]` — but with the encounter rolled at *apply* time, the payload would need to be either (i) lazy / sentinel-valued, or (ii) a list of `(Biome, Encounter?)` where `null` means "roll at apply". That's a fair amount of design ergonomics. Pros: one record. Cons: mushroom shift loses its "uniform roll across umbra's pool" idiom unless authors hand-pick (which is a feature for them — Umbra is a curated experience anyway), and apply-time rolls need an explicit `Encounter?` or a separate `RolledAdventureStep` shape.
   - **(C) `BiomeShiftOutcome` keeps shape (a); extend the payload to a small DSL.** E.g. `BiomeShiftOutcome(string Text, IReadOnlyList<BiomeStepSpec> Steps)` where `BiomeStepSpec` is `record BiomeStepSpec(Biome Biome, Encounter? FixedEncounter)`. Mushroom shift gets `[(Umbra, null), (Umbra, null)]`; this ticket gets `[(Jungle, null), (Beach, null)]`. Hand-authoring a specific sequence is `[(Umbra, WhisperingSpore), (Umbra, EchoSelf)]`. Pros: one record, expressive for all three current/future shapes. Cons: more upfront design for one consumer, mild over-engineering risk if (A) would have sufficed.
   - **Default: (A) sibling record `ReplaceStepsOutcome(string Text, AdventureStep[] NewSteps)`** authored in *this* ticket. Lowest cross-ticket coordination cost — `biome-umbra` ships its default shape (a) unchanged; this ticket adds a clearly-named sibling. The two records can be unified later if a third caller appears with overlap. Implementer should briefly raise with user if (B) or (C) feels meaningfully better at implementation time. The encoded `AdventureStep[]` payload in shape (b)-style records hits the apply-time-roll question: each `AdventureStep` is a `(Biome, Encounter)` pair, so encounters have to be picked at construction. If the encounter pick should be at *apply* time, the new record is `record ReplaceStepsOutcome(string Text, IReadOnlyList<Biome> Biomes) : Outcome(Text)` — the resolver rolls a uniform encounter from each biome at apply time. **Sub-default within (A): `record ReplaceStepsOutcome(string Text, IReadOnlyList<Biome> Biomes)`**, since apply-time rolling preserves the anti-save-scum invariant uniformly with the rest of the system.
2. **Outcome flavor text.** Single outcome, single string. Must fit ~21 chars in 6×8 font @ 128 px. Suggested shortlist:
   - `"Washed ashore."` (15)
   - `"Swept to the shore."` (19)
   - `"Caught a current."` (17)
   - `"To the shore!"` (13)
   - **Default: `"Washed ashore."`** — sets up the jungle-then-beach detour without overpromising. Implementer / user iterates.
3. **Resolver helper name.** If shape (A) sub-default lands (`ReplaceStepsOutcome(Text, IReadOnlyList<Biome> Biomes)`), the apply-time helper on `Adventuring` is e.g. `ReplaceRemainingSteps(IReadOnlyList<Biome> biomes)`. If `biome-umbra` already named its helper `ShiftToBiome`, mirror that density — `ReplaceWithSteps`, `JumpThroughBiomes`, etc. **Default: `ReplaceRemainingSteps`.** Reads as the literal mutation it performs.
4. **Whether the new outcome record (if any) lives in `Outcome.cs` or in a sibling file.** `biome-umbra` lands `BiomeShiftOutcome` in `Outcome.cs` (per its Implementation step 2). **Default: same file.** Splitting Outcome.cs is not justified by record count yet.

## Acceptance Criteria
- [ ] `Encounter.Rapids` in `EncounterExtensions.Info` has **2 options**: the existing `"Avoid rocks"` (unchanged) and a new `"Swim to shore"`.
- [ ] The `"Swim to shore"` option has a non-empty `Outcomes` array containing **exactly one** outcome whose `Text` fits the 128-px-viewport / 6×8-font budget (≤~21 chars, eyeball verified).
- [ ] That outcome's effect, when applied via `Adventuring.ApplyOutcome`, results in `GameData.CurrentAdventure.RemainingSteps` containing **exactly two** new `AdventureStep`s after the apply: the first has `Biome.Jungle` with an `Encounter` drawn uniformly at random from `Biome.Jungle.GetInfo().PossibleEncounters`; the second has `Biome.Beach` with an `Encounter` drawn uniformly at random from `Biome.Beach.GetInfo().PossibleEncounters`. The pre-existing remaining steps (post-Rapids tail of the original adventure, if any) are **gone**.
- [ ] `SaveService.Save(GameData)` fires during the apply (so a quit mid-resolution preserves the new sub-adventure, not the pre-swim state).
- [ ] `EnterCurrentStep` is called during the apply so the option `ButtonList` rebuilds for the rolled jungle encounter, a fresh random default is chosen, the 5s option timer resets, and `CurrentPhase` returns to `ChoosingOption`.
- [ ] `EncounterExtensions` static-ctor sanity check (every option's `Outcomes` non-empty) still passes.
- [ ] If Open Decision 1 lands on (A) with a new record: the record is `public sealed`, lives in `PetDoodle/Encounters/Outcome.cs`, and `Adventuring.ApplyOutcome` has a dedicated arm for it before the `default: throw UnreachableException()`.
- [ ] If `Biome.Jungle.GetInfo().PossibleEncounters` or `Biome.Beach.GetInfo().PossibleEncounters` is empty at apply time, the resolver throws `InvalidOperationException` (loud, not silent fallback).
- [ ] No new dependencies added to `PetDoodle.Data` (zero-deps rule).
- [ ] `docs/biomes/river.md`'s Rapids section now describes both options and the cross-biome detour; the "second option deferred" note is removed.

## Implementation

### 1. Resolve Open Decision 1 before touching code
Pick the payload-shape resolution path with the user (or call (A) sub-default if user is async). Everything downstream branches on this. The rest of the steps below assume (A) sub-default — `ReplaceStepsOutcome(string Text, IReadOnlyList<Biome> Biomes)` — and note in-line where the alternative paths diverge.

### 2. Add the new outcome record (path (A) sub-default only)
In `PetDoodle/Encounters/Outcome.cs`, append:
```
public sealed record ReplaceStepsOutcome(string Text, IReadOnlyList<Biome> Biomes) : Outcome(Text);
```
Constructor guard: `Biomes` must be non-empty (otherwise the resolver clears the adventure and there's nothing to enter — silently equivalent to `EndAdventureOutcome`, which is a footgun). Throw `ArgumentException` if `Biomes.Count == 0`. (If path (B) — extend `BiomeShiftOutcome` — is selected, no new record; modify the existing one in coordination with the `biome-umbra` implementer.)

### 3. Rebuild the `Encounter.Rapids` entry in `EncounterExtensions.Info`
Replace the existing single-option `Rapids` entry with one carrying both options. The first `EncounterOption` (`"Avoid rocks"`) is byte-identical to today. The second is:
```
new EncounterOption
{
    Label = "Swim to shore",
    Outcomes = [new ReplaceStepsOutcome("Washed ashore.", new[] { Biome.Jungle, Biome.Beach })],
}
```
The static-ctor non-empty-`Outcomes` sanity check passes automatically (single-element array is non-empty).

### 4. Add the `ReplaceStepsOutcome` arm to `Adventuring.ApplyOutcome`
Locate the existing exhaustive switch in `Adventuring.cs`. Add an arm before `default`:
```
case ReplaceStepsOutcome r:
    ReplaceRemainingSteps(r.Biomes);
    break;
```
Then add the helper:
- `private void ReplaceRemainingSteps(IReadOnlyList<Biome> biomes)`:
  - Pull `GameData.CurrentAdventure ?? throw new InvalidOperationException(...)`.
  - For each `biome` in `biomes`: get `biome.GetInfo().PossibleEncounters`; if empty, throw `InvalidOperationException($"Cannot replace remaining steps with biome '{biome}': PossibleEncounters is empty.")`. Otherwise build a new `AdventureStep(biome, Random.Shared.Next(possibleEncounters))`.
  - Mutate the existing `adventure.RemainingSteps`: `Clear()` then `AddRange(newSteps)` (preserves the same `List<AdventureStep>` reference, mirroring `biome-umbra`'s `ShiftToBiome` implementation guidance and the existing list-reference invariants).
  - `SaveService.Save(GameData);`
  - `EnterCurrentStep();` — rebuilds the option `ButtonList`, fresh random default, resets the 5s timer, sets `CurrentPhase = ChoosingOption`.

(If `biome-umbra` already added a comparable `ShiftToBiome` helper, this method sits alongside it — same conventions, different signature. If path (C) of Open Decision 1 unifies the two outcomes, both call sites collapse into one helper; collapse during this ticket if the user picks (C), don't pre-collapse otherwise.)

### 5. Update `docs/biomes/river.md`
Rewrite the Rapids subsection to describe both options:
- `"Avoid rocks"` (Engage): existing outcome distribution + intent — unchanged copy.
- `"Swim to shore"`: single outcome, replaces the rest of the adventure with a jungle step followed by a beach step (encounters rolled from each biome's pool at apply time). Capture the design intent: Rapids becomes the **biome-shift waypoint** of the river, mirroring the way Mushrooms is grasslands' shift-to-Umbra waypoint, except the destination here is two pool biomes in sequence rather than a single secret biome.
- Remove any "TODO: second option deferred" / "see `rapids-swim-to-shore.md`" note that the parent river ticket dropped in.

### 6. Sanity-check sibling-ticket coordination
If `biome-umbra` is already merged at this ticket's implementation time, confirm: (a) its `BiomeShiftOutcome` arm is present in `Adventuring.ApplyOutcome`; (b) the new `ReplaceStepsOutcome` arm sits next to it, before `default`; (c) no payload-shape collision. If `biome-umbra` is still in-flight when this ticket starts, coordinate with that ticket's implementer to ensure both new records and both arms land coherently in the same review cycle.

## Test Plan
- [ ] `dotnet build` passes with no warnings.
- [ ] Launch the game. Trigger an adventure that contains a `Rapids` step (template 3 `river → river → waterfall → jungle` or template 6 `river → river → lagoon` will roll one with high probability across multiple runs).
- [ ] On the Rapids step: confirm **two** option buttons render — `"Avoid rocks"` and `"Swim to shore"`. Confirm the 5s timer + default-option selection still behave correctly with two options.
- [ ] Click `"Swim to shore"`. Confirm the outcome text (`"Washed ashore."` or per Open Decision 3) renders for ~2.5s in the outcome phase.
- [ ] After auto-advance: confirm the next step's biome is **jungle**, the encounter is one of jungle's authored encounters, and the option `ButtonList` rebuilds with that encounter's options + a fresh 5s timer.
- [ ] After resolving the jungle step: confirm the next step is **beach**, encounter is one of beach's authored encounters, options render fresh.
- [ ] After resolving the beach step: confirm return to `Playing`. `save.json` shows `CurrentAdventure: null`.
- [ ] Inspect `save.json` immediately after the swim outcome applies (e.g. quit during the jungle step). Confirm `RemainingSteps` is exactly two entries: `[(Jungle, <encounter>), (Beach, <encounter>)]`. **The post-Rapids tail of the original adventure is gone** — this is the key behavioral difference from `SubstituteOutcome` (which only swaps the current step).
- [ ] Repeat the swim 5–10 times. Confirm the rolled jungle and beach encounters vary (uniformity isn't statistically verified, but multiple distinct encounters should appear over enough runs).
- [ ] Confirm `"Avoid rocks"` still behaves as before: 50/50 between `FlavorOutcome("Avoided them.")` (resolve step, continue adventure) and `EndAdventureOutcome("Hit a rock. Limped home.")` (end adventure).
- [ ] (Defensive) Temporarily empty `Biome.Jungle`'s `PossibleEncounters` (comment out its pool entries) and trigger `"Swim to shore"`. Confirm the loud `InvalidOperationException` fires rather than a silent crash or empty-step bird-stares-at-nothing softlock. Revert the test edit.
- [ ] Spot-check outcome text fits in the 128 px viewport at 6×8 font on the actual `ShowingOutcome` draw.
- [ ] Verify `Adventuring.ApplyOutcome`'s `default: throw UnreachableException()` arm still fires when a fake unhandled subclass is dropped in (defensive — same spot-check as `outcome-resolution.md`'s test plan; ensures the new arm didn't accidentally fall through).

## Learnings

### Architectural decisions

- **Open Decision 1 → (A) sub-default.** Landed `ReplaceStepsOutcome(string Text, IReadOnlyList<Biome> Biomes)` as a sibling sealed record alongside the existing `FlavorOutcome` / `SubstituteOutcome` / `EndAdventureOutcome` in `DoodleBird/Encounters/Outcome.cs`. `biome-umbra` hadn't landed yet at implementation time, so option (B) "extend `BiomeShiftOutcome`" was moot — there was no `BiomeShiftOutcome` to extend. Path (A) sub-default also keeps payload semantics crystal clear: a `IReadOnlyList<Biome>` says "one step per biome, encounter rolled at apply", with no `Encounter?` sentinel sneaking in. If/when `biome-umbra` ships its single-target `BiomeShiftOutcome`, the two records sit side-by-side and the rename/unify question can be revisited then.
- **Constructor guard via custom ctor body.** The non-empty `Biomes` invariant has to throw at construction time (silent equivalent to `EndAdventureOutcome` is a footgun). Couldn't put the guard in a primary-ctor record because the parameter check needs an explicit body, so the record uses an explicit ctor + readonly property — same shape as the rest of the Outcome hierarchy externally, just with a guard inside. `ArgumentException` rather than `ArgumentOutOfRangeException` to match the "violates a structural rule, not a numeric range" framing of an empty list.
- **Resolver helper takes `Adventure adventure` parameter explicitly.** Could have re-derived it inside `ReplaceRemainingSteps` via `GameData.CurrentAdventure ?? throw`, but `ApplyOutcome` had already null-checked + bound it locally, and threading the bound reference through is both cheaper and easier to read. Pit-of-success: helper signature signals "you must already have a live adventure."
- **`ChangeState<Adventuring>` rather than calling `EnterCurrentStep` directly.** The existing `FlavorOutcome` / `SubstituteOutcome` arms do `ChangeState<Adventuring, AdventuringConfig>(new(GameData))` to refresh; the new arm mirrors that. The Adventuring ctor itself calls `EnterCurrentStep`, so the acceptance criterion is satisfied transitively — same code path the other outcomes take, no parallel "stay in this state, just rebuild buttons" code path needed.

### Interesting tidbits

- **`Adventure.RemainingSteps` is a mutable `List<AdventureStep>`** — `Clear()` + `AddRange()` preserves the existing list reference, which avoids any consumer holding a stale reference. Mirrors the in-place mutation that `SubstituteOutcome`'s arm does with `RemainingSteps[0] = …`.
- **`BenMakesGames.RandomHelpers` `rng.Next(IList<T>)` extension** is the established uniform-pick idiom across the codebase (used in `AdventureGenerator.TryRoll`). New code in this ticket uses it too rather than calling `Random.Shared.Next(arr.Length)` + indexing.

### Related areas affected

- **`docs/biomes/river.md`** — Rapids subsection rewritten; deferral note dropped; "single-option Rapids is a knowing AC violation" design-rationale bullet removed; "No retreat option" bullet rewritten to reflect that Swim-to-shore now occupies the bail-out slot via biome-shift rather than end-adventure.

### Rejected alternatives

- **Path (B) — `biome-umbra` adopts shape (b) for everything.** Not viable at implementation time — `biome-umbra` hadn't landed. Even if it had, the "Mushroom shift uniformly rolls across Umbra's pool" idiom would have needed an `Encounter?` sentinel to express apply-time rolling, which is more design ergonomics for one extra consumer than path (A) costs.
- **Path (C) — `BiomeShiftOutcome` with `IReadOnlyList<BiomeStepSpec>`.** Same blocker (no `BiomeShiftOutcome` yet) plus mild over-engineering risk — speculative DSL for one current consumer.
- **Authoring the two new steps' encounters at outcome construction.** Considered baking specific encounters into the `Outcome` record (e.g. `ReplaceStepsOutcome(Text, AdventureStep[] Steps)`). Rejected for the anti-save-scum reason captured in Constraints — apply-time rolls keep the new sub-adventure uniformly subject to the same "roll committed when persisted" invariant as adventure rolls at start-of-adventure.
- **Returning a retreat-style escape on Rapids.** Considered briefly because "Swim to shore" reads vaguely like a retreat. Kept the escape off — the swim is mechanically a biome-shift, not an adventure-end, and that distinction is the design point of the second option (it's the *biome-shift waypoint* of the river, not a second escape hatch).
