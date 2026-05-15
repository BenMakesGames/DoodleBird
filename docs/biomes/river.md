# River

The first "wet" biome — bird picks its way along a river. Two pool
encounters today: one fish, one hazard. Lower-variety than grasslands by
design (the river ticket scoped 1-3 encounters and the design session
landed at 2).

## Colors

- Sky: `DawnBringers16.LightBlue`
- Ground: `DawnBringers16.Blue`

Inherited from the biome-info-pattern ticket; not re-designed here. River
shares its sky with grasslands (continuity) and uses the brightest blue
ground on the 16-color palette. `Waterfall` currently shares the same
pair — a future biome ticket may differentiate.

## Pool encounters

### Muscly Trout

A burly trout in the shallows. Two options, both safe:

| Option | Kind | Outcomes |
|---|---|---|
| Eat | Engage | Flavor "Tasty fish!" · Flavor "Too strong to grab." |
| Ignore | Ignore | Flavor "Swam past." |

Modelled on the grasslands `Snake` shape — a "speed-bump" encounter.
Engage is a coin-flip flavor (food vs. fail); Ignore is the deterministic
safe pass. No end-adventure risk on either branch — the trout is annoying,
not dangerous. Like Snake, this teaches the player that not every
encounter has to be perilous.

### Rapids

Fast water. Currently a **single-option encounter** — see deferral note
below.

| Option | Kind | Outcomes |
|---|---|---|
| Avoid rocks | Engage | Flavor "Avoided them." · EndAdventure "Hit a rock. Limped home." |

A 50/50 risk on the one option: either thread the rocks cleanly, or get
slammed and end the adventure early. Mirrors the `GiantToad` / `FightSquirrel`
risk shape (one flavor, one end-adventure) — pure "real fight, you might
lose" framing on an Engage kind, even though the "opponent" here is the
river itself.

> **Deferred**: a second option "Swim to shore" — a single outcome that
> replaces the rest of the adventure with a `[Jungle, Beach]` sub-adventure.
> This needs a biome-shift outcome mechanic that doesn't exist yet (see
> `docs/tickets/biome-umbra.md` for the parallel `BiomeShiftOutcome`
> work, and `docs/tickets/rapids-swim-to-shore.md` for the follow-up that
> appends this option once T6's resolver and the shift-outcome record
> land). Until then, Rapids is single-option. This deviates from the
> river ticket's "≥2 options per encounter" Acceptance Criterion — an
> explicit user-approved divergence, not an oversight.

## Design rationale notes

- **No `HollowLog` reuse.** `docs/adventures.md` suggested reusing the
  grasslands `HollowLog` (floating log in the river). Design session
  rejected it: the river-side log would inherit grasslands' substitute
  chain (Mushrooms, GiantToad), which doesn't fit the river biome's
  intent. Keep encounters biome-specific until a substitute chain is
  worth authoring across biomes.
- **No substitute-only encounters.** Grasslands has three (Mushrooms,
  GiantToad, FightSquirrel) reached only via `SubstituteOutcome`. River
  ships flat — every authored encounter is in the pool, every option is
  a terminal flavor / end-adventure. Less branching, smaller surface.
  Substitute chains can be added later if a design need surfaces.
- **No `Retreat` option on either encounter.** Both river encounters are
  Engage+Ignore. The grasslands `Snake` / `LoneTree` also lack `Retreat`;
  authoring convention is `Retreat` is *common*, not guaranteed. Rapids
  felt a poor fit for Retreat — "fleeing rapids" overlaps semantically
  with "Swim to shore" (the deferred option), so leaving Retreat off
  today keeps the design coherent when the second option lands.
- **Single-option Rapids is a knowing AC violation.** River ticket's
  Acceptance Criteria say "1-3 encounters, each with at least 2 options."
  Rapids ships with one option because the intended second option needs
  a mechanic deferred to a follow-up ticket. Captured here so it's
  visible to the next reader. The single option still has two outcomes,
  so the ≥1-outcome invariant is intact.
- **Text length budget.** All `DisplayName` / outcome `Text` strings
  sized for 128 px / 6×8 font (~17–21 chars around the bird). Tightest
  string today: "Hit a rock. Limped home." at 24 chars — borderline,
  same magnitude as grasslands' "Knocked silly. Home." (20). Real fit
  will be revisited when T6 lands and outcome text renders.
