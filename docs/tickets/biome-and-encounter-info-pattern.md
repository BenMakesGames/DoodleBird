# Biome & Encounter Info Pattern

## Context
**Current behavior**: `Biome` enum is populated (9 values); `Encounter` enum is empty. No metadata exists for either. Adventure data structures compile but carry no display information, no colors, no option model.
**New behavior**: Every authoring primitive the system will ever need exists in the codebase, registered into `FrozenDictionary` lookups, but the per-encounter dictionary itself is empty (per-biome tickets fill it). Specifically: `BiomeInfo` sealed record + `BiomeExtensions.GetInfo()`, populated for all 9 biomes with display name + sky/ground colors + empty `PossibleEncounters`; `EncounterInfo` sealed record + `EncounterExtensions.GetInfo()` — record defined, dictionary empty; `EncounterOption` sealed record + `OptionKind` enum; `Outcome` abstract record + the three sealed derived records (`FlavorOutcome`, `SubstituteOutcome`, `EndAdventureOutcome`). Per-biome design tickets then drop in their `Encounter` enum values + dictionary entries with full options + outcomes, and append to that biome's `PossibleEncounters`.

## Prerequisites
- [Adventure Data Model & Save Scaffold](./adventure-data-model-and-save-scaffold.md) — `Biome` and `Encounter` enums must exist in `PetDoodle.Data`.

## Scope
### In scope
- `PetDoodle` (game): new `BiomeInfo` sealed record + `BiomeExtensions` static class with `FrozenDictionary<Biome, BiomeInfo>` and `GetInfo()` extension method. Dictionary populated for all 9 biomes — `DisplayName`, `SkyColor`, `GroundColor`, **empty** `PossibleEncounters`.
- `PetDoodle` (game): new `EncounterInfo` sealed record + `EncounterExtensions` static class with `FrozenDictionary<Encounter, EncounterInfo>` and `GetInfo()` extension method. Dictionary **empty** (Encounter enum has no values yet). Includes a static-ctor sanity check that asserts every option in every authored encounter has a non-empty `Outcomes` array (no-op today; live the moment encounters land).
- `PetDoodle` (game): new `OptionKind` enum with values `Engage`, `Ignore`, `Retreat`.
- `PetDoodle` (game): new `EncounterOption` sealed record with `Label`, `Kind`, `Outcomes` (non-empty `Outcome[]`).
- `PetDoodle` (game): new `Outcome` abstract record + three sealed derived records: `FlavorOutcome(string Text)`, `SubstituteOutcome(string Text, Encounter NewEncounter)`, `EndAdventureOutcome(string Text)`.

### Out of scope
- Any encounter authoring (enum values, dictionary entries, option lists, outcome lists). All belong to per-biome design tickets.
- `IGroundRenderer` / per-biome ground textures / sky decorations. `BiomeInfo` carries flat colors only (per `docs/adventures.md`).
- Adventure generation logic — T4.
- Rendering that consumes these types — T4 (`Adventuring` state) and T5/T6.
- Outcome resolution logic — T6.

## Relevant Docs & Anchors
- `docs/adventures.md` §Encounters, §Outcomes, §Presentation (color-only), §Conventions. Canonical decisions this ticket implements.
- `docs/tickets/complete/2026-05-14 bird-movement-and-renderer.md` — uses `DawnBringers16.LightBlue` / `DawnBringers16.DarkGreen`. Established palette source.
- `BenMakesGames.MonoGame.Palettes` (already referenced) — provides `DawnBringers16`.
- Code anchors:
  - `Biome` (`PetDoodle.Data/Biome.cs`) — 9 values
  - `Encounter` (`PetDoodle.Data/Encounter.cs`) — empty enum
  - `Playing.Draw` (`PetDoodle/GameStates/Playing.cs`) — current grasslands-equivalent colors: `DawnBringers16.LightBlue` sky, `DawnBringers16.DarkGreen` ground. Reuse for `Grasslands`.

