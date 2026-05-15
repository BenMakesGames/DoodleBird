# Mountain

Stone and altitude — the bird picks its way across a rocky biome where
nothing is "safe to mess with". Three pool encounters, all with a
**fight-or-run** authoring stance: every option either commits to risk
(a peck, climb, or sneak with an EndAdv tail) or bails (a single
`EndAdventureOutcome` "Retreat"). No flavor speed-bumps here —
mountain is the "you're punching above your weight" biome.

## Colors

- Sky: `DawnBringers16.LightGray`
- Ground: `DawnBringers16.DarkGray`

Inherited from the biome-info-pattern ticket; not re-designed here.
`MountainPeak` differentiates with a `White` sky and `LightGray` ground,
so the two mountain biomes read as "rising in altitude" when an
adventure walks them in sequence (template 2: grasslands → mountain →
mountain peak).

## Pool encounters

### Limestone Golem

A weathered stone golem on the slope. The bird either commits to a real
fight or flaps off.

| Option | Outcomes |
|---|---|
| Peck | Flavor "Golem crumbled." · EndAdventure "Smashed flat. Home." |
| Retreat | EndAdventure "Flapped home!" |

A pure 50/50 risk on Peck: limestone is brittle, so pecking might
crumble the golem entirely — or get the bird smashed and end the
adventure. Retreat is the consistent escape. Mirrors the GiantToad /
FightSquirrel risk shape but without the "safer" disengage option —
there's no walking around a golem mid-fight.

### Steep Climb

A near-vertical stretch of rock. The bird either commits to the climb
or turns back.

| Option | Outcomes |
|---|---|
| Climb | Flavor "Made it up!" · EndAdventure "Slipped! Home." |
| Retreat | EndAdventure "Flapped home!" |

50/50 on the climb: make it or fall. Terrain hazard reframed as a
"fight" — the cliff is the opponent. No "go around" option here:
authoring intent was that all mountain encounters force a real
commitment, not a free pass.

### Griffin

An apex predator the bird cannot fight. Sneak or flee — engaging is not
on the table.

| Option | Outcomes |
|---|---|
| Sneak past | Flavor "Slipped past." · EndAdventure "Spotted! Home." |
| Retreat | EndAdventure "Flapped home!" |

50/50 sneak: slip past unseen, or get spotted and snatched. Sneak is
the bird *not* engaging the griffin, even though the option carries
end-adventure risk. Griffin is the **only mountain encounter without a
peck-the-thing option** — the bird is explicitly outmatched. Retreat
is the consistent escape.

## Design rationale notes

- **"Fight-or-run" stance.** Every mountain encounter is two options,
  one risk + a Retreat. No safe-only option, no flavor-only branch.
  Mountain reads as the first overtly perilous biome in the early
  template set (template 2: grasslands → mountain → mountain peak);
  authoring tone matches.
- **No substitute chains.** Like river, mountain ships flat — every
  authored encounter is in the pool, no `SubstituteOutcome` swaps.
  Keeps surface narrow. Chain opportunities (e.g. "Climb dislodges a
  Golem") were considered and rejected — would have widened scope and
  the simple 2-option shape was already pulling its weight.
- **Griffin is the only mountain encounter where the non-retreat option
  is a sneak rather than a fight.** "Sneak past" is the bird *not*
  engaging the griffin — narrative intent is bypass, even though the
  option carries end-adventure risk. Earlier authoring confusion ("no
  sneak option") applied to Golem and Climb, where the option set is
  *fight* vs *flee* and any third "sneak" would have diluted the
  commitment. Griffin is the explicit exception because the bird cannot
  fight it.
- **Every Retreat option owns a single `EndAdventureOutcome`.** Matches
  the grasslands authoring convention — the option's only outcome ends
  the adventure when it applies. One mechanism, one source of truth.
- **Text length budget.** All `DisplayName` / outcome `Text` strings
  sized for 128 px / 6×8 font. Tightest strings: "Smashed flat. Home."
  (19), "Hit a rock. Limped home." (24 — from river, mountain stays
  comfortably under). Mountain's longest is "Smashed flat. Home." at
  19 chars, in line with grasslands' "Knocked silly. Home." (20).
