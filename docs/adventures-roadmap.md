# Adventures Roadmap

Overview of the tickets that ship the adventures feature. Pairs with
`docs/adventures.md` (design) and `docs/tickets/*.md` (per-ticket detail).

## Engineering tickets (sequenced)

| # | File | Depends on |
|---|---|---|
| T1 | [`bird-position-and-renderer-refactor.md`](./tickets/bird-position-and-renderer-refactor.md) | — |
| T2 | [`adventure-data-model-and-save-scaffold.md`](./tickets/adventure-data-model-and-save-scaffold.md) | T1 |
| T3 | [`biome-and-encounter-info-pattern.md`](./tickets/biome-and-encounter-info-pattern.md) | T2 |
| T4 | [`adventure-generation-and-state-transition.md`](./tickets/adventure-generation-and-state-transition.md) | T3 |
| T5 | [`encounter-option-ui-and-timer.md`](./tickets/encounter-option-ui-and-timer.md) | T4 + all 9 biome tickets |
| T6 | [`outcome-resolution.md`](./tickets/outcome-resolution.md) | T5 |

## Biome design tickets (parallel after T3)

Each is a design-session-plus-author ticket: collaborative session with the
user to design 1-3 encounters, then drop `Encounter` enum values +
`EncounterExtensions.Info` entries + biome pool append + `docs/biomes/<name>.md`
rationale doc.

- [`biome-grasslands.md`](./tickets/biome-grasslands.md)
- [`biome-river.md`](./tickets/biome-river.md)
- [`biome-jungle.md`](./tickets/biome-jungle.md)
- [`biome-cave.md`](./tickets/biome-cave.md)
- [`biome-mountain.md`](./tickets/biome-mountain.md)
- [`biome-mountain-peak.md`](./tickets/biome-mountain-peak.md)
- [`biome-waterfall.md`](./tickets/biome-waterfall.md)
- [`biome-beach.md`](./tickets/biome-beach.md)
- [`biome-lagoon.md`](./tickets/biome-lagoon.md)

All 9 depend only on T3. Engineering can land T4 first (system runs but no
adventures roll until biomes have content); T5 + T6 wait for all 9 biome
tickets so the option UI and outcome resolver have full content to operate
on.

## Key architectural decisions baked in

- **Sealed-record `Outcome` hierarchy** (`FlavorOutcome` /
  `SubstituteOutcome` / `EndAdventureOutcome`) — exhaustive type-switch
  with `UnreachableException` default arm.
- **`AdventureGenerator.TryRoll()`** returns null until biomes have
  content → T4 lands before any biome ticket without crashing. The
  idle-timer trigger no-ops on null.
- **`Adventuring.Phase` enum** (`ChoosingOption` / `ShowingOutcome`) —
  introduced in T5 (with only `ChoosingOption` used), extended in T6.
- **T3 ships option/outcome types** (`EncounterOption`, `Outcome`
  hierarchy, `EncounterInfo.Options`) so biome tickets can author content
  before T5/T6 land.
- **Save fires per `CurrentAdventure` mutation only** — no per-frame
  writes. Mutation sites: adventure rolled (T4), step resolved (T4),
  encounter substituted (T6), adventure ended (T4 + T6).
- **`Bird` POCO** stays bird-intrinsic (name today; hunger / skills /
  tiredness later). Visual position lives in the view layer
  (`WanderingBirdView` in `Playing`; fixed constant in `Adventuring`).
- **`PetDoodle.Data` stays zero-deps** — info records + `Outcome`
  hierarchy live in `PetDoodle` (consumer), not in the data project.
  `System.Text.Json` is also `PetDoodle`-only.

## Suggested implementation order

1. **T1** — bird renderer refactor (no deps, project still runs unchanged).
2. **T2** — data model + save scaffold (Biome enum populated, Encounter
   enum empty, SaveService loads/saves, no behavior change yet).
3. **T3** — info pattern + option/outcome types (all 9 biomes have
   metadata, encounter dict empty).
4. **T4** — adventure generation + `Adventuring` state with placeholder
   `Continue` link. System runs end-to-end but `TryRoll` returns null
   until biomes are authored.
5. **Biome design tickets (×9)** — any order, parallel-friendly. As each
   lands, more adventure templates become rollable.
6. **T5** — encounter option UI + timer. Replaces placeholder advance link
   with real option `ButtonList` driven by authored content. Gated on
   all 9 biome tickets.
7. **T6** — outcome resolution. `ShowingOutcome` phase, outcome roll,
   substitute / end-adventure dispatch.

## Out of scope for this feature batch

- Bird-intrinsic stats (hunger, skills, tiredness) and their effect
  hooks on encounters.
- Inventory / items as outcome effects.
- Per-encounter art beyond the encounter-name text placeholder.
- Background pictures, ground textures, biome decorations.
- Biome transition animations between steps.
- Multi-save / save slots.
- Save versioning / migration. Revisit when a save-breaking change
  ships.
- Weighted outcome rolling. Uniform random for now; authors duplicate
  entries to fake rarity if needed.
- Keyboard / gamepad navigation of the option list. Mouse only,
  matching existing `ButtonList`.
