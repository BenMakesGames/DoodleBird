# Adventure Data Model & Save Scaffold

## Context
**Current behavior**: `GameData` holds only `Bird`. No persistence — `Startup` builds a fresh `GameData` on every launch.
**New behavior**: `GameData` gains an optional `CurrentAdventure`. `Adventure` is a popable ordered list of `(Biome, Encounter)` steps. The `Biome` enum is populated with all 9 biomes named in the design doc (fixed by design); the `Encounter` enum exists but starts **empty** — values are added incrementally by per-biome design tickets. A new save service serialises `GameData` to a single JSON file and loads it on startup; `Startup` uses the loaded data if present, otherwise constructs a default. Saving is **explicit** — invoked from code on `CurrentAdventure` mutations, not auto-triggered each frame.

## Prerequisites
- [Bird Position & Renderer Refactor](./bird-position-and-renderer-refactor.md) — `Bird` must already be slimmed to bird-intrinsic data so its serialised shape doesn't include throwaway visual fields.

## Scope
### In scope
- `PetDoodle.Data`: new `Biome` enum (all 9 values: `Grasslands`, `River`, `Jungle`, `Cave`, `Mountain`, `MountainPeak`, `Waterfall`, `Beach`, `Lagoon`).
- `PetDoodle.Data`: new `Encounter` enum — **empty** (no values declared). Per-biome design tickets add values.
- `PetDoodle.Data`: new `sealed record AdventureStep(Biome Biome, Encounter Encounter)`.
- `PetDoodle.Data`: new `Adventure` class with a mutable `List<AdventureStep> RemainingSteps` property.
- `PetDoodle.Data`: new `CurrentAdventure` optional field on `GameData`.
- `PetDoodle` (game): new `SaveService` (load + save). `System.Text.Json`. Single file at `DirectoryHelpers.SaveDirectory/save.json`. Atomic write (temp + rename). Graceful corrupt-file handling.
- `PetDoodle` (game): DI registration of `SaveService`.
- `Startup.Update`: load existing save if present; fall back to current default `GameData` construction otherwise.

### Out of scope
- Encounter enum values (per-biome tickets).
- `BiomeInfo` / `EncounterInfo` / `EncounterOption` / `Outcome` types — T3.
- Adventure rolling logic / `AdventureGenerator` — T4.
- Triggering save from gameplay (3s idle, step resolve, retreat) — T4 and T6 wire those in.
- Save versioning / migration. Format is throwaway; revisit when a save-breaking change ships.
- Multi-save / save slots.

## Relevant Docs & Anchors
- `docs/adventures.md` §Adventure structure, §Persistence — canonical decisions this ticket implements.
- `docs/tickets/complete/2026-05-14 bird-movement-and-renderer.md` — for the data-project-stays-POCO precedent and the nullable-warnings-as-errors constraint.
- `PetDoodle.Data/CLAUDE.md` — zero third-party deps. `System.Text.Json` is .NET BCL but **must live in `PetDoodle`** (consumer), not `PetDoodle.Data`. Adventure POCOs themselves must not carry serialisation attributes.
- Code anchors:
  - `GameData` (`PetDoodle.Data/GameData.cs`)
  - `Bird` (`PetDoodle.Data/Bird.cs`)
  - `DirectoryHelpers.SaveDirectory` (`PetDoodle/DirectoryHelpers.cs`) — already exists; `EnsureDirectoryExists` already creates it.
  - `Startup.Update` (`PetDoodle/GameStates/Startup.cs`) — current default-GameData construction site.
  - `Program.cs` `AddServices` block — where `SaveService` registers.

## Constraints & Gotchas
- `PetDoodle.Data` stays zero-deps. Do not add `System.Text.Json` references to that csproj. No serialisation attributes on the POCOs.
- `WarningsAsErrors=Nullable` — handle `Load()` returning null cleanly.
- `Adventure.RemainingSteps` is mutable (callers pop from the front). Don't expose as `IReadOnlyList`.
- `Encounter` enum being empty is intentional. An enum with zero declared values is valid C#. No code references `Encounter.X` until per-biome tickets land.
- Empty enum means **deserialising any pre-existing save with encounter values will fail** — but there is no such save, so this isn't a real concern at this stage.
- Save file write must be atomic-ish: write to a temp file and rename, so a crash mid-write doesn't shred the existing save. `File.Move(tmp, final, overwrite: true)` or `File.Replace`.
- `Load()` must tolerate a missing file (first launch) and return null. Existence check before reading.
- `Load()` must also tolerate a malformed file (corrupted save) gracefully — log via Serilog and return null rather than crashing.

## Open Decisions
1. **JSON enum serialisation** — integer (default) or string (via `JsonStringEnumConverter`). Default: **string**, for diffable / human-readable saves. Tiny size cost on a save this small.
2. **`Adventure.RemainingSteps` collection type** — `List<AdventureStep>` (simple, mutable, JSON-friendly) vs `Queue<AdventureStep>`. Default: `List<AdventureStep>` with `RemoveAt(0)` to pop. Single-step lookahead reads `RemainingSteps[0]`.
3. **`SaveService` shape** — interface + impl, or plain class registered concretely. Default: plain `class SaveService` registered as itself with Autofac.
4. **Default save path indirection** — accept the path via ctor (testable) or hard-code from `DirectoryHelpers.SaveDirectory`. Default: hard-code.

