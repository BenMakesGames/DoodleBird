# Adventure Generation & State Transition

## Context
**Current behavior**: Bird wanders inside `Playing` forever. Adventure data model, save service, info pattern, and option/outcome types all exist but nothing reads them, no encounters are authored, no state transition exists.
**New behavior**: After the bird is idle for **3 seconds** in `Playing`, `AdventureGenerator.TryRoll()` attempts to generate an adventure. Generation picks uniformly from the 6 fixed adventure templates, **filtered** to those whose every biome has a non-empty `PossibleEncounters` pool. If no template is runnable (which is the case until per-biome tickets land), `TryRoll` returns null and the idle timer resets — no crash. When generation succeeds, the resulting `Adventure` is saved and the game transitions to a new `Adventuring` state that renders the current step's biome (sky + ground colors) and bird, with the encounter name drawn as placeholder text to the right of the bird. A single placeholder `"Continue"` link advances to the next step; when the list empties, the bird returns to `Playing`. The save service fires whenever `CurrentAdventure` is mutated.

## Prerequisites
- [Bird Position & Renderer Refactor](./bird-position-and-renderer-refactor.md)
- [Adventure Data Model & Save Scaffold](./adventure-data-model-and-save-scaffold.md)
- [Biome & Encounter Info Pattern](./biome-and-encounter-info-pattern.md)

## Scope
### In scope
- `PetDoodle` (game): new `AdventureGenerator` static class — fixed template list, **`TryRoll()`** returning `Adventure?`. Filters templates whose every biome has a non-empty pool; uniform pick from the runnable subset; per-biome uniform encounter pick using `BenMakesGames.RandomHelpers` `rng.Next(IList<T>)`. Returns null if no template is runnable.
- `Playing`: 3-second idle timer; on expiry, call `TryRoll()`; on success → save → transition to `Adventuring`; on null → reset idle counter and continue wandering.
- `Playing`: on enter / activate, if `GameData.CurrentAdventure` is non-null (e.g. loaded from save mid-adventure), transition immediately to `Adventuring`.
- `PetDoodle` (game): new sealed `Adventuring` game state with `AdventuringConfig(GameData)`. Reads `GameData.CurrentAdventure.RemainingSteps[0]`. Renders biome sky + ground (from `BiomeInfo`), bird (frozen, fixed left position, frame 0), encounter `DisplayName` as text to the right.
- `Adventuring`: a single placeholder `LinkLabel` that resolves the current step (pops the front of `RemainingSteps`, saves) — used only until T5 brings in the real option-button UI. When the list empties: clear `CurrentAdventure`, save, transition back to `Playing`.

### Out of scope
- Real encounter options / buttons / timer / random default / outcomes — T5 and T6.
- Per-encounter art beyond the encounter-name text label.
- Biome transitions / fade animations between steps.
- Authored encounter content — per-biome design tickets.

## Relevant Docs & Anchors
- `docs/adventures.md` §Triggers, §Adventure structure, §Adventure templates, §Presentation, §Persistence.
- `docs/tickets/complete/2026-05-14 bird-movement-and-renderer.md` — `GameState` patterns (ctor injection, `Update` / `FixedUpdate` / `Draw`, sealed-class convention).
- `BenMakesGames.RandomHelpers` (already referenced) — exposes `rng.Next(IList<T>)` and related extensions on `Random`. Use `Random.Shared` instance.
- Code anchors:
  - `Playing` (`PetDoodle/GameStates/Playing.cs`) — model `Adventuring` on this; especially `PlayingConfig` record / `GameState<TConfig>` pattern.
  - `Startup.Update` → `GSM.ChangeState<Playing, PlayingConfig>(new(gameData))` — how state changes are invoked.
  - `BiomeExtensions.GetInfo` / `EncounterExtensions.GetInfo` — lookups consumed here.
  - `LinkLabel.CreateCentered` / `CreateBottomRight` (`PetDoodle/UI/LinkLabel.cs`) — placeholder advance button.
  - `BirdSprite` (from T1) — draws the bird at fixed position in `Adventuring`.
  - `SaveService` — invoked on every `CurrentAdventure` mutation.

