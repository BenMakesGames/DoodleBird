# Biome: Beach

## Context
`Biome.Beach` exists with sky/ground colors and an **empty** `PossibleEncounters` pool. This ticket runs a design session with the user to author **1-3 encounters** for beach, then lands the corresponding `Encounter` enum values, `EncounterExtensions.Info` entries, and `BiomeExtensions` pool entry.

No seeds suggested in design doc. Beach appears in template 5 (`grasslands → beach → lagoon`) — transitional between land and water. Possible themes: shells / driftwood / sandcastles / crabs / message in a bottle / shore birds.

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

Capture the agreed design in `docs/biomes/beach.md`.

## Authoring checklist

- [x] New `Encounter` enum values added.
- [x] New `EncounterExtensions.Info` entries with options + non-empty outcomes.
- [x] `BiomeExtensions.Info[Biome.Beach]` rebuilt with authored encounters appended.
- [x] Design doc at `docs/biomes/beach.md` written.

## Acceptance Criteria
- [x] 1-3 encounters authored for beach, each with at least 2 options.
- [x] Every option has a non-empty `Outcomes[]` and a designed kind.
- [x] `Biome.Beach.GetInfo().PossibleEncounters` contains every authored encounter.
- [x] `EncounterExtensions` static-ctor sanity check passes.
- [x] `docs/biomes/beach.md` describes design rationale.

## Test Plan
- [x] `dotnet build` passes with no warnings.
- [ ] If T4 landed: template 5 can roll once grasslands + beach + lagoon are all authored. **Lagoon still empty — template 5 remains filtered until that biome is authored.**
- [ ] If T5/T6 landed: each authored option renders; timer / click / outcome behavior verified. **T5/T6 not yet landed — cannot exercise.**
- [x] Outcome text fits 128 px width with 6×8 font. **Longest outcome "Crab scuttled off." (18) and longest display name "Aggressive Seagull" (18); both within budget.**

## Learnings

### Architectural decisions
- **Three pool encounters chosen** to fill the standard design spread: trap (Sandcastle), speed-bump (PurpleSeaweed), fight (AggressiveSeagull). Same density as grasslands.
- **StartledCrab authored as substitute-only target.** Spec's Sandcastle → Investigate → "crab inside" required a crab encounter; per `docs/adventures.md`, substitute targets must have a fully-authored `EncounterInfo` entry or `GetInfo()` throws at runtime when the substitute fires. StartledCrab excluded from `PossibleEncounters` so it only appears via the Sandcastle chain. Mirrors grasslands' Mushrooms / GiantToad / FightSquirrel "trap target" pattern.
- **StartledCrab risk shape: Peck (Engage, 50/50 end-adventure) + Ignore (Ignore, safe disengage).** User-driven design decision in mid-implementation: replaced an initial mirror-GiantToad shape (`Flee` Retreat) with `Ignore` "move on". Kind matters — `Ignore` keeps the adventure going after the bird walks away from the crab, whereas `Retreat` would end the whole adventure. Direct parity with `FightSquirrel.Glide to surface` rather than `GiantToad.Flee`. Rename from `SandCrab` to `StartledCrab` is also from this round: emphasises that the bird startled it (and can simply walk past).
- **PurpleSeaweed outcomes copied verbatim from Mushrooms** per user spec ("possible outcomes identical to Mushroom"). Encounter name and Eat label are beach-flavored; outcome text strings are shared exactly across biomes.
- **AggressiveSeagull Retreat option labeled "Retreat".** Spec named the option "Retreat" matching the kind. GiantToad's analogous option is "Flee" — diverging here is faithful to the user's spec; future consistency pass can normalize.
- **Sandcastle Destroy second outcome "Crab fled to sea."** kept as flavor-only (no Substitute, no EndAdventure). Per spec — "nothing with different flavor: crab runs away into the ocean" is purely narrative texture.

### Problems encountered
- **Designed crab encounter without user input on first pass.** Spec implied "crab fight encounter" but did not specify shape or label. Mirrored GiantToad on autopilot; user flagged that we hadn't actually discussed it. Lesson: when a spec references an entity not fully designed in the spec, **ask** before mirroring — design-session tickets are user-driven for a reason. Saved to memory as feedback.

### Interesting tidbits
- **Ignore-kind disengage vs Retreat-kind disengage is a meaningful authoring distinction**, not a duplicate. Retreat ends the adventure (modelled mechanically by `EndAdventureOutcome` on every outcome); Ignore continues it (modelled by `FlavorOutcome`). StartledCrab is the second authored example of this pattern (FightSquirrel was the first); the distinction is now codified by repetition.
- **Static-ctor sanity check on `EncounterExtensions.Info` doesn't catch missing substitute targets** — those only fire at runtime when a `SubstituteOutcome` resolves. Authoring discipline (not infrastructure) protects against the failure mode today.
- **PurpleSeaweed shares outcome strings with Mushrooms** but is a distinct encounter. The architecture supports cross-biome thematic variants of the same gameplay shape with zero shared code — they're just dictionary entries that happen to have the same `Text` values.

### Workarounds / limitations
- Template 5 (`grasslands → beach → lagoon`) still filtered at adventure-roll time because Lagoon has an empty pool. Beach is unblocked — Lagoon ticket is the next gate.

### Related areas affected
- `PetDoodle.Data/Encounter.cs` gained four enum values (Sandcastle, PurpleSeaweed, AggressiveSeagull, StartledCrab). No save-format impact: serialization is by name and old saves never contained these values.
- `docs/biomes/beach.md` created.

### Rejected alternatives
- **Mirror GiantToad's Peck+Flee shape for the crab** — initial pass; rejected by user in favor of Peck+Ignore with rename to StartledCrab. The "startled" framing makes Ignore the natural fit (the crab is more surprised than threatening).
- **Reuse `HollowLog` for the beach** (driftwood log) — `docs/adventures.md` floated it; same trade-off river rejected. Substitute chain wouldn't transfer thematically.
- **Chain PurpleSeaweed off Sandcastle** parallel to Mushrooms-off-HollowLog — spec put it directly in the pool, so honored. Future iteration can add the chain if desired.
