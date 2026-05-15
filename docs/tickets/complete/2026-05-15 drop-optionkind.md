# Drop `OptionKind` enum

## Context
**Current behavior**: `EncounterOption` carries a required `Kind` of type `OptionKind { Engage, Ignore, Retreat }`. The kind is **pure metadata today** — `Adventuring` doesn't read it (no T5/T6 yet). The two pending consumer tickets (`encounter-option-ui-and-timer.md`, `outcome-resolution.md`) plan to dispatch on it: `Engage` and `Ignore` both fall through to `ResolveCurrentStep` (mechanically identical), and `Retreat` short-circuits to `EndAdventure` skipping the outcome roll + `ShowingOutcome` phase. The "retreat" UX intent is already encodable in outcome data — every authored Retreat option in the codebase is `Outcomes = [new EndAdventureOutcome("Flapped home!")]`.

**New behavior**: `OptionKind` enum is deleted. `EncounterOption` no longer carries a `Kind` property. **All options behave uniformly**: clicked or timer-fired → roll one outcome from `Outcomes[]` → show outcome text for `OutcomeDisplaySeconds` → apply effect via the existing exhaustive switch on the `Outcome` hierarchy. `EndAdventureOutcome` is the only signal that the adventure ends; existing "Retreat" options simply own a single `EndAdventureOutcome` (already true) and the bird gets the same 2.5s "Flapped home!" splash before returning to `Playing` instead of an instant cut. `Engage` / `Ignore` distinction disappears entirely — they were authorial-intent tags with no behavioral consequence.

The deferred-T5/T6 dispatch logic shrinks to "fire option → roll outcome → apply outcome", with no special-cased option kind. Pit-of-success: one path through option resolution, one source of truth for "this ends the adventure" (outcome shape).

## Prerequisites
None. `OptionKind` has no live consumer in the implemented codebase (`Adventuring.cs` today builds a single placeholder `"Continue"` link). The two pending tickets that *would* consume it (`encounter-option-ui-and-timer.md`, `outcome-resolution.md`) are unimplemented; this ticket modifies their text so they're written without the enum from the start.

This ticket should land **before** `encounter-option-ui-and-timer.md` is implemented — otherwise that ticket builds the `Engage` / `Ignore` / `Retreat` switch and immediately tears it out.

## Scope
### In scope
- `PetDoodle/Encounters/OptionKind.cs`: **delete** the file.
- `PetDoodle/Encounters/EncounterOption.cs`: remove the `Kind` property. Leave `Label`, `Outcomes`, and the non-empty-`Outcomes` init guard.
- `PetDoodle/Encounters/EncounterExtensions.cs`: strip every `Kind = OptionKind.X,` line from every `EncounterOption` initialiser. Roughly 50+ lines deleted; no other change to Info entries.
- `docs/adventures.md`: rewrite the §Encounters bullet about `Kind` and the §Encounters bullet about `Retreat` to describe the uniform-resolution model. Update §Outcomes' `EndAdventureOutcome` description to note it's the **sole** end-adventure mechanism (no longer "same final effect as a Retreat option").
- `docs/adventures-roadmap.md`: drop the `OptionKind` reference (currently in the T3-ships-types bullet) and the "Retreat bypasses outcomes" bullet (which becomes false).
- `docs/biomes/*.md` (grasslands, river, jungle, cave, mountain, beach): drop the `Kind` column from every per-encounter option table. Drop or rewrite design-rationale paragraphs that lean on `OptionKind.Ignore` / `OptionKind.Engage` semantics — most read fine with the `Kind` column gone, since the substantive design intent (sneak past, commit to fight, flee home) lives in the option *label*, not the kind tag.
- Pending tickets that mention `OptionKind`: rewrite each to assume the no-kind world. Specifically:
  - `docs/tickets/encounter-option-ui-and-timer.md`: option dispatch becomes "every option's `Action` rolls + shows + applies an outcome" (behavior previously deferred to T6 collapses into T5+T6 as one uniform path); the `OptionKind`-dispatch table in Implementation step 4 disappears; Constraints' "Retreat semantics" bullet becomes "EndAdventureOutcome semantics" (resolver applies, not option kind); Acceptance Criteria item dispatching by `Kind` becomes the uniform-roll item.
  - `docs/tickets/outcome-resolution.md`: drop the "Retreat continues to short-circuit" bullets in Context, Scope, AC. The `Retreat` short-circuit ceases to exist; `EndAdventureOutcome` reaches `EndAdventure()` via the standard `ApplyOutcome` arm. Implementation step 3 collapses to "every option fires `FireOption(option)`" — no per-kind branch.
  - `docs/tickets/biome-umbra.md`: AC item "every option has … a designed `OptionKind`" becomes "every option has a designed outcome list". Implementation step 5b's `Kind = OptionKind.Engage,` line drops. Out-of-scope bullet "New `OptionKind` values" drops (no enum to extend).
  - `docs/tickets/rapids-swim-to-shore.md`: Open Decision 2 ("OptionKind for Swim to shore") drops entirely; Implementation step 3's `Kind = OptionKind.Ignore,` line drops; Acceptance Criterion mentioning kind drops.
  - `docs/tickets/biome-lagoon.md`: design-session step 3's `OptionKind` bullet drops; Test Plan's "if any encounter is Retreat-less" item rewrites to "if any encounter has no end-adventure escape option…" or simply drops (Retreat is no longer a kind).
  - `docs/tickets/biome-mountain-peak.md`: same as biome-lagoon — drop `OptionKind` references in the design-session shape.
  - `docs/tickets/biome-waterfall.md`: same — drop the `OptionKind` design-session bullet.

