# Mountain Peak: MysteriousCave encounter (post-BiomeShiftOutcome)

## Context

The [Mountain Peak biome ticket](./complete/2026-05-15%20biome-mountain-peak.md)
authored a single pool encounter (`Thunderstorm`) and **deferred** a second
encounter — `MysteriousCave` — because its intended outcomes need
`BiomeShiftOutcome`, an `Outcome` derived type that does not yet exist. The
parent ticket's design doc at [`docs/biomes/mountain-peak.md`](../biomes/mountain-peak.md)
records the deferral in its "Deferred encounters" section and points here.

`BiomeShiftOutcome` is owned by [`biome-umbra.md`](./biome-umbra.md) (which is
itself gated on [T6 outcome resolution](./outcome-resolution.md) — the
exhaustive `Adventuring.ApplyOutcome` switch where the new outcome arm lives).
This ticket consumes the mechanic once it lands and wires `MysteriousCave`
into the mountain peak pool.

**Design intent (authored by user in the parent ticket's design session):**

- `Encounter.MysteriousCave`, display name `"Mysterious Cave"`.
- A cave mouth appears at the summit. The bird either enters or walks past.
- **One option** for now: `"Enter"`, with two outcomes (50/50) — both
  `BiomeShiftOutcome`s — one targeting `Biome.Cave`, one targeting
  `Biome.Umbra`. Walking into the mouth shifts the rest of the adventure
  into either a normal cave detour or the trippy Umbra side-quest.

> **Deliberate AC deviation from the parent ticket.** The parent biome
> ticket's Acceptance Criteria asked for "at least 2 options" per encounter.
> The user explicitly directed: *"yes, only one 'choice' for mysterious cave
> for now — we'll add more later."* `MysteriousCave` ships **1-option** in
> this iteration on user instruction; expanding to ≥2 options is captured in
> Open Decision 3 below and may itself become a follow-up ticket.

## Prerequisites

- [Biome: Umbra (+ BiomeShiftOutcome)](./biome-umbra.md) — must ship the
  `BiomeShiftOutcome` sealed record + its `Adventuring.ApplyOutcome` arm.
  Without this ticket landing first, the outcomes authored here are
  inexpressible. **Transitively depends on
  [Outcome Resolution (T6)](./outcome-resolution.md)** because
  `BiomeShiftOutcome`'s resolver arm lives in T6's exhaustive switch — if T6
  is not yet landed, `biome-umbra` is itself blocked, which blocks this
  ticket.
- [Biome: Cave](./complete/) — `Biome.Cave.GetInfo().PossibleEncounters` must
  be non-empty at the moment this ticket's `BiomeShiftOutcome` rolls,
  otherwise the apply-time uniform pick from the cave pool has nothing to
  draw from. (Cave is already authored — `GlowingMushrooms`, `GiantBat`,
  `LargeBoulder` — so this is a "don't regress" constraint, not a fresh
  prereq.)
- [Biome: Umbra encounter pool](./biome-umbra.md) — same reason, for the
  umbra step. `biome-umbra` already promises ≥1 authored Umbra encounter as
  one of its own ACs; this ticket inherits that guarantee.
- Parent: [Biome: Mountain Peak (archived)](./complete/2026-05-15%20biome-mountain-peak.md)
  — authored `Encounter.Thunderstorm` and set
  `Biome.MountainPeak.PossibleEncounters = [Encounter.Thunderstorm]`. This
  ticket **appends** to that pool; the existing entry stays.

## Design session

The design is largely captured in **Context** above (encounter name, display
name, option label, outcome kinds + target biomes). Two items remain
undecided and are surfaced in **Open Decisions** below rather than
front-loaded in a design session:

1. Flavor text strings for each of the two `BiomeShiftOutcome`s (Open
   Decision 1).
2. `StepCount` value for each shift (Open Decision 2).

A brief sync with the user to lock those two before authoring is
sufficient — no full design session needed. If the user opts to expand
`MysteriousCave` to ≥2 options before shipping (Open Decision 3), that
**does** warrant a short design session for the additional option(s).

## Authoring checklist

- [ ] `Encounter.MysteriousCave` appended to `DoodleBird.Data/Encounter.cs`.
- [ ] `EncounterInfo` entry for `Encounter.MysteriousCave` added to
      `DoodleBird/Encounters/EncounterExtensions.cs` with a single `"Enter"`
      option whose `Outcomes` array contains exactly two `BiomeShiftOutcome`s
      (one targeting `Biome.Cave`, one targeting `Biome.Umbra`).
- [ ] `BiomeExtensions.Info[Biome.MountainPeak].PossibleEncounters` rebuilt
      from `[Encounter.Thunderstorm]` to
      `[Encounter.Thunderstorm, Encounter.MysteriousCave]`.
- [ ] [`docs/biomes/mountain-peak.md`](../biomes/mountain-peak.md) updated:
      move `Mysterious Cave` out of "Deferred encounters" and into a real
      "Pool encounters" entry (alongside `Thunderstorm`); drop the
      "Deferred encounters" section if it becomes empty.

## Open Decisions

1. **Flavor text for the two `BiomeShiftOutcome`s.** 6×8 font @ 128 px
   viewport ≈ 21 chars per line (same budget as
   [`biome-umbra.md`](./biome-umbra.md) and the parent mountain-peak
   ticket). Suggested defaults:
   - Cave shift: `"Into the cave..."` (16) — straight description.
   - Umbra shift: `"Reality bends..."` (16) — borrows from `biome-umbra.md`'s
     own shortlist for the Mushrooms shift; reads as the "trippy" branch.
   - Alternatives if the user wants more distinct flavor between the two:
     `"Deep cool dark."` (15) for cave / `"Trippy! Wobble..."` (17) for
     umbra; or `"A cave!"` (7) / `"Not a cave!"` (11). Final pick during
     authoring sync.
2. **`StepCount` for each shift.** `biome-umbra.md`'s default for the
   Mushrooms → Umbra shift is **2 or 3**. Mountain peak's MysteriousCave
   sits at the end of template 2 (grasslands → mountain → mountain peak),
   so the bird has already walked 2 steps; a 2-step sub-adventure keeps the
   total at 4–5 steps, a 3-step at 5–6. **Default: 2 for both branches**,
   matching the shorter end of `biome-umbra`'s range. If the user wants the
   two branches to feel different in pacing, asymmetric values
   (e.g. Cave 2, Umbra 3) are fine and authored at construction time.
3. **Expand to ≥2 options before shipping?** User explicitly said
   "we'll add more later." Three paths:
   - **(a) Ship 1-option as authored.** Default. Documents the AC deviation
     in `docs/biomes/mountain-peak.md`; future expansion is its own
     ticket.
   - **(b) Add a `"Move on"` Ignore-kind second option in this ticket.**
     Cheap (single `FlavorOutcome("Hopped past.")` matches grasslands'
     idiom). Restores conformance with the parent ticket's "≥2 options" AC.
     Slight scope creep relative to the user's stated intent.
   - **(c) Spawn yet another follow-up ticket** for the second option.
     Mirrors how this very ticket was spawned. Cleanest split, but adds
     ticket overhead for a one-line option.
   - **Default: (a).** Honor the user's "add more later" — this ticket's
     job is the `BiomeShiftOutcome` consumption, not encounter shape
     conformance. The 1-option deviation is a known, recorded deviation,
     not a bug.

## Acceptance Criteria

- [ ] `DoodleBird.Data/Encounter.cs` contains `MysteriousCave` as a new enum
      value.
- [ ] `EncounterExtensions.Info[Encounter.MysteriousCave]` is populated:
      `DisplayName == "Mysterious Cave"`, exactly **one** `EncounterOption`
      with `Label == "Enter"` and a non-empty `Outcomes` array containing
      **exactly two** `BiomeShiftOutcome`s — one with
      `TargetBiome == Biome.Cave`, one with `TargetBiome == Biome.Umbra`.
- [ ] Each `BiomeShiftOutcome`'s `Text` fits the 128-px viewport at 6×8 font
      (≤ ~21 chars).
- [ ] Each `BiomeShiftOutcome`'s `StepCount` is ≥ 1 (the `biome-umbra` ctor
      guard already enforces this; AC restates for clarity).
