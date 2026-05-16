# Umbra

The "secret" biome — Umbra is **not** part of any adventure template in
`AdventureGenerator.Templates`. It is reachable **only** via
`ReplaceStepsOutcome` from a small handful of trigger encounters in other
biomes (grasslands `Mushrooms.Eat one`, cave `GlowingMushrooms.Eat`, jungle
`NanerBird.Listen`). When one of those triggers fires, the rest of the
in-flight adventure is wiped and replaced with a 2-step Umbra sub-adventure.

Aesthetic intent: trippy, unreal, dreamlike. The biome is the bird's
"the mushrooms hit" moment — strange creatures, strange waters, strange
texts. Each encounter ships safe-leaning (no end-adventure on pool
encounters) so the detour feels weird rather than dangerous; the only
end-adventure risk in Umbra is the substitute-only `HungrySpirit`
reached from `LostSpirit.Help`.

## Colors & Presentation

- Sky: `DawnBringers16.Pink` (#d2aa99)
- Ground: `DawnBringers16.DarkPurple` (#442434)
- Bird pose: `Standing` (frame 0)

Pink-on-DarkPurple was chosen for saturated-unreal candy-trippy contrast
against the rest of the palette's mostly-natural pairings (LightBlue/
DarkGreen grasslands, Black/Brown cave, etc.). Pink is otherwise unused
as a sky in any biome today, which reinforces "you are somewhere else."
DarkPurple is otherwise unused as a ground, same reason. Standing pose
matches most biomes — Umbra is solid-ish ground, not water.

## Pool encounters

These three live in `Biome.Umbra.PossibleEncounters`. The Umbra
sub-adventure rolls 2 steps uniformly from this list (so a shift can
yield same-encounter twice or two different).

### Lost Spirit

A spirit drifting in the void. Two options across the engage / ignore
spread.

| Option | Outcomes |
|---|---|
| Help | Flavor "Guided home." · Substitute → HungrySpirit ("A trick!") |
| Ignore | Flavor "Hopped past." |

Help is the only Umbra option that branches into a substitute target.
50/50 between a benign flavor ("Guided home.") and the only real
end-adventure risk in the biome (HungrySpirit's Feed/Intimidate both
carry a 50% end-adventure outcome). Ignore is the canonical
deterministic-no-op pass, mirroring the pattern across grasslands
`LoneTree`, cave `LargeBoulder`, beach `Sandcastle.Ignore`, jungle
`CarnivorousPlant.Ignore`.

### Dark River

A river in shadow. Two options, both pure flavor (no end-adventure
risk, no substitutes).

| Option | Outcomes |
|---|---|
| Fish | Flavor "Beautiful catch!" · Flavor "Awful catch!" · Flavor "Caught nothing." |
| Search shore | Flavor "Found two coins!" · Flavor "Found nothing." |

Fish is a uniform 33/33/33 flavor roll. Search shore is a 50/50 flavor
roll. Both options are safe — Dark River is the speed-bump of Umbra,
matching the role grasslands `Snake` / river `MusclyTrout` / cave
`LargeBoulder` play in their biomes. The "two coins" flavor hints at
a future inventory system without committing to one (no inventory
exists today).

### Magic Library

A library that shouldn't exist. **Single-option encounter** — knowing
AC violation, see Design Rationale.

| Option | Outcomes |
|---|---|
| Browse | Flavor "Confusing books." · Flavor "Fascinating books!" |

Browse is a 50/50 flavor roll. Magic Library is the most abstract
Umbra encounter — pure curiosity, no risk, no chain. The single-option
shape is intentional: the encounter intent is "the bird wandered into
a library; there is one thing to do in a library."

## Substitute-only encounters

Not in `Biome.Umbra.PossibleEncounters` — only reached via
`SubstituteOutcome`.

### Hungry Spirit

Reached from `LostSpirit.Help` (50% roll). The spirit was hungry, not
lost. Two options across the engage / engage spread (no retreat-style).

| Option | Outcomes |
|---|---|
| Feed | Flavor "Sated. Hopped past." · EndAdventure "Chased. Home." |
| Intimidate | Flavor "Spirit fled." · EndAdventure "Attacked. Home." |

Both options are 50/50 flavor / end-adventure. No retreat option —
once the trick is sprung, the bird has to handle the spirit one way or
the other. Hungry Spirit is the only end-adventure carrier in Umbra;
without it the biome would be a no-stakes detour.

## Design rationale notes

- **Umbra is not in any adventure template.** Per `AdventureGenerator.Templates`,
  Umbra is unreachable via the normal roll path. It exists only as a
  shift target from three trigger encounters elsewhere (Mushrooms,
  GlowingMushrooms, NanerBird `Listen`). This is the first "secret"
  biome — discovered by player action, not rolled by template.
- **2-step sub-adventure (StepCount = 2).** Long enough to feel like a
  real detour, short enough not to drag. Each shift trigger hard-codes
  `[Biome.Umbra, Biome.Umbra]` as the replacement biome list, so the
  player gets two Umbra rolls before returning to `Playing`.
- **Reused `ReplaceStepsOutcome`, not a new `BiomeShiftOutcome`.** The
  ticket originally specified a new `BiomeShiftOutcome(Text, Biome,
  StepCount)` record. The existing `ReplaceStepsOutcome(Text,
  IReadOnlyList<Biome>)` (landed by the rapids-swim-to-shore ticket)
  subsumes it — `[Biome.Umbra, Biome.Umbra]` gives the same effect as
  `(Biome.Umbra, 2)` would. Avoiding two records that do the same job
  is a YAGNI / pit-of-success call. User-confirmed when surfaced.
- **No `BiomeShiftOutcome` from Umbra back to other biomes.** Recursive
  shifts (e.g. Umbra → Grasslands "come down") are out of scope per
  ticket. Future ticket can add if a design need surfaces.
- **Single-option Magic Library is a knowing AC violation.** Same shape
  of deviation as river `Rapids` (one option), cave `GlowingMushrooms`
  (one option pre-this-ticket), jungle `Quicksand` (one option). The
  Browse option still has two outcomes, so the data-model invariant
  holds. No follow-up ticket — the design intent is exactly "one thing
  to do in a library."
- **No retreat-style option on any Umbra encounter.** Same shape as
  river / jungle. Umbra is mostly safe (only HungrySpirit carries
  end-adventure risk); a retreat doesn't fit the biome's "you've
  wandered into a dream" framing — the bird leaves Umbra by completing
  its 2 steps, not by fleeing.
- **Text length budget.** All `DisplayName` / outcome `Text` strings
  sized for 128 px / 6×8 font (~21 chars max around the bird).
  Tightest: "Trippy! To the Umbra" (20 chars), "Wobble! Umbra calls."
  (20), "Sated. Hopped past." (19), "Fascinating books!" (18).
  All within budget. None exceed grasslands' borderline "Knocked
  silly. Home." (20).