### Out of scope
- **Renaming "Retreat"-labelled options.** Options labelled `"Retreat"` keep the label — it's flavor text the player reads. Only the *kind tag* is dropped. Some authors may want to rename `"Flee"` / `"Retreat"` for consistency in a future pass; not this ticket.
- **Changing existing outcomes.** No outcome data is added, removed, or reshaped. The 2.5s `"Flapped home!"` splash that "Retreat" options now get is purely a consequence of the resolver no longer special-casing — the data is unchanged.
- **Introducing a different escape-hatch mechanism.** No "instant exit" replacement. The 2.5s outcome-reveal delay is the deliberate uniform behavior; if a future design needs an instant escape, it lands as its own outcome record (e.g. `InstantEndAdventureOutcome`) with its own `ApplyOutcome` arm. YAGNI for now.
- **Updating completed-ticket archives** (`docs/tickets/complete/2026-05-15 *.md`). Those are historical snapshots; rewriting them rewrites history. The corresponding **living** design docs in `docs/biomes/*.md` get updated; the archived tickets stay as-is.
- **Rewriting `EncounterOption` to be a positional record.** Today it's a `record` with `required` init properties + a backing-field guard for `Outcomes`. Stripping `Kind` doesn't force a shape change; keep the existing pattern.
- **Bird-stat / inventory effects on outcomes.** Out of scope for the same reasons listed in `docs/adventures.md` §Outcomes.

## Relevant Docs & Anchors
- **Design docs**:
  - `docs/adventures.md` §Encounters (bullets re Kind + Retreat) and §Outcomes (`EndAdventureOutcome` description, `Retreat` cross-reference).
  - `docs/adventures-roadmap.md` (T3-ships-types and Retreat-bypasses-outcomes bullets).
  - `docs/biomes/*.md` (per-encounter tables and any rationale paragraphs leaning on kind).
- **Pending tickets to rewrite**: `outcome-resolution.md`, `encounter-option-ui-and-timer.md`, `biome-umbra.md`, `rapids-swim-to-shore.md`, `biome-lagoon.md`, `biome-mountain-peak.md`, `biome-waterfall.md`.
- **Code anchors**:
  - `OptionKind` (`PetDoodle/Encounters/OptionKind.cs`) — delete.
  - `EncounterOption` (`PetDoodle/Encounters/EncounterOption.cs`) — drop `Kind` property.
  - `EncounterExtensions.Info` static-ctor (`PetDoodle/Encounters/EncounterExtensions.cs`) — strip `Kind = …,` from every initialiser.
  - `Adventuring` (`PetDoodle/GameStates/Adventuring.cs`) — **no change** today (single placeholder Continue link, no kind dispatch yet).

