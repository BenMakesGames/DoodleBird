# Jungle

A dense, lightly-perilous biome. Jungle appears in three of the six
adventure templates (1, 3, 4) so its content rolls relatively often.
Four pool encounters today: one trap-flora (CarnivorousPlant), one
hazard (Quicksand), one branching flora-trap (NanerTree), and one
multi-substitute exploration (LongAbandonedVillage). One substitute-only
encounter (NanerBird) reached from NanerTree.

## Colors

- Sky: `DawnBringers16.Green`
- Ground: `DawnBringers16.DarkGreen`

Inherited from the biome-info-pattern ticket; not re-designed here.
Green sky reads as canopy-filtered light; DarkGreen ground as
undergrowth shadow. No off-palette mix needed — both entries already
exist in the 16-color palette.

## Pool encounters

### Carnivorous Plant

A plant with a maw. Two options across the engage / ignore spread.

| Option | Outcomes |
|---|---|
| Ignore | Flavor "Hopped past." |
| Look inside | EndAdventure "Chomped! Home." · Flavor "Saved a butterfly!" |

Look inside is a 50/50 between catastrophe (end-adventure) and a
freebie save. Mirrors grasslands `GiantToad` / beach `AggressiveSeagull`
risk shape on the engage option, but inverts the outcome ordering: the
*bad* outcome is the EndAdventure, the *good* outcome is a flavor rescue
(rather than the toad/seagull shape where the good outcome is "they
fled"). Reuse of the `"Hopped past."` line for Ignore matches grasslands
`LoneTree` / cave `LargeBoulder` / beach `Sandcastle.Ignore` — the
canonical "deterministic no-op pass" string.

### Quicksand

A patch of quicksand. **Single-option encounter** — see AC carve-out
below.

| Option | Outcomes |
|---|---|
| Get out! | Flavor "Wriggled free." · EndAdventure "Exhausted. Home." |

A 50/50 risk on the one option: either escape clean, or burn all energy
and head home. Mirrors river `Rapids` shape exactly — same single-option
shape, same 50/50 between flavor success and end-adventure on a
hazard-not-creature opponent ("the quicksand itself"). Reused as a
SubstituteOutcome target from `LongAbandonedVillage.Explore`, but is
*also* in the pool — substitute targets don't have to be pool-excluded
(per `docs/adventures.md` §Outcomes).

### Naner Tree

A banana ("naner") tree. Two options.

| Option | Outcomes |
|---|---|
| Climb | Flavor "Naners! Tasty!" · Substitute → NanerBird |
| Ignore | Flavor "Hopped past." |

Climb is a 50/50 between a flavor freebie ("Naners! Tasty!") and a chain
into the NanerBird substitute target. Modelled on grasslands `LoneTree`
shape exactly — same two-option (Climb + Ignore), same 50/50 split on
Climb between flavor and substitute. NanerBird substitute parallels
LoneTree's FightSquirrel substitute, but with very different content
(see Substitute-only encounters below).

### Abandoned Village

A long-abandoned jungle village. Display name shortened from the spec's
"Long-Abandoned Village" (22 chars) to "Abandoned Village" (17 chars) to
fit the 128 px / 6×8-font budget; enum value `LongAbandonedVillage`
preserves the spec name.

| Option | Outcomes |
|---|---|
| Explore | Flavor "Weird machines!" · Substitute → Quicksand · Substitute → Snake · Flavor "Nothing inside." |
| Ignore | Flavor "Hopped past." |

Explore is the **highest-variance option in the codebase today** — four
outcomes, uniform 25/25/25/25. Two flavor branches (interesting find,
nothing), two substitute branches into existing pool encounters
(Quicksand from this biome, Snake from grasslands). The substitute into
grasslands `Snake` is a cross-biome chain — first one in the codebase.
Snake's existing two-option shape works fine for a jungle context; no
rebuild needed.

Spec said "33/33/33" but listed four outcomes — taken as a literal
4-element array (25/25/25/25), confirmed with user.

## Substitute-only encounters

These are not in `Biome.Jungle.PossibleEncounters` — they only appear
when another encounter's `SubstituteOutcome` swaps them in.

### Naner Bird

Reached from NanerTree → Climb. A bird in the canopy babbling
nonsense. Two options.

| Option | Outcomes |
|---|---|
| Listen | Flavor "Words made no sense." · ReplaceSteps "Wobble! Umbra calls." → [Umbra, Umbra] |
| Ignore | Flavor "Ate naners. Tasty!" |

Listen is a 50/50 between a flavor speed-bump and a 2-step Umbra
sub-adventure shift (the babble was a portal in disguise). Ignore is
the deterministic reward — the bird steals naners from underneath the
Naner Bird's perch. The Umbra-shift outcome landed via the biome-umbra
ticket; reuses `ReplaceStepsOutcome` rather than a parallel
`BiomeShiftOutcome` record.

## Design rationale notes

- **Four pool encounters — densest biome to date.** Beach / grasslands /
  cave each ship three. Jungle's four covers wider variety (trap-flora,
  hazard, branching-flora, exploration) because jungle appears in three
  of six adventure templates and the design wanted enough distinct
  rolls that repeat-template runs feel different. Knowing deviation
  from the jungle ticket's "1-3 encounters" AC; user-driven.
