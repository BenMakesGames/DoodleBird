# Outcome Text via Dialog State

## Context
**Current behavior**: After `outcome-resolution`, `Adventuring` enters a `ShowingOutcome` phase that draws the rolled outcome's `Text` inline (over the biome/bird backdrop), ticks down `OutcomeRemainingSeconds`, and on expiry calls `ApplyOutcome` to dispatch to `ResolveCurrentStep` / `SubstituteCurrentEncounter` / `EndAdventure`.
**New behavior**: Outcome text is shown via the `Dialog` game state instead of inline. On option fire, `Adventuring` rolls the outcome, then transitions to `Dialog` with the outcome's `Text` and an `OnComplete` delegate that applies the outcome's effect. When `Dialog`'s timer expires, the delegate fires and drives the same three effect paths as today (resolve / substitute / end). The `ShowingOutcome` phase, `OutcomeRemainingSeconds`, `PendingOutcome`, `TickOutcome`, and inline outcome-text rendering all go away.

## Prerequisites
- [Outcome Resolution](./outcome-resolution.md)

## Scope
### In scope
- `Adventuring.FireOption`: roll outcome (unchanged), then `GSM.ChangeState<Dialog, DialogConfig>(...)` with the outcome's `Text` and an `OnComplete` that applies the effect.
- Effect application from `OnComplete`: same three arms as today — `FlavorOutcome` → resolve current step (advance or `EndAdventure`); `SubstituteOutcome` → swap encounter in `RemainingSteps[0]`, save, re-enter `Adventuring`; `EndAdventureOutcome` → clear `CurrentAdventure`, save, return to `Playing`.
- Remove `ShowingOutcome` arm and supporting state (`PendingOutcome`, `OutcomeRemainingSeconds`, `OutcomeDisplaySeconds`, `TickOutcome`). `Phase` enum collapses back to a single value — delete it and any `CurrentPhase` gates that become trivially true.
- Remove inline outcome-text drawing in `Adventuring.Draw` (no `ShowingOutcome` branch remains).
- Re-entering `Adventuring` after `Substitute` or `Flavor`-advances-to-next-step: `GSM.ChangeState<Adventuring, AdventuringConfig>(new(GameData))`. Ctor's existing `EnterCurrentStep()` rebuilds buttons / fresh default / 5s timer for whatever step is now at index 0.

### Out of scope
- Any change to `Dialog` itself (text rendering, duration formula, border art, input handling). `Adventuring` is a plain consumer of the existing `DialogConfig(Text, OnComplete)` contract.
- New outcome kinds.
- Animated transitions between `Adventuring` and `Dialog`.
- Persisting the rolled-but-not-applied outcome across a quit during the `Dialog` reveal. Same anti-save-scum boundary as today: roll is transient, apply mutates + saves.

## Relevant Docs & Anchors
- **Analogue ticket**: `docs/tickets/complete/2026-05-15 dialog-game-state.md` — defines `DialogConfig(string Text, Action OnComplete)` and the `Dialog : GameState<DialogConfig>` shape consumed here. Test Plan in that ticket already exercised the `GSM.ChangeState<Dialog, DialogConfig>(new(text, () => GSM.ChangeState<...>(...)))` call pattern.
- **Prerequisite ticket**: `docs/tickets/outcome-resolution.md` — defines `PendingOutcome`, `OutcomeRemainingSeconds`, `OutcomeDisplaySeconds`, `Phase.ShowingOutcome`, `TickOutcome`, `ApplyOutcome`, `SubstituteCurrentEncounter`. This ticket replaces or removes most of those.
- **Code anchors**:
  - `Adventuring.FireOption` — current seam where roll + phase-transition happens; becomes roll + Dialog-transition.
  - `Adventuring.ApplyOutcome` (from outcome-resolution) — the exhaustive `switch` whose arms move into the `OnComplete` closure (or stay as private methods called from the closure — Open Decision 2).
  - `Adventuring.ResolveCurrentStep`, `EndAdventure`, `SubstituteCurrentEncounter` — effect helpers; either retained as instance methods invoked by the closure, or inlined into the closure.
  - `docs/adventures.md §Adventuring phases` — phases table should be updated (or removed) since `ShowingOutcome` no longer exists as an `Adventuring`-internal phase. Captured under Implementation.

