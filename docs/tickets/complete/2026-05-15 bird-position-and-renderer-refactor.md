# Bird Position & Renderer Refactor

## Context
**Current behavior**: `Bird` (POCO in `PetDoodle.Data`) carries `X` and `TargetX` — purely visual state. `Playing.FixedUpdate` mutates them; `BirdRenderer` reads them. There is one fixed render — a wandering bird on grass.
**New behavior**: `Bird` carries only bird-intrinsic data (currently just `Name`; future hunger/skills/tiredness). Screen position and movement intent move out of `Bird` and into a new game-side view class owned by `Playing`. A stateless sprite-draw helper centralises the actual `DrawPictureWithTransformations` call so future game states (notably the upcoming `Adventuring` state) can render the bird at their own fixed position without re-implementing draw math.

## Prerequisites
None.

## Scope
### In scope
- `PetDoodle.Data.Bird`: remove `X` and `TargetX`. `Name` is the only remaining property.
- `PetDoodle` (game): new `WanderingBirdView` (or similar) class owning the wander simulation (`X`, `TargetX`) and the existing visual state (hop phase, facing, last-X). Constructed by `Playing` with an initial position and target.
- `PetDoodle` (game): new `BirdSprite` stateless helper (static method or static class) wrapping the `DrawPictureWithTransformations` call with bird-specific anchoring (feet on the ground band at `Graphics.Height - 6`, sprite-height lookup, center-coord conversion, flip selection, frame index).
- `Playing`: stop reading/writing `Bird.X` / `Bird.TargetX`; delegate sim + draw to `WanderingBirdView`.
- `Startup`: seed `WanderingBirdView` with the initial `X=20`, `TargetX=100` values that previously lived on `Bird`. `Bird` is constructed with just `Name`.

### Out of scope
- Adventure data, save system, biome/encounter types — later tickets.
- Multi-bird, frame animation, alternate sprite frames beyond preparing `BirdSprite` to accept a frame index.
- Any change to bird movement rules (speed, hop wavelength, sticky facing, snap-on-arrival) — refactor preserves observable behavior exactly.

## Relevant Docs & Anchors
- `docs/adventures.md` §Presentation — "Bird screen position is not part of `Bird` data — it is purely visual, owned by the rendering layer in whichever game state is active."
- `docs/tickets/complete/2026-05-14 bird-movement-and-renderer.md` — the original implementation of bird movement + renderer. Movement rules, hop curve, sticky facing, ground-band anchoring all came from this ticket and must round-trip unchanged.
- `PetDoodle.Data/CLAUDE.md` — Data project stays POCO + zero third-party deps. `Bird` cannot reference MonoGame / PlayPlayMini.
- Code anchors:
  - `Bird` (`PetDoodle.Data/Bird.cs`)
  - `Playing.FixedUpdate` and `Playing.Update` / `Playing.Draw` (`PetDoodle/GameStates/Playing.cs`)
  - `BirdRenderer` (`PetDoodle/BirdRenderer.cs`)
  - `Startup.Update` (`PetDoodle/GameStates/Startup.cs`)
  - `Pictures.Bird` (`PetDoodle/Pictures.cs`)

## Constraints & Gotchas
- `PetDoodle.Data` cannot reference `PetDoodle` or any framework type. `Bird` POCO must not gain or retain visual fields.
- `WarningsAsErrors=Nullable` is set in both csprojs — any nullable warning fails the build.
- Logical resolution is 128×32; pixel-snap floats to int at the draw boundary. Don't introduce sub-pixel jitter.
- Mixing `DrawPicture` (top-left) and `DrawPictureWithTransformations` (center) for the bird is a half-sprite offset bug. Keep the transformations variant for the bird unconditionally.
- The current movement code uses a `FixedTickStep = BirdSpeed / 60f` constant that assumes the 60 Hz `FixedUpdate` contract. Preserve that contract — the new view's `FixedUpdate` (or equivalent) must run from `Playing.FixedUpdate`, not `Update`.
- The hop animation is updated in `Update` (frame rate), not `FixedUpdate`. This split must survive the refactor: sim mutation at 60 Hz, view interpolation at frame rate. See the prior ticket's Learnings.

## Open Decisions
1. **Class name** — `WanderingBirdView`, `WanderingBird`, `BirdWanderController`, etc. Local-taste; pick one.
2. **Where `BirdSprite` lives** — static class in the `PetDoodle` namespace alongside `WanderingBirdView`, or in a `View/` or `Rendering/` subfolder. Default: top-level `BirdSprite` static class in `PetDoodle`.
3. **`BirdRenderer.cs` fate** — its current responsibility splits cleanly into `WanderingBirdView` (state + Update) and `BirdSprite` (stateless Draw). Default: delete and replace. No transitional shim.
4. **Initial X/TargetX seed location** — constants on `Playing`, ctor args plumbed from `Startup` via `PlayingConfig`, or hard-coded inside `WanderingBirdView` defaults. Default: ctor args on `WanderingBirdView`, supplied by `Playing` from constants on itself.

