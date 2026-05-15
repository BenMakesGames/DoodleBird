# Outcome Resolution

## Context
**Current behavior**: `Adventuring` builds an option `ButtonList` per step, runs a 5s timer, and on click / timer expiry fires the active option's `Action` — which currently dispatches by `OptionKind`: `Engage` / `Ignore` resolves the step; `Retreat` ends the adventure. Options' `Outcomes` arrays exist but are never read.
**New behavior**: When an option fires, `Adventuring` picks a uniform random `Outcome` from that option's `Outcomes` array and applies it. The state transitions from `ChoosingOption` to a new `ShowingOutcome` phase: the outcome's text is rendered on screen (replacing the option buttons), and after a fixed delay (~2-3 seconds) the outcome's effect is applied:
- `FlavorOutcome` → resolve the current step (advance, or end if last).
- `SubstituteOutcome` → replace the current step's encounter with the outcome's `NewEncounter`; transition back to `ChoosingOption` with the new encounter's options, a fresh random default, and a reset 5s timer.
- `EndAdventureOutcome` → clear `CurrentAdventure`, save, return to `Playing`.
`OptionKind.Retreat` continues to short-circuit directly to `EndAdventure` without rolling outcomes (retreat is a player escape hatch, not a narrative outcome).

## Prerequisites
- [Encounter Option UI & Timer](./encounter-option-ui-and-timer.md)

## Scope
### In scope
- `Adventuring`: extend `Phase` enum to include `ShowingOutcome`. Tick + draw gate on phase.
- `Adventuring`: on option fire (non-retreat), roll outcome via `Random.Shared.Next(option.Outcomes)`. Transition to `ShowingOutcome` with a `float OutcomeRemainingSeconds` countdown. Store the chosen `Outcome` for rendering.
- `Adventuring.Draw`: in `ShowingOutcome` phase, render the outcome's `Text` (instead of option buttons). Position TBD by implementer — likely centered or replacing the option row.
- `Adventuring.FixedUpdate`: when `OutcomeRemainingSeconds <= 0`, apply the outcome's effect via exhaustive switch on the sealed-record hierarchy. Switch arms: `FlavorOutcome` → `ResolveCurrentStep`; `SubstituteOutcome s` → `SubstituteCurrentEncounter(s.NewEncounter)`; `EndAdventureOutcome` → `EndAdventure`. `default` throws `UnreachableException` (compiler-flagged when a new derived record is added without a switch arm).
- `Adventuring`: new `SubstituteCurrentEncounter(Encounter newEncounter)` private method — rebuild `CurrentAdventure.RemainingSteps[0]` with the same `Biome` and the new `Encounter`, save, call `EnterCurrentStep()` to load options / reset timer / pick default for the new encounter.
- `Adventuring`: retreat path unchanged — `OptionKind.Retreat` still calls `EndAdventure` directly (no outcome roll, no `ShowingOutcome` phase). Confirmed by design (retreat is escape hatch).

### Out of scope
- New outcome kinds beyond the three defined. New kinds land when their effects are designable (e.g. stat changes after bird stats land).
- Outcome text wrapping / multi-line layout. First-cut text fits on one line of 6×8 font in 128 px wide; long-text wrapping is a follow-up if it becomes a problem.
- Skip-outcome-text-by-clicking. The auto-advance is the only path forward in `ShowingOutcome`.
- Animated outcome reveals (typewriter effect, fade-in, etc.).

## Relevant Docs & Anchors
- `docs/adventures.md` §Outcomes, §Adventuring phases.
- Code anchors:
  - `Adventuring` (from T5): `Phase` enum, `EnterCurrentStep`, `ResolveCurrentStep`, `EndAdventure`, `ButtonList` lifecycle, timer logic. This ticket extends the same class.
  - `Outcome` hierarchy (from T3): `FlavorOutcome`, `SubstituteOutcome`, `EndAdventureOutcome`, base `Outcome.Text`.
  - `AdventureStep` (`PetDoodle.Data/AdventureStep.cs`): positional record; for substitute, construct a new `AdventureStep` with the same `Biome` and new `Encounter`, replace `RemainingSteps[0]`.