## Constraints & Gotchas
- `BiomeInfo` / `EncounterInfo` / `EncounterOption` / `Outcome` hierarchy live in `PetDoodle`, not `PetDoodle.Data`. They reference MonoGame `Color` and the `Encounter` enum from data — data project remains POCO + zero-deps.
- `FrozenDictionary` is in `System.Collections.Frozen` (BCL, .NET 8+). Built once at static initialisation.
- `EncounterExtensions.Info` is empty at the close of this ticket — `GetInfo()` on any `Encounter` value will throw `KeyNotFoundException`. That's fine: no `Encounter` values exist yet to call it with.
- The static-ctor sanity check on `EncounterExtensions` (assert every option's `Outcomes` is non-empty) must be defensive against `null` arrays as well as zero-length ones, even though the `EncounterOption` ctor should prevent both. Double-guard — pit of success.
- `WarningsAsErrors=Nullable` is on. `GetInfo()` returns non-null; ensure dictionary lookup uses indexer (which throws) rather than `TryGetValue` (which makes nullability awkward).
- `BiomeInfo.PossibleEncounters` is `Encounter[]` — empty array (`[]`) for every biome at the close of this ticket. Per-biome tickets append by replacing the array (records are immutable; the dictionary entry is rebuilt).
- `SubstituteOutcome.NewEncounter` references the `Encounter` enum — empty today. The sealed record compiles fine; no `Encounter` instances are constructed in this ticket.
- The static-ctor sanity check on `BiomeExtensions.Info` should assert `Info.Count == Enum.GetValues<Biome>().Length` — protects against an enum value being added without a dictionary entry.

## Open Decisions
1. **Color choices for the 9 biomes** — `Grasslands` must match current `Playing` (`DawnBringers16.LightBlue` sky, `DawnBringers16.DarkGreen` ground). The other 8 are placeholder eyeball picks from `DawnBringers16`. Suggested starts (implementer can adjust):
   - River: light blue sky, mid-blue ground
   - Jungle: pale-green sky, dark green ground
   - Cave: near-black sky, brown ground
   - Mountain: light grey sky, dark grey ground
   - MountainPeak: white sky, light grey ground
   - Waterfall: light blue sky, blue ground
   - Beach: light blue sky, tan/yellow ground
   - Lagoon: cyan-ish sky, teal ground
   These will get iterated when biome design tickets land. Don't agonise.
2. **`BiomeInfo.PossibleEncounters` type** — `Encounter[]` vs `IReadOnlyList<Encounter>`. Default: `Encounter[]`. Per-biome tickets rebuild the entire `BiomeInfo` record when appending an encounter (records are immutable).
3. **`EncounterOption.Outcomes` type** — same shape. Default: `Outcome[]`. Non-empty guard in ctor.
4. **`EncounterOption` ctor non-empty guard** — throw `ArgumentException` if `Outcomes.Length == 0`, vs static-ctor sanity check on the dictionary. Default: **both** — ctor guard fails fast at construction, static-ctor sanity check catches any direct-record-construction edge case before deserialise / discovery weirdness.
5. **Outcome record naming** — `FlavorOutcome` / `SubstituteOutcome` / `EndAdventureOutcome` (verbose, type-name-includes-purpose) vs `Flavor` / `Substitute` / `EndAdventure` (terse, nested under `Outcome` namespace). Default: verbose, mirrors `EncounterOption` / `OptionKind` naming density.

## Acceptance Criteria
- [ ] `PetDoodle` contains `public sealed record BiomeInfo(string DisplayName, Color SkyColor, Color GroundColor, Encounter[] PossibleEncounters)`.
- [ ] `PetDoodle` contains `BiomeExtensions` static class with a private `FrozenDictionary<Biome, BiomeInfo>` and a public `static BiomeInfo GetInfo(this Biome biome)` returning `Info[biome]`. Every `Biome` enum value has an entry. `Biome.Grasslands.GetInfo().SkyColor` matches `DawnBringers16.LightBlue` and `.GroundColor` matches `DawnBringers16.DarkGreen`. Every biome's `PossibleEncounters` is `[]`.
- [ ] `PetDoodle` contains `public enum OptionKind { Engage, Ignore, Retreat }`.
- [ ] `PetDoodle` contains `public sealed record EncounterOption(string Label, OptionKind Kind, Outcome[] Outcomes)`. The ctor throws `ArgumentException` (or equivalent) when `Outcomes` is null or zero-length.
- [ ] `PetDoodle` contains `public abstract record Outcome(string Text);` and three sealed derived records: `FlavorOutcome(string Text) : Outcome(Text)`, `SubstituteOutcome(string Text, Encounter NewEncounter) : Outcome(Text)`, `EndAdventureOutcome(string Text) : Outcome(Text)`.
- [ ] `PetDoodle` contains `public sealed record EncounterInfo(string DisplayName, EncounterOption[] Options)`.
- [ ] `PetDoodle` contains `EncounterExtensions` static class with a private `FrozenDictionary<Encounter, EncounterInfo>` (empty) and a public `static EncounterInfo GetInfo(this Encounter encounter)` returning `Info[encounter]`. Static-ctor sanity check asserts every option in every entry has a non-empty `Outcomes` array (no-op now; activates as content lands).
- [ ] No new types in `PetDoodle.Data`. All info records and extension classes live in `PetDoodle`.
- [ ] `BiomeExtensions` static-ctor sanity check asserts `Info.Count == Enum.GetValues<Biome>().Length`.

## Implementation

### 1. `OptionKind` enum
New file `PetDoodle/Encounters/OptionKind.cs`: `public enum OptionKind { Engage, Ignore, Retreat }`. Folder name is suggestion only.

### 2. `Outcome` hierarchy
New file `PetDoodle/Encounters/Outcome.cs` (or split if preferred):
```
public abstract record Outcome(string Text);
public sealed record FlavorOutcome(string Text) : Outcome(Text);
public sealed record SubstituteOutcome(string Text, Encounter NewEncounter) : Outcome(Text);
public sealed record EndAdventureOutcome(string Text) : Outcome(Text);
```

### 3. `EncounterOption`
New file `PetDoodle/Encounters/EncounterOption.cs`:
```
public sealed record EncounterOption
{
    public required string Label { get; init; }
    public required OptionKind Kind { get; init; }
    public required Outcome[] Outcomes { get; init; }
    // ctor guard:
    // throw ArgumentException if Outcomes is null or empty
}
```
Or use a positional ctor with explicit validation in the body. Implementer's call. The required ctor-time guard is the pit-of-success contract.

### 4. `EncounterInfo` + `EncounterExtensions`
New files in `PetDoodle/Encounters/`. `EncounterInfo` is a sealed record with `DisplayName` and `Options`. `EncounterExtensions` mirrors the `BiomeExtensions` pattern: private `FrozenDictionary<Encounter, EncounterInfo> Info` initialised to **empty** in the static ctor. Public `GetInfo` extension method. Static ctor sanity check iterates `Info.Values.SelectMany(e => e.Options)` and asserts each option's `Outcomes` is non-null and `Length > 0` (Debug.Assert or hard throw — pick one consistently).

### 5. `BiomeInfo` + `BiomeExtensions`
New files in `PetDoodle/Biomes/` (folder name suggestion). `BiomeInfo` is the sealed record. `BiomeExtensions` initialises the `FrozenDictionary` in a static ctor with all 9 entries — display names (space-cased: "Mountain Peak", etc.), colors (Open Decision 1), empty `PossibleEncounters`. Static ctor sanity check asserts `Info.Count == Enum.GetValues<Biome>().Length`.

## Test Plan
- [ ] `dotnet build` passes with no warnings.
- [ ] Launch the game — no behavior change. Bird walks as before.
- [ ] In a debug session: enumerate `Enum.GetValues<Biome>()` and call `.GetInfo()` on each — no `KeyNotFoundException`. All 9 biomes return populated `BiomeInfo` with non-null display name, colors, and empty `PossibleEncounters`.
- [ ] `Enum.GetValues<Encounter>().Length == 0` — confirms the enum is still empty.
- [ ] Attempt to construct `new EncounterOption { Label = "test", Kind = OptionKind.Engage, Outcomes = [] }` — confirm the ctor (or required-init validation) throws.
- [ ] Construct a valid `EncounterOption` with one `FlavorOutcome("hello")` — succeeds, properties round-trip.
- [ ] `Biome.Grasslands.GetInfo().SkyColor == DawnBringers16.LightBlue` and `.GroundColor == DawnBringers16.DarkGreen`. (These match the live `Playing.Draw` colors.)
