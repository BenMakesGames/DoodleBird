# Decision Timer As Bottom Bar

## Context
**Current behavior**: While `Adventuring` is in the option-choosing phase, remaining time is drawn as `$"{remaining:0.0}"` text in the top-right corner of the screen.
**New behavior**: The numeric timer readout is replaced by a 3px-tall red bar pinned to the bottom edge of the screen. Its width equals `Graphics.Width * (RemainingSeconds / OptionTimerSeconds)`, shrinking from full width to zero as the timer counts down. When the bar reaches zero width the default option fires (unchanged behavior).

## Scope
### In scope
- `Adventuring.Draw`: remove the timer-text block; draw a filled rectangle along the bottom edge sized by the remaining-fraction.
- `Adventuring`: remove the now-unused `TimerTopMargin` / `TimerRightMargin` constants. Add a `TimerBarHeight = 3` constant.
- Bar color: `DawnBringers16.Red`.

### Out of scope
- Per-encounter timer durations (still fixed 5s).
- Easing / animation on bar width (linear shrink, one update per `FixedUpdate` decrement — same cadence as today's text).
- Showing the bar during `Dialog` outcome reveal — the bar lives in `Adventuring` only, like today's text.

## Relevant Docs & Anchors
- Analogue ticket: `docs/tickets/complete/2026-05-15 encounter-option-ui-and-timer.md` — introduced `RemainingSeconds`, the timer-text render, and the option-row layout this ticket coexists with.
- `docs/colors.md` — `DawnBringers16.Red` (#d04648).
- Code anchors:
  - `Adventuring.Draw` — the timer-text block to remove and the new bar to add.
  - `Adventuring.FixedUpdate` — already decrements `RemainingSeconds`; no change needed.
  - `OptionTimerSeconds` constant on `Adventuring` — denominator for the width fraction.
  - `GraphicsManager.DrawFilledRectangle` — used elsewhere in `Adventuring.Draw` for the ground strip; same call shape.

## Constraints & Gotchas
- **Viewport is 128×32**. Bottom 3px (y=29..31) overlaps:
  - The ground strip (`y = Graphics.Height - 8` through `Graphics.Height`, i.e. y=24..31 — drawn with biome `GroundColor`).
  - The option row (`OptionRowY = 22`, height 10 → y=22..31), including the active-option underline drawn at `textY + font.MaxCharacterHeight + 1`.
  Draw the bar *after* the option buttons so it sits on top — the bar is the load-bearing UI element here. Some option-row visual clipping at the bottom edge is accepted; this is the same class of cosmetic compromise already documented under "Workarounds / limitations" in the analogue ticket.
- **Pixel-integer width**. `Graphics.Width * (RemainingSeconds / OptionTimerSeconds)` is a float; cast to `int` for the rectangle's width parameter. Clamp `RemainingSeconds` ≥ 0 before computing — `FixedUpdate` decrements without clamping, so a negative value can briefly exist before the option fires.
- **Bar fully disappears at timer expiry** — width 0 is fine; `DrawFilledRectangle` with `width = 0` is a no-op visually.

## Open Decisions
1. **Rounding mode for width** — floor (`(int)…`) vs. round (`(int)MathF.Round(…)`). Default: floor. Either reads fine at 60fps; floor is the cheaper, more conventional choice.
2. **Bar height** — `3` is the user's "maybe 3px". Default: 3, adjustable by eyeball during manual test.

## Acceptance Criteria
- [ ] `Adventuring.Draw` no longer renders `RemainingSeconds` as text anywhere on screen.
- [ ] `Adventuring.Draw` renders a filled red rectangle along the bottom edge of the screen whose width is proportional to `RemainingSeconds / OptionTimerSeconds`, height `TimerBarHeight` (3 by default).
- [ ] At step entry the bar spans full screen width (`Graphics.Width`); just before auto-fire the bar width is 0 (or 1).
- [ ] Bar is drawn after option buttons and before the mouse cursor, so the cursor remains on top and option-row underlines fall behind the bar where they overlap.
- [ ] `TimerTopMargin` and `TimerRightMargin` constants are removed (unused after this change).

## Implementation

### 1. Swap timer text for timer bar in `Adventuring.Draw`
In `Adventuring.Draw`, replace the timer-text block (`timerText` formatting, `timerWidth`, `timerX`, `Graphics.DrawText(...)`) with a `Graphics.DrawFilledRectangle` call:

- Compute `var fraction = MathF.Max(RemainingSeconds, 0f) / OptionTimerSeconds;` — kept ≥0 by the same clamp the old text formatter used.
- Compute integer width: `var barWidth = (int)(Graphics.Width * fraction);` (Open Decision 1 default).
- Draw at `x = 0`, `y = Graphics.Height - TimerBarHeight`, `width = barWidth`, `height = TimerBarHeight`, color `DawnBringers16.Red`.

Position the call after `Buttons.Draw(Graphics)` and before `Mouse.Draw(this)` — matches AC ordering.

### 2. Constants
On `Adventuring`:
- Remove `TimerTopMargin` and `TimerRightMargin`.
- Add `private const int TimerBarHeight = 3;` near the other layout constants.

## Test Plan
- [ ] `dotnet build` passes with no new warnings.
- [ ] Launch the game, idle the bird until an adventure starts. On entering each encounter step the bottom edge of the screen shows a full-width red bar.
- [ ] Watch the bar shrink toward the left as the timer counts down. At ~0 width the default option fires — same trigger point as today's text reaching `0.0`.
- [ ] Click an option mid-countdown; bar disappears (next step re-enters with full bar, or `Dialog` takes over).
- [ ] Confirm no timer text appears in the top-right or anywhere else on screen.
- [ ] Visually check at least one biome where the ground color is close to red (none in the current palette, but worth a spot check) — bar should still be distinguishable since it overdraws the ground strip.

## Learnings

### Architectural decisions
- **Open Decision 1 (rounding)**: chose floor `(int)…` per default. Cheaper, conventional, indistinguishable from round at 60fps for a shrinking width.
- **Open Decision 2 (bar height)**: kept `TimerBarHeight = 3` per default. Adjustable by single constant if user wants different feel after eyeballing.
- **Draw order**: bar drawn after `Buttons.Draw` and before `Mouse.Draw` — bar overdraws option-row underline where they overlap (y=29..31), but cursor stays on top. Same load-bearing-UI-over-cosmetic compromise documented in the analogue ticket.

### Interesting tidbits
- `font` local in `Draw` is still needed after removing timer-text: used for `font.MaxCharacterHeight` when computing encounter-name `textY`. Compiler did not flag, kept untouched.
- Clamp `MathF.Max(RemainingSeconds, 0f)` preserved from the old text formatter — `FixedUpdate` can briefly leave `RemainingSeconds` negative in the same tick the default option fires.

### Rejected alternatives
- Drawing the bar *before* `Buttons.Draw` (so option underline stays visible at the bottom edge): rejected by ticket; the timer is the load-bearing element.