## Constraints & Gotchas
- `Adventuring` reads `GameData` from its config like `Playing` does. The two states share the same `GameData` instance — don't deep-copy.
- The 3-second idle timer resets when the bird is *not* idle (`TargetX != null`) and also when `TryRoll` returns null after timer expiry. Don't accumulate across walking periods.
- The idle trigger must **not** fire if `GameData.CurrentAdventure` is already non-null. On entering `Playing`, if `CurrentAdventure` is non-null, transition immediately to `Adventuring`.
- `AdventureGenerator.TryRoll` must filter templates whose biomes have **empty** `PossibleEncounters`. If no templates pass the filter, return null — don't throw. This is the runtime condition while biomes are being authored.
- Save calls must wrap every mutation of `CurrentAdventure` or its `RemainingSteps`. Pit-of-success: helper methods on `Adventuring` (`ResolveCurrentStep`, `EndAdventure`) that do the mutation + save in one call. Don't sprinkle raw mutations across the codebase.
- `WarningsAsErrors=Nullable` — `GameData.CurrentAdventure` is nullable; pattern-match (`is { } adventure`) rather than `!.`.
- Random source: `Random.Shared` (BCL static, thread-safe). Implementer can swap to injected later if a test demands determinism.

## Open Decisions
1. **Idle timer location** — counter on `Playing` vs `TimeIdle` accumulator on `WanderingBirdView`. Default: counter on `Playing`.
2. **`AdventureGenerator` template storage** — `Biome[][]` static field. Default: `Biome[][]`.
3. **Bird position in `Adventuring`** — fixed constant. Default: `X = 24`, `groundY = Graphics.Height - 6` (same baseline as `Playing`). Encounter text starts at `X = 24 + birdSpriteWidth + 4`.
4. **Returning from `Adventuring` to `Playing`** — `GSM.ChangeState<Playing, PlayingConfig>(new(GameData))` rebuilds `Playing` from scratch; `WanderingBirdView` reseeds to its default initial X/TargetX. Acceptable per `docs/adventures.md`. Implementer can pass a "spawn from edge" hint via `PlayingConfig` if it feels better visually; not required.
5. **Placeholder advance link label** — `"Continue"` (clear intent) vs the encounter's display name (cute but ambiguous). Default: `"Continue"`. Replaced wholesale in T5.

## Acceptance Criteria
- [ ] `PetDoodle` contains `AdventureGenerator` (static class) with a private list of 6 templates exactly matching `docs/adventures.md` §Adventure templates and a public `Adventure? TryRoll()` method. Method returns null when no template's biomes are all non-empty; returns a valid `Adventure` otherwise.
- [ ] `Playing` tracks idle time. After 3 seconds of `WanderingBirdView` being idle (no `TargetX`), `Playing` calls `AdventureGenerator.TryRoll()`. If non-null: assign to `GameData.CurrentAdventure`, save, transition to `Adventuring`. If null: reset idle counter to 0 and continue.
- [ ] On entering `Playing`, if `GameData.CurrentAdventure` is non-null, `Playing` transitions immediately to `Adventuring` (no idle wait). Defensive guard for save-mid-adventure.
- [ ] `PetDoodle` contains a sealed `Adventuring` game state with `AdventuringConfig(GameData)`. Constructor takes standard PlayPlayMini services (`GraphicsManager`, `GameStateManager`, `MouseManager`) plus `SaveService`.
- [ ] `Adventuring.Draw` clears with the current biome's `SkyColor`, draws a flat ground band at `Graphics.Height - 8` with the current biome's `GroundColor`, draws the bird at the fixed encounter position (frame 0, facing right) via `BirdSprite`, and draws the current encounter's `DisplayName` as text to the right of the bird.
- [ ] A single placeholder `LinkLabel` (label `"Continue"`) is rendered. Clicking it: removes the front of `RemainingSteps`, calls `SaveService.Save`. If `RemainingSteps` becomes empty: `GameData.CurrentAdventure = null`, save, transition to `Playing`.
- [ ] Save is called exactly at: (a) `Playing` rolling a new adventure, (b) `Adventuring` resolving a step, (c) `Adventuring` ending the adventure. No save-per-frame.
- [ ] `TryRoll` uses `BenMakesGames.RandomHelpers` `Random.Shared.Next(IList<T>)` extension for both template pick and per-biome encounter pick.

## Implementation

### 1. `AdventureGenerator.TryRoll`
New file `PetDoodle/Adventures/AdventureGenerator.cs`. Static class.
- `private static readonly Biome[][] Templates = [ ... ]` — 6 sequences from `docs/adventures.md` §Adventure templates.
- `public static Adventure? TryRoll()`:
  - `var runnable = Templates.Where(t => t.All(b => b.GetInfo().PossibleEncounters.Length > 0)).ToArray();`
  - If `runnable.Length == 0`, return null.
  - `var template = Random.Shared.Next(runnable);` (using the RandomHelpers extension)
  - For each biome in `template`, `Random.Shared.Next(biome.GetInfo().PossibleEncounters)` to pick an encounter. Build `AdventureStep` per biome.
  - Return `new Adventure { RemainingSteps = [.. steps] }`.