## Constraints & Gotchas
- `AdventureStep` is a sealed record (immutable). Substitute = replace `RemainingSteps[0]` with a new record, not mutate it in place.
- Save fires on every mutation: rolling outcome doesn't mutate save state (the roll result is transient until applied); applying does (resolve / substitute / end). Save at the apply boundary.
- The outcome display duration is a constant. Default `2.5f` seconds. Don't make it per-outcome unless a design need surfaces.
- During `ShowingOutcome`, clicks on the option-row area must not fire option actions. Easiest: don't draw the option `ButtonList` at all in this phase, and gate the `ButtonList.Update` call in `Adventuring.Update` on `CurrentPhase == ChoosingOption`. Defence in depth: nothing in the click path should route through an `ButtonList` that's not rendered.
- Exhaustive switch on a sealed-record hierarchy: use a `switch` statement (or expression) with type patterns. Add a `_ => throw new UnreachableException()` (or `default: throw …`) arm — the compiler does not enforce exhaustiveness on sealed-class hierarchies the way it does on enums, but the throw makes the failure mode loud at runtime and the explicit type arms make the addition obvious in code review.
- `WarningsAsErrors=Nullable` — `Outcome.Text` is non-null (positional ctor); `SubstituteOutcome.NewEncounter` is non-null. No nullability surprises.
- Substituted encounter does not need to be in the current biome's `PossibleEncounters` pool — author intent rules. Don't add a runtime check.

## Open Decisions
1. **Outcome display duration** — fixed constant for all outcomes? Per-outcome override? Default: **fixed `OutcomeDisplaySeconds = 2.5f`** on `Adventuring`. YAGNI per-outcome until a designer asks for it.
2. **Outcome text placement** — center horizontally, mid-vertical (e.g. `Y = Graphics.Height / 2 - fontHeight / 2`); top-of-screen; or replace the option row at the bottom. Default: **center horizontally, vertically positioned where the option row used to be** (e.g. the bottom strip), so the eye doesn't have to track. Implementer iterates.
3. **Outcome timer display** — show the auto-advance countdown like the option timer (`"2.5"` etc.) or just let the text sit silently and auto-advance. Default: **silently auto-advance**. The option timer is meaningful (player can intervene); the outcome timer isn't (no input accepted).
4. **Switch idiom for outcome dispatch** — `switch` statement with type patterns + `default: throw UnreachableException()` vs `switch` expression assigned to nothing. Default: `switch` statement; the body of each arm is a method call (a statement), and assigning the result to `_` reads worse than the statement form.
5. **Where the chosen outcome is stored on `Adventuring`** — `Outcome? CurrentOutcome` field, populated when entering `ShowingOutcome`, cleared on apply. Default: `private Outcome? PendingOutcome = null;`.

## Acceptance Criteria
- [ ] `Adventuring.Phase` enum has at least `ChoosingOption` and `ShowingOutcome`.
- [ ] On a non-retreat option fire (click or timer expiry in `ChoosingOption`): `Adventuring` picks a uniform random outcome via `Random.Shared.Next(option.Outcomes)`, stores it as `PendingOutcome`, sets `OutcomeRemainingSeconds = OutcomeDisplaySeconds`, and transitions to `Phase.ShowingOutcome`.
- [ ] In `Phase.ShowingOutcome`: option buttons are **not** drawn and **not** updated. The outcome's `Text` is drawn on screen. The 5-second option timer is **not** shown. The outcome timer ticks down silently (or visibly per Open Decision 3 — default silent).
- [ ] When `OutcomeRemainingSeconds` reaches 0 in `Phase.ShowingOutcome`: the outcome's effect is applied via exhaustive type-switch:
  - `FlavorOutcome` → `ResolveCurrentStep()` (existing T4 method).
  - `SubstituteOutcome s` → `SubstituteCurrentEncounter(s.NewEncounter)` (new method this ticket).
  - `EndAdventureOutcome` → `EndAdventure()` (existing T4 method).
  - `default` → `throw new UnreachableException()` (loud failure on unhandled subclass).
- [ ] `SubstituteCurrentEncounter(Encounter newEncounter)`: replaces `RemainingSteps[0]` with `new AdventureStep(currentBiome, newEncounter)`, calls `SaveService.Save`, calls `EnterCurrentStep` (which rebuilds the `ButtonList` for the new encounter, picks a fresh random default, resets the 5s option timer). Transitions back to `Phase.ChoosingOption`.
- [ ] `OptionKind.Retreat` continues to call `EndAdventure` directly — no outcome roll, no `ShowingOutcome` phase.
- [ ] Save is called exactly at: (a) `Playing` rolling adventure (T4), (b) `ResolveCurrentStep` (T4), (c) `SubstituteCurrentEncounter` (this ticket), (d) `EndAdventure` (T4 + this ticket via outcome). Still no per-frame saves.

## Implementation

### 1. Extend `Phase` enum
In `Adventuring`, change `private enum Phase { ChoosingOption }` (from T5) to include `ShowingOutcome`.

### 2. Add outcome-phase fields
On `Adventuring`:
- `private Outcome? PendingOutcome = null;`
- `private float OutcomeRemainingSeconds = 0f;`
- `private const float OutcomeDisplaySeconds = 2.5f;` (Open Decision 1)

