# Adventures

The bird occasionally leaves home and travels through a sequence of biomes,
facing a single encounter in each. The player can steer the bird's choices
during an encounter; if they don't, the bird picks for itself. Each choice
rolls a random outcome — sometimes flavor text, sometimes a curveball that
swaps the current encounter for another, sometimes an early end to the
adventure entirely.

This document records the design decisions that frame the first iteration.
Specific encounters, options, and outcomes are designed and authored in
**per-biome design tickets** (see §Authoring workflow).

## Triggers

- The bird starts an adventure after **3 seconds of being at home** (i.e. no
  active movement target). This is intentionally simple and will likely be
  replaced later with a mood/stat-driven roll.

## Adventure structure

- An **adventure** is an ordered list of `(biome, encounter)` steps,
  rolled up-front when the adventure starts.
- The first step in the list is the current one. When it is resolved, it is
  removed from the list. When the list is empty, the adventure ends and the
  bird returns home.
- Adventures are pre-rolled to mitigate save-scumming: the player cannot
  re-load to re-roll an encounter outcome they didn't like.

### Adventure templates

The biome sequence for an adventure is chosen uniformly at random from a
fixed set of **adventure templates**. The initial set:

| # | Biomes |
|---|---|
| 1 | grasslands → river → jungle → cave |
| 2 | grasslands → mountain → mountain peak |
| 3 | river → river → waterfall → jungle |
| 4 | jungle → cave → cave |
| 5 | grasslands → beach → lagoon |
| 6 | river → river → lagoon |

A template defines biome sequence only. The encounter for each step is then
rolled (uniform random) from that biome's possible-encounters list at
generation time.

If a template references any biome whose encounter pool is currently empty,
that template is **filtered out** of the roll. The system is designed to
land before all biomes are authored, so this filter prevents crashing while
content is still being filled in. Once all biomes have at least one
authored encounter, every template is eligible.

### Biome encounter pools

Each biome has a single shared list of possible encounters; the pool does
not vary between adventures. Pools are authored incrementally via per-biome
design tickets (§Authoring workflow). At the time of this writing the
intended starting sets are:

- **grasslands**: hollow log, snake (suggested)
- **mountain**: limestone golem, steep climb, griffin (suggested)
- **river**: hollow log, muscly trout, rapids (suggested)
- **lagoon**: mermaid (suggested)
- (other biomes — jungle, cave, mountain peak, waterfall, beach — TBD)

These are loose seeds; each biome's design ticket finalises 1-3 encounters
for that biome.

## Encounters

- Each encounter presents a **variable number of options**, declared per
  encounter. This is intentional: we want freedom to hand-author wacky
  encounters (e.g. an Infinity Imp that drags the bird into an astral maze,
  a fae feast with many forking responses) without being boxed into a fixed
  option count.
- Each option has a **kind**: `Engage`, `Ignore`, or `Retreat`. Kinds are
  extensible; new ones get added when a future encounter demands different
  semantics.
- The bird has a randomly-selected **default option** highlighted when the
  encounter begins, with a fixed **5-second timer** counting down. The
  remaining time is drawn on screen to **0.1s** precision. If the timer
  expires before the player picks something else, the default fires.
- A `Retreat` option, when present, cancels the entire adventure and
  returns the bird home immediately. Retreat is *common*, not guaranteed —
  some encounters may not offer it (e.g. once-you're-in-the-maze scenarios).
  **Authoring convention**: every outcome on a `Retreat` option should be
  an `EndAdventureOutcome`. The outcome data then mechanically matches the
  kind's intent — the resolver can dispatch on either signal. Single-outcome
  options are valid when the design wants deterministic "always succeeds"
  feedback (e.g. an Ignore option that just hops the bird past).

### Outcomes

When an option is fired (by click or by timer expiry), one of its **outcomes**
is picked uniformly at random and applied. Each option has a non-empty list
of outcomes — an option with no outcomes is invalid by construction.

Outcomes are sealed-record-hierarchy in the codebase:

- **`FlavorOutcome(Text)`** — show the text, then resolve the current step
  (advance to the next, or end the adventure if it was the last step).
- **`SubstituteOutcome(Text, NewEncounter)`** — show the text, then replace
  the current step's encounter with `NewEncounter` (same biome). The new
  encounter's options load, a fresh random default is picked, and the timer
  resets to 5 seconds. The substituted encounter does not have to be in the
  current biome's `PossibleEncounters` pool — author intent rules. (Useful
  for trap-style outcomes like `Look inside hollow log` → giant toad.)
  **Constraint**: the target `Encounter` must have a fully-authored
  `EncounterInfo` entry in `EncounterExtensions.Info`, or `GetInfo()` throws
  `KeyNotFoundException` at runtime when the substitute fires. Substitute
  targets are "secondary" encounters — authored but excluded from
  `PossibleEncounters` so they're only reachable via chain.
- **`EndAdventureOutcome(Text)`** — show the text, then end the adventure
  immediately (clear `CurrentAdventure`, save, return to `Playing`).
  Same final effect as a `Retreat` option, but driven by an option's
  outcome rather than the option's kind. (Useful for narrative "you wake up
  back at home" or "the maze swallows you and spits you out" effects.)