### 2. Idle counter in `Playing`
Add `private float IdleSeconds` on `Playing`. In `FixedUpdate`:
- If `view.TargetX is null` and `GameData.CurrentAdventure is null`: `IdleSeconds += 1f/60f;` (or read from `gameTime.ElapsedGameTime`).
- Else: reset to `0f`.
- If `IdleSeconds >= 3f`: call `TryBeginAdventure()` (next step).

### 3. `TryBeginAdventure` helper
Private method on `Playing`:
- `var adventure = AdventureGenerator.TryRoll();`
- If null: `IdleSeconds = 0f; return;`.
- Else: `GameData.CurrentAdventure = adventure; SaveService.Save(GameData); GSM.ChangeState<Adventuring, AdventuringConfig>(new(GameData));`.

### 4. Guard `Playing` against re-entering with an existing adventure
Override the appropriate enter / resume lifecycle hook on `Playing` (PlayPlayMini convention — `Enter` / `OnActivate`; if no such hook, check at top of `FixedUpdate`). If `GameData.CurrentAdventure is { } adv`, immediately `GSM.ChangeState<Adventuring, AdventuringConfig>(new(GameData))`. Handles save-mid-adventure load on launch.

### 5. `Adventuring` state
New file `PetDoodle/GameStates/Adventuring.cs`. Sealed class. Config record `public sealed record AdventuringConfig(GameData GameData)`. Ctor injects `GraphicsManager`, `GameStateManager`, `MouseManager`, `SaveService`. Stores `GameData` and constructs a single `ButtonList` containing one placeholder `LinkLabel` (label `"Continue"`, action `ResolveCurrentStep`).

Current step accessor: `private AdventureStep CurrentStep => GameData.CurrentAdventure!.RemainingSteps[0];` — null-forgiving safe because we only enter this state with a non-null adventure (and an assertion in the ctor confirms it).

### 6. `Adventuring.Draw`
- `var biomeInfo = CurrentStep.Biome.GetInfo();`
- `Graphics.Clear(biomeInfo.SkyColor);`
- `Graphics.DrawFilledRectangle(0, Graphics.Height - 8, Graphics.Width, 8, biomeInfo.GroundColor);`
- Draw bird via `BirdSprite.Draw(Graphics, encounterBirdX, groundY - spriteHeight/2, facingRight: true, frame: 0)` — encounter X per Open Decision 3.
- Compute encounter text X = `encounterBirdX + (spriteWidth / 2) + 4`. `Graphics.DrawText("Font", textX, textY, CurrentStep.Encounter.GetInfo().DisplayName, /* legible color, e.g. DawnBringers16.White */);`
- Draw the placeholder `ButtonList`.
- `Mouse.Draw(this);`.

### 7. Wire input / update
Mirror existing `ButtonList` usage (none yet — this is the first state with buttons). In `Adventuring.Input`, call `ButtonList.Input()`. In `Adventuring.Update`, call `ButtonList.Update(this)`. The click handler routes through `LinkLabel.Action`, which calls `ResolveCurrentStep`.

### 8. `ResolveCurrentStep` / `EndAdventure`
Private methods on `Adventuring`:
- `ResolveCurrentStep()`: `GameData.CurrentAdventure!.RemainingSteps.RemoveAt(0); SaveService.Save(GameData); if (GameData.CurrentAdventure.RemainingSteps.Count == 0) EndAdventure();`.
- `EndAdventure()`: `GameData.CurrentAdventure = null; SaveService.Save(GameData); GSM.ChangeState<Playing, PlayingConfig>(new(GameData));`.

These are the only sites that mutate `CurrentAdventure` in this ticket. Every mutation pairs with a save.

### 9. Register `Adventuring` if needed
Check whether PlayPlayMini auto-discovers `GameState<TConfig>` subclasses or whether `Program.cs` needs an explicit registration call. Mirror `Playing`.

## Test Plan
- [ ] `dotnet build` passes with no warnings.
- [ ] Launch the game with no biomes authored yet. Bird walks, idles for ≥3s. Nothing happens — `TryRoll` returns null, idle counter resets. Bird remains in `Playing` indefinitely. No exceptions logged.
- [ ] Manually patch `BiomeExtensions.Info` to give one biome (say grasslands) a one-entry `PossibleEncounters` referencing a dummy `Encounter` value temporarily added to the enum + dictionary. Idle 3s — adventure rolls if any template is now fully grasslands (template 1 is not, but for the test, a single-grasslands template can be added inline). Confirm transition to `Adventuring` with sky/ground colors, bird, and encounter name displayed.
- [ ] Click `"Continue"` repeatedly until `RemainingSteps` empties. Confirm return to `Playing`. `save.json` shows `CurrentAdventure: null`.
- [ ] Quit mid-adventure (after one click). Relaunch. The game boots directly into `Adventuring` with the saved remaining steps (no idle wait).
- [ ] Revert the test patch — back to all-empty pools. Idle 3s again — `TryRoll` returns null, no crash, no transition.
- [ ] Inspect `save.json` mid-adventure — `CurrentAdventure.RemainingSteps` is a list of `(Biome, Encounter)` pairs in expected order.

