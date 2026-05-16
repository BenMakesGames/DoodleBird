# Encounter Option UI & Timer

## Context
**Current behavior**: `Adventuring` renders the current step's biome, bird, and encounter name. A single placeholder `"Continue"` link advances steps. No real per-encounter options, no timer, no random default selection.
**New behavior**: `Adventuring` builds a `ButtonList` from the current encounter's `Options`. One option is randomly pre-selected as the bird's default (visually highlighted as `Active`). A **5-second timer** counts down on screen with **0.1-second precision**; when it expires, the default option fires automatically. If the player clicks any option (default or not) the timer stops and that option fires. Firing an option hands it to the resolver — see `outcome-resolution.md`. This ticket wires the option UI, the timer, and the seam to the resolver; the resolver's roll-show-apply pipeline lands in T6.

## Prerequisites
- [Adventure Generation & State Transition](./adventure-generation-and-state-transition.md)
- **All 9 biome design tickets complete** (so every authored `Encounter` has a non-empty `Options` array; `ButtonList` ctor requires ≥1 button).

## Scope
### In scope
- `PetDoodle/UI`: new `IButton` implementation suited to the encounter row (e.g. `OptionButton` — wider hit target, label-only, styled similarly to `LinkLabel` but positioned by an external layout helper).
- `ButtonList`: allow callers to set the initial `Active` button (currently `private set`, defaults to `Buttons[0]`). New ctor overload accepting `IButton initialActive`.
- `Adventuring`: build a `ButtonList` from the current encounter's options on step enter, pre-set `Active` to a random option, start a 5-second timer, draw remaining time top-right, on timer expiry invoke the active button's `Action`. Replace the placeholder `"Continue"` link.
- `Adventuring`: timer resets when the step changes (new encounter → new 5s window, new random default).
- Option dispatch in this ticket: each button's `Action` is `() => FireOption(option)` — a uniform seam to the resolver (`outcome-resolution.md`). Today's placeholder `FireOption` simply resolves the current step; T6 replaces it with the full outcome pipeline.

### Out of scope
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
  - `EncounterInfo` / `EncounterOption` (T3) — authored options consumed here.
  - `BenMakesGames.RandomHelpers` — `Random.Shared.Next(IList<T>)` for default-option pick.

