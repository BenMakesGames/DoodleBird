# Beach

The transitional biome between land and water. Template 5 routes
`grasslands → beach → lagoon`, so beach acts as the dry-side stepping
stone into the lagoon. Three pool encounters cover the design spread:
one trap-shape (Sandcastle), one speed-bump (PurpleSeaweed), one
high-risk fight (AggressiveSeagull). One substitute-only target
(StartledCrab) is reached from the Sandcastle trap chain.

## Colors

- Sky: `DawnBringers16.LightBlue`
- Ground: `DawnBringers16.Yellow`

Inherited from the biome-info-pattern ticket; not re-designed here.
Yellow ground is the closest palette match for sand.

## Pool encounters

### Sandcastle

A small sandcastle on the beach. Three options across the engage /
ignore spectrum. Investigate is the **trap-style** option — coin-flip
between flavor nothing and waking a crab.

| Option | Kind | Outcomes |
|---|---|---|
| Investigate | Engage | Flavor "Empty inside." · Substitute → StartledCrab |
| Destroy | Engage | Flavor "Stomped flat." · Flavor "Crab fled to sea." |
| Ignore | Ignore | Flavor "Hopped past." |

Modelled on grasslands `HollowLog` shape — Investigate's substitute
chain into StartledCrab mirrors HollowLog → GiantToad. Destroy is the safe
engage: two flavor branches, no risk, but the "Crab fled to sea." line
hints that crabs are lurking elsewhere too. Ignore is the deterministic
no-op pass.

### Purple Seaweed

A pile of purple seaweed on the sand. Two options, both safe.

| Option | Kind | Outcomes |
|---|---|---|
| Eat | Engage | Flavor "Tasty!" · Flavor "Bitter. Yuck!" |
| Ignore | Ignore | Flavor "Hopped past." |

Designed (per user spec) as a clone of grasslands `Mushrooms` outcomes:
Eat is a coin-flip between "Tasty!" and "Bitter. Yuck!", Ignore is the
safe pass. Like Mushrooms, this is a benign forage. Unlike Mushrooms,
Purple Seaweed appears directly in the pool — there is no Sandcastle
chain into it (yet).

### Aggressive Seagull

A territorial seagull. Two options, one high-risk engage and one
guaranteed retreat.

| Option | Kind | Outcomes |
|---|---|---|
| Intimidate | Engage | Flavor "Seagull flew off." · EndAdventure "Tackled! Home." |
| Retreat | Retreat | EndAdventure "Flapped home!" |

Mirrors grasslands `GiantToad` risk shape: Engage is a 50/50 between
flavor success and end-adventure, Retreat is the consistent escape.
Retreat option is named `Retreat` (matching the kind) rather than
`Flee` (GiantToad's label) — the user spec used the bare word.

## Substitute-only encounters

These are not in `Biome.Beach.PossibleEncounters` — they only appear
when another encounter's `SubstituteOutcome` swaps them in.

### Startled Crab

Reached from Sandcastle → Investigate. Risk-on-Engage, safe-on-Ignore
shape — closer to `FightSquirrel`'s Peck / Glide-to-surface than
`GiantToad`'s Peck / Flee.

| Option | Kind | Outcomes |
|---|---|---|
| Peck | Engage | Flavor "Crab scuttled off." · EndAdventure "Pinched. Home." |
| Ignore | Ignore | Flavor "Hopped past." |

Pecking is a 50/50 between safe scare-off and getting pinched into a
forced return home. Ignore is the safe disengage — kind is `Ignore`
(not `Retreat`) because the adventure *continues* afterward. The bird
walked away from a startled crab, not abandoned the whole adventure.
This is intentional design parity with `FightSquirrel.Glide to surface`.

## Design rationale notes

- **Three pool encounters fill the standard design spread.** Trap
  (Sandcastle), speed-bump (PurpleSeaweed), fight (AggressiveSeagull).
  Matches grasslands' three-encounter density; deliberately denser than
  river's two-encounter pool because beach is one of the more iconic
  biomes in the template list.
- **Sandcastle is the only encounter with three options.** Reuse of the
  HollowLog three-option pattern keeps the trap shape recognizable
  across biomes. The other two pool encounters are two-option (matches
  river / grasslands speed-bumps).
- **PurpleSeaweed mirrors Mushrooms outcomes exactly per user spec.**
  Both encounters model "forage and risk a bad taste." Whether to chain
  PurpleSeaweed off Sandcastle (parallel to Mushrooms off HollowLog) is
  deferred — for now PurpleSeaweed sits in the pool directly.
- **AggressiveSeagull uses the `Retreat` label rather than `Flee`.** The
  spec named the option "Retreat" so the label matches the kind. Both
  labels are valid by authoring convention — GiantToad's "Flee" is
  fictionally evocative; "Retreat" is mechanically literal. Future
  consistency pass can pick one.
- **StartledCrab substitute target is authored even though only Sandcastle
  reaches it.** Per `docs/adventures.md`, substitute targets must have
  a fully-authored `EncounterInfo` entry or `GetInfo()` throws at
  runtime when the substitute fires. StartledCrab is excluded from
  `PossibleEncounters` so the random-roll pool stays narrow (3) — it
  only appears via the Sandcastle trap chain.
- **Text length budget.** All `DisplayName` / outcome `Text` strings
  sized for 128 px / 6×8 font (~17–21 chars around the bird). Longest:
  "Aggressive Seagull" display name at 18 chars; "Crab scuttled off."
  outcome at 18 chars. Comfortably within budget — no string here
  approaches grasslands' borderline "Knocked silly. Home." (20).