## Constraints & Gotchas
- `GSM.ChangeState<Dialog, DialogConfig>(...)` discards the current `Adventuring` instance. The `OnComplete` closure captures whatever it needs (`GameData`, `SaveService`, `GSM`, the rolled `Outcome`) — it cannot rely on `this` being the active state when it fires, but C# closures hold their captured references regardless. Calling helper methods on the captured `this` is fine (the methods are pure logic + service calls; they don't depend on lifecycle position).
- `SubstituteOutcome` path must call `SaveService.Save(GameData)` before re-entering `Adventuring`, same as today's `SubstituteCurrentEncounter`. The new `Adventuring` instance's ctor reads the mutated `GameData`.
- `EndAdventureOutcome` and `FlavorOutcome`-when-last-step both transition to `Playing`, not back to `Adventuring`. Don't accidentally re-enter `Adventuring` when `RemainingSteps` is empty.
- Exhaustive switch idiom from outcome-resolution carries over: `_ => throw new UnreachableException()` arm stays loud.
- `AdventureStep` is a sealed record (immutable). Substitute still replaces `RemainingSteps[0]` with a new record — no mutation.
- `Dialog` ignores input. Clicks during the reveal cannot fire option actions because the option `ButtonList` belongs to the now-defunct `Adventuring` instance and isn't on screen.

## Open Decisions
1. **Where the effect-apply logic lives** — inline in the `OnComplete` lambda, or kept as private methods on `Adventuring` (`ResolveCurrentStep`, `EndAdventure`, `SubstituteCurrentEncounter`) invoked from the lambda. Default: keep as methods, lambda dispatches via the same `switch (outcome) { ... }` shape as today's `ApplyOutcome`. Avoids duplicating the substitute/resolve plumbing inside a closure.
2. **Whether the exhaustive `switch` stays as `ApplyOutcome(Outcome)` method or moves inline** — coupled with Decision 1. Default: keep `ApplyOutcome` method, call it from the lambda (`() => ApplyOutcome(outcome)`).
3. **Whether to keep the `Phase` enum as a single-value placeholder for future phases** — Default: delete it along with `CurrentPhase`. The `Input`/`Update`/`FixedUpdate` phase-gates become unconditional. YAGNI; re-add when a second phase exists.

## Acceptance Criteria
- [ ] On any option fire (click or 5s timer expiry), `Adventuring` rolls the outcome via `Random.Shared.Next(option.Outcomes)` and immediately transitions to `Dialog` via `GSM.ChangeState<Dialog, DialogConfig>(new(outcome.Text, <onComplete>))`. No intermediate `ShowingOutcome` rendering on the `Adventuring` side.
- [ ] When `Dialog`'s `OnComplete` fires, the rolled outcome's effect is applied via exhaustive type-switch:
  - `FlavorOutcome` → resolve current step (advance to next via `GSM.ChangeState<Adventuring, AdventuringConfig>(new(GameData))`, or end via `GSM.ChangeState<Playing, PlayingConfig>(new(GameData))` when `RemainingSteps` becomes empty).
  - `SubstituteOutcome s` → replace `RemainingSteps[0]` with `new AdventureStep(currentBiome, s.NewEncounter)`, `SaveService.Save(GameData)`, then `GSM.ChangeState<Adventuring, AdventuringConfig>(new(GameData))`.
  - `EndAdventureOutcome` → clear `CurrentAdventure`, `SaveService.Save(GameData)`, `GSM.ChangeState<Playing, PlayingConfig>(new(GameData))`.
  - `default` → `throw new UnreachableException()`.
- [ ] `Adventuring` no longer defines `PendingOutcome`, `OutcomeRemainingSeconds`, `OutcomeDisplaySeconds`, `TickOutcome`, or a `ShowingOutcome` phase. The `Phase` enum and `CurrentPhase` field are deleted (Open Decision 3).
- [ ] `Adventuring.Draw` no longer contains a `ShowingOutcome` branch. The draw method renders biome / bird / encounter name / option buttons / option timer unconditionally.
- [ ] `Adventuring.Input`, `Adventuring.Update`, `Adventuring.FixedUpdate` no longer gate on `CurrentPhase`. The option `ButtonList` is updated and the 5s option timer ticks every frame `Adventuring` is the active state.
- [ ] Save sites: (a) `Playing` rolling adventure, (b) resolve-current-step path in `OnComplete`, (c) substitute path in `OnComplete`, (d) end-adventure path in `OnComplete`. No save on outcome roll; no per-frame saves.
- [ ] `docs/adventures.md §Adventuring phases` updated (or removed) to reflect that outcome reveal now happens in `Dialog`, not as an `Adventuring`-internal phase.

## Implementation

### 1. Rewrite `FireOption`
Replace outcome-resolution's body (`PendingOutcome = ...; OutcomeRemainingSeconds = ...; CurrentPhase = Phase.ShowingOutcome;`) with:
- Roll: `var outcome = Random.Shared.Next(option.Outcomes);`
- Transition: `GSM.ChangeState<Dialog, DialogConfig>(new(outcome.Text, () => ApplyOutcome(outcome)));`

Drop the placeholder `_ = option;` and the `RemainingSeconds = OptionTimerSeconds;` line — both obsolete here (the option timer reset happens in `EnterCurrentStep` on the next `Adventuring` instance).