## Acceptance Criteria
- [ ] `PetDoodle.Data.Bird` has exactly one public property: `Name` (required string, get/set). No `X`, no `TargetX`. No PlayPlayMini / MonoGame / `Microsoft.Xna.*` imports anywhere in `PetDoodle.Data`.
- [ ] A new class exists in the `PetDoodle` project that owns `X` (float), `TargetX` (float?), hop phase, sticky facing, and last-observed-X. It exposes a fixed-tick movement step, a frame-rate `Update`, and a `Draw` taking the `GraphicsManager`. None of these methods mutate `Bird`.
- [ ] A stateless `BirdSprite` (or equivalent) draw helper exists in the `PetDoodle` project, taking center X / center Y / facing-right / frame index and producing the same on-screen pixel result the current `BirdRenderer.Draw` produces today.
- [ ] `Playing.FixedUpdate` no longer references `GameData.Bird.X` or `GameData.Bird.TargetX`. Movement is invoked on the new view.
- [ ] Visual behavior is unchanged: bird hops by up to 2 px, hop rate keys to distance not time, snap-and-clear on arrival, sticky facing when target is null, feet sit on the ground band at `Graphics.Height - 6`.
- [ ] `Startup` constructs `Bird` with only `Name`. The initial X (`20`) and target X (`100`) are supplied to the wander view, not to `Bird`.

## Implementation

### 1. Slim `Bird` to bird-intrinsic data
In `PetDoodle.Data/Bird.cs`, delete `X` and `TargetX`. Only `Name` remains. File stays free of any framework type.

### 2. Extract `BirdSprite` stateless draw helper
Create a new file in `PetDoodle` containing a static method that takes `(GraphicsManager graphics, int centerX, int centerY, bool facingRight, int frame = 0)` and renders the bird via `DrawPictureWithTransformations(Pictures.Bird, centerX, centerY, null, flip, 0f, 1f, Color.White)`. The helper doesn't know about ground bands or hop offsets — callers compute those and pass center coords. Future game states (`Adventuring`) call this directly without going through `WanderingBirdView`.

### 3. Create `WanderingBirdView`
New file in `PetDoodle`. Mirrors the responsibility split of the existing `BirdRenderer` but additionally owns the sim state previously on `Bird`. Fields:
- `float X` (initialized from ctor)
- `float? TargetX` (initialized from ctor)
- hop phase (float, modulo hop wavelength)
- last-observed X (`float?`; null until first `Update`)
- sticky facing (bool, default true)

