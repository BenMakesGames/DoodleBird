# Bird Movement and Renderer

## Context
**Current behavior**: `Bird` has only `X`. No movement target, no on-screen draw. `Playing` state renders sky + grass but no bird.
**New behavior**: `Bird` gains a nullable `TargetX` movement intent. `Playing.FixedUpdate` steps `X` toward `TargetX` and clears the target on arrival. A new `BirdRenderer` (game-side, not data-side) draws the bird at its current `X`, applying a distance-keyed hop (visual Y offset only, peak 2 px) and a sticky horizontal flip derived intentionally from `sign(TargetX - X)`.

## Prerequisites
None.

## Scope
### In scope
- `PetDoodle.Data.Bird`: add `TargetX`.
- `PetDoodle` (game) sim: bird movement step in `Playing.FixedUpdate`.
- `PetDoodle` (game) view: new `BirdRenderer` class owning all visual-only state (hop phase, facing). Updated and drawn from `Playing`.
- `Startup` seeds an initial non-null `TargetX` so the first launch shows the bird walking, hopping, and facing correctly.

### Out of scope
- `BirdAction` enum / on-arrive dispatch — deferred until a concrete action exists. (See architectural conversation; YAGNI.)
- Y position, gravity, jumping, multi-bird support.
- Sprite-sheet frame animation (wing flap, etc.). Visual animation in this ticket is Y-offset hop + horizontal flip only, using the existing static `Pictures.Bird`.

## Relevant Docs & Anchors
- `CLAUDE.md` (root): null safety, YAGNI/KISS, pit-of-success, principle of least surprise.
- `PetDoodle.Data/CLAUDE.md`: Data project must stay POCO with zero third-party deps. `Bird` must not import MonoGame or PlayPlayMini.
- `Playing.Draw` (`PetDoodle/GameStates/Playing.cs`): current draw order is sky clear → grass rect → `TopGrass` picture → mouse. Bird draws after the grass picture and before the mouse.
- `Startup.Update` (`PetDoodle/GameStates/Startup.cs`): existing `GameData`/`Bird` construction site. Extend to set `TargetX`.
- `Pictures.Bird` (`PetDoodle/Pictures.cs`): single static picture asset (already registered in `Program.cs`).
- PlayPlayMini source at `C:\Development\PlayPlayMini`. Key API: `GraphicsManager.DrawPictureWithTransformations(string pictureName, int centerX, int centerY, Rectangle? clippingRectangle, SpriteEffects flip, float angle, float scale, Color c)` in `Services/GraphicsManager.Pictures.cs`. Note: takes **center** coords, not top-left — different from `DrawPicture(string, int x, int y)`.

## Constraints & Gotchas
- `PetDoodle.Data` cannot reference `PetDoodle` or any framework type. `Bird` stays POCO. No `using Microsoft.Xna.Framework;` etc.
- Hop Y offset is purely visual. Must NOT mutate `Bird.X` or any `Bird` field. Renderer reads `Bird`, never writes.
- Logical resolution is 128×32 (see `Program.cs`). Pixel-snap floats to int at the draw boundary or motion will visibly jitter.
- `DrawPictureWithTransformations` is center-based. If mixed with `DrawPicture` (top-left), offsets will be off by half a sprite. Use `DrawPictureWithTransformations` for the bird unconditionally so flipped vs unflipped share the same anchor math.
- `WarningsAsErrors=Nullable` is set in both csprojs — any nullable warning fails the build.

## Open Decisions
1. **Bird speed (px/sec)** — start with a `private const float` in `Playing` near the movement code. Default: ~30 px/sec; tune by feel during manual test.
2. **Hop wavelength (px per full hop cycle)** — default 4 px. Adjust during manual test if it looks off at the default speed.
3. **Hop curve shape** — half-sine over each wavelength feels smoothest. Sawtooth is the lazier fallback. Default: half-sine.
4. **Arrival threshold** — snap-and-clear when next step distance would meet or overshoot the target. No separate epsilon needed.
5. **Initial sticky facing** — when the bird has never had a target, default to facing right.
6. **Renderer lifetime** — `new BirdRenderer()` inside `Playing` ctor; not DI-registered. Single owner, no cross-state sharing — PlayPlayMini's DI is for shared services.

## Acceptance Criteria
- [ ] `PetDoodle.Data.Bird` has exactly two public properties: existing `X` (required float, get/set) and new `TargetX` (nullable float, get/set, not required). No PlayPlayMini/MonoGame/Microsoft.Xna imports in `PetDoodle.Data`.
- [ ] In `Playing.FixedUpdate`, when `GameData.Bird.TargetX` is non-null: `X` moves toward `TargetX` by a fixed-per-tick step; when the next step would reach or overshoot, `X` snaps to `TargetX.Value` and `TargetX` is set to `null`.
- [ ] A `BirdRenderer` class exists in the `PetDoodle` project as a non-static class owning hop-phase and facing state. It exposes an `Update` method (takes the `Bird` and `GameTime`) and a `Draw` method (takes the `Bird` and the graphics service). Neither writes to `Bird`.
- [ ] Visual Y offset applied by the renderer is in the range `[-2, 0]` and is keyed to cumulative pixels of `|ΔX|` traveled (distance-keyed), not wall-clock time.
- [ ] Renderer's horizontal flip is intentional: when `TargetX` is non-null, facing = `sign(TargetX - X)`; when `TargetX` is null, last-known facing is retained (sticky).
- [ ] In `Playing.Draw`, the bird is drawn after the `TopGrass` picture and before `Mouse.Draw`.
- [ ] `Startup` builds `GameData` with `Bird.TargetX` set to a value in screen bounds that is meaningfully far from the initial `X` (so the walk + animation + facing are immediately visible on launch).

