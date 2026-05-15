# Mountain Peak

The summit — terminal biome of template 2 (grasslands → mountain →
mountain peak). Authored thin at first pass: a single `Thunderstorm`
pool encounter capturing the "extreme weather at altitude" theme. A
follow-up `MysteriousCave` encounter is deferred behind the
[biome-umbra](../tickets/biome-umbra.md) ticket because it needs
`BiomeShiftOutcome` (which doesn't exist yet); when that mechanic
lands, a separate ticket appends MysteriousCave to this pool with a
50/50 Cave / Umbra biome shift.

## Colors

- Sky: `DawnBringers16.White`
- Ground: `DawnBringers16.LightGray`

Inherited from the biome-info-pattern ticket; not re-designed here.
Differentiates from `Mountain` (LightGray sky / DarkGray ground) by
shifting both bands one step brighter — reads as "thinner air, more
sky" when an adventure walks the two in sequence.

## Pool encounters

### Thunderstorm

Lightning hammers the summit. The bird can try to catch a bolt or
hunker down.

| Option | Outcomes |
|---|---|
| Catch lightning | Flavor "Caught it!" · Flavor "Missed it." · EndAdventure "Zapped! Home." |
| Take Shelter | Flavor "Waited it out." |

Catch lightning is a 1-in-3 across success / whiff / zap. Take Shelter
is a deterministic single-outcome flavor — always safe, always
continues the adventure. Distinct from Mountain's "Retreat" pattern:
Take Shelter does **not** end the adventure (no
`EndAdventureOutcome`), it just waits the storm out and the bird
walks on.

The catch-lightning success outcome is flavor-only for now; once
stat / inventory systems exist, a future ticket could promote it
into a real reward (a "charge" buff, a tail-feather frizz, a one-time
zap-the-next-encounter token).

## Deferred encounters

### Mysterious Cave (deferred → after biome-umbra)

A cave mouth at the summit. The bird either enters or moves on.

Intended outcomes for the "Enter" option:
- 50/50 between two `BiomeShiftOutcome`s — one targeting `Biome.Cave`,
  one targeting `Biome.Umbra`. Walking into the cave shifts the rest
  of the adventure into either a normal cave detour or the trippy
  Umbra side-quest.

Deferred because `BiomeShiftOutcome` is owned by
[`biome-umbra.md`](../tickets/biome-umbra.md) and doesn't exist yet.
Follow-up ticket: `docs/tickets/mountain-peak-mysterious-cave.md`
(spawned by this ticket).

## Design rationale notes

- **Take Shelter is not a Retreat.** Mountain's three encounters all
  have a Retreat option that ends the adventure via
  `EndAdventureOutcome`. Mountain Peak deliberately breaks that
  pattern — the summit is the *destination* of template 2, so the
  "safe" option keeps the bird at the peak rather than bouncing home.
  Different authoring intent for the terminal biome.
- **Catch lightning's success outcome is flavor-only.** Authored
  intent: the bird catches the bolt and it's just a vibe ("Caught
  it!"). Promoting this into a stat / inventory effect waits on those
  systems and a follow-up ticket — YAGNI for now.
- **Single pool encounter is acceptable at this stage.** AC allows
  1-3. Mountain peak ships with 1 because the obvious "MysteriousCave"
  second encounter is blocked on `BiomeShiftOutcome`. Once
  biome-umbra lands, the follow-up ticket fills the pool out.
- **Text length budget.** 6×8 font, 128 px viewport (~21 chars per
  line). Longest string here: "Waited it out." (14) — comfortable.
- **Default options.** All `EncounterOption.Outcomes` arrays
  non-empty (sanity check enforced at static-ctor time). No
  `SubstituteOutcome` chains in this iteration — flat shape, like
  Mountain.