## Constraints & Gotchas
- **Viewport is 128×32**. Font is 6×8. Plan layout for up to 4 options in a single row; if a future encounter has more, the UI will need to wrap or scroll — flag at that time, don't build for it now.
- **`ButtonList.Active` is currently `private set`**. Setting the bird's random default requires extending the API. Smallest change: ctor overload `ButtonList(GSM, MouseManager, IList<IButton>, IButton initialActive)`. Validates `buttons.Contains(initialActive)`.
- **Timer ticks in `FixedUpdate`** (60 Hz, deterministic). Decrement a `float RemainingSeconds` and clamp at 0. Frame rate dips shouldn't extend the timer.
- **Timer fires exactly once per step**. Track a `bool TimerFired` (or the phase enum if introduced here) so a `<=0` accumulator doesn't repeatedly invoke the action.
- **Display precision**. Format as `$"{remaining:0.0}"` (one decimal). Don't display negative values once expired.
- **EndAdventureOutcome semantics**. An option whose rolled outcome is `EndAdventureOutcome` ends the adventure when applied (after the outcome-reveal delay). The resolver — not the option — owns end-adventure dispatch. No per-option short-circuit.
- **`WarningsAsErrors=Nullable`** — the `IButton.Action` delegate is non-null (initialised at button construction).
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
- [ ] `Adventuring` no longer constructs the placeholder `"Continue"` link. Instead, on step enter, it builds a `ButtonList` of `OptionButton`s — one per `EncounterOption` in the current encounter — with each button's `Action` set to `() => FireOption(option)` (see `outcome-resolution.md` for the resolver).
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
- Each button's `Action` is `() => FireOption(option)` — the uniform seam to the resolver (`outcome-resolution.md`). In this ticket, `FireOption` is a placeholder that calls `ResolveCurrentStep()`; T6 replaces its body with the roll-show-apply pipeline.
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
- [ ] Encounter an option-list with no retreat-labelled option (per `Mermaid` or whichever biome ticket authored it). Confirm there's no `Retreat` button; timer still fires the default at 0.0.
- [ ] Click any option on a multi-step adventure mid-flight. Confirm the placeholder `FireOption` resolves the current step (next step's encounter shows). Full retreat-via-EndAdventureOutcome behavior lands with T6.
- [ ] Quit during a countdown. Relaunch — same encounter restored (`save.json` step list unchanged); timer resets to `5.0` (transient, not persisted); fresh random default picked.
- [ ] Instrument `SaveService.Save` to log — confirm one log line per step resolve, retreat, and adventure end. No per-frame logging.

## Learnings

### Architectural decisions

- **`ButtonList` ctor chained** rather than duplicating init. Old `ButtonList(gsm, cursor, buttons)` now chains into the new `(…, initialActive)` overload via a tiny `RequireFirst(buttons)` helper that re-throws the empty-buttons `ArgumentException`. Single init path, no copy-paste.
- **Random default picked by index, not by element-then-IndexOf.** Open Decision 5 suggested `Random.Shared.Next(options)` (the `BenMakesGames.RandomHelpers` overload). Picking the *index* directly via the built-in `Random.Shared.Next(int)` is equivalent uniform distribution and avoids an `Array.IndexOf` lookup to map element → button. Same effect, simpler. No new `using BenMakesGames.RandomHelpers` import in `Adventuring`.
- **Phase enum introduced now** (Open Decision 1 default). Single value (`ChoosingOption`); `Input` / `Update` / `FixedUpdate` all gate on `CurrentPhase != Phase.ChoosingOption ? return`. Today every guard is a no-op, but T6 just adds the second arm — no retrofit.
- **`OptionButton` mirrors `LinkLabel` styling exactly** (Open Decision 6 default). Same color contrast, same centered-label + underline draw. The only structural difference: no static `Create…` factories — encounter row layout is computed by `Adventuring` since per-slot X is a function of slot count + viewport width.
- **Re-entry guard in `FireOption`**: `RemainingSeconds = OptionTimerSeconds` is set *before* calling `ResolveCurrentStep`. Belt-and-suspenders for the `EndAdventureOutcome` path — see "Problems encountered" below.

### Problems encountered

- **`GameStateManager.ChangeState` is deferred, not immediate.** `SwitchState()` runs at the *top* of the next `Update` cycle (`GameStateManager.cs:120`). Within the same cycle, multiple `FixedUpdate` iterations can still run on the old state (`GameStateManager.cs:144-154`). Without a guard, this sequence is possible:
  1. `FixedUpdate` ticks `RemainingSeconds` to `<= 0` → `Buttons.Active.Action()` → `FireOption` → `ResolveCurrentStep` → `EndAdventure` (which sets `GameData.CurrentAdventure = null` and queues state change).
  2. The `while (FixedUpdateAccumulator >= FixedTimestepMs)` loop runs *another* iteration before the cycle ends → `RemainingSeconds` is still `<= 0` → `Buttons.Active.Action()` fires *again* → `ResolveCurrentStep` throws on the `CurrentAdventure is not { }` guard.
  Fix: `FireOption` sets `RemainingSeconds = OptionTimerSeconds` before calling `ResolveCurrentStep`. The non-end path then calls `EnterCurrentStep` which sets it to `5f` again — same value, no harm. The end path retains the `5f`, preventing a second fire.
- **`ButtonList.Input(true)` can override `initialActive` immediately** if the cursor happens to be hovering a button at construction (`ButtonList.Input` sets `Hovered` and then `Active = Hovered` whenever `Hovered is not null`). Acceptable: player intent (hovering) reasonably wins over a random default. Ticket explicitly OKs this ("Sets `Active = initialActive` before the initial `Input(true)` call"). No code path attempts to suppress the hover override.

### Workarounds / limitations

- **Option row layout vs. viewport at 128×32.** Options sit at `Y=22, Height=10` along the bottom, equal-width slots `(Graphics.Width - 4) / N`. Several authored labels are wider than their slot (e.g. `"Crawl through"` is 78px, slot for 3 options is ~41px). Label text overflows neighbor slots visually. Ticket flagged this as "Implementer eyeballs spacing for 1-4 options" — accepted as known cosmetic limitation. Cleanup is a follow-up if it becomes a real readability problem (truncate, abbreviate authored labels, or vertical layout).
- **Option row overlaps bird sprite.** Bird occupies roughly `y=12..27` (15-tall sprite, feet on ground at `y=26`); option row starts at `y=22`. Lower-left options visually clip the bird's feet. Same accept-as-cosmetic call; the design doc explicitly leaves "Background pictures, ground textures, decorations" out of scope, so the visual cramping is consistent with the spartan first-cut UI.
- **`Buttons` field is `null!`-initialised** in `Adventuring` because it's assigned in `EnterCurrentStep` (called from the ctor). C# constructor flow can't prove the field is non-null after the indirect call without the `null!` forgiveness. The runtime guarantee is held by ctor → `EnterCurrentStep` being unconditional.

### Rejected alternatives

- **`bool TimerFired` flag** to prevent timer re-fire after expiry. Ticket explicitly steered away ("don't set a `TimerFired` flag — rely on `Action()` mutating `CurrentAdventure` and rebuilding the `ButtonList`"). Rejected: the deferred-`ChangeState` window means rebuild-doesn't-happen-for-EndAdventure-path, so *something* has to gate re-fire. Chose to reset `RemainingSeconds` instead of introducing a new field — same effect, no new state to keep in sync.
- **Duplicate `ButtonList` ctor init block** instead of chaining. Considered, but two copies of "validate buttons.Count, assign fields, Input(true)" invites drift. Tiny `RequireFirst` helper preserves the original `ArgumentException` for the no-arg case while keeping init in one place.
- **`Random.Shared.Next(options)` via `BenMakesGames.RandomHelpers`** (Open Decision 5 default). Rejected in favor of `Random.Shared.Next(slotCount)` (built-in, no extra `using`). The extension's value-add — picking an element — was negated because we need the *index* to identify the matching button. Picking-index-directly is the same uniform distribution.

### Related areas affected

- `DoodleBird/UI/ButtonList.cs` — new ctor overload; existing ctor now delegates. Behavior identical for existing callers.
- `DoodleBird/UI/OptionButton.cs` — new file. Sibling to `LinkLabel`, same shape.
- `DoodleBird/GameStates/Adventuring.cs` — rewritten around `EnterCurrentStep` / `FireOption` / `Phase`. `ResolveCurrentStep` now calls `EnterCurrentStep` on the non-last-step path (previously did nothing).
- No tests added — Adventuring's behavior is UI/runtime-heavy. `dotnet test` still passes the existing single test.
