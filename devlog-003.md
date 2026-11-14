# Devlog: DevFlow UI Regression Suite (Mac Catalyst)

**Date:** 2026-11-14
**Implementing:** plan-uiTests.md (companion to plan.md / the unit lane in devlog-002)

## What Was Done
- Onboarded the DevFlow in-app agent into the **sample app, Debug-only** (the
  library `Mvvm.Nucleus.Maui` is untouched):
  - `MauiProgram.cs`: `#if DEBUG … builder.AddMauiDevFlowAgent()` (plus the
    `using Microsoft.Maui.DevFlow.Agent;` guarded by `#if DEBUG`).
  - `Mvvm.Nucleus.Maui.Sample.csproj`: `Microsoft.Maui.DevFlow.Agent`
    `PackageReference` inside `<ItemGroup Condition="'$(Configuration)' == 'Debug'">`.
  - `Platforms/MacCatalyst/Entitlements.Debug.plist`: base entitlements **plus**
    `com.apple.security.network.server` (the agent's in-app HTTP server needs it),
    wired via `CodeSignEntitlements` for `net11.0-maccatalyst` + Debug only.
- Made `Details.xaml` labels DevFlow-assertable: replaced the `FormattedText`
  labels (bold prefix `<Span>` + value `<Span>`) with plain
  `Text="{Binding …, StringFormat='…'}"` labels. This also removed the
  now-orphaned `ViewIdentifierSpan`, so its dead-code initializer was deleted from
  `Details.xaml.cs`.
- Authored the script-driven suite under `tests/ui/`: 22 flows (nav ×9, popup ×6,
  dialog ×5, details ×2) + `run-all.sh` + `lib/devflow.sh` (shared driver) +
  `README.md`. Every `maui devflow` call goes through the `df` wrapper with
  `--ci --json`; `jq` is the only external dependency; execution is sequential
  (DevFlow driving-lease constraint).
- Verified the plan §7 exit criteria:
  - `run-all.sh` **22/22 ×2 back-to-back** (two consecutive full passes).
  - Release Catalyst bundle contains **no** `*DevFlow*` artifacts and no DevFlow
    strings in any Mach-O.
  - Unit lane **65/65** (Release).
  - No new warnings — only the pre-existing `CS0618` (CommunityToolkitV1*) and
    `NU1902` (Microsoft.Build.Tasks.Git) baseline.

## Tricky Parts
- **LaunchServices error -54 on `open` (the biggest one).** Three launch attempts
  failed with `_LSOpenURLsWithCompletionHandler() failed with error -54`. Root
  cause was *not* a stale-instance or stale-LS-registration problem (both chased
  first): it was **bundle selection**. `run-all.sh` used
  `find … -path '*net11.0-maccatalyst*' … | head -1`, and the Release verification
  build had emitted extra bundles (`maccatalyst-x64`, `maccatalyst-arm64`,
  universal) sharing the same name — `head -1` grabbed the **Release x86_64**
  bundle, which cannot launch on this arm64 host → -54. The earlier 22/22 run only
  passed because the Release bundles didn't exist yet. Fix: restrict the search to
  `bin/Debug` (only Debug embeds the agent; it yields exactly one
  `net11.0-maccatalyst` bundle = the host arch, so selection is unambiguous).
  `lsregister -f` is kept as a secondary defense.
- **Agent-driven no-op Shell navigation wedges the app (library finding).** A
  `ui navigate` to the *current* location fires Shell `Navigating` (the library's
  `NavigationService` sets `IsNavigating=true`) but never fires `Navigated`, so the
  flag is never reset and every later `INavigationService` request is silently
  ignored. Any *real* navigation (tab switch, pop) fires `Navigated` and unsticks
  it. This drove the design of `reset_to` (see Decisions).
- **Non-key windows swallow synthesized touches.** A bundle binary launched from a
  terminal (or a leftover background instance) ends up with a non-key window that
  silently swallows DevFlow taps (CLI reports success; the command never runs).
  Hence the mandatory `open` launch + the `df` `success:false` → reactivate-retry
  recovery.

## Decisions Made
- **`reset_to` is 100% app-UI-driven and never issues `ui navigate`.** It unwinds
  stray dialogs, popups (`Close using IPopupService`/`Close Popup`), pushed pages
  (first tree-order `Navigate Backwards`), and modals (`Close All Modals (if any)`),
  then switches tabs via the stable `Tab_<name>` elements. If already on target with
  nothing to unwind it performs an *un-wedge dance* (real tab switch away and back)
  to clear a possibly-wedged `IsNavigating`. `_tap_tab` pre-checks the agent route
  to skip the no-op tap (a tap on the active tab reports `success:false` + non-zero
  exit) and tolerates that failure when the route already confirms the target.
- **Debug-only, sample-only agent onboarding** via `#if DEBUG` + a Debug-conditional
  `PackageReference`, so Release bundles provably contain no DevFlow artifacts.
- **`success:false` reactivate-retry in `df`.** If a tap/fill is reported
  `success:false` (touch synthesized but not accepted — non-key window), `df`
  reactivates the app to restore key status and retries (bounded by
  `DF_MAX_ATTEMPTS`).
- **Deterministic teardown** (graceful quit → wait loop → SIGKILL) so a run can't
  leave a background instance that poisons the next run's launch.

## Deviations from Plan
- **CT 17 popup lifecycle order (`popup_ct_compat`).** The plan assumed the
  nucleus-service order `State: Default, Prepared, Opened`. The CT 17
  `IPopupService` path actually reads `State: Default, Opened, Prepared`, because
  `NucleusMvvmPopupBehavior.OnPopupOpened` fires `OnOpened()` **before**
  `Prepare()`. Both orders are deterministic library behavior; the flow pins the
  CT 17 order (the nucleus path is pinned by `popup_advanced`).
- **Alert message-body assertions replaced by button assertions.** Catalyst native
  NSAlert bodies are not queryable (spike #1), so flows assert on the button set /
  follow-up state instead of the alert text.
- **Sample bug found by the suite and fixed.** `PopupTabViewModel.
  ShowBackgroundThreadPopup` built the `IsMainThread = …` navigation parameter but
  called `ShowPopupAsync<SimplePopup>()` *without* it (the popup showed the
  "Sample Text" fallback). Fixed to pass `navigationParameters` (and dropped the
  unused `result`); `popup_background_thread.sh` now pins the real parameter.
- **`Details.xaml`/`.cs` simplification** was a means to the plan's assertability
  goal, not specified line-by-line: `FormattedText`→`Text`+`StringFormat`, and the
  dead `ViewIdentifierSpan` initializer removed as a consequence.
- **`run-all.sh` ROOT path fix.** The suite root resolved to `tests/` (one level
  up) instead of the repo root (two levels up), so the build targeted a
  non-existent `tests/Mvvm.Nucleus.Maui.Sample`. Fixed the `ROOT` `cd` depth.
- **No CI workflow files** (per plan): the macOS-runner job is documented in
  `tests/ui/README.md` instead.

## Next Steps
- Optional (out of scope for this feature): file the `ui navigate` no-op /
  `IsNavigating`-wedge finding upstream as a DevFlow + library interaction issue —
  the library is out of scope for change here, so the suite works around it.
- When a macOS GitHub runner is available, promote the documented `tests/ui`
  pipeline into a real CI job.