Bird-stat / inventory effects (hunger, skills, tiredness, items) are out of
scope while those systems don't exist yet. New outcome kinds get added as
those systems land.

## Presentation

- Adventures run in a **separate game state** (`Adventuring`) from `Playing`.
- The active biome controls two colors only (no textures or decor yet):
  - **Sky color** — background clear color
  - **Ground color** — flat band along the bottom
  Background pictures, ground textures, and decorations (grass tufts, etc.)
  are out of scope for the first iteration.
- The bird is **frozen** (no hop animation) during encounters and is drawn
  at a fixed left-of-centre position, using sprite frame 0 (standing).
  Frame 1 (roosting) is reserved for future idle/sleep states.
- Bird **screen position is not part of `Bird` data** — it is purely
  visual, owned by the rendering layer in whichever game state is active.
  `Bird` carries only bird-intrinsic data (name today; hunger, skills,
  tiredness, etc. later).
- The encounter name is drawn as text to the **right** of the bird
  (placeholder until per-encounter art exists).
- Option buttons use the existing `IButton` / `ButtonList` / `LinkLabel` UI
  primitives. New `IButton` implementations are added as needed.
- The encounter timer's remaining time is drawn top-right, formatted to
  0.1s precision.

### Adventuring phases

`Adventuring` operates as a small state machine:

| Phase | What happens |
|---|---|
| `ChoosingOption` | Buttons rendered, timer ticks down. Player click or timer expiry → fire selected option → roll its outcome → transition to `ShowingOutcome`. |
| `ShowingOutcome` | Outcome text rendered (no buttons). Auto-advance after a fixed delay (~2–3s) applies the outcome's effect: flavor → resolve step; substitute → swap encounter and return to `ChoosingOption`; end-adventure → clear adventure and return to `Playing`. |

The phase enum is the canonical source of "what's on screen and how do
clicks behave". Tick logic and draw logic both gate on it.

## Persistence

- A save system is **scaffolded** as part of this feature, because the
  pre-rolled adventure has to survive the player quitting mid-adventure to
  preserve the anti-save-scum guarantee.
- For now, save is **explicitly invoked**, not automatic. The trigger is
  **whenever `GameData.CurrentAdventure` or its `RemainingSteps` list is
  mutated**: adventure rolled, step resolved, step's encounter substituted,
  adventure ended.
- Serialized as a single JSON file at
  `DirectoryHelpers.SaveDirectory/save.json` (one save only — no slots, no
  subdir). `System.Text.Json` lives in the consumer project; the
  `PetDoodle.Data` project stays dependency-free.
- Static authored data (`BiomeInfo`, `EncounterInfo`, `EncounterOption`,
  `Outcome` hierarchy) lives in the game project, not the data project, and
  is **not** serialised — it's rebuilt from code on every launch.

## Authoring workflow

After the engineering infrastructure lands (renderer refactor, data model
+ save, info pattern + option/outcome types, adventure generation +
`Adventuring` state with a placeholder advance), encounter content is
authored via **per-biome design tickets**.

One ticket per biome:

- `docs/tickets/biome-grasslands.md`
- `docs/tickets/biome-river.md`
- …and so on, one per `Biome` enum value (9 total).

Each biome ticket is a **design + implementation** unit:

1. **Design** (collaborative session with the user): finalise 1–3 encounters
   for the biome — each encounter's display name, its option list (label +
   kind), and each option's outcome list (text + effect kind).
2. **Author** (code drops):
   - Add new `Encounter` enum values in `PetDoodle.Data` for any encounters
     introduced.
   - Add entries in `EncounterExtensions.Info` for each new encounter with
     full options and outcomes.
   - Append the new encounters to that biome's `PossibleEncounters` in
     `BiomeExtensions.Info`.
3. **Design doc** at `docs/biomes/<biome>.md` capturing the encounter intents
   and any flavor notes (carrier for future expansion / rebalancing).

Encounters may be referenced by `SubstituteOutcome` targets across biomes;
shared encounters (e.g. `HollowLog` appearing in both grasslands and river
pools) are fine — whichever ticket lands first authors it, later tickets
reference the existing value.

The full encounter UI ticket and outcome resolution ticket are gated on
**all 9 biome tickets being complete**, because the UI needs every
encounter to have a valid (non-empty) option list to render and the outcome
resolver needs the full effect taxonomy authored.

## Conventions

- Biomes, encounters, and option kinds are modelled as `enum` values.
  Biomes and encounters are backed by sealed `*Info` records, looked up via
  an extension method (`enum.GetInfo()`) backed by a `FrozenDictionary`.
  Established pattern from prior games.
- Outcomes are modelled as a **sealed-record hierarchy** with an abstract
  base record (`Outcome(string Text)`) and one sealed derived record per
  effect kind. Switch expressions over the hierarchy give compiler-flagged
  exhaustiveness when new kinds are added.
- Authored data (info records, option lists, outcome lists) is built once
  at startup into `FrozenDictionary` lookups. Static-ctor sanity checks
  assert every enum value has a corresponding info entry and that every
  option's `Outcomes` array is non-empty.
- RNG: `BenMakesGames.RandomHelpers` extension `rng.Next(IList<T>)` over
  `Random.Shared`. Injectable RNG only if a test demands determinism.
