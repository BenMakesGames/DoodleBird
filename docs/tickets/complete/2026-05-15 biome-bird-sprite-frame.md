# Biome-Driven Bird Sprite Frame

## Context
**Current behavior**: `Adventuring.Draw` always renders the bird with sprite frame 0 (standing). The `Pictures.Bird` sheet has a second frame — a sitting / roosting pose — which is currently unused. `docs/adventures.md` §Presentation reserves frame 1 "for future idle/sleep states".

**New behavior**: Each `Biome` declares which bird pose its `Adventuring` step should render. Water biomes (River, Waterfall, Lagoon) show the bird sitting (frame 1) — visually, the bird is floating in / sitting on the water — while land biomes show the bird standing (frame 0). The choice is data on `BiomeInfo`, not a flag-check in the renderer.

## Prerequisites
- [Biome & Encounter Info Pattern](./complete/2026-05-15%20biome-and-encounter-info-pattern.md) — `BiomeInfo` record and `BiomeExtensions.Info` dictionary are the surfaces this ticket extends.

## Scope
### In scope
- `PetDoodle` (game): new `BirdFrame` enum (`Standing`, `Sitting`) in `PetDoodle/Biomes/`.
- `PetDoodle` (game): new property on `BiomeInfo` carrying the per-biome bird pose.
- `PetDoodle` (game): every entry in `BiomeExtensions.Info` (all 9 biomes) populated with a `BirdFrame` value.
- `PetDoodle` (game): `Adventuring.Draw` passes the biome's frame to `BirdSprite.Draw` instead of the hardcoded `0`.
- `docs`: update `docs/adventures.md` §Presentation to reflect frame 1's new biome-driven use; update each existing per-biome design doc (`docs/biomes/grasslands.md`, `docs/biomes/river.md`) with a one-line note on which pose the biome uses.

### Out of scope
- A roosting/sitting pose for the home (`Playing`) state. `Playing.Draw` renders via `WanderingBirdView` / its own path and is not touched. If a home idle/sleep state needs frame 1 later, that's a separate ticket.
- Animation / tweening between standing and sitting (transition frames). Static pose only.
- Per-encounter pose override (e.g. a specific encounter forces a sitting bird in a non-water biome, or vice versa). YAGNI — the biome-level default covers every authored encounter.
- New bird sprite frames. The art today has exactly frames 0 (standing) and 1 (sitting).

## Relevant Docs & Anchors
- `docs/adventures.md` §Presentation — currently asserts frame 0 always, with frame 1 "reserved for future idle/sleep states". This ticket changes that wording.
- `docs/biomes/grasslands.md`, `docs/biomes/river.md` — existing per-biome design docs; need a one-line pose note added. Both have a "Colors" section; that's a reasonable place for the pose line (rename to "Colors & Presentation" or similar), or add a sibling "Presentation" section. Implementer's call.
- Code anchors:
  - `BiomeInfo` (`PetDoodle/Biomes/BiomeInfo.cs`) — sealed positional record; this ticket extends it with one new property.
  - `BiomeExtensions.Info` static initializer (`PetDoodle/Biomes/BiomeExtensions.cs`) — every entry must pass the new value at construction (no defaulting).
  - `Adventuring.Draw` (`PetDoodle/GameStates/Adventuring.cs`) — the `BirdSprite.Draw(..., frame: 0)` call site is the only consumer.
  - `BirdSprite.Draw` (`PetDoodle/BirdSprite.cs`) — already takes a `frame` parameter; no signature change needed. The `int frame = 0` default stays for `WanderingBirdView` / `Playing` callers that don't need to think about poses.

## Constraints & Gotchas
- **`BiomeInfo` is a sealed positional record.** Adding a property is a small breaking-construction change at the source level — every `BiomeExtensions.Info` entry must pass the new value. That's a feature, not a defect: the compiler enforces full coverage across all 9 biomes. Treat it as a pit-of-success outcome of the design.
- **No nullability concerns.** `WarningsAsErrors=Nullable` is on, but the new property is an enum (value type) — non-nullable by construction.
- **Static-ctor sanity check is unaffected.** `BiomeExtensions` asserts `Info.Count == Enum.GetValues<Biome>().Length`. Adding a property to the value type doesn't touch the count. No change required to the check.
- **`BirdSprite.Draw` already takes `int frame`, not `BirdFrame`.** The `Adventuring.Draw` call site must convert the enum to an int — straightforward `(int)pose` cast or explicit mapping. Don't change `BirdSprite.Draw`'s signature on this ticket; the int-keyed API is fine for now and changing it would cascade into `WanderingBirdView` for no behavioral gain.
- **Frame 1's documented meaning changes.** `docs/adventures.md` currently says frame 1 is reserved for "future idle/sleep states". This ticket repurposes frame 1 as a multi-use "sitting" pose (water biomes today, potentially home-state idle later). See Open Decisions for the exact wording change.

