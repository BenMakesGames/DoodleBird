# Dialog Game State

## Context
**Current behavior**: No game state exists for showing a timed, unskippable message to the player. `DialogBorder.png` has been added to the `.mgcb` but is not loaded as an asset, and no consumer code references it.
**New behavior**: A new sealed `Dialog` game state renders the `DialogBorder` full-screen picture plus a configured message string. The state displays the text for a duration computed from its character count, ignores all input (cannot be skipped, cannot be delayed), and on timer expiry invokes a caller-supplied `Action` exactly once. The `DialogBorder` asset is loaded in `Program.cs` alongside the other pictures. The state is not wired into any caller in this ticket — that is a follow-up.

## Prerequisites
None.

## Scope
### In scope
- `DoodleBird` (game): new sealed `Dialog` game state under `DoodleBird/GameStates/` with a `DialogConfig` record carrying the text to show and the `Action` to invoke on completion.
- `Dialog` lifetime: timer derived from text length; renders `DialogBorder` picture at `(0, 0)` plus the text; on timer expiry calls the configured `Action` exactly once.
- `Pictures.DialogBorder` constant added to `Pictures.cs` and registered in `Program.cs` `AddAssets(...)`. Not pre-loaded (mirrors `TopGrass`/`Bird` — only `Font`/`Cursor` are pre-loaded today).
- Input override is a no-op (text cannot be skipped).

### Out of scope
- Wiring `Dialog` into any caller (`Playing`, `Adventuring`, `Startup`, etc.) — future ticket.
- Text wrapping. The config string is treated as already wrapped — `\n`-separated lines, same convention as `Startup.Tip`.
- Sound effects, fade in/out, typewriter reveal — the text appears all at once and disappears all at once.
- Persisting "dialog already shown" state.

## Relevant Docs & Anchors
- **Analogue tickets**: `docs/tickets/complete/2026-05-15 adventure-generation-and-state-transition.md` — pattern for `GameState<TConfig>` with a config record and `GSM.ChangeState<T, TConfig>(...)` wiring.
- **Code anchors**:
  - `DoodleBird/GameStates/Playing.cs` — `PlayingConfig` record + `GameState<PlayingConfig>` shape to mirror.
  - `DoodleBird/GameStates/Startup.cs` — shows the `TTL -= gameTime.ElapsedGameTime.TotalSeconds` countdown idiom and the `\n`-split multi-line text-drawing idiom (`Tip.Split('\n')`, `lines.Max(...)`, etc.).
  - `DoodleBird/Program.cs` — `AddAssets([...])` block where `PictureMeta(Pictures.DialogBorder)` must be added.
  - `DoodleBird/Pictures.cs` — registry of picture path constants.

## Constraints & Gotchas
- Screen is 128×32. `DialogBorder.png` is exactly 128×32 and includes its own background fill, so `Dialog.Draw` must **not** call `Graphics.Clear(...)` — drawing the picture at `(0, 0)` is the background.
- The `Action` must fire exactly once. Once fired, the state should remain in a benign render-only mode (the `Action` is expected to transition states, but `Dialog` cannot assume that — guard with a `_fired` flag so a long-lived `Dialog` never re-invokes).
- `Dialog` does not call `GSM.ChangeState` itself; that responsibility belongs to whatever the `Action` does. Follow-up tickets will pass an `Action` that transitions state.
- Input override is intentionally empty. Don't add a "press any key to dismiss" path — the ticket explicitly forbids skipping.
- Mouse cursor: `Startup` and other states call `Mouse.Draw(this)` at the end of `Draw`. Mirror that so the OS cursor doesn't fight with the custom cursor mid-dialog. (`MouseManager` is already DI-registered.)

## Open Decisions
1. **Character-to-millisecond rate** — default: **60 ms/char** with a **1500 ms minimum**. Rationale: ~200 WPM ≈ 50 ms/char baseline, padded slightly because the screen is tiny and the audience may include kids. Implementer can adjust by eye during manual test; surface tuning constants near the top of the file.
2. **Text color** — `DialogBorder.png` ships its own background fill; pick a palette entry from `BenMakesGames.MonoGame.Palettes.DawnBringers16` that contrasts cleanly with the border art (likely `Black` or `DarkGray` if the background is light). Eyeball during manual test.
3. **Text layout inside the border** — center-aligned horizontally, vertically centered within the bordered interior. Exact pixel insets depend on the border art's interior margins; eyeball during manual test using the `Startup` centering math (`(Graphics.Width - textWidth) / 2`) as a starting point and nudge as needed if the border has asymmetric padding.
4. **Field for tracking elapsed time** — countdown (`TTL -= elapsed`) like `Startup`, or accumulator (`elapsed += ...`). Default: countdown, mirroring `Startup`. Either is fine.