### 2. Move / retain `ApplyOutcome`
Keep `ApplyOutcome(Outcome outcome)` as a private method on `Adventuring` (Open Decision 2). Its three arms change from "step in place" to "transition to next state":
- `FlavorOutcome`: same logic as today's `ResolveCurrentStep` body — remove `RemainingSteps[0]`, save, then `if (RemainingSteps.Count == 0) GSM.ChangeState<Playing>(...); else GSM.ChangeState<Adventiring>(new(GameData));`.
- `SubstituteOutcome s`: same logic as today's `SubstituteCurrentEncounter` — replace `RemainingSteps[0]` with new `AdventureStep(currentBiome, s.NewEncounter)`, save, then `GSM.ChangeState<Adventuring, AdventuringConfig>(new(GameData))`.
- `EndAdventureOutcome`: same logic as today's `EndAdventure` — clear `CurrentAdventure`, save, `GSM.ChangeState<Playing, PlayingConfig>(new(GameData))`.
- `default`: `throw new UnreachableException()`.

`ResolveCurrentStep`, `EndAdventure`, `SubstituteCurrentEncounter` can either remain as helpers that `ApplyOutcome` delegates to, or be inlined — implementer's call. Either way, the no-longer-used `EnterCurrentStep`-after-substitute path is gone (re-entering `Adventuring` re-runs the ctor, which calls `EnterCurrentStep`).

### 3. Delete `ShowingOutcome` phase + supporting state
- Remove `Phase` enum, `CurrentPhase` field, and all `if (CurrentPhase != Phase.ChoosingOption) return;` gates in `Input`/`Update`/`FixedUpdate`.
- Remove `PendingOutcome`, `OutcomeRemainingSeconds`, `OutcomeDisplaySeconds`, `TickOutcome`.
- Remove the `switch (CurrentPhase)` in `FixedUpdate` (becomes a direct `RemainingSeconds -= ...; if (<= 0f) Buttons.Active.Action();`).
- Remove the `ShowingOutcome` arm in `Draw` (the existing inline biome / bird / encounter-name / buttons / timer becomes the only draw path).
- Remove the `CurrentPhase = Phase.ChoosingOption;` line at the end of `EnterCurrentStep`.

### 4. Update `docs/adventures.md`
Edit §Adventuring phases: either drop the phases table (no longer a state machine inside `Adventuring`) and replace with one sentence — "Option chosen → outcome rolled → `Dialog` state shows the outcome's text → on completion the effect applies and either `Adventuring` re-enters (next step / substituted encounter) or `Playing` resumes (adventure ended)." — or rewrite the table to describe the cross-state flow (`Adventuring` ⇄ `Dialog`). Keep the doc honest about where the timer / text rendering lives.

## Test Plan
- [ ] `dotnet build` passes with no warnings.
- [ ] Start an adventure. On the first encounter, click any option. The screen transitions to the `Dialog` border with the outcome's text. After the dialog's auto-advance, either the next encounter appears (`Adventuring` re-entered) or the bird returns home (`Playing`).
- [ ] Trigger a `SubstituteOutcome` (e.g. `HollowLog.LookInside` if biome data ships one; otherwise inline-edit for testing). Dialog shows the substitute's text; on dismissal the same biome + new encounter renders with fresh 5s timer + fresh random default.
- [ ] Trigger an `EndAdventureOutcome` (Retreat option). Dialog shows the retreat text; on dismissal `Playing` resumes and `save.json` shows `CurrentAdventure: null`.
- [ ] Let the 5s option timer expire without clicking. Default option fires → Dialog appears → flow continues as above.
- [ ] During Dialog reveal, click anywhere / press keys. Nothing happens (Dialog ignores input by design); auto-advance still fires at its scheduled time.
- [ ] Quit during a Dialog reveal. Relaunch. The save reflects the **pre-outcome** step (mutation only happens in `OnComplete`). `Adventuring` re-enters with the original encounter, fresh 5s timer, fresh random default.
- [ ] Chain test: encounter A's option rolls a `SubstituteOutcome` to encounter B; B's option rolls a `FlavorOutcome`. Both dialogs appear in sequence; adventure advances past A's slot once B resolves.
- [ ] Grep confirms no remaining references to `Phase.ShowingOutcome`, `OutcomeRemainingSeconds`, `OutcomeDisplaySeconds`, `PendingOutcome`, or `TickOutcome` in `Adventuring.cs`.

## Learnings

### Architectural decisions