## Learnings

### Architectural decisions
- **Idle counter on `Playing`** (Open Decision 1, default kept). `WanderingBirdView.IsIdle` already gives the signal; the counter doesn't belong on the view because the trigger semantics are gameplay, not visual.
- **`Biome[][]` templates** as a `private static readonly` field on `AdventureGenerator` (Open Decision 2, default). LINQ filter + `Random.Shared.Next(IReadOnlyList<T>)` keep `TryRoll` to ~10 lines.
- **`Enter()` is the right hook for the save-mid-adventure guard.** PlayPlayMini's `GameStateManager` swap loop (`GameStateManager.cs:181–189`) processes queued `ChangeState` calls back-to-back, so calling `ChangeState<Adventuring,…>` inside `Playing.Enter()` immediately re-enters the swap loop with `Adventuring` as the next state — clean cascade, no first-frame flash of `Playing`.
- **Helper-only mutation of `CurrentAdventure`** (Constraint). `Playing.TryBeginAdventure`, `Adventuring.ResolveCurrentStep`, `Adventuring.EndAdventure` are the only three sites that touch `GameData.CurrentAdventure` or `RemainingSteps`, each paired with `SaveService.Save`. Pit-of-success: any future mutation that bypasses these helpers is a code-review red flag.
- **Pattern-match (`is { } adventure`) over `!.`** on `GameData.CurrentAdventure` even where the ctor assertion guarantees non-null. Cheap insurance; throws an explanatory `InvalidOperationException` rather than a bare `NullReferenceException` if the invariant ever breaks.
- **Placeholder `"Continue"` via `LinkLabel.CreateBottomRight`** (Open Decision 5, default). Will be replaced wholesale by the real option `ButtonList` in T5.
- **Fixed bird position constants `BirdLeftX = 24` / encounter text starts at `BirdLeftX + spriteWidth + 4`** (Open Decision 3, default). Text Y centers on bird sprite center (`birdCenterY - font.MaxCharacterHeight / 2`) so the encounter label aligns visually with the bird regardless of future font swaps.

### Problems encountered
- **`Random.Shared.Next(IList<T>)` API name vs. actual signature.** The ticket and `docs/adventures.md` reference `rng.Next(IList<T>)`, but the actual `BenMakesGames.RandomHelpers` extension is `Next<T>(IReadOnlyList<T>)` (`RandomExtensions.cs:36`). `T[]` satisfies `IReadOnlyList<T>` so the call site is identical; only the docs are slightly imprecise.
- **`Playing` ctor now requires `SaveService`** that it didn't before. The DI container resolves it automatically (registered in `Program.cs`), so no plumbing change beyond the constructor parameter.

### Interesting tidbits
- **`ButtonList.Input(forceUpdate: true)` runs from its own ctor**, so initial `Hovered`/`Active` state is correct on the first frame without a cursor move. No extra wiring needed in `Adventuring.Enter()`.
- **Adventures are reachable today only if a per-biome ticket has populated `PossibleEncounters`.** Until then `TryRoll` returns null forever and the bird stays in `Playing` — the "T4 lands before biomes" property baked into the roadmap.

### Workarounds / limitations
- **Cannot exercise the live UI from this shell.** The game opens a MonoGame window; build is verified clean (0 warnings, 0 errors) but the Test Plan items that require launching the game (idle-3s trigger, click-through, save-mid-adventure resume, `save.json` shape) need a human run-through.

### Related areas affected
- `Playing.cs` — gained `SaveService` dependency, `IdleSeconds` field, `Enter` override, `TryBeginAdventure` helper. No change to existing `Draw` / `Update` logic.
- T5 will replace the `Adventuring` `ButtonList` content wholesale (single `Continue` link → per-encounter option list with timer). The `ButtonList` plumbing (Input/Update/Draw wiring) is already correct and reusable as-is.
- T6 will add a `Phase` enum and a `ShowingOutcome` branch; the current single-phase `Adventuring` is the degenerate case.

### Rejected alternatives
- **`TimeIdle` accumulator on `WanderingBirdView`** (alternative for Open Decision 1). Rejected: idle-time-to-adventure is gameplay logic, not view concern; view's `IsIdle` is the only signal gameplay needs.
- **`Bird.X` / `Bird.TargetX` carrying screen position into `Adventuring`.** Convention from `docs/adventures.md` §Presentation: bird screen position lives in the view layer, not in `Bird` data — fixed constant in this state instead.
- **Per-frame save.** Explicitly rejected by the ticket; helper-method pattern enforces it structurally.
