# Devlog: NavigationService no-op wedge fix

**Date:** 2026-11-14
**Implementing:** library finding from the DevFlow UI suite (devlog-003 "Tricky
Parts", tests/ui README "Operational caveats" #2) — the `ui navigate` no-op /
`IsNavigating`-wedge interaction.

## What Was Done
- **Fixed the wedge in `NavigationService`** (the library, which devlog-003 left
  out of scope): `ShellNavigating` no longer sets `IsNavigating` for a **no-op
  navigation** (target location == current location). Such a navigation fires
  `Shell.Navigating` but never fires `Shell.Navigated` — and MAUI 11 has no
  navigation-failed event on `Shell` (verified against the compiled API surface)
  — so under the old code the flag stayed `true` forever and, with
  `IgnoreNavigationWhenInProgress` (default true), every subsequent
  `INavigationService` request was silently dropped.
  - New private `IsNoOpNavigation(ShellNavigatingEventArgs)`: null-safe comparison
    of `Current.Location` / `Target.Location` (case-insensitive).
  - The no-op branch also skips the `_transientPagesOnNavigating` capture (there is
    no `Navigated` to consume it) and logs an Information line so the skip is
    diagnosable in app logs.
  - Canceled navigations still reset the flag; real navigations still set it —
    the in-progress protection is unchanged.
- **Pinned the behavior with unit tests:**
  `tests/Mvvm.Nucleus.Maui.UnitTests/Services/NavigationServiceNoOpNavigationTests.cs`
  (3 tests: no-op leaves `IsNavigating` false; no-op raises no
  `IsNavigatingChanged`; real navigation still sets the flag on `Navigating` and
  clears it on `Navigated` — the last via a small `RaiseNavigated` helper mirroring
  `ShellNavigatingEventArgsBuilder`).
- **Verified:** full unit lane **68/68** (65 baseline + 3 new, only the pre-existing
  `CS0618`/`NU1902` warnings); library builds clean for net11.0 /
  net11.0-maccatalyst / net11.0-ios / net11.0-android (the net11.0-windows TFM
  fails only at the `MakePri.exe` packaging step — x86 Windows tool on macOS,
  pre-existing environment limit).
- **Proved the tests catch the bug:** with the library fix stashed, the two no-op
  tests FAIL (the wedge reproduces) and the real-navigation regression test
  passes; with the fix, all pass.
- **Docs:** tests/ui README caveat #2 and the CONTRIBUTING.md troubleshooting
  line updated to record the fix (the suite's no-op avoidance + un-wedge dance
  stay as defense against agent/MAUI churn until this release ships).

## Tricky Parts
- **MAUI 11 has no `NavigationFailed`-style event on `Shell`** (only
  `Navigating`/`Navigated` — confirmed by probing the compiled
  `Microsoft.Maui.Controls` API), so the fix cannot hook a failure event; it
  must recognize the no-op in `ShellNavigating` itself.
- **API details that don't match intuition** (all discovered via the compiler /
  a reflection probe, not the docs): `ShellNavigationSource` has **no
  `Programmatic` member** (members: `Unknown, Push, Pop, PopToRoot, Insert,
  Remove, ShellItemChanged, ShellSectionChanged, ShellContentChanged`);
  `ShellNavigationState.Location` is a `Uri`; `ShellNavigatedEventArgs` takes
  `(previous, current, source)`.
- **Why "leave untouched" instead of "set false" on a no-op:** conservative — a
  `Navigating` handler can still be in flight (the `IConfirmNavigationAsync`
  deferral path suspends the async-void handler), so clobbering a legitimately
  in-progress flag would be worse than leaving it; the flag starts false, so
  "leave untouched" fixes the wedge without a race.

## Decisions Made
- **Fix in the library, keep the suite workaround.** The suite's
  never-issue-no-op + un-wedge dance stays as defense-in-depth against
  DevFlow-agent/MAUI churn until this library release ships and is bumped in the
  sample; the driver is version-agnostic by design.
- **Source-agnostic detection.** The no-op check compares locations, not
  `ShellNavigationSource` — whichever source an automation driver reports, a
  target==current navigation is a no-op.

## Next Steps
- Bump the library version and re-verify the UI suite against it (the suite
  should still pass unchanged; the dance becomes pure defense).
- Optional: add a flow that issues an agent `ui navigate` to the *current*
  location and then a real navigation, to pin the fix end-to-end against the
  live agent (currently pinned only at the unit level with synthetic events).