- **Cross-biome substitute is a new pattern.** `LongAbandonedVillage.Explore`
  substitutes into grasslands `Snake`. Until now every substitute chain
  was within-biome (HollowLog → GiantToad/Mushrooms, LoneTree →
  FightSquirrel, Sandcastle → StartledCrab, NanerTree → NanerBird).
  `docs/adventures.md` §Outcomes explicitly allows it: "The substituted
  encounter does not have to be in the current biome's
  `PossibleEncounters` pool — author intent rules." Cross-biome reuse
  keeps the substitute roster shared rather than duplicating "snake-like
  encounter" content per biome.
- **Single-option Quicksand is a knowing AC violation.** Jungle ticket
  AC: "each with at least 2 options." Quicksand ships with one — same
  shape as river `Rapids` and cave `GlowingMushrooms`. The Get out!
  option still has two outcomes, so the data-model invariant holds. No
  follow-up ticket because quicksand's "second option" doesn't have a
  clear design — unlike Rapids (Swim to shore is queued) or
  GlowingMushrooms (Eat is queued), Quicksand's authoring intent is
  "the only thing you can do is try to get out, and you either succeed
  or you don't." YAGNI / KISS.
- **NanerBird Listen ships single-outcome.** Same shape as the cave
  GlowingMushrooms deferral — the intended second outcome needs a
  mechanic that doesn't exist yet. Captured in the deferral note
  above; biome-umbra ticket appends.
- **No retreat-style option on any jungle encounter.** No jungle option
  ships a single-`EndAdventureOutcome` escape; same shape as river. A
  retreat-style option is *common*, not guaranteed. Carnivorous Plant
  could have had a flee-the-maw option, but the Look inside risk shape
  is already a clean 50/50; adding an escape would dilute the bite. Naner
  Bird is too low-stakes to need one. Quicksand's "Get out!" is
  semantically the escape already — the player is fleeing the hazard,
  even though "Get out!" is the active commit rather than a separate
  retreat button. Future ticket can revisit if a design need surfaces.
- **`Abandoned Village` display name dropped "Long-".** Display name
  budget is ~21 chars at 6×8 font; "Long-Abandoned Village" overflows
  at 22 chars. Enum value `LongAbandonedVillage` keeps the spec name —
  code is unconstrained by viewport width. This is the first
  display-name-vs-enum-name divergence in the codebase; previous
  biomes' display names were all within budget.
- **Text length budget.** All `DisplayName` / outcome `Text` strings
  sized for 128 px / 6×8 font (~17–21 chars around the bird). Longest:
  "Carnivorous Plant" display name at 17 chars; "Words made no sense."
  outcome at 20 chars; "Saved a butterfly!" outcome at 18 chars;
  "Exhausted. Home." outcome at 16 chars. All within budget. None
  approach grasslands' borderline "Knocked silly. Home." (20).