## Open Decisions
1. **Beach pose** — `Standing` (bird-on-dry-sand) or `Sitting` (toes-in-water)? Default: `Standing` — the ground band is yellow (sand), and the "wet" semantics live in River/Waterfall/Lagoon. Implementer to pick during coding; flag if the user wants to weigh in.
2. **Waterfall pose** — `Sitting` (floating in the plunge pool) or `Standing` (perched at the top of the falls)? Default: `Sitting` — the ground color is `Blue` (water), same as River. If the design intent is "bird perches above the falls looking down", `Standing` fits; otherwise `Sitting`. Default to `Sitting` unless the user objects.
3. **Frame 1's documented role.** Two reasonable rewordings of `docs/adventures.md` §Presentation:
   (a) Reframe frame 1 as a multi-purpose "sitting" pose used by water biomes today and available for idle/sleep states later.
   (b) Keep "reserved for idle/sleep" and note that idle/sleep will need a *third* frame (frame 2) if the bird sprite art ever grows it.
   Default: (a). Cheaper, no new art needed, and frame 1 is already the right shape for "bird at rest". Implementer should pick (a) unless the user explicitly requests (b).
4. **`BirdFrame` enum location.** `PetDoodle/Biomes/BirdFrame.cs` (next to `BiomeInfo`) vs. `PetDoodle/BirdFrame.cs` (next to `BirdSprite`). Default: `Biomes/`, because the enum's *purpose* today is biome metadata. If a future ticket adds non-biome consumers (home idle/sleep), promotion is a 30-second move.
5. **Where to put the pose note in `docs/biomes/*.md`.** Append to the existing "Colors" section (rename to "Colors & Presentation"?), or add a separate one-line "Presentation" section. Default: rename to "Colors & Presentation" and add the pose line under it — keeps biome-visual decisions co-located.

## Acceptance Criteria
- [ ] `PetDoodle/Biomes/` contains `public enum BirdFrame { Standing, Sitting }` (or equivalent declaration).
- [ ] `BiomeInfo` has a new property of type `BirdFrame` carrying the per-biome pose (positional or `required` init — implementer's call, matches the existing record's style).
- [ ] Every entry in `BiomeExtensions.Info` constructs `BiomeInfo` with a `BirdFrame` value. Build fails (compiler-enforced) if any entry is missed.
- [ ] `Biome.River.GetInfo()`, `Biome.Waterfall.GetInfo()` (subject to Open Decision 2), and `Biome.Lagoon.GetInfo()` return `BirdFrame.Sitting`. The other biomes return `BirdFrame.Standing` (subject to Open Decision 1 for Beach).
- [ ] `Adventuring.Draw` passes `biomeInfo`'s `BirdFrame` to `BirdSprite.Draw` (converted to int). No `frame: 0` literal remains in `Adventuring.Draw`.
- [ ] `docs/adventures.md` §Presentation no longer asserts frame 0 always or reserves frame 1 for "future idle/sleep states" exclusively. Wording reflects the chosen rephrasing from Open Decision 3.
- [ ] `docs/biomes/grasslands.md` and `docs/biomes/river.md` each contain a one-line note recording the biome's bird pose.
- [ ] `BiomeExtensions` static-ctor sanity check still passes (no logical change required, but the check still runs at startup).

## Implementation

### 1. `BirdFrame` enum
New file (default location `PetDoodle/Biomes/BirdFrame.cs`; see Open Decision 4). `public enum BirdFrame { Standing, Sitting }`. Values intentionally ordered so `(int)BirdFrame.Standing == 0` and `(int)BirdFrame.Sitting == 1` — matches the underlying sprite-sheet frame indices, so `(int)pose` is the natural cast at the draw site. Document this alignment in a one-line XML or `//` comment on the enum; the next reader shouldn't have to chase the sprite sheet to verify.

### 2. Extend `BiomeInfo`
Add a `BirdFrame` property to the sealed record. The existing record is positional (`(string DisplayName, Color SkyColor, Color GroundColor, Encounter[] PossibleEncounters)`); add `BirdFrame` as a new positional field, preserving the existing order so the existing parameter positions remain stable. Order: append at the end (`..., Encounter[] PossibleEncounters, BirdFrame BirdFrame`).

### 3. Populate every `BiomeExtensions.Info` entry
Every entry must pass a `BirdFrame` value. Defaults per Open Decisions 1 & 2:
- `Sitting`: River, Waterfall, Lagoon.
- `Standing`: Grasslands, Jungle, Cave, Mountain, MountainPeak, Beach.

The compiler will refuse to build until every entry is updated — that's the safety guarantee.

### 4. Wire `Adventuring.Draw` to the biome's pose
At the `BirdSprite.Draw(...)` call in `Adventuring.Draw`, replace the literal `frame: 0` argument with `(int)biomeInfo.BirdFrame`. `biomeInfo` is already in scope (the local at the top of the method). No new locals needed.