### 3. Refactor option dispatch
T5 dispatch was: `Engage`/`Ignore` → `ResolveCurrentStep`; `Retreat` → `EndAdventure`. Replace with:
- `Retreat` → `EndAdventure` (unchanged).
- `Engage` / `Ignore` → new helper `FireOption(EncounterOption option)`:
  - `PendingOutcome = Random.Shared.Next(option.Outcomes);`
  - `OutcomeRemainingSeconds = OutcomeDisplaySeconds;`
  - `CurrentPhase = Phase.ShowingOutcome;`

`OptionButton.Action` for non-retreat options closes over the `EncounterOption` (or its `Outcomes` array — either works) and calls `FireOption`.

### 4. Tick / draw gating
`Adventuring.FixedUpdate`:
- `switch (CurrentPhase) { case ChoosingOption: TickOptionTimer(); break; case ShowingOutcome: TickOutcome(); break; }`

`TickOutcome`:
- `OutcomeRemainingSeconds -= 1f/60f;`
- If `<= 0f`: capture `var outcome = PendingOutcome!; PendingOutcome = null;` then `ApplyOutcome(outcome);`.

`ApplyOutcome(Outcome outcome)`:
```
switch (outcome)
{
    case FlavorOutcome: ResolveCurrentStep(); break;
    case SubstituteOutcome s: SubstituteCurrentEncounter(s.NewEncounter); break;
    case EndAdventureOutcome: EndAdventure(); break;
    default: throw new UnreachableException();
}
```

`Adventuring.Update`:
- Gate `ButtonList.Update(this)` and `Input` on `CurrentPhase == ChoosingOption`. In `ShowingOutcome`, skip — no interactive elements.

`Adventuring.Draw`:
- `switch (CurrentPhase)`:
  - `ChoosingOption`: existing T5 draw — biome, bird, encounter name, option buttons, option timer text.
  - `ShowingOutcome`: biome, bird, encounter name, outcome text (no option buttons, no option timer).

### 5. `SubstituteCurrentEncounter`
New private method on `Adventuring`:
- `var currentBiome = CurrentStep.Biome;`
- `GameData.CurrentAdventure!.RemainingSteps[0] = new AdventureStep(currentBiome, newEncounter);`
- `SaveService.Save(GameData);`
- `EnterCurrentStep();` (rebuilds `ButtonList` for new encounter, fresh random default, resets 5s option timer, sets `CurrentPhase = Phase.ChoosingOption`).

### 6. Verify `EnterCurrentStep` resets phase
T5's `EnterCurrentStep` should set `CurrentPhase = Phase.ChoosingOption` at the end (already noted in T5 as a no-op then; load-bearing now). Double-check.

### 7. (No new save sites in T6 except `SubstituteCurrentEncounter`)
All other save sites already exist from T4. Don't add more.

## Test Plan
- [ ] `dotnet build` passes with no warnings.
- [ ] Run an adventure. On the first encounter, click an `Engage` option. Outcome text appears for ~2.5s, then the next step (or adventure end) shows.
- [ ] Author or temporarily inject a `SubstituteOutcome` (e.g. on `HollowLog.LookInside`, swap to a `GiantToad` encounter — biome ticket may or may not have done this; if not, edit data inline for testing). Trigger that option. Outcome text shows for ~2.5s, then the screen rebuilds with the same biome but the substituted encounter: new name, new options, new random default, timer back to 5.0.
- [ ] Trigger an `EndAdventureOutcome` (similar inline test if no biome ticket has one yet). Outcome text shows for ~2.5s, then return to `Playing`. `save.json` shows `CurrentAdventure: null`.
- [ ] Click any option. While `ShowingOutcome`, click anywhere on the screen — including where the option row used to be. Nothing happens; the auto-advance still fires at its time.
- [ ] Click `Retreat` (where available). No outcome text — direct return to `Playing`. Confirms retreat bypasses `ShowingOutcome`.
- [ ] Quit during `ShowingOutcome` (within the 2.5s window). Relaunch. The save reflects the **pre-outcome** state — the current step is unchanged (because no mutation happens until the outcome applies). Timer state is not persisted; on reload, the game starts the step fresh (back in `ChoosingOption` with a fresh 5s timer + fresh random default). Confirms the anti-save-scum boundary: the outcome roll itself isn't persisted, but once it's applied, save reflects it.
- [ ] Add a new sealed derived record to the `Outcome` hierarchy in a temporary edit (e.g. `sealed record TestOutcome(...) : Outcome(...)`) and an `EncounterOption` referencing it. Confirm the `default: throw new UnreachableException()` arm fires at runtime when the test outcome rolls. Revert the test edit.
- [ ] Eyeball at least one `SubstituteOutcome` chain (encounter A substitutes to B; B's options can resolve normally). Confirm the chain ends naturally with a `FlavorOutcome` (or further substitution that eventually ends).