- [ ] `BiomeExtensions.Info[Biome.MountainPeak].PossibleEncounters` contains
      both `Encounter.Thunderstorm` (existing) and `Encounter.MysteriousCave`
      (new).
- [ ] `EncounterExtensions` static-ctor non-empty-`Outcomes` sanity check
      still passes.
- [ ] `BiomeExtensions` static-ctor `Info.Count == Enum.GetValues<Biome>().Length`
      sanity check still passes (no new biome added; this AC just guards
      against accidental regression).
- [ ] `docs/biomes/mountain-peak.md` updated: `Mysterious Cave` documented
      under a "Pool encounters" subsection with its options + outcomes; the
      previous "Deferred encounters → Mysterious Cave" section is removed
      (or, if other deferrals get added by other tickets in the meantime,
      just the `Mysterious Cave` entry is removed from that section).
- [ ] The 1-option AC deviation is explicitly noted in
      `docs/biomes/mountain-peak.md`'s design rationale — future readers
      shouldn't see the 1-option encounter as a missed authoring guideline.
- [ ] No new dependencies added to `DoodleBird.Data` (zero-deps rule per
      `DoodleBird.Data/CLAUDE.md`).

## Test Plan

- [ ] `dotnet build` passes with no warnings.
- [ ] Static-ctor sanity checks for both `EncounterExtensions` and
      `BiomeExtensions` fire on first access (game launch) without
      throwing.