Methods:
- `FixedUpdate(GameTime gameTime)` — runs the snap-and-clear movement loop currently in `Playing.FixedUpdate`, mutating its own `X` / `TargetX`. Constants `BirdSpeed = 45f`, `FixedTickStep = BirdSpeed / 60f`, `HopWavelength = 6f`, `HopAmplitude = 2f` live on this class.
- `Update(GameTime gameTime)` — runs the hop-phase / sticky-facing / last-X bookkeeping currently in `BirdRenderer.Update`, but reads its own `X` / `TargetX`. Reset `HopPhase = 0` when `TargetX is null` (preserves the prior ticket's post-feedback fix).
- `Draw(GraphicsManager graphics)` — computes hop Y offset, `groundY = Graphics.Height - 6`, looks up sprite height, computes center coords, delegates to `BirdSprite`.

Optional convenience: `bool IsIdle => TargetX is null;` — useful for the 3s-idle trigger in T4. Implementer call.

### 4. Wire `WanderingBirdView` into `Playing`
In `Playing.cs`:
- Replace `private BirdRenderer Renderer { get; }` with `private WanderingBirdView Bird { get; }` (or analogous name — avoid colliding with `GameData.Bird`).
- Construct it in the ctor with the seeded initial X / target X.
- `Playing.FixedUpdate` calls `view.FixedUpdate(gameTime)`; old `bird.X` / `bird.TargetX` code deleted.
- `Playing.Update` calls `view.Update(gameTime)`.
- `Playing.Draw` calls `view.Draw(Graphics)` immediately after the `Graphics.DrawPicture(Pictures.TopGrass, …)` line and before `Mouse.Draw(this);`.

### 5. Delete `BirdRenderer.cs`
Responsibilities fully absorbed by `WanderingBirdView` + `BirdSprite`. No back-compat shim.

### 6. Update `Startup`
In `Startup.Update`, build `GameData` with `Bird = new Bird { Name = "Alain" }`. Initial `X=20` / `TargetX=100` go to `PlayingConfig` (extend the record) or to `Playing`'s ctor constants. End-to-end behavior unchanged.

## Test Plan
- [ ] `dotnet build` passes with no warnings (Nullable = error).
- [ ] Launch the game. Bird walks from `X=20` toward `X=100`, hops while moving, faces motion direction, sits flat when idle. No regression from the prior ticket.
- [ ] Inspect `PetDoodle.Data.Bird` — only `Name` present.
- [ ] Grep `PetDoodle/` for `Bird.X` and `Bird.TargetX` — no hits in game code.
- [ ] Inspect `PetDoodle.Data` compiled output — no MonoGame, no PlayPlayMini, no `Microsoft.Xna.*` references.

## Learnings

### Architectural decisions
- **Open Decisions resolved**: class name `WanderingBirdView`; `BirdSprite` is a top-level static class in the `PetDoodle` namespace; `BirdRenderer.cs` deleted outright (no shim); initial X/TargetX are `private const`s on `Playing` passed to `WanderingBirdView` ctor (Open Decision 4 default — `PlayingConfig` not extended since the seed is a hard-coded launch constant, not user data).
- **`WanderingBirdView` owns sim state, not just visual state.** Replaces `BirdRenderer`'s view-only role and absorbs the `X` / `TargetX` formerly on `Bird`. `Bird` (POCO in `PetDoodle.Data`) is now bird-intrinsic only (`Name`), as the adventures design requires.
- **`BirdSprite` stays stateless**: pure draw helper. Caller computes feet anchor, hop offset, center coords. Future game states (`Adventuring`) can call `BirdSprite.Draw` directly at a fixed position without going through `WanderingBirdView`.
- **`IsIdle` convenience property** added to `WanderingBirdView` — flagged optional in the ticket; included now because T4's 3s-idle adventure trigger will key off it and adding it later would require touching the view's API again.

### Workarounds / limitations
- **Ticket prescribed `DrawPictureWithTransformations`, but `Pictures.Bird` is registered as a `SpriteSheetMeta` (15×15), not a `PictureMeta`.** `GraphicsManager.DrawPictureWithTransformations` looks up `Pictures[name]` and would throw `KeyNotFoundException` at runtime for a sprite-sheet asset. `BirdSprite` therefore wraps `DrawSpriteFlipped` (which uses `SpriteSheets[name]`), matching `BirdRenderer`'s pre-refactor behavior. The Acceptance Criterion "same on-screen pixel result" was honored over the prescriptive method name in Implementation step 2 — the prior ticket's Learnings claimed `DrawPictureWithTransformations` was used, but the shipped code in fact used `DrawSpriteFlipped`. Both tickets' "use DrawPictureWithTransformations" guidance reflects an aspirational pattern that does not fit a sprite-sheet asset; future work that wants center-coord semantics on the bird should either re-register as a picture or extend `BirdSprite` with a sprite-sheet-aware center-coord overload.
- **`DrawSpriteFlipped` takes top-left, not center.** To satisfy the ticket's prescribed `BirdSprite.Draw(graphics, centerX, centerY, …)` signature without changing pixel output, `BirdSprite` converts center → top-left internally using `sheet.SpriteWidth / 2` (integer division). `WanderingBirdView.Draw` computes top-left first (matching the old renderer's math exactly), then offsets to center to pass through the helper. Round-trips losslessly because both ends use the same integer division.

### Interesting tidbits
- `SpriteSheet.SpriteWidth` / `SpriteHeight` are the per-frame dims (here 15×15), distinct from the underlying texture's full dims. `graphics.SpriteSheets[name]` is the way to get them for a sprite asset, mirroring `graphics.Pictures[name]` for whole-picture assets.
- The 60 Hz `FixedUpdate` contract is preserved by hosting `BirdSpeed`/`FixedTickStep` on `WanderingBirdView` and calling its `FixedUpdate` from `Playing.FixedUpdate`. Pulling sim into the view means `Playing` no longer needs its own movement constants.

### Rejected alternatives
- **Extending `PlayingConfig` with seed X/TargetX** (Open Decision 4 alternative): rejected. These are launch defaults, not per-session config; nothing varies them at construction. `private const`s on `Playing` keep the seed close to its use without inflating the config record.
- **Keeping `BirdRenderer.cs` as a transitional shim**: rejected per Open Decision 3 default. Responsibilities split cleanly into `WanderingBirdView` + `BirdSprite`; a shim would only serve to delay the rename it implies.

### Related areas affected
- `PetDoodle.Data.Bird` schema lost `X` and `TargetX`. Save migration is not yet a concern (no save system shipped) — T2 will introduce save/load against the slim shape, so no transitional concern between this ticket and T2.
- Future game states that render the bird at a fixed position (notably `Adventuring` in T4+) should call `BirdSprite.Draw` directly, computing their own center coords and feet anchor. They do not need a `WanderingBirdView` if there is no wander sim.
