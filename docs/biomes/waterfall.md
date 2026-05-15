# Waterfall

A short biome — bird is in the river and reaches the edge. One pool
encounter today: the falls themselves. Smallest authored pool to date,
matching the biome's role in template 3 (`river → river → waterfall →
jungle`) as a single transitional beat between river and jungle.

## Colors

- Sky: `DawnBringers16.LightBlue`
- Ground: `DawnBringers16.Blue`

Inherited from the biome-info-pattern ticket; not re-designed here.
Same pair as `River` (LightBlue / Blue) — visually continuous when an
adventure walks river → river → waterfall in template 3. The flat-color
presentation can't render falling water yet, so biome differentiation
relies on the encounter, not the palette. A future pass may
differentiate (e.g. white sky for spray) when palette / decor budget
allows.

## Pool encounters

### Waterfall Drop

The river ends at a cliff. The bird is already in the water — descent
is mandatory. Two options, both **active commits** to going down: ride
the water, or break free and glide.

| Option | Outcomes |
|---|---|
| Ride water down | Flavor "Splashed down!" · EndAdventure "Battered. Home." |
| Glide down | Flavor "Glided down." |

50/50 risk on Ride: either land safely in the plunge pool and continue,
or get battered on the rocks and end the adventure. Mirrors river
`Rapids` and jungle `Quicksand` risk shape (one Flavor + one
EndAdventure outcome) — same "the terrain itself is the opponent"
framing.

Glide is the deterministic safe pass: bird leaves the water, glides
down the falls, lands clean. No risk; adventure continues. This is the
"don't engage the rapids" option, except — unlike Snake "Go around"
(grasslands) or Sandcastle "Ignore" (beach) — there's no *not*
descending the falls. Both options actively engage the descent; one
just trades risk for the bird's flight ability.

## Design rationale notes

- **Single pool encounter.** Smallest pool to date (river ships 2;
  grasslands / cave / beach ship 3; jungle ships 4). Waterfall is a
  one-beat biome in template 3 — the falls *are* the biome.
  Authoring a second encounter ("rainbow vision", "hidden cave behind
  the falls", "amphibian denizen") was considered during the design
  session and deferred — none felt load-bearing for the single
  template appearance, and YAGNI / KISS won.
- **Both options are "active commits."** Other biomes ship a "bird hops
  past / doesn't engage hazard" option (Snake "Go around", FightSquirrel
  "Glide to surface", Sandcastle "Ignore"). Waterfall doesn't fit that
  pattern — the bird is already in the river; descent is mandatory.
  Both options actively choose *how* to descend, not *whether* to.
  Options carry no kind tag; every option uniformly resolves "fire →
  roll outcome → apply."
- **No retreat-style option.** Same reasoning: the bird is already at
  the falls, in the water. Flapping back upstream against a river
  that's about to drop into a waterfall is not a coherent escape. The
  risky option (Ride) carries the EndAdventure tail; the safe option
  (Glide) continues. No third "go back the way you came" option.
- **No substitute chains.** Like river / mountain, waterfall ships
  flat — no `SubstituteOutcome` swaps into a hidden cave, rainbow
  vision, or amphib creature. Surface stays narrow, and "hidden cave
  behind the falls" / "cliff-diving" themes from the ticket's seed list
  are explicitly not authored — they were considered and dropped to
  keep the biome's single-encounter shape.
- **AC compliance**: 1 encounter (within the ticket's "1-3"); both
  options have ≥1 outcome (2 + 1). The ticket AC item "each with at
  least 2 options" is satisfied (Waterfall Drop has 2). No AC
  violations to flag — unlike river / jungle / cave where single-option
  encounters needed an explicit carve-out.
- **Text length budget.** All `DisplayName` / outcome `Text` strings
  sized for 128 px / 6×8 font (~17–21 chars around the bird). Longest
  here: `"Waterfall Drop"` display name at 14 chars; `"Battered. Home."`
  outcome at 15 chars. Comfortably within budget — none approach
  grasslands' borderline `"Knocked silly. Home."` (20).