- [ ] Launch the game. Trigger an adventure containing a mountain peak step
      (template 2: grasslands → mountain → mountain peak). Roll the peak
      step repeatedly across runs (or temporarily force the peak's
      `PossibleEncounters` to `[Encounter.MysteriousCave]` for the
      manual test — revert after) until `MysteriousCave` rolls. Confirm
      the encounter renders with the `"Enter"` button and that the option
      timer / default-pick behave correctly with a single option.
- [ ] Click `"Enter"`. Across multiple runs (sample ~10–20), confirm both
      outcomes appear: one transitions to a `Biome.Cave` sub-adventure, the
      other to a `Biome.Umbra` sub-adventure. Confirm a roughly 50/50 split
      over enough rolls (not a strict statistical test — just "both branches
      are reachable").
- [ ] On the cave-shift branch: confirm `save.json` mid-shift shows
      `CurrentAdventure.RemainingSteps` is a fresh list of `StepCount`
      `(Cave, <encounter>)` pairs; the original
      `Thunderstorm`-or-whatever-was-next tail is gone. Resolve the cave
      sub-adventure to completion; confirm return to `Playing`.
- [ ] On the umbra-shift branch: same check against `Biome.Umbra`. Resolve
      to completion; confirm return to `Playing`.
- [ ] Quit mid-sub-adventure (on each branch). Relaunch. Confirm game boots
      back into `Adventuring` with the saved sub-adventure intact — no
      replay of the MysteriousCave roll, no return to the pre-shift state.
- [ ] Each outcome text fits the 128 px viewport at 6×8 font (eyeball on
      the actual `ShowingOutcome` draw).
- [ ] Confirm `Adventuring.ApplyOutcome`'s `default: throw UnreachableException()`
      arm still fires when a fake unhandled subclass is dropped in
      (defensive — same spot-check as `biome-umbra.md` and
      `outcome-resolution.md` test plans; ensures appending another
      `BiomeShiftOutcome` call site didn't accidentally introduce
      fall-through).
