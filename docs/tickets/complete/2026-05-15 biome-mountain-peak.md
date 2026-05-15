# Biome: Mountain Peak

## Context
`Biome.MountainPeak` exists with sky/ground colors and an **empty** `PossibleEncounters` pool. This ticket runs a design session with the user to author **1-3 encounters** for mountain peak, then lands the corresponding `Encounter` enum values, `EncounterExtensions.Info` entries, and `BiomeExtensions` pool entry.

No seeds suggested in design doc. Mountain peak is the terminal biome of template 2 (`grasslands → mountain → mountain peak`) — feels like a climactic biome. Possible themes: summit views / extreme weather / divine encounters / rare flora / dizzying heights.

## Prerequisites
- [Biome & Encounter Info Pattern](./biome-and-encounter-info-pattern.md)

## Design session

**To be filled during collaborative session with the user.** For each encounter:

1. **Name** (`Encounter` enum value, PascalCase).
2. **Display name** (user-facing, space-cased).
3. **Options** — variable count. For each option:
   - Label (short string).
   - **Outcomes** — non-empty list. For each outcome:
     - Text (short flavor string).
     - Effect kind: `FlavorOutcome`, `SubstituteOutcome`, or `EndAdventureOutcome`.
     - For `SubstituteOutcome`: target `Encounter`.

Capture the agreed design in `docs/biomes/mountain-peak.md`.

## Authoring checklist

- [x] New `Encounter` enum values added.
- [x] New `EncounterExtensions.Info` entries with options + non-empty outcomes.
- [x] `BiomeExtensions.Info[Biome.MountainPeak]` rebuilt with authored encounters appended.
- [x] Design doc at `docs/biomes/mountain-peak.md` written.

## Acceptance Criteria
- [x] 1-3 encounters authored for mountain peak, each with at least 2 options.
- [x] Every option has a non-empty `Outcomes[]`.
- [x] `Biome.MountainPeak.GetInfo().PossibleEncounters` contains every authored encounter.
- [x] `EncounterExtensions` static-ctor sanity check passes.
- [x] `docs/biomes/mountain-peak.md` describes design rationale.

## Test Plan
- [x] `dotnet build` passes with no warnings. (Only warning: NETSDK1194 about `--output` flag at solution level — unrelated to changes.)
- [x] If T4 landed: template 2 can roll once grasslands + mountain + mountain peak are all authored. **Template 2 is now fully eligible.** All 3 biomes have non-empty pools.
- [ ] If T5/T6 landed: each authored option renders; timer / click / outcome behavior verified. **T5/T6 not yet landed — unverifiable.**
- [x] Outcome text fits 128 px width with 6×8 font. Longest: "Waited it out." (14 chars) — well under budget.

## Learnings

### Architectural decisions
- **Single pool encounter (Thunderstorm) shipped; MysteriousCave deferred.** User's spec called for two encounters: Thunderstorm and MysteriousCave. MysteriousCave's "Enter" option required a 50/50 between a Cave-biome shift and an Umbra-biome shift — neither is expressible with the current `Outcome` taxonomy (`FlavorOutcome` / `SubstituteOutcome` / `EndAdventureOutcome`). `SubstituteOutcome` is same-biome swap only (per `docs/adventures.md` §Outcomes). The needed mechanic is `BiomeShiftOutcome`, owned by the deferred `biome-umbra.md` ticket. Per `feedback-design-session-style` memory: surface scope conflict, defer the dependent encounter into a follow-up ticket, ship the in-scope encounter today. Result: mountain peak ships 1 of intended 2 pool encounters; follow-up `mountain-peak-mysterious-cave.md` lands once `biome-umbra.md` introduces `BiomeShiftOutcome`.
- **Take Shelter deliberately not a Retreat.** Every other authored encounter with a "non-engage" option labels it Retreat / Flee / Ignore and (for Retreat / Flee) wires it to a single `EndAdventureOutcome`. Mountain Peak breaks the convention: Take Shelter ends with a single `FlavorOutcome("Waited it out.")` — the bird stays at the summit and the adventure continues. Mountain Peak is the *terminal* biome of template 2; the bird arriving at the destination shouldn't be undone by the local safe option. Different authoring intent from the perilous-detour biomes.
- **Catch lightning is 3-outcome (33/33/33), not 50/50.** User-specified directly. Outcomes: success-flavor / fail-flavor / EndAdventure-zap. Mirrors GiantBat's "Sneak around" 3-outcome shape (slip / spotted / caught), albeit Thunderstorm is in the pool while GiantBat sneak is on a substitute encounter.
- **No `SubstituteOutcome` chains.** Like Mountain and River, Mountain Peak ships flat — the single pool encounter authored does not chain into any substitute targets. The MysteriousCave follow-up will introduce biome-shift chaining, not encounter-substitute chaining.