## Acceptance Criteria
- [ ] `PetDoodle.Data` contains `public enum Biome { Grasslands, River, Jungle, Cave, Mountain, MountainPeak, Waterfall, Beach, Lagoon }` (exact set, exact order — order matters because integer fallbacks land at the first value).
- [ ] `PetDoodle.Data` contains `public enum Encounter { }` — declared but with zero values. Compiles cleanly.
- [ ] `PetDoodle.Data` contains `public sealed record AdventureStep(Biome Biome, Encounter Encounter)`.
- [ ] `PetDoodle.Data` contains a class `Adventure` exposing a mutable `List<AdventureStep> RemainingSteps` property (settable for deserialisation, required at construction).
- [ ] `GameData` gains `Adventure? CurrentAdventure { get; set; }`. Null means "no active adventure". `Bird` remains `required`.
- [ ] `PetDoodle.Data.csproj` has no new package references. None of the new types carry attributes from `System.Text.Json` or any other non-BCL namespace.
- [ ] `PetDoodle` contains a `SaveService` with `GameData? Load()` and `void Save(GameData data)`. `Load` returns null when no save file exists. `Save` writes to `DirectoryHelpers.SaveDirectory + "/save.json"` atomically.
- [ ] `Startup.Update` calls `SaveService.Load()`; if non-null, uses it for the `GameData` passed to `Playing`; if null, constructs a default `GameData` exactly as before.
- [ ] `SaveService` is DI-registered (Autofac) and constructor-injected into `Startup`. Registration occurs in `Program.cs`'s `AddServices` block.
- [ ] Round-trip test (manual, see Test Plan): saving a `GameData` with a populated `CurrentAdventure` and reloading produces the same biome list in the same order. (Encounter values can't be tested here yet — enum is empty.)

## Implementation

### 1. Add `Biome` enum
New file `PetDoodle.Data/Biome.cs`. Public enum with the 9 values listed in Acceptance Criteria.

### 2. Add `Encounter` enum
New file `PetDoodle.Data/Encounter.cs`. `public enum Encounter { }` — no values declared. File looks empty inside the braces; this is intentional.

### 3. Add `AdventureStep`
New file `PetDoodle.Data/AdventureStep.cs`. `public sealed record AdventureStep(Biome Biome, Encounter Encounter);`. Records serialise cleanly with System.Text.Json's positional-record support in .NET 7+.

### 4. Add `Adventure`
New file `PetDoodle.Data/Adventure.cs`. Plain class. Single property: `public required List<AdventureStep> RemainingSteps { get; set; }`. Mutable list — callers in T4 / T6 pop `[0]` after resolving each step.

### 5. Extend `GameData`
In `PetDoodle.Data/GameData.cs`, add `public Adventure? CurrentAdventure { get; set; }`. Not `required`. Default null.

### 6. Create `SaveService`
New file `PetDoodle/Persistence/SaveService.cs`. Shape:
- `Load()`: if file doesn't exist, return null. Otherwise read text, `JsonSerializer.Deserialize<GameData>(json, options)`. Wrap the deserialise in `try / catch (JsonException)` that logs (Serilog `Log.Error`) and returns null — corrupted save shouldn't crash launch.
- `Save(GameData data)`: serialise with the same `options`, write to `save.json.tmp` in the same directory, then `File.Move(tmp, finalPath, overwrite: true)`.
- `options`: a static `JsonSerializerOptions` with `WriteIndented = true` and `Converters = { new JsonStringEnumConverter() }`. Cache as a private static readonly field.

### 7. Register `SaveService` and inject into `Startup`
In `Program.cs`'s `AddServices((s, c) => {...})` block, register `SaveService` (Autofac syntax — single-instance fine). In `Startup.cs`, add it as a ctor dependency alongside `GraphicsManager` / `GameStateManager` / `MouseManager`. Store in a private property.

### 8. Wire load into `Startup.Update`
At the top of the `if (Graphics.FullyLoaded)` branch, call `SaveService.Load()`. If non-null, use it; otherwise build the default `GameData` exactly as the current code does. Pass the resulting `GameData` to `Playing` via `PlayingConfig` as today.

## Test Plan
- [ ] `dotnet build` passes with no warnings.
- [ ] Launch the game with no save file present. Bird appears as before, walks. No exceptions in the log.
- [ ] Manually exercise round-trip: construct a `GameData` with `CurrentAdventure = new Adventure { RemainingSteps = [new(Biome.Grasslands, default(Encounter)), new(Biome.River, default(Encounter))] }`, call `SaveService.Save(data)`, then `SaveService.Load()`, assert the biome sequence round-trips. (Encounter values are uninteresting until per-biome tickets land.)
- [ ] Open `%AppData%/PetDoodle/Saves/save.json` after a manual save — confirm `Biome` values appear as strings (`"Grasslands"`, `"River"`), and the file is human-readable (indented).
- [ ] Corrupt the save file (delete a closing brace), relaunch the game. Launch still succeeds (falls back to default GameData), an error is logged.
- [ ] Inspect `PetDoodle.Data` compiled output — still no MonoGame, no PlayPlayMini, no `System.Text.Json` reference.
