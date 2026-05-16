# River

The first "wet" biome — bird picks its way along a river. Two pool
encounters today: one fish, one hazard. Lower-variety than grasslands by
design (the river ticket scoped 1-3 encounters and the design session
landed at 2).

## Colors & Presentation

- Sky: `DawnBringers16.LightBlue`
- Ground: `DawnBringers16.Blue`
- Bird pose: `Sitting` (frame 1) — the bird floats in the river during encounters.

Sky and ground inherited from the biome-info-pattern ticket; not
re-designed here. River shares its sky with grasslands (continuity) and
uses the brightest blue ground on the 16-color palette. `Waterfall`
currently shares the same pair — a future biome ticket may differentiate.

## Pool encounters

### Muscly Trout

A burly trout in the shallows. Two options, both safe:

| Option | Outcomes |
|---|---|
| Eat | Flavor "Tasty fish!" · Flavor "Too strong to grab." |
| Ignore | Flavor "Swam past." |

Modelled on the grasslands `Snake` shape — a "speed-bump" encounter.
Eat is a coin-flip flavor (food vs. fail); Ignore is the deterministic
safe pass. No end-adventure risk on either branch — the trout is annoying,
not dangerous. Like Snake, this teaches the player that not every
encounter has to be perilous.

### Rapids

Fast water. Two options: hold the line, or bail out across biomes.

| Option | Outcomes |
|---|---|
| Avoid rocks | Flavor "Avoided them." · EndAdventure "Hit a rock. Limped home." |
| Swim to shore | ReplaceSteps "Washed ashore." → `[Jungle, Beach]` |

**Avoid rocks** is the 50/50 risk option: thread the rocks cleanly, or
get slammed and end the adventure early. Mirrors the `GiantToad` /
`FightSquirrel` risk shape (one flavor, one end-adventure) — pure "real
fight, you might lose" framing, even though the "opponent" here is the
river itself.

**Swim to shore** is Rapids' **biome-shift waypoint**: a single outcome
that clears the post-Rapids tail of the current adventure and replaces
it with a fresh two-step detour — a jungle encounter followed by a
beach encounter, each rolled uniformly from that biome's pool at apply
time. Mechanically analogous to the way Mushrooms is grasslands'
shift-to-Umbra waypoint, except the destination here is two pool
biomes in sequence rather than a single secret biome. The encounter
rolls happen at *apply* time, not at outcome construction, preserving
the anti-save-scum invariant — once committed, the new sub-adventure
is persisted before the player can quit.

This biome-shift effect is implemented as
`ReplaceStepsOutcome(string Text, IReadOnlyList<Biome> Biomes)` — a
sibling to `BiomeShiftOutcome` in the outcome hierarchy. The two
records can be unified later if a third caller appears with overlap.

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
- **No retreat-style option on either encounter.** Neither river encounter
  ships an end-adventure escape; grasslands `Snake` / `LoneTree` also
  lack one. Authoring convention is retreat options are *common*, not
  guaranteed. Rapids stays escape-free because "Swim to shore" already
  occupies the bail-out slot — it's a *biome-shift* escape rather than
  an end-adventure escape, but it serves the same "get me out of here"
  narrative beat.
- **Text length budget.** All `DisplayName` / outcome `Text` strings
  sized for 128 px / 6×8 font (~17–21 chars around the bird). Tightest
  string today: "Hit a rock. Limped home." at 24 chars — borderline,
  same magnitude as grasslands' "Knocked silly. Home." (20). Real fit
  will be revisited when T6 lands and outcome text renders.