### 5. Update `docs/adventures.md` §Presentation
Rewrite the "drawn at a fixed left-of-centre position, using sprite frame 0 (standing). Frame 1 (roosting) is reserved for future idle/sleep states." sentence per Open Decision 3 (default: option (a)). Suggested phrasing for (a): "drawn at a fixed left-of-centre position. The pose comes from `BiomeInfo.BirdFrame` — water biomes (River, Waterfall, Lagoon) render the sitting pose (frame 1, bird floating in the water); other biomes render the standing pose (frame 0). Frame 1 is also available for future idle/sleep states in the home view." Mirror the doc's existing voice — implementer should tune.

### 6. Update existing per-biome design docs
- `docs/biomes/grasslands.md`: add a one-line pose note (default: under a renamed "Colors & Presentation" section, per Open Decision 5). Suggested text: "Bird pose: `Standing` (frame 0)."
- `docs/biomes/river.md`: same shape. Suggested text: "Bird pose: `Sitting` (frame 1) — the bird floats in the river during encounters."

Future per-biome design docs should include the pose line in their Colors / Presentation section as a matter of authoring convention; not enforced by code today.

## Test Plan
- [ ] `dotnet build` passes with no new warnings.
- [ ] Launch the game; trigger an adventure that includes a `Grasslands` step (or other land-biome step). Confirm the bird renders in the standing pose (frame 0) during that step. Compare visually to the pre-change behavior — no regression.
- [ ] Trigger an adventure that includes a `River` step. Confirm the bird renders in the sitting pose (frame 1) during the river step. The pose visibly differs from the grasslands step in the same adventure.
- [ ] Inspect the rendered position: the sitting bird's center is still where the standing bird's center was (no vertical jump between biomes within an adventure). `BirdSprite.Draw` centers on `centerX/centerY`; both frames have the same sprite dimensions, so this should be free — sanity-check anyway.
- [ ] Spot-check `Biome.Waterfall` and `Biome.Lagoon` pose values match Open Decisions 1 & 2 outcomes (debug session: call `.GetInfo().BirdFrame` on each).
- [ ] Confirm `Playing` (home state) is unchanged — bird still renders via `WanderingBirdView` / `BirdSprite.Draw`'s default `frame: 0`. No accidental coupling.

## Learnings

### Architectural decisions
- **Open Decision 1 — Beach pose**: chose `Standing`. Yellow sand ground does not read as water; "toes in water" semantics belong to the explicit water biomes (River, Waterfall, Lagoon).
- **Open Decision 2 — Waterfall pose**: chose `Sitting` (default). Ground color is `Blue` (plunge pool), matching River. If a future design clearly wants "bird perched above the falls looking down", a one-line flip in `BiomeExtensions.Info` covers it.
- **Open Decision 3 — frame-1 documented role**: chose (a) — reframe frame 1 as a multi-purpose "sitting" pose available for future idle/sleep states. Cheaper than reserving a hypothetical frame 2.
- **Open Decision 4 — enum location**: kept `BirdFrame` in `DoodleBird/Biomes/` next to `BiomeInfo`. The enum's purpose today is biome metadata; if a non-biome consumer (home idle/sleep) ever appears, promotion is a trivial move.
- **Open Decision 5 — biome-doc layout**: renamed each touched biome doc's "Colors" section to "Colors & Presentation" and added a "Bird pose:" bullet under it. Keeps biome-visual decisions co-located.
- **Enum values explicitly assigned `= 0` / `= 1`** even though the defaults would have been the same. Pins the contract that `(int)BirdFrame` matches the sprite-sheet frame index, so the `(int)biomeInfo.BirdFrame` cast at the draw site does not need a translation table. Comment on the enum records the alignment.
- **Did not change `BirdSprite.Draw`'s signature** to take `BirdFrame` instead of `int`. The `WanderingBirdView` / `Playing` callers pass the default `0`; pushing the enum down would either force them to import biome metadata they don't use or require an overload — neither is justified for this ticket's scope.

### Interesting tidbits
- The project's namespace / folder is `DoodleBird` even though the ticket text consistently says `PetDoodle/`. The two refer to the same project; the rename has not propagated through the design-doc/ticket prose. Worth a one-shot doc-sweep at some point but not in scope here.
- `BiomeInfo` being a sealed positional record + `BiomeExtensions.Info` being explicit per-entry construction means adding a property is genuinely compiler-enforced across all 9 biomes. The static-ctor `.Count` check is a belt-and-braces guard for enum-vs-dictionary drift, not for new-property coverage — the latter is the compiler's job already. Worked exactly as the prerequisite ticket designed it to.
- Test suite is one (1) test today. Build was the load-bearing verifier here.

### Verification gaps
- Visual / runtime Test Plan items (steps 2–5) were not executed in this implementation pass — they require launching the game. Static code inspection confirms the wiring (`Adventuring.Draw` passes `(int)biomeInfo.BirdFrame`; River/Waterfall/Lagoon entries carry `BirdFrame.Sitting`; all other entries carry `BirdFrame.Standing`). User to confirm visually.