- **`ApplyOutcome` kept as a method, helpers inlined** (Open Decisions 1 + 2 defaults, then a step further). The outcome-resolution ticket had `ResolveCurrentStep` / `SubstituteCurrentEncounter` / `EndAdventure` as named private methods because they were called from multiple seams (ctor flow, substitute re-enter, outcome dispatch). After this rewrite every effect arm has exactly one caller — `ApplyOutcome` — and the arm bodies collapse to two or three lines (mutate, save, `GSM.ChangeState`). Inlining beats three single-use helpers; the switch reads as a flat list of effects.
- **`Phase` enum deleted** (Open Decision 3 default). With outcome reveal living in `Dialog`, the only remaining `Adventuring` state is "buttons up, timer ticking". Single-value enums are deferred premature structure; YAGNI says delete and re-add when a second phase appears.
- **Re-entry via `GSM.ChangeState<Adventuring, AdventuringConfig>(new(GameData))`** rather than mutating in-place. The substitute path used to mutate `RemainingSteps[0]` and call `EnterCurrentStep()` on the same instance; now that mutation is followed by a state change. The ctor reads the mutated `GameData` and rebuilds from scratch — fresh buttons, fresh default, fresh 5s timer. No per-arm helper, no second `EnterCurrentStep` call path.
- **`OnComplete` closure captures `this` implicitly via `ApplyOutcome`.** `Dialog` discards `Adventuring`, but the lambda holds the captured `Adventuring` reference alive; calling `ApplyOutcome(outcome)` on it works because the method's body only touches captured services (`GameData`, `SaveService`, `GSM`), not lifecycle position. The defunct `Adventuring` instance is GC'd once `OnComplete` runs and the closure dies.
- **Flavor-end path consolidated to a single save.** The old `ResolveCurrentStep` + `EndAdventure` chain saved twice on the last step (remove-and-save, then null-and-save). The inlined arm checks `RemainingSteps.Count == 0` after the `RemoveAt` and saves once — either after nulling `CurrentAdventure` (end) or with the step removed (advance). One write per state transition.

### Problems encountered

None. The rewrite was a pure reshape; the outcome-resolution ticket's exhaustive switch idiom carried over unchanged, just with the arm bodies changed from "mutate in place" to "mutate + save + ChangeState".

### Interesting tidbits

- The save-scum boundary is enforced for free by the new structure: the outcome roll happens in `FireOption` *before* the `Dialog` transition, and the rolled `Outcome` is captured only by the closure — it never lives on `GameData`. Quitting during the `Dialog` reveal serializes only the pre-roll state (matches `RemainingSteps[0]` unchanged); the closure dies with the process, so on relaunch the encounter is re-entered fresh.
- `Dialog` is unaware that its caller is `Adventuring`. Future callers can pass any `Action` — chain dialogs (`new(textA, () => GSM.ChangeState<Dialog, DialogConfig>(new(textB, finalAction)))`), end-of-flow handoffs, etc. The `OnComplete` contract is the only coupling.

### Workarounds / limitations

- **No animated transition between `Adventuring` and `Dialog`** — out of scope per ticket. The state swap is a frame-perfect cut. Cosmetic only; if it reads jarring in playtest, add a fade in either state or a wrapper transition state.
- **Long outcome text still overflows 128px in `Dialog`.** Out of scope here; same limitation noted in `outcome-resolution.md`'s Learnings. `Dialog` has its own centering math which behaves the same as the old inline render.

### Related areas affected

- `DoodleBird/GameStates/Adventuring.cs` — only file changed. Significant deletions (`Phase` enum, `PendingOutcome`, `OutcomeRemainingSeconds`, `OutcomeDisplaySeconds`, `TickOutcome`, `SubstituteCurrentEncounter`, `ResolveCurrentStep`, `EndAdventure` helpers, all phase-gate `if`s in lifecycle hooks, the `Draw` switch). Net: shorter file, flatter control flow.
- `docs/adventures.md` — §Adventuring phases replaced with §Outcome reveal flow describing the `Adventuring` ⇄ `Dialog` cross-state choreography.
- No `Dialog` changes (out of scope; it's a plain consumer of the existing `DialogConfig` contract).
- No test changes — behavior is UI/runtime-heavy and the single existing test still passes.

### Rejected alternatives

- **Lambda-inlined effect dispatch** instead of `() => ApplyOutcome(outcome)`. Considered (Open Decision 1 alt). Lambda body would have been the same switch with the same arms — strictly worse readability and harder to unit-test should that ever be wanted. Kept `ApplyOutcome` as a named method per Decision 1/2 defaults.
- **Keeping `Phase` enum as a single-value placeholder.** Open Decision 3 alt. Rejected per default; YAGNI. The `CurrentPhase != Phase.ChoosingOption` gates and the `switch` in `FixedUpdate` were the only consumers, and they all became trivial after the deletion.
- **Retaining `ResolveCurrentStep` / `EndAdventure` / `SubstituteCurrentEncounter` as named helpers.** Considered explicitly in ticket §2 ("implementer's call"). Rejected because each is called from exactly one site after this rewrite; inlining into the switch arms keeps the effect description local to the dispatch.
