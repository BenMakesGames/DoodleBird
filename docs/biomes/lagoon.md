# Lagoon

The destination biome of templates 5 (`grasslands → beach → lagoon`) and
6 (`river → river → lagoon`). Two pool encounters: one reused
speed-bump (`MusclyTrout`, shared with River) and one no-risk
single-option commitment encounter (`Mermaid`). Lagoon is the original
"no retreat" design example — `Mermaid` ships without an end-adventure
escape because there is nothing to flee from.

## Colors

- Sky: `DawnBringers16.LightBlue`
- Ground: `DawnBringers16.DarkBlue`

Inherited from the biome-info-pattern ticket; not re-designed here. The
16-color palette has no teal/cyan distinct from `LightBlue`, so DarkBlue
ground gives the deepest contrast for "deep water."

## Pool encounters

### Muscly Trout

Reused unchanged from `Biome.River`'s pool. Same `Encounter.MusclyTrout`
entry in `EncounterExtensions.Info` — no new authoring needed. See
`docs/biomes/river.md` for the option / outcome shape.

Cross-biome reuse is explicitly endorsed by `docs/adventures.md`
§Authoring workflow: shared encounters (e.g. `HollowLog` in both
grasslands and river) are fine; whichever ticket lands first authors
them, later tickets reference the existing value.

### Mermaid

A mermaid in the lagoon. Single option, two safe flavor outcomes — the
"commit once you engage" shape with no end-adventure risk.

| Option | Outcomes |
|---|---|
| Listen | Flavor "Words made no sense." · Flavor "Sang with her!" |

50/50 between the bird not understanding her speech and the bird
joining her song. Both outcomes are flavor — the encounter always
resolves cleanly and advances to the next step. No fight, no escape,
no substitute chain. Mermaid is the deliberate "encounter with no
hazard" shape.

The reused outcome wording "Words made no sense." also appears on
`NanerBird.Listen` — that's intentional cross-biome flavor consistency
(both encounters are "creature speaks; bird is a bird; bird does not
have grammar"), not an oversight.

## Design rationale notes

- **Two pool encounters; one cross-biome, one new.** River sets the
  precedent for two-encounter pools. Reusing `MusclyTrout` from river
  gives lagoon a familiar speed-bump on top of its signature `Mermaid`
  without inflating the encounter count. The shared pool entry also
  means template 6 (`river → river → lagoon`) can roll `MusclyTrout`
  three steps in a row — that's fine; uniform random and small pools
  guarantee occasional repetition.
- **`Mermaid` is single-option — knowing AC "≥2 options" violation.**
  Lagoon ticket's Acceptance Criteria say "1-3 encounters, each with at
  least 2 options." Mermaid ships with one because the design is
  intentionally "Listen, hear something, move on" — there is no
  meaningful second option that isn't redundant ("Ignore" would just be
  a flavor-only no-op). Single-option Rapids in `docs/biomes/river.md`
  is the precedent; like Rapids, Mermaid's single option still has
  two outcomes so the ≥1-outcome invariant holds. Captured here for
  visibility.
- **No retreat-style option on either encounter.** Lagoon was called
  out in the ticket as the original "no retreat" example. `MusclyTrout`
  inherits river's no-retreat shape (neither option is a single
  `EndAdventureOutcome`). `Mermaid` has no end-adventure outcome
  anywhere — once Listen fires, the adventure continues. This matches
  the "player commits once they engage" framing for lagoon.
- **No substitute-only encounters.** Lagoon ships flat — both pool
  encounters are terminal flavor. No `SubstituteOutcome` chains, no
  hidden encounters. Smaller surface area than grasslands/beach.
- **Text length budget.** All `DisplayName` / outcome `Text` strings
  sized for 128 px / 6×8 font. Longest string: "Words made no sense."
  at 20 chars — same as NanerBird's existing copy. Within budget.