### Problems encountered
- **`OptionKind` is gone — but the ticket template still asks for it.** This ticket's Design session template includes a `Kind` (`Engage` / `Ignore` / `Retreat`) field per option, inherited from the original `biome-and-encounter-info-pattern.md` shape. Current `EncounterOption.cs` has only `Label` + `Outcomes`; `Kind` was removed in `drop-optionkind.md` (memory `project-optionkind-semantics` documents this). Authoring matched the current source. Note: future per-biome design tickets generated from a stale template will continue to ask for `Kind` — a `/create-ticket` template refresh would close the gap.

### Interesting tidbits
- **Template 2 now fully eligible.** With grasslands, mountain, and mountain peak all having non-empty `PossibleEncounters`, `AdventureGenerator.TryRoll`'s template filter no longer excludes template 2. First 3-step template to come online.
- **Empty-pool template filter has a clean trigger.** Mountain Peak going from `[]` → `[Thunderstorm]` is a 1-line change in `BiomeExtensions.Info` that unblocks a whole template — pit-of-success in action.
- **Project rename.** Ticket prose and prior docs/learnings reference `PetDoodle` / `PetDoodle.Data` project names; actual project directories are `DoodleBird` / `DoodleBird.Data`. Code edits applied to the actual paths; ticket prose archived as-written.

### Workarounds / limitations
- **MysteriousCave deferral** (see Architectural decisions). Permanent dependency: blocked on `biome-umbra.md` shipping `BiomeShiftOutcome`. Follow-up ticket `mountain-peak-mysterious-cave.md` spawned.
- **No automated test coverage for the new encounter.** Same pattern as prior biome tickets — `EncounterExtensions` is in `DoodleBird` (MonoGame-dependent) and the test project has no ProjectReference. Static-ctor sanity check is the only runtime guarantee; fires on first access (game launch).

### Related areas affected
- `DoodleBird.Data/Encounter.cs` — 1 new enum value (`Thunderstorm`).
- `DoodleBird/Encounters/EncounterExtensions.cs` — 1 new dictionary entry.
- `DoodleBird/Biomes/BiomeExtensions.cs` — `Biome.MountainPeak.PossibleEncounters` rebuilt from `[]` to `[Encounter.Thunderstorm]`.
- Template 2 (`grasslands → mountain → mountain peak`) — was filtered out due to empty peak pool; now eligible.

### Rejected alternatives
- **3-option Thunderstorm with a Retreat.** Considered: Catch lightning + Take Shelter + Retreat (EndAdventure). Rejected — user spec was 2-option, and Take Shelter is the deliberate "safe" branch that *doesn't* end the adventure (preserving mountain peak as the destination).
- **Promoting "Caught it!" to a stat / inventory reward.** Considered (a "charge" buff, a one-time zap-next-encounter token). Deferred — stat / inventory systems don't exist. Flavor-only today; future ticket can promote when those systems land.
- **Shipping MysteriousCave with placeholder `SubstituteOutcome`s.** Considered (`SubstituteOutcome → some cave encounter` / `SubstituteOutcome → some umbra encounter`). Rejected — `SubstituteOutcome` is same-biome only by spec; using it for cross-biome swap would have set a footgun precedent. Wait for the real mechanic.
- **Rolling the MysteriousCave authoring into `biome-umbra.md`.** Considered as one of the offered paths; user chose the follow-up-ticket route. Cleaner separation: `biome-umbra.md` owns the `BiomeShiftOutcome` mechanic + Umbra biome content; the follow-up ticket owns the mountain-peak-side wiring.