## Constraints & Gotchas
- **`EncounterExtensions` static-ctor sanity check is unaffected.** It iterates `Info.Values.SelectMany(e => e.Options)` and asserts each `option.Outcomes` is non-null and non-empty. No `Kind` reference; nothing to update there.
- **`EncounterOption` non-empty-`Outcomes` init guard stays.** That's the surviving pit-of-success contract. The `Kind` property's `required` init is the only thing that's gone.
- **Records are immutable; the dictionary literal in `EncounterExtensions.Info` is rebuilt on every static-ctor run.** Stripping `Kind = …,` from the initialisers is a textual edit; no migration / data-shape concern.
- **No save-format impact.** `EncounterOption` is static authored data, not serialised (per `docs/adventures.md` §Persistence). Save format unchanged.
- **`PetDoodle.Data` is zero-deps.** `OptionKind` lived in `PetDoodle/Encounters/`, not the data project. Deletion has no cross-project ripple.
- **`WarningsAsErrors=Nullable`.** Removing a `required` property + dropping a few using-site assignments doesn't introduce nullability concerns. The remaining `EncounterOption` properties (`Label`, `Outcomes`) keep their `required` annotations.
- **No usages outside `EncounterExtensions.Info` initialisers.** A grep pass before deletion will confirm — `Adventuring.cs` and `ButtonList.cs` don't reference `OptionKind` today.
- **Pending-ticket coordination.** This ticket rewrites the Implementation / AC of `outcome-resolution.md` and `encounter-option-ui-and-timer.md` rather than adding prerequisites — the modifications are textual, not behavioral. After this ticket lands, those pending tickets implement the simpler uniform-resolution model from the start.

