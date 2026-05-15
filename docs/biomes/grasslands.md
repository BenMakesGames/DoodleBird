# Grasslands

The bird's home biome — open, lightly perilous, mostly safe but the
occasional curveball reminds the player that adventures aren't just
sightseeing. Three encounters live in the pool; three more (Mushrooms,
GiantToad, FightSquirrel) are reachable only via `SubstituteOutcome` from
those pool encounters.

## Colors

- Sky: `DawnBringers16.LightBlue`
- Ground: `DawnBringers16.DarkGreen`

These match the original `Playing` state so the transition into Grasslands
adventures feels continuous with home.

## Pool encounters

### Hollow Log

A fallen log lying across the grass. The bird can approach it in three
ways, each with very different risk shapes:

| Option | Kind | Outcomes |
|---|---|---|
| Crawl through | Engage | Flavor "Nothing inside." · Substitute → Mushrooms · Substitute → GiantToad |
| Jump over | Ignore | Flavor "Cleared it!" · Flavor "Tripped and fell." |
| Roll it away | Engage | Flavor "Rolled it away!" · Flavor "Too heavy." · Substitute → GiantToad |

Crawling is the highest-variance option: 1/3 chance of nothing, 1/3 of
finding mushrooms (which itself is a benign branch), 1/3 of waking a
giant toad (which carries an end-adventure risk). Jumping is pure flavor.
Rolling is mostly safe but still has a 1/3 chance of disturbing the toad.

Hollow Log is the **trap-style** encounter of the biome — the docs flag it
as the archetype for `SubstituteOutcome` design (`docs/adventures.md`
mentions `Look inside hollow log` → giant toad as the worked example).

### Snake

A small snake in the grass. Two options, both essentially safe — the
docs called this a "speed-bump" shape and that's intentional. Snake is
the encounter that *teaches* the player that not every encounter has to
be risky.

| Option | Kind | Outcomes |
|---|---|---|
| Go around | Ignore | Flavor "Wide berth given." |
| Intimidate | Engage | Flavor "Snake fled." |

Single-outcome options mean the result is deterministic — author-intent
"always succeeds" is preserved. A future ticket may add risk variants
(e.g. snake bite → EndAdventure) once the game has anything resembling
hit points or status.

### Lone Tree

A single tree in the field. Two options — one passive, one engaged with
a branching surprise:

| Option | Kind | Outcomes |
|---|---|---|
| Climb it | Engage | Flavor "Found bananas!" · Substitute → FightSquirrel |
| Ignore it | Ignore | Flavor "Hopped past." |

Climbing is a coin-flip between a freebie ("Found bananas!" — flavor
only, no inventory yet) and a fight. Tree's design intent: a low-cost
gamble compared to Hollow Log's three-way roulette.

> **Deferred**: a third "Eat one" outcome on Mushrooms that triggers a
> biome shift into the Umbra biome. Captured in `docs/tickets/biome-umbra.md`.
> Once that ticket lands, climb-the-tree → bananas could similarly chain
> into wackier substitutes.

## Substitute-only encounters

These are not in `Biome.Grasslands.PossibleEncounters` — they only ever
appear when another encounter's `SubstituteOutcome` swaps them in.

### Mushrooms

Reached from Hollow Log → Crawl through. Bird found a mushroom patch.

| Option | Kind | Outcomes |
|---|---|---|
| Eat one | Engage | Flavor "Tasty!" · Flavor "Bitter. Yuck!" |
| Hop away | Ignore | Flavor "Hopped away." |

Currently safe on both branches. The "Trippy" outcome that biome-shifts
to Umbra is **deferred** to the biome-umbra ticket — it requires a new
`Outcome` derived record (`BiomeShiftOutcome` or similar) which doesn't
exist yet.

### Giant Toad

Reached from Hollow Log → Crawl through or Roll it away. The toad is
big enough to actually threaten the bird.

| Option | Kind | Outcomes |
|---|---|---|
| Peck at it | Engage | Flavor "Toad hopped off." · EndAdventure "Knocked silly. Home." |
| Flee | Retreat | EndAdventure "Flapped home!" |

Pecking is a 50/50 between a safe-ish poke and getting bowled over (end
adventure). Fleeing is the consistent escape — kind `Retreat` is
expressed via a single `EndAdventureOutcome`. This makes the *data* say
"this option ends the adventure" without relying on the outcome resolver
to treat `OptionKind.Retreat` specially. Pit-of-success.

### Fight Squirrel

Reached from Lone Tree → Climb it. A squirrel in the canopy.

| Option | Kind | Outcomes |
|---|---|---|
| Peck | Engage | Flavor "Squirrel fled." · EndAdventure "Lost the fight." |
| Glide to surface | Ignore | Flavor "Glided down." |

Mirrors GiantToad in risk shape: half safe, half catastrophic on the
engage option. Gliding is the safe disengage — but kind is `Ignore`, not
`Retreat`, because the adventure continues afterward (the bird just left
the tree, not the adventure).

## Design rationale notes

- **Single-outcome options are valid** when the design wants deterministic
  feedback. Snake's two options are both single-outcome to model "always
  succeeds." Multi-outcome options exist for variety / risk variance.
- **Retreat kind is modelled via EndAdventureOutcome on every outcome of
  the Retreat option.** This double-locks the contract: the kind says
  "this option cancels the adventure," and the outcome data says the
  same thing mechanically. The resolver (T6) can use either signal.
- **EndAdventure shows up on Engage options too** (GiantToad Peck,
  FightSquirrel Peck) — that's how we communicate "this is a real fight
  and you might lose." EndAdventure ≠ Retreat: a Retreat option *intends*
  to end the adventure; an Engage option *can* end it as a bad outcome.
- **Substitute targets don't need their own pool entry.** Mushrooms,
  GiantToad, FightSquirrel are authored but not added to
  `Biome.Grasslands.PossibleEncounters` — they exist purely as
  substitute destinations. This keeps the random-roll pool narrow (3
  encounters) while letting individual paths chain into specifics.
- **Text length budget.** All `DisplayName` and outcome `Text` strings
  are sized to fit the 128×32 viewport with the 6×8 font, allowing for
  bird sprite + gap on the left. Target ≤17 chars; current max is
  "Knocked silly. Home." at 20 chars — borderline, will be revisited
  when T6 lands and we can see real text positioning. Option `Label`s
  are similar (max 16 chars: "Glide to surface").
