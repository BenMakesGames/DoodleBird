# Biome: Cave

## Context
`Biome.Cave` exists with sky/ground colors and an **empty** `PossibleEncounters` pool. This ticket runs a design session with the user to author **1-3 encounters** for cave, then lands the corresponding `Encounter` enum values, `EncounterExtensions.Info` entries, and `BiomeExtensions` pool entry.

No seeds suggested in design doc. Cave is one of the more common biomes in the template list (templates 1 and 4) — content here is high-leverage. Possible themes: darkness / claustrophobia / underground denizens / mineral discovery / abyssal lure.

## Prerequisites
- [Biome & Encounter Info Pattern](./biome-and-encounter-info-pattern.md)

## Design session

**To be filled during collaborative session with the user.** For each encounter:

1. **Name** (`Encounter` enum value, PascalCase).
2. **Display name** (user-facing, space-cased).
3. **Options** — variable count. For each option:
   - Label (short string).
   - `OptionKind` (`Engage` / `Ignore` / `Retreat`).
   - **Outcomes** — non-empty list. For each outcome:
     - Text (short flavor string).
     - Effect kind: `FlavorOutcome`, `SubstituteOutcome`, or `EndAdventureOutcome`.
     - For `SubstituteOutcome`: target `Encounter`.

Capture the agreed design in `docs/biomes/cave.md`.

## Authoring checklist

- [x] New `Encounter` enum values added.
- [x] New `EncounterExtensions.Info` entries with options + non-empty outcomes.
- [x] `BiomeExtensions.Info[Biome.Cave]` rebuilt with authored encounters appended.
- [x] Design doc at `docs/biomes/cave.md` written.

## Acceptance Criteria
- [~] 1-3 encounters authored for cave, each with at least 2 options. **3 authored; Glowing Mushrooms ships with 1 option — explicit user-approved divergence, see Learnings.**
- [x] Every option has a non-empty `Outcomes[]` and a designed kind.
- [x] `Biome.Cave.GetInfo().PossibleEncounters` contains every authored encounter.
- [x] `EncounterExtensions` static-ctor sanity check passes.
- [x] `docs/biomes/cave.md` describes design rationale.

## Test Plan
- [x] `dotnet build` passes with no warnings (only NETSDK1194 orthogonal solution-level warning, unchanged from prior tickets).
- [ ] If T4 landed: a cave-containing template can roll once cave + all other biomes in that template are authored. **Not yet roll-runnable**: T4 landed, but every cave-containing template (1: grasslands→river→jungle→cave; 4: jungle→cave→cave) still gates on Jungle which is unauthored. Cave's authoring is correct; gate will lift the moment Jungle lands.
- [ ] If T5/T6 landed: each authored option renders; timer / click / outcome behavior verified. **T5/T6 not landed** — deferred until those tickets ship.
- [x] Outcome text fits 128 px width with 6×8 font. Longest strings: "Glowing Mushrooms" display name (17), "Spotted! Ran past." outcome (18). Within budget.

## Learnings

### Architectural decisions

- **3 pool encounters, no substitute-only encounters.** Cave ticket scoped 1–3 encounters; design session landed at 3. Cave ships flat like river — no substitute chains. Trap-style chains (à la grasslands HollowLog → GiantToad or beach Sandcastle → StartledCrab) can be added later if a design need surfaces; YAGNI today.
- **Glowing Mushrooms ships single-option (Ignore).** User's spec said "Eat → replaces remaining adventure with Umbra biome." That mechanic (`BiomeShiftOutcome` + `Biome.Umbra` content + resolver wiring) lives in the deferred `biome-umbra` ticket. The grasslands-Mushrooms precedent was *ship Eat with placeholder flavor outcomes today, append the shift later*. Cave Glowing Mushrooms is **different**: per user direction, glowing mushrooms are *always* trippy when eaten — there's no benign-eat outcome to ship as a placeholder. So we shipped single-option (Ignore only) like river `Rapids`, and updated `biome-umbra.md` to specify that the missing "Eat" option gets *added entirely* (not just an outcome appended) when the shift mechanic lands. This deviates from the cave ticket's AC ("each with at least 2 options") — explicit user-approved divergence, captured in `docs/biomes/cave.md` and in the biome-umbra ticket's scope. The data-model invariant (non-empty `Outcomes[]`) still holds.
- **Giant Bat's `Sneak around` is the first three-outcome `Ignore` option in the codebase.** Most Ignore options elsewhere are single-outcome safe passes. Sneak has three outcomes (two flavor "pass" branches, one EndAdventure "caught" branch) because bypassing a Giant Bat isn't free — stealth can fail. `Ignore` kind communicates "the bird isn't seeking conflict," not "the outcome is guaranteed safe." This is a deliberate carve-out of `OptionKind.Ignore` semantics: it's about *intent*, not *safety*.
- **Sneak's kind is `Ignore`, not `Engage`, despite an end-adventure outcome.** The narrative framing is "pass through quietly." `Engage` would have implied actively interacting with the bat (intimidating, attacking) — that's the separate `Intimidate` option. Same distinction `FightSquirrel.Glide to surface` makes: glide is `Ignore` because the bird is *leaving*, not engaging.
- **Reused canonical Retreat line `"Flapped home!"`.** Grasslands GiantToad and beach AggressiveSeagull both retreat with `EndAdventureOutcome("Flapped home!")`. Cave Giant Bat reuses the exact string — three biomes now share the canonical retreat line, deliberately reinforcing "Retreat always works the same way" via repeated text.
- **Reused `"Too heavy."` for Large Boulder Move-fail.** Same string as HollowLog's "Roll it away" fail outcome. Same semantic — bird's strength has limits — so cross-encounter consistency is a feature, not duplication noise.

