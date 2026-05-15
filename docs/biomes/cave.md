# Cave

Underground biome — dark, claustrophobic, peppered with strange flora and
denizens. Cave shows up in two of the six adventure templates (templates 1
and 4) so its content rolls relatively often. Three pool encounters today:
one deferred forage (Glowing Mushrooms), one fight (Giant Bat), one
speed-bump (Large Boulder). No substitute-only encounters.

## Colors

- Sky: `DawnBringers16.Black`
- Ground: `DawnBringers16.Brown`

Inherited from the biome-info-pattern ticket; not re-designed here. Black
sky reads as cave ceiling / lack of sky; brown ground is the closest
palette match for cave floor / rock.

## Pool encounters

### Glowing Mushrooms

A cluster of bioluminescent mushrooms. Currently a **single-option
encounter** — see deferral note below.

| Option | Kind | Outcomes |
|---|---|---|
| Ignore | Ignore | Flavor "Hopped past." |

> **Deferred**: an "Eat" option that **always** ships the bird into the
> Umbra biome — Glowing Mushrooms are intrinsically trippy, unlike
> grasslands `Mushrooms` where "Eat" is a coin-flip and only one outcome
> shifts. Needs the `BiomeShiftOutcome` record + `Biome.Umbra` content +
> resolver wiring from `docs/tickets/biome-umbra.md`. When that ticket
> lands, "Eat" gets appended as a single-outcome Engage option whose only
> outcome is a `BiomeShiftOutcome` targeting Umbra. Until then, Glowing
> Mushrooms is single-option. This deviates from the cave ticket's
> "≥2 options per encounter" Acceptance Criterion — an explicit
> user-approved divergence, captured here and in the biome-umbra ticket's
> scope.

### Giant Bat

A bat clinging to the ceiling. Three options across the engage / ignore /
retreat spread — the highest-option encounter in cave today.

| Option | Kind | Outcomes |
|---|---|---|
| Intimidate | Engage | Flavor "Bat flew off." · EndAdventure "Rebuffed. Home." |
| Sneak around | Ignore | Flavor "Slipped past." · Flavor "Spotted! Ran past." · EndAdventure "Caught! Home." |
| Retreat | Retreat | EndAdventure "Flapped home!" |

Intimidate mirrors grasslands `GiantToad` / beach `AggressiveSeagull`
risk shape — 50/50 between flavor success and end-adventure. Sneak around
is the variance option: 2/3 chance of getting past unscathed (one clean,
one close call), 1/3 chance of getting caught. Retreat is the consistent
escape with the canonical `"Flapped home!"` text reused from GiantToad
and AggressiveSeagull — three biomes now share that exact retreat line,
which deliberately reinforces "Retreat always works the same way."

Sneak's `Ignore` kind (not `Engage`) is intentional: the bird is *passing
through quietly*, not picking a fight. The kind matches narrative intent
even though one of the three outcomes is end-adventure. `Engage` would
have implied the bird sought conflict, which sneak doesn't.

### Large Boulder

A boulder blocking the path. Two options, both safe — pure flavor speed-bump.

| Option | Kind | Outcomes |
|---|---|---|
| Move | Engage | Flavor "Too heavy." · Flavor "Shoved it past!" |
| Ignore | Ignore | Flavor "Hopped past." |

Modelled on grasslands `Snake` / river `MusclyTrout` speed-bump shape —
no end-adventure risk. Move is a coin-flip between fail and success
(reuses HollowLog's `"Too heavy."` line for cross-biome consistency on
"object too big to move"). Ignore is the deterministic safe pass.

Cave needs at least one fully-safe encounter to balance Giant Bat's
end-adventure risk and Glowing Mushrooms' deferred-future-risk. Boulder
fills that slot.

## Design rationale notes

- **Three pool encounters, no substitute-only encounters.** Matches
  river's substitute-free shape rather than grasslands' / beach's
  trap-chain shape. Cave can grow chains later if a design need surfaces
  (e.g. Boulder → DisturbedColony substitute, Bat → BatSwarm), but
  shipping flat keeps the surface narrow.
- **Glowing Mushrooms is the cave equivalent of grasslands `Mushrooms`
  but with a sharper rule.** Grasslands mushrooms: eating is a coin-flip
  between safe and trippy. Cave glowing mushrooms: eating is *always*
  trippy. The cave variant intentionally has fewer outcomes per option
  (1 vs. 3) to encode that determinism. Ignore is the only ship-today
  option because the always-trippy Eat needs the deferred biome-shift
  mechanic.
- **Single-option Glowing Mushrooms is a knowing AC violation.** Cave
  ticket Acceptance Criteria says "each with at least 2 options." Same
  shape of deferral as river's Rapids — single-option ships today, the
  follow-up ticket (biome-umbra) appends the second option. The single
  option still has a non-empty `Outcomes[]`, so the data-model invariant
  holds.
- **Giant Bat's `Sneak around` is the first three-outcome `Ignore`
  option in the codebase.** Most Ignore options in other biomes are
  single-outcome safe passes. Sneak's variance is intentional —
  bypassing a Giant Bat isn't free; it's quieter than fighting it but
  not guaranteed. The end-adventure outcome on a sneak roll communicates
  "stealth can fail." This is a deliberate design carve-out: `Ignore`
  semantically means "don't engage", not "always safe."
- **Retreat option uses `EndAdventureOutcome` per convention.** Same as
  grasslands GiantToad and beach AggressiveSeagull — every outcome on a
  Retreat option is an `EndAdventureOutcome`. Both the kind and the
  outcome data say "this option ends the adventure." Resolver can use
  either signal.
- **Text length budget.** All `DisplayName` / outcome `Text` strings
  sized for 128 px / 6×8 font (~17–21 chars around the bird). Tightest
  string today: "Glowing Mushrooms" display name at 17 chars; "Spotted!
  Ran past." outcome at 18 chars; "Shoved it past!" at 15 chars.
  Comfortably within budget — no string here approaches grasslands'
  borderline "Knocked silly. Home." (20).