## Acceptance Criteria
- [ ] `DoodleBird/GameStates/Dialog.cs` exists, defines `public sealed record DialogConfig(string Text, Action OnComplete)` and `public sealed class Dialog : GameState<DialogConfig>`.
- [ ] `Dialog` constructor takes `DialogConfig` plus the standard PlayPlayMini services it needs (`GraphicsManager`, `MouseManager`) — no `GameStateManager` or `SaveService` required.
- [ ] `Dialog.Draw` draws `Pictures.DialogBorder` at `(0, 0)` and renders `DialogConfig.Text` (line-split on `\n`) over it. Does not call `Graphics.Clear`.
- [ ] `Dialog.Input` is a no-op — input is not consulted for skip/advance.
- [ ] `Dialog` computes its display duration once at construction from `Text.Length` (counting `\n` as one character is fine; precision not load-bearing). Duration = `max(MinDurationMs, Text.Length * MsPerChar)` with `MsPerChar = 60` and `MinDurationMs = 1500` as defaults (see Open Decision 1).
- [ ] When the timer reaches zero, `DialogConfig.OnComplete` is invoked exactly once. A subsequent `Update` does not re-invoke it.
- [ ] `Pictures.DialogBorder` constant equals `"Graphics/DialogBorder"` and is added to the `AddAssets([...])` block in `Program.cs` as `new PictureMeta(Pictures.DialogBorder)` (not pre-loaded).
- [ ] No call site references `Dialog` yet — this ticket only defines the state and registers the asset.

## Implementation

### 1. Register the picture
Add `public const string DialogBorder = "Graphics/DialogBorder";` to `Pictures.cs` (next to `TopGrass` / `Bird`). In `Program.cs`, add `new PictureMeta(Pictures.DialogBorder),` to the `AddAssets([...])` list near the other `PictureMeta`/`SpriteSheetMeta` entries. Do **not** mark it pre-loaded — match `TopGrass`.

### 2. Define `DialogConfig`
In a new file `DoodleBird/GameStates/Dialog.cs`, declare `public sealed record DialogConfig(string Text, Action OnComplete);` above the class — same shape as `PlayingConfig` / `AdventuringConfig`.

### 3. Define the `Dialog` game state
`public sealed class Dialog : GameState<DialogConfig>`. Constructor injects `DialogConfig`, `GraphicsManager`, `MouseManager`. Store `Text`, `OnComplete`, and a precomputed `_remainingSeconds` (or `_remainingMs`) derived from `Text.Length`. Use tuning constants at the top of the file: `private const int MsPerChar = 60;` and `private const int MinDurationMs = 1500;`. Also store a `_fired` `bool` to guard one-shot invocation.

Mirror `Startup`'s pre-split-lines pattern in the constructor: split `Text` on `\n`, cache the line array + computed width/height for use in `Draw`.

### 4. `Dialog.Update`
Decrement the timer using `gameTime.ElapsedGameTime.TotalSeconds` (or `.TotalMilliseconds`). When the timer reaches `<= 0` and `_fired` is false, set `_fired = true` and call `OnComplete()`. After firing, do nothing further — the `Action` is expected to transition states; if it doesn't, the state continues to render the dialog harmlessly.

### 5. `Dialog.Input`
Override and leave empty. Add a one-line comment explaining the intentional no-op only if it's truly non-obvious — likely unnecessary, since the ticket-implied "unskippable" intent is clear from the empty body.

### 6. `Dialog.Draw`
- `Graphics.DrawPicture(Pictures.DialogBorder, 0, 0);` (no `Clear`).
- Draw the cached text lines centered horizontally and vertically over the border interior, following the `Startup.Draw` centering math as a starting point. Color per Open Decision 2.
- `Mouse.Draw(this);` at the end.

## Test Plan
- [ ] `dotnet build` passes with no warnings.
- [ ] Temporarily replace `Startup`'s `GSM.ChangeState<Playing, PlayingConfig>(new(gameData));` with `GSM.ChangeState<Dialog, DialogConfig>(new("Hello, world!", () => GSM.ChangeState<Playing, PlayingConfig>(new(gameData))));`. Launch the game; confirm:
  - The dialog border picture fills the screen.
  - The text "Hello, world!" appears centered and legibly contrasted against the border background.
  - The dialog is visible for roughly `max(1500, 13 * 60) = 1500` ms ≈ 1.5 seconds.
  - Mouse / keyboard input during the dialog has no effect (text does not skip).
  - After the timer elapses, the game transitions into `Playing` (bird wandering) exactly once.
- [ ] Test with a longer string (~60 chars) — confirm duration scales (~3.6 s) and the action still fires exactly once.
- [ ] Revert the `Startup` patch — `Dialog` should remain unreferenced after this ticket.
