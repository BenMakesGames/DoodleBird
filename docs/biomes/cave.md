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

A cluster of bioluminescent mushrooms. Two options across the engage /
ignore spread.

| Option | Outcomes |
|---|---|
| Eat | ReplaceSteps "Trippy! To the Umbra" → [Umbra, Umbra] |
| Ignore | Flavor "Hopped past." |

Eat is a deterministic shift into a 2-step Umbra sub-adventure — its
single outcome is a `ReplaceStepsOutcome` targeting Umbra. Unlike
grasslands `Mushrooms` where "Eat one" is a 33/33/33 roll between safe
flavor and the trippy shift, cave Glowing Mushrooms eats *always* trip
(matches the design intent: glowing mushrooms are intrinsically more
potent). Ignore is the canonical deterministic safe pass. The
biome-umbra ticket added the Eat option; reuses `ReplaceStepsOutcome`
rather than a parallel `BiomeShiftOutcome` record.

### Giant Bat

A bat clinging to the ceiling. Three options across the engage / ignore /
retreat spread — the highest-option encounter in cave today.

| Option | Outcomes |
|---|---|
| Intimidate | Flavor "Bat flew off." · EndAdventure "Rebuffed. Home." |
| Sneak around | Flavor "Slipped past." · Flavor "Spotted! Ran past." · EndAdventure "Caught! Home." |
| Retreat | EndAdventure "Flapped home!" |

Intimidate mirrors grasslands `GiantToad` / beach `AggressiveSeagull`
risk shape — 50/50 between flavor success and end-adventure. Sneak around
is the variance option: 2/3 chance of getting past unscathed (one clean,
one close call), 1/3 chance of getting caught. Retreat is the consistent
escape with the canonical `"Flapped home!"` text reused from GiantToad
and AggressiveSeagull — three biomes now share that exact retreat line,
which deliberately reinforces "Retreat always works the same way."

Sneak's intent is *passing through quietly*, not picking a fight — even
though one of the three outcomes is end-adventure. The label communicates
the intent (the bird sneaks rather than seeks conflict); the outcome mix
encodes that stealth can fail.

### Large Boulder

A boulder blocking the path. Two options, both safe — pure flavor speed-bump.

| Option | Outcomes |
|---|---|
| Move | Flavor "Too heavy." · Flavor "Shoved it past!" |
| Ignore | Flavor "Hopped past." |

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
  but with a sharper rule.** Grasslands mushrooms: eating is a 33/33/33
  roll between two safe flavors and the trippy Umbra shift. Cave glowing
  mushrooms: eating is *always* trippy (single-outcome shift). The cave
  variant intentionally has fewer outcomes on Eat (1 vs. 3) to encode
  that determinism.
- **Giant Bat's `Sneak around` is the first three-outcome "don't engage"
  option in the codebase.** Most don't-engage options in other biomes are
  single-outcome safe passes. Sneak's variance is intentional —
  bypassing a Giant Bat isn't free; it's quieter than fighting it but
  not guaranteed. The end-adventure outcome on a sneak roll communicates
  "stealth can fail." Don't-engage labels mean "don't pick a fight," not
  "always safe."
- **Retreat option owns a single `EndAdventureOutcome` per convention.**
  Same as grasslands GiantToad and beach AggressiveSeagull — the option's
  only outcome is `EndAdventureOutcome("Flapped home!")`. That's the
  whole "end the adventure" mechanism.
- **Text length budget.** All `DisplayName` / outcome `Text` strings
  sized for 128 px / 6×8 font (~17–21 chars around the bird). Tightest
  string today: "Glowing Mushrooms" display name at 17 chars; "Spotted!
  Ran past." outcome at 18 chars; "Shoved it past!" at 15 chars.
  Comfortably within budget — no string here approaches grasslands'
  borderline "Knocked silly. Home." (20).