## Implementation

### 1. Add `TargetX` to `Bird`
In `PetDoodle.Data/Bird.cs`, add `public float? TargetX { get; set; }`. Not `required`. Nothing else changes. File stays free of any `using` other than the existing namespace declaration.

### 2. Step bird toward target in `Playing.FixedUpdate`
Why: sim mutates `Bird.X` only here, so the renderer can treat `Bird` as a read-only snapshot per frame. Where: `Playing.FixedUpdate`. What: declare `private const float BirdSpeed = …;` (px/sec) on `Playing`. Per tick, compute `step = BirdSpeed * (1f/60f)` (fixed-tick rate; the override is called at 60 Hz per the PlayPlayMini contract documented in the existing comment). If `bird.TargetX` is null, do nothing. Otherwise, if `MathF.Abs(bird.TargetX.Value - bird.X) <= step`, set `bird.X = bird.TargetX.Value; bird.TargetX = null;`. Else nudge `bird.X` by `±step` toward `TargetX`.

### 3. Create `BirdRenderer`
Why: keep all visual-only state out of `Bird` (Data project must stay POCO + zero-deps) and out of `Playing` (separation of view from game-state). Where: new file `PetDoodle/BirdRenderer.cs`, namespace `PetDoodle`. What: a sealed class with three private mutable fields:
- cumulative-distance hop phase (float, modulo hop wavelength),
- last-observed `Bird.X` (float; nullable or `float.NaN` sentinel — implementer's call — to detect "first frame, no delta yet"),
- sticky facing (bool, default `true` = facing right).

`Update(Bird bird, GameTime gameTime)`:
- On first call, just record `bird.X` and return (no delta yet, no flip yet).
- Add `MathF.Abs(bird.X - lastX)` to hop phase; wrap modulo the hop wavelength (Open Decision 2).
- If `bird.TargetX` is non-null and `bird.TargetX.Value != bird.X`, set facing to `bird.TargetX.Value > bird.X`. Else leave facing as-is (sticky).
- Update `lastX = bird.X`.

`Draw(Bird bird, GraphicsManager graphics)`:
- Compute hop Y offset using the curve from Open Decision 3. For the default half-sine: `yOffset = -MathF.Sin((phase / wavelength) * MathF.PI) * 2f`. Range `[-2, 0]`.
- Compute draw center: round `bird.X` to int for X; for Y, anchor the bird so its feet sit on top of the grass band (the grass rect in `Playing.Draw` starts at `Graphics.Height - 8`). Use the bird picture's height from the graphics service (look up by name — see the PlayPlayMini source for the accessor; do not hard-code).
- Call `graphics.DrawPictureWithTransformations(Pictures.Bird, centerX, centerY, null, facingRight ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f, 1f, Color.White)`.

Renderer reads `Bird`; never writes.

### 4. Wire renderer into `Playing`
In `Playing.cs`:
- Add `private BirdRenderer Renderer { get; }`.
- In the constructor, `Renderer = new BirdRenderer();`.
- In `Playing.Update(gameTime)`, call `Renderer.Update(GameData.Bird, gameTime)`. (`Update` runs at frame rate; visual interpolation should match frame rate, not the 60 Hz fixed tick.)
- In `Playing.Draw(gameTime)`, call `Renderer.Draw(GameData.Bird, Graphics)` immediately after the `Graphics.DrawPicture(Pictures.TopGrass, …)` line and before `Mouse.Draw(this);`.

### 5. Seed an initial target in `Startup`
In `Startup.Update`, where the `GameData` is built, also set `Bird.TargetX` to an in-screen value visibly far from the initial `X = 20` (e.g., ~100). This makes the walking + hop + facing visible on first launch with no further input.

## Test Plan
- [ ] `dotnet build` succeeds at solution root with no warnings (Nullable warnings are errors).
- [ ] Launch the game. Bird is visible standing on the grass and walks from its starting X toward the seeded target.
- [ ] During the walk: bird hops upward by up to 2 pixels; the hop rate visibly correlates to walking speed (slower bird = slower hop). On arrival, bird sits flat (no hop offset).
- [ ] Bird faces the direction of motion. Temporarily set a target to the left of the bird (e.g., edit `Startup` or set via debugger) and confirm the sprite is horizontally flipped.
- [ ] Idle bird (no `TargetX`) keeps the facing it had at the moment the target was cleared (sticky).
- [ ] No visible pixel jitter while walking — confirms float-to-int rounding at the draw boundary.
- [ ] Inspect `PetDoodle.Data.csproj` and its compiled output: no MonoGame, no PlayPlayMini, no `Microsoft.Xna.*` references.