## Open Decisions
1. **`"Retreat"` label uniformity.** Today some options are labelled `"Retreat"` (Limestone Golem, Steep Climb, Griffin, Aggressive Seagull, Giant Bat) and some `"Flee"` (Giant Toad). Both are end-adventure escape options. Default: **leave labels as-is** in this ticket; flavor pass later. Implementer should not bundle a label-rename refactor.
2. **`Adventuring.FireOption` helper signature.** When `outcome-resolution.md` is rewritten by this ticket, the per-option dispatch becomes `() => FireOption(option)`. Whether `FireOption` takes the full `EncounterOption` or just the `Outcome[]` is local taste. Default: **take the `EncounterOption`** so future signals (e.g. label-aware logging, kind-replacement metadata) have a hook. Implementer of `outcome-resolution.md` calls finally; this ticket's rewrite of that ticket should leave the helper signature unspecified beyond "takes the option".
3. **Whether to mention `OptionKind` removal in `docs/biomes/*.md` design-rationale paragraphs.** Some paragraphs explicitly call out kind choices (e.g. mountain.md's "Griffin uses `OptionKind.Ignore` on the sneak option, not `Engage`"). Default: **rewrite the rationale to drop the kind reference** — the substantive intent ("the bird is *not* engaging the griffin — it's sneaking past") survives without naming the enum value. The paragraph reads as "Sneak past is the bird *not* engaging the griffin, even though the option carries end-adventure risk."

## Acceptance Criteria
- [ ] `PetDoodle/Encounters/OptionKind.cs` no longer exists.
- [ ] `EncounterOption` (in `PetDoodle/Encounters/EncounterOption.cs`) has exactly two properties: `string Label` and `Outcome[] Outcomes` (both `required`). The `Outcomes` init-accessor non-empty guard is unchanged. The `Kind` property is gone.
- [ ] Every `EncounterOption` initialiser in `EncounterExtensions.Info` compiles without a `Kind = …,` line. No `OptionKind` symbol referenced anywhere in `PetDoodle/`.
- [ ] `EncounterExtensions` static-ctor sanity check (every option's `Outcomes` non-empty) still passes.
- [ ] `dotnet build -o /tmp/build-check` and `dotnet test` pass with no warnings.
- [ ] `docs/adventures.md` §Encounters and §Outcomes describe the no-kind, uniform-resolution model. The `Retreat` bullet is rewritten or removed; "Retreat" is no longer a kind, only a (conventional) label paired with a single `EndAdventureOutcome`.
- [ ] `docs/adventures-roadmap.md` no longer references `OptionKind`. The "Retreat bypasses outcomes" bullet is rewritten or deleted.
- [ ] Every `docs/biomes/*.md` (grasslands, river, jungle, cave, mountain, beach) per-encounter table has its `Kind` column removed. Design-rationale paragraphs that named `OptionKind.X` either drop the reference (per Open Decision 3) or are rewritten to phrase the intent without the enum.
- [ ] Each pending ticket listed in Scope (`outcome-resolution.md`, `encounter-option-ui-and-timer.md`, `biome-umbra.md`, `rapids-swim-to-shore.md`, `biome-lagoon.md`, `biome-mountain-peak.md`, `biome-waterfall.md`) reads coherently after the rewrite — no orphaned `OptionKind` mention, no contradictions between Context / Scope / AC / Implementation about whether kind exists.
- [ ] Completed-ticket archives in `docs/tickets/complete/` are unchanged (out-of-scope per Scope).
- [ ] `Adventuring.cs` is unchanged by this ticket (today's placeholder Continue link doesn't reference `OptionKind`; the future T5/T6 implementations will reference the rewritten tickets).

## Implementation

### 1. Grep + audit
Before deleting anything, `grep -rn OptionKind` to confirm the file list (currently 20 files, mix of code + docs). Use the result as the punch list for steps 2–6.

### 2. Strip `Kind` from every `EncounterOption` initialiser
In `PetDoodle/Encounters/EncounterExtensions.cs`, delete every `Kind = OptionKind.X,` line. Each `EncounterOption` initialiser now has just `Label` and `Outcomes`. Roughly 50+ lines deleted; no other change to entries.

### 3. Remove `Kind` from `EncounterOption`
In `PetDoodle/Encounters/EncounterOption.cs`, delete the `public required OptionKind Kind { get; init; }` line. Keep `Label`, `Outcomes`, and the backing-field non-empty guard.

### 4. Delete `OptionKind.cs`
Remove `PetDoodle/Encounters/OptionKind.cs`. Build should still pass after step 2 stripped every reference. If build fails with an "unknown symbol `OptionKind`" error, return to step 2 and find the missed call site.

### 5. Update `docs/adventures.md`
- §Encounters: remove the "Each option has a kind: Engage / Ignore / Retreat" bullet. Rewrite the `Retreat` bullet to describe **the convention**, not the enum: "An option whose only outcome is an `EndAdventureOutcome` cancels the adventure when fired (after the standard outcome-reveal delay). Such options are commonly labelled `"Retreat"` or `"Flee"`. Retreat is *common*, not guaranteed."
- §Outcomes `EndAdventureOutcome`: drop the "Same final effect as a Retreat option, but driven by an option's outcome rather than the option's kind" sentence. Now `EndAdventureOutcome` is **the** end-adventure mechanism; nothing else competes.

### 6. Update `docs/adventures-roadmap.md`
- T3 ships-types bullet: remove `OptionKind` from the type list.
- "Retreat bypasses outcomes" bullet: delete entirely. The new model has no bypass; every option goes through the same `ApplyOutcome` path.

### 7. Update `docs/biomes/*.md`
For each of grasslands, river, jungle, cave, mountain, beach:
- In every per-encounter option table, drop the `Kind` column.
- Audit the design-rationale notes section for `OptionKind.X` references (e.g. mountain.md's Griffin paragraph) and rewrite each to describe intent without the enum (per Open Decision 3).

### 8. Rewrite pending tickets
For each pending ticket in Scope, edit in place:
- **`encounter-option-ui-and-timer.md`**: Context "(this ticket: same behavior as today — `Engage`/`Ignore` advance, `Retreat` ends adventure)" rewrites to "(this ticket: every option's `Action` rolls one outcome from `option.Outcomes`, transitions to `ShowingOutcome`, and after a fixed delay applies the outcome — see `outcome-resolution.md`)". Scope's "Option dispatch in this ticket" bullet rewrites the same way. Out-of-scope's "Outcomes: every option's Action for now is 'resolve step' or 'end adventure' by Kind" bullet is **deleted** — outcomes are now in scope here too, since there's no kind-based shortcut. Constraints' "Retreat semantics" bullet rewrites to "EndAdventureOutcome semantics: an option whose rolled outcome is an `EndAdventureOutcome` ends the adventure when applied (after the outcome-reveal delay)". Constraints' Nullable bullet drops "via `Kind`". AC item dispatching by `Kind` rewrites to "every option's `Action` calls `FireOption(option)` — see `outcome-resolution.md` for the resolver." Implementation step 4's three-line `OptionKind.X →` table collapses to "Each button's `Action` is `() => FireOption(option);`". Note: this ticket's prereq order vs `outcome-resolution.md` is unchanged — they still land in sequence, but the seam between them shifts (T5 hands a fired option to T6's resolver instead of dispatching by kind).
- **`outcome-resolution.md`**: Context "Adventuring builds an option ButtonList per step, runs a 5s timer, and on click / timer expiry fires the active option's Action — which currently dispatches by `OptionKind`…" rewrites to "Adventuring builds an option ButtonList per step, runs a 5s timer, and on click / timer expiry fires the active option's Action — which is `() => FireOption(option)` (see `encounter-option-ui-and-timer.md`). Today the placeholder `FireOption` resolves the step; this ticket replaces it with the outcome-roll + show + apply pipeline." Drop the trailing "OptionKind.Retreat continues to short-circuit…" sentence. Scope's "Adventuring: retreat path unchanged" bullet drops. AC item "OptionKind.Retreat continues to call EndAdventure directly" drops. Implementation step 3 ("Refactor option dispatch") collapses: no more `Retreat` arm; every option goes through `FireOption(option)` which rolls + shows + applies. Test Plan's "Click Retreat (where available). No outcome text — direct return to Playing. Confirms retreat bypasses ShowingOutcome." rewrites to: "Click an option whose only outcome is an `EndAdventureOutcome` (e.g. one labelled `"Retreat"`). Outcome text shows for ~2.5s ('Flapped home!' or similar), then return to Playing — no special short-circuit; the EndAdventure outcome reaches `EndAdventure()` via the standard `ApplyOutcome` arm."
- **`biome-umbra.md`**: Out-of-scope bullet "New `OptionKind` values" drops. Implementation step 5b's `Kind = OptionKind.Engage,` line drops from the `new EncounterOption` initialiser. AC item "every option has a non-empty `Outcomes` array and a designed `OptionKind`" rewrites to "every option has a non-empty `Outcomes` array".
- **`rapids-swim-to-shore.md`**: Open Decision 2 (`OptionKind for "Swim to shore"`) deletes entirely; renumber the remaining decisions. Implementation step 3's `Kind = OptionKind.Ignore,` line drops. AC item "kind per Open Decision 2; default `Ignore`" rewrites to "with a single non-empty Outcomes entry".
- **`biome-lagoon.md`**: design-session step 3's `OptionKind` bullet drops. Test Plan's "if any lagoon encounter is `Retreat`-less, confirm the option row contains no `Retreat` button" rewrites to "if any lagoon encounter has no end-adventure escape option, confirm the option row contains no escape-labelled button" (or simply delete the item — the design intent is now "the encounter has no option whose only outcome is `EndAdventureOutcome`", which is observable from the data without a separate UI check).
- **`biome-mountain-peak.md`**: same as biome-lagoon — drop the `OptionKind` bullet from the design-session shape.
- **`biome-waterfall.md`**: same — drop the `OptionKind` bullet from the design-session shape.

### 9. Build + run sanity
`dotnet build -o /tmp/build-check`. `dotnet test`. Launch the game and walk the bird around — no behavior change expected (Adventuring still uses the placeholder Continue link). The change is purely structural; nothing in the live UI flow is affected yet.

## Test Plan
- [ ] `dotnet build -o /tmp/build-check` passes with **no warnings**.
- [ ] `dotnet test` passes.
- [ ] `grep -rn OptionKind` returns zero hits in `PetDoodle/`, `PetDoodle.Data/`, `docs/adventures.md`, `docs/adventures-roadmap.md`, `docs/biomes/`, and the pending-ticket files listed in Scope. Hits in `docs/tickets/complete/` are expected (archives).
- [ ] Launch the game. Bird walks as before. Idle to trigger an adventure — `Adventuring` state opens, biome + bird + encounter name + Continue link render. Click Continue — adventure advances or ends as before. (No behavior change today; this is a structural refactor.)
- [ ] Open `EncounterExtensions.cs` and visually confirm a sampled handful of `EncounterOption` initialisers (e.g. one Engage-flavored, one Ignore-flavored, one Retreat-flavored) all read as just `Label = "…"` + `Outcomes = […]`.
- [ ] Read `docs/adventures.md` end-to-end. Confirm no contradiction between §Encounters (no kind) and §Outcomes (`EndAdventureOutcome` is sole end-adventure mechanism).
- [ ] Read each updated `docs/biomes/*.md`. Confirm tables drop the Kind column cleanly and design-rationale paragraphs read coherently without enum references.
- [ ] Read each rewritten pending ticket. Confirm Context / Scope / AC / Implementation are internally consistent — no surviving "Retreat short-circuits" claim, no `Kind = OptionKind.X` in code samples.

## Learnings

### Architectural decisions
- **Open Decision 1 (Retreat label uniformity)** — default upheld. Existing "Retreat" / "Flee" labels preserved as flavor. No label-rename refactor bundled. Any future consistency pass is its own ticket.
- **Open Decision 2 (`FireOption` signature)** — deferred to `outcome-resolution.md` implementer per the ticket's own guidance. The rewrite of that ticket leaves the helper signature open beyond "takes the option" — implementer of T6 picks the final shape.
- **Open Decision 3 (biome rationale rewrites)** — default upheld. Paragraphs that named `OptionKind.X` rewritten to phrase intent without the enum (e.g. mountain.md's Griffin paragraph now reads "Sneak is the bird *not* engaging the griffin" instead of "`OptionKind.Ignore` on the sneak option"). Substantive design intent survives; the enum tag was scaffolding.

### Problems encountered
- **`docs/biomes/waterfall.md` carried a self-referential pointer** to this ticket — the design-rationale paragraph called out the planned removal explicitly. Test Plan's `grep -rn OptionKind` requirement against `docs/biomes/` forced a rewrite of that paragraph to drop the term entirely. Surviving content: "Options carry no kind tag; every option uniformly resolves fire → roll outcome → apply."
- **`dotnet build -o /tmp/build-check` emits NETSDK1194** ("--output isn't supported when building a solution"). Flag warning, not source. Verified `dotnet build` (no `-o`) yields 0 warnings/errors. The `-o` flag in the ticket's Implementation step 9 is the source of the only warning — harmless, expected.

### Interesting tidbits
- **All ~25 `EncounterOption` initialisers had byte-identical `Kind = OptionKind.X,\n` formatting** (same 20-space indent), so three `Edit replace_all` calls (`.Engage,` / `.Ignore,` / `.Retreat,`) covered every site without a single per-site edit. Pit-of-success for mechanical refactors.
- **`docs/biomes/waterfall.md` was the *original* motivator** for the ticket. Its design-session paragraph documented the authoring confusion that arose when Engage/Ignore had no mechanical meaning on a "both options actively descend" encounter. Rewriting it closed the loop.

### Related areas affected
- **Pending tickets rewritten** (`encounter-option-ui-and-timer.md`, `outcome-resolution.md`, `biome-umbra.md`, `rapids-swim-to-shore.md`, `biome-lagoon.md`, `biome-mountain-peak.md`) — all now describe the uniform-resolution model. When those tickets are implemented they'll build the simpler path from the start instead of building a kind-dispatch and tearing it out.
- **Encounter-option-ui-and-timer ticket gains a small re-shape**: the seam to T6 is now "every button's `Action` is `() => FireOption(option)`" instead of a kind-dispatch table. T5's placeholder `FireOption` calls `ResolveCurrentStep()`; T6 replaces the body with the roll-show-apply pipeline. The seam is uniform across both tickets.
- **`docs/biomes/waterfall.md`** lost its self-reference to this ticket as part of the rewrite. No outstanding cross-doc pointers left.

### Rejected alternatives
- **Rename "Retreat" / "Flee" labels for consistency** — out of scope (Open Decision 1). Labels are player-facing flavor; mixing them is a soft inconsistency that doesn't need fixing here.
- **Convert `EncounterOption` to a positional record** — out of scope. The `required` init-property pattern with a non-empty-`Outcomes` guard is the surviving pit-of-success contract; collapsing to positional buys nothing.
- **Update `docs/tickets/complete/` archives** — out of scope. Archives are historical snapshots. Rewriting them rewrites history; only living docs (`docs/biomes/*.md`, pending tickets, design docs) reflect the new state.
- **Introduce an `InstantEndAdventureOutcome` to replace the old `OptionKind.Retreat` short-circuit** — YAGNI. The 2.5s outcome-reveal delay on `EndAdventureOutcome` is the deliberate uniform behavior. If a future design needs an instant exit, it lands as its own outcome record then.