### Problems encountered

- **Initial design surface gap on Glowing Mushrooms Eat.** Spec specified "Eat → umbra" with no benign fallback. The grasslands precedent (Mushrooms shipped Eat with flavor outcomes, deferred only the third trippy outcome) couldn't be mirrored because that would have meant inventing benign Eat content the spec explicitly forbade ("glowing mushrooms are always trippy when eaten"). Asked the user how to handle; landed on the single-option Ignore approach with the biome-umbra ticket extended to *add the entire Eat option* when the shift mechanic ships.

### Interesting tidbits

- **Cave's empty-pool gate.** Cave authoring alone doesn't unlock any adventure template, because every cave-containing template (1: grasslands→river→jungle→cave; 4: jungle→cave→cave) still includes Jungle, which has an empty `PossibleEncounters`. `AdventureGenerator.TryRoll` filters templates by `All(b => b.GetInfo().PossibleEncounters.Length > 0)`, so until Jungle lands, no cave roll can fire. The filter is operating correctly — this is the intended design of the "data drives availability" pattern. The moment Jungle ships, two templates (1 and 4) become runnable simultaneously.
- **`SubstituteOutcome` not used in cave today.** Beach and Grasslands both use substitute chains (Sandcastle → StartledCrab; HollowLog → GiantToad / Mushrooms; LoneTree → FightSquirrel). Cave ships substitute-free like River. The data model supports it; the design just doesn't need it yet. A future cave addition could chain (e.g. Boulder → DisturbedColony, Bat → BatSwarm) without any pattern change.

### Workarounds / limitations

- **Glowing Mushrooms single-option** documented above. Lifts when biome-umbra lands.

### Related areas affected

- `PetDoodle.Data/Encounter.cs`: 3 new enum values appended (`GlowingMushrooms`, `GiantBat`, `LargeBoulder`).
- `PetDoodle/Encounters/EncounterExtensions.cs`: 3 new dictionary entries.
- `PetDoodle/Biomes/BiomeExtensions.cs`: `[Biome.Cave]` rebuilt with non-empty `PossibleEncounters`.
- `docs/biomes/cave.md`: new design doc.
- `docs/tickets/biome-umbra.md`: scope extended — `BiomeShiftOutcome` ticket now also adds an entire "Eat" option to cave `GlowingMushrooms` (not just appends an outcome, as it does for grasslands `Mushrooms`). Prerequisites updated to list `biome-cave`. Acceptance Criteria and Implementation section 5b added.
- `AdventureGenerator.TryRoll`: still returns `null` for every template because Jungle (and Mountain, MountainPeak, Waterfall, Lagoon) remain empty. Cave's authoring is a partial unlock — it doesn't gate any template open by itself.

### Rejected alternatives

- **Ship Glowing Mushrooms `Eat` with placeholder flavor outcomes** (grasslands-Mushrooms shape). User rejected: glowing mushrooms are *always* trippy when eaten — a benign flavor outcome contradicts the canon. Single-option Ignore + deferred Eat is faithful to the design intent.
- **Make Sneak around an `Engage` option** because one of its outcomes is end-adventure. Rejected — `OptionKind` describes *intent*, not outcome safety. Sneaking is intent-level "don't engage", regardless of variance in outcome. The convention is consistent with `FightSquirrel.Glide to surface` (Ignore + EndAdventure-free) and is now also extended to "Ignore can carry end-adventure variance when stealth can fail."
- **Add a substitute chain off Large Boulder** (e.g. shoving the boulder disturbs a sleeping creature). Considered for cross-biome design richness. Deferred — cave's three pool encounters fill the design spread (deferred-shift / fight / speed-bump) and substitute chains can be added in a follow-up if Cave's run-frequency demands more variety.
