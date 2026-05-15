# Encounter Option UI & Timer

## Context
**Current behavior**: `Adventuring` renders the current step's biome, bird, and encounter name. A single placeholder `"Continue"` link advances steps. No real per-encounter options, no timer, no random default selection.
**New behavior**: `Adventuring` builds a `ButtonList` from the current encounter's `Options`. One option is randomly pre-selected as the bird's default (visually highlighted as `Active`). A **5-second timer** counts down on screen with **0.1-second precision**; when it expires, the default option fires automatically. If the player clicks any option (default or not) the timer stops and that option fires. Firing an option resolves the step (this ticket: same behavior as today — `Engage` / `Ignore` advance, `Retreat` ends adventure). **Outcome resolution is deferred to T6** — this ticket wires the option UI and behavior but options' `Outcomes` arrays are not yet read.

## Prerequisites
- [Adventure Generation & State Transition](./adventure-generation-and-state-transition.md)
- **All 9 biome design tickets complete** (so every authored `Encounter` has a non-empty `Options` array; `ButtonList` ctor requires ≥1 button).

## Scope
### In scope
- `PetDoodle/UI`: new `IButton` implementation suited to the encounter row (e.g. `OptionButton` — wider hit target, label-only, styled similarly to `LinkLabel` but positioned by an external layout helper).
- `ButtonList`: allow callers to set the initial `Active` button (currently `private set`, defaults to `Buttons[0]`). New ctor overload accepting `IButton initialActive`.
- `Adventuring`: build a `ButtonList` from the current encounter's options on step enter, pre-set `Active` to a random option, start a 5-second timer, draw remaining time top-right, on timer expiry invoke the active button's `Action`. Replace the placeholder `"Continue"` link.
- `Adventuring`: timer resets when the step changes (new encounter → new 5s window, new random default).
- Option dispatch in this ticket: `Engage` / `Ignore` → resolve current step (same as today's `"Continue"`); `Retreat` → end adventure entirely.

### Out of scope
- **Outcomes**: every option's `Action` for now is "resolve step" or "end adventure" by `Kind`. Reading `Outcomes` arrays + applying their effects is **T6**.
- Per-encounter art / unique animations for options.
- Sound effects.
- Per-encounter timer durations — fixed 5s across all encounters.
- Keyboard / gamepad navigation of the option list (mouse only, matching `ButtonList`'s current behavior).

## Relevant Docs & Anchors
- `docs/adventures.md` §Encounters — variable option count, retreat-not-universal, 5s timer with 0.1s precision, random default option.
- `docs/adventures.md` §Adventuring phases — note that this ticket only implements the `ChoosingOption` phase (no `ShowingOutcome` yet; that's T6). The phase enum itself can be introduced here in anticipation, or in T6 — see Open Decisions.
- Code anchors:
  - `Adventuring` (from T4): owns the current step + the `ButtonList`. Most changes land here.
  - `ButtonList` (`PetDoodle/UI/ButtonList.cs`) — extend to allow caller-set initial `Active`. Current `Active` setter is private; current ctor always uses `buttons[0]`.
  - `LinkLabel` (`PetDoodle/UI/LinkLabel.cs`) — model for any new `IButton` impl.
  - `IButton` (`PetDoodle/UI/IButton.cs`) — the contract.
  - `EncounterInfo` / `EncounterOption` / `OptionKind` (T3) — authored options consumed here.
  - `BenMakesGames.RandomHelpers` — `Random.Shared.Next(IList<T>)` for default-option pick.

## Constraints & Gotchas
- **Viewport is 128×32**. Font is 6×8. Plan layout for up to 4 options in a single row; if a future encounter has more, the UI will need to wrap or scroll — flag at that time, don't build for it now.
- **`ButtonList.Active` is currently `private set`**. Setting the bird's random default requires extending the API. Smallest change: ctor overload `ButtonList(GSM, MouseManager, IList<IButton>, IButton initialActive)`. Validates `buttons.Contains(initialActive)`.
- **Timer ticks in `FixedUpdate`** (60 Hz, deterministic). Decrement a `float RemainingSeconds` and clamp at 0. Frame rate dips shouldn't extend the timer.
- **Timer fires exactly once per step**. Track a `bool TimerFired` (or the phase enum if introduced here) so a `<=0` accumulator doesn't repeatedly invoke the action.
- **Display precision**. Format as `$"{remaining:0.0}"` (one decimal). Don't display negative values once expired.
- **Retreat semantics**. An option whose `Kind == OptionKind.Retreat` clears `CurrentAdventure` entirely (`EndAdventure`), not just pops the front step.
- **`WarningsAsErrors=Nullable`** — `EncounterOption.Action` is dispatched via `Kind`; the `IButton.Action` delegate itself is non-null (initialised at button construction).
- **Empty `Options` arrays are impossible** per the T3 ctor guard + static-ctor sanity check, but `Adventuring` should still fail loudly (assertion) if it somehow encounters one. Defence in depth.

## Open Decisions
1. **Phase enum introduction** — introduce `Adventuring.Phase { ChoosingOption, ShowingOutcome }` here (with only `ChoosingOption` used) or wait for T6. Default: **introduce here**. Cheap to add now, makes the gating logic explicit, T6 just adds the second arm. Cleaner than retrofitting later.
2. **Option button impl** — new `OptionButton : IButton` vs extend `LinkLabel`. Default: new sibling. `LinkLabel`'s factories are for one-offs; encounter rows need an externally-laid-out sequence.
3. **Layout for N options in 128×32** — flow horizontally from the left along a strip near the bottom, equal-width slots dividing `Graphics.Width` minus side margins. Implementer eyeballs spacing for 1–4 options.
4. **Timer position** — top-right corner. Default: `Graphics.Width - margin - textWidth` X coord, small Y from top.
5. **Default-option pick API** — `Random.Shared.Next(options)` (`BenMakesGames.RandomHelpers`). Once per step at `ButtonList` build, not re-rolled per frame.
6. **`OptionButton` highlight style** — mirror `LinkLabel`'s `isActive` color contrast (`DawnBringers16.LightBlue` active, `DawnBringers16.Blue` inactive) or invert (e.g. filled rectangle behind active). Default: mirror `LinkLabel`. Visual consistency.

## Acceptance Criteria
- [ ] `ButtonList` supports caller-supplied initial `Active` via a new ctor overload `ButtonList(GameStateManager gsm, MouseManager cursor, IList<IButton> buttons, IButton initialActive)`. Throws `ArgumentException` if `initialActive` not in `buttons`. Existing ctor unchanged.
- [ ] `PetDoodle/UI` contains `OptionButton` (or equivalent) implementing `IButton`. Renders a label centered in its rect with the active/inactive color contrast.
- [ ] `Adventuring` no longer constructs the placeholder `"Continue"` link. Instead, on step enter, it builds a `ButtonList` of `OptionButton`s — one per `EncounterOption` in the current encounter — with actions dispatched by `Kind`: `Engage` / `Ignore` → resolve current step; `Retreat` → end adventure.
- [ ] A random option from the current step's `Options` is pre-selected as the bird's default, set via the new `ButtonList` ctor overload. The active option visibly stands out from the others.
- [ ] A 5-second timer counts down in `Adventuring.FixedUpdate`. When the timer reaches 0, the `Active` button's `Action` fires exactly once.
- [ ] Remaining time is drawn top-right formatted to one decimal (`$"{remaining:0.0}"` or `…s"`).
- [ ] Player click on any option fires that option's action immediately and prevents the timer from auto-firing on the same step.
- [ ] When a step is resolved (substituted later via T6, or simply popped here), the timer resets to 5.0 seconds and a new random default is picked for the next step's options.
- [ ] (If Open Decision 1 default stands) `Adventuring` has a private `Phase` field of an enum type with at least `ChoosingOption`. Tick + draw gate on phase. Only `ChoosingOption` is actually used in this ticket; T6 adds the `ShowingOutcome` arm.

## Implementation

### 1. Extend `ButtonList`
Add ctor overload:
```
public ButtonList(GameStateManager gsm, MouseManager cursor, IList<IButton> buttons, IButton initialActive)
```
Validates `buttons.Contains(initialActive)` (throw `ArgumentException` if not). Sets `Active = initialActive` before the initial `Input(true)` call. Delegates to existing ctor for the rest, or duplicates the small init block — implementer's call.

### 2. Add `OptionButton`
New file `PetDoodle/UI/OptionButton.cs`. Implements `IButton` with `X`, `Y`, `Width`, `Height`, `Label`, `Action`. `Draw(GraphicsManager graphics, bool isActive)` renders label centered with `isActive` color contrast (mirror `LinkLabel.Draw` styling). No factory methods — encounter row layout is computed by `Adventuring`.

### 3. (Optional but recommended) Introduce `Phase` enum
On `Adventuring`: `private enum Phase { ChoosingOption /*, ShowingOutcome added in T6 */ }` and `private Phase CurrentPhase = Phase.ChoosingOption;`. Reference `CurrentPhase` in tick/draw gating from the start; T6 only adds the new arm and the transition.

### 4. `EnterCurrentStep()` helper
On `Adventuring`, private method `EnterCurrentStep()`:
- Read current encounter's `Options`.
- Build one `OptionButton` per option, laid out across the bottom row (equal width slots, small margin).
- Each button's `Action` dispatches on `option.Kind`:
  - `OptionKind.Engage` → `() => ResolveCurrentStep();`
  - `OptionKind.Ignore` → `() => ResolveCurrentStep();`
  - `OptionKind.Retreat` → `() => EndAdventure();`
- Pick a random `EncounterOption` via `Random.Shared.Next(options)`; the matching `OptionButton` is the `initialActive` argument.
- Construct `ButtonList` with the new ctor overload.
- Reset `RemainingSeconds = 5f`. Set `CurrentPhase = Phase.ChoosingOption` (no-op today; explicit for T6).
- Store the `ButtonList` in a field.

Call `EnterCurrentStep()` from `Adventuring` ctor and after a step resolves (in `ResolveCurrentStep`, after the `RemoveAt(0)`, only if `RemainingSteps.Count > 0`).

### 5. Delete placeholder rendering
Remove the placeholder `"Continue"` `LinkLabel` from `Adventuring` (introduced in T4). Replace with `ButtonList.Draw` for the option row.

Draw order: sky/ground → bird → encounter name → option buttons → timer text → mouse.

### 6. Timer countdown
In `Adventuring.FixedUpdate`:
- If `CurrentPhase != Phase.ChoosingOption`, skip timer logic (gates ready for T6).
- `RemainingSeconds -= 1f/60f;` (or read from `gameTime`).
- If `RemainingSeconds <= 0f`: capture `var action = ButtonList.Active.Action;` then call `action()`. To prevent the same tick path firing twice in case `Active.Action()` re-enters `EnterCurrentStep` (which resets the timer), the action invocation must be the last statement in this branch. Cleanest: don't set a `TimerFired` flag — rely on `Action()` mutating `CurrentAdventure` and rebuilding the `ButtonList` (which resets `RemainingSeconds = 5f`).

### 7. Click path
`ButtonList.Update(this)` (called from `Adventuring.Update`) already fires `Hovered.Action()` on click. Same path, same idempotency — clicking rebuilds `ButtonList` via `EnterCurrentStep` (next step) or transitions out of `Adventuring` entirely (retreat / end). No additional guard needed.

### 8. Timer readout
In `Adventuring.Draw`, after option buttons, before `Mouse.Draw`:
- Format text as `$"{MathF.Max(RemainingSeconds, 0f):0.0}"` (or `…s"`).
- Top-right: `X = Graphics.Width - font.ComputeWidth(text) - margin`, `Y = small margin from top`.
- Color: pick something legible against varied skies (e.g. `DawnBringers16.White`).

## Test Plan
- [ ] `dotnet build` passes with no warnings.
- [ ] Launch the game (with biomes authored). Idle the bird, adventure rolls. First encounter shows: `DisplayName` to right of bird, option row along bottom, one option visibly highlighted as default, `"5.0"` top-right.
- [ ] Watch the timer count down at one-decimal precision. At `0.0`, the highlighted option's action fires — step resolves (visible: biome and encounter change) or adventure ends (visible: return to `Playing`).
- [ ] Click a non-default option mid-countdown. That option's action fires immediately; timer stops.
- [ ] Run several adventures. Random default option visibly varies between encounters of the same type — eyeball at least 2 distinct defaults across 5–10 rolls of a multi-option encounter.
- [ ] Encounter an option-list with no retreat (per `Mermaid` or whichever biome ticket authored it). Confirm there's no `Retreat` button; timer still fires the default at 0.0.
- [ ] Click `Retreat` on a multi-step adventure mid-flight. Confirm immediate return to `Playing`. `save.json` shows `CurrentAdventure: null`. Remaining steps discarded.
- [ ] Quit during a countdown. Relaunch — same encounter restored (`save.json` step list unchanged); timer resets to `5.0` (transient, not persisted); fresh random default picked.
- [ ] Instrument `SaveService.Save` to log — confirm one log line per step resolve, retreat, and adventure end. No per-frame logging.
