# UI regression suite (DevFlow integration lane)

Script-driven UI regression suite for the `Mvvm.Nucleus.Maui` sample app, driven
against the running **Mac Catalyst** app with the DevFlow in-app agent
(`maui devflow ui … --ci --json`). It covers the integration surface that has
historically regressed on every MAUI/CommunityToolkit major jump: Shell
navigation, all four popup flavors (incl. CommunityToolkit 17 `IPopupService` and
V1-compat), native dialogs, parameter passing, double-navigation debounce, and
confirm-navigation.

Companion to the unit lane in `tests/Mvvm.Nucleus.Maui.UnitTests` (see
`research-testingOptions.md`, `decision-ui-automation-devflow.md`,
`plan-uiTests.md`).

## Prerequisites

- macOS (the only host that can run Mac Catalyst; ubuntu runners cannot).
- .NET 11 SDK + MAUI workloads for `net11.0-maccatalyst`.
- The `maui` DevFlow CLI (`0.1.0-preview.14.26555.2` verified) — `~/.dotnet/tools/maui`.
- `jq` (preinstalled on GitHub macOS runners; locally: `brew install jq`).

The sample app embeds the DevFlow agent **Debug-only** (`Microsoft.Maui.DevFlow.Agent`
with `Condition="'$(Configuration)' == 'Debug'"` in the sample csproj, registered in
`#if DEBUG`, and `com.apple.security.network.server` in
`Platforms/MacCatalyst/Entitlements.Debug.plist`). Release builds contain no agent.

## How to run

```bash
tests/ui/run-all.sh
```

The runner builds the sample (Debug, `net11.0-maccatalyst`), launches the bundle
via LaunchServices (`open` — see "Operational caveats" below), starts the DevFlow
broker, waits for the agent, executes the 22 flow scripts **sequentially** (a
DevFlow driving-lease constraint), fail-fasts on the first failure, and quits the
app (the broker daemon is left running — stopping it is the recovery step, not the
teardown step).

Individual flows can be run standalone (the app must already be running and the
agent connected):

```bash
tests/ui/nav_by_type.sh
```

Every flow script sources `lib/devflow.sh`, which provides the driver helpers:

| Helper | Purpose |
|---|---|
| `df <step> <args…>` | `maui devflow <args> --ci --json` with spike #1 recovery: driving-lease conflict → wait + retry; broker lease-authority hiccup → broker stop/start + retry; stale element/epoch → `ui tree` refresh + retry |
| `ensure_broker` / `wait_agent` | broker + agent health |
| `reset_to <tab>` | land on a known tab (`//Tabs/NavigationTab` / `PopupTab` / `DialogTab`) using **only the app's own UI** — dismiss stray dialogs, close open popups, pop pushed pages, close modals, then tab-bar switch (or an un-wedge tab dance if already there); see "Operational caveats" |
| `tap_text <label> [kind]` | exact-text lookup → tap; **topmost match wins** (overlays — popups, pushed pages, native dialogs — are appended after underlying content in the tree) |
| `tap_text_first <label> [kind]` | same, but the **first** tree-order match (e.g. the plain `Navigate Backwards` button, first of the three `Navigate Backwards*` buttons) |
| `assert_text` / `assert_absent` | exact-text presence / absence (presence waits up to 15s for in-flight transitions) |
| `wait_dialog` / `assert_dialog` | a dialog button (native `DialogAction`) is registered, i.e. the dialog is up |
| `dismiss_dialog <button>` | `ui alert detect`/`dismiss` first (in-app/off-Catalyst dialogs); fallback: tap the button (Catalyst native pattern) |
| `enter_prompt <value> [ok]` | find the native `UITextField`, `ui fill`, then confirm (Catalyst prompt buttons are `OK`/`Cancel`) |

Tunables via environment:

| Variable | Default | Meaning |
|---|---|---|
| `DF_GAP_SECS` | 12 | spacing after a **mutating** CLI call (keeps the next mutation outside the ~10–15s driving-lease window) |
| `DF_RO_GAP_SECS` | 1 | spacing after a read-only CLI call (queries/trees don't take the lease) |
| `DF_RETRY_WAIT_SECS` | 12 | wait before retrying after a driving-lease conflict |
| `DF_MAX_ATTEMPTS` | 4 | attempts per CLI call, including the first |
| `DF_WAIT_TIMEOUT` | 15 | default `--wait-until` timeout (seconds) |

## Operational caveats (baked into the driver, found during verification)

1. **Launch via LaunchServices (`open`), never by executing the bundle binary
   directly.** A Catalyst app started from a terminal ends up with a non-key
   window that silently swallows DevFlow-synthesized touches — the CLI reports
   `{"success":true}`, but the button's command never runs. Side effect: the app's
   `AddDebug()` log lines (stderr) are only visible in that terminal-launched mode,
   so the running suite has no live app log; flows assert on UI state instead.
   `run-all.sh` hardens this end to end:
   - **Select the right bundle, or `open` fails with error -54 (`procNotFound`).**
     Both the Debug and Release builds of `net11.0-maccatalyst` emit a
     `Mvvm.Nucleus.Maui.Sample.app` bundle (Release adds `maccatalyst-x64`,
     `maccatalyst-arm64`, and a universal one). A broad `find … | head -1` then
     non-deterministically grabs a **Release x64** bundle, which cannot launch on
     an arm64 host → -54 (the run-2 / 2026-11-14 failure). `run-all.sh` therefore
     restricts the search to `bin/Debug` — only the Debug build embeds the DevFlow
     agent, and it yields exactly one `net11.0-maccatalyst` bundle (the host arch),
     so the selection is unambiguous. It also runs
     `…/LaunchServices…/lsregister -f <bundle>` before `open` as a secondary
     defense against stale same-bundle-ID LS registrations (idempotent).
   - It hard-kills any stale app instance before launch, launches via `open` with
     one retry, verifies the agent's `processId` is alive after connecting, and
     tears down deterministically (graceful quit → wait loop → SIGKILL).
   - The `df` driver also self-recovers a non-key window: if a tap/fill is reported
     `{"success":false}` (the touch was synthesized but not accepted — the run-2
     failure mode, 2026-11-14) it reactivates the app to make its window key again
     and retries.
2. **Agent-driven no-op shell navigation used to wedge the app (library finding,
   now fixed in the library).** A `ui navigate` to the *current* location fired
   Shell's `Navigating` — the library's `NavigationService` set `IsNavigating =
   true` — but never fires `Navigated`, so the flag was never reset and every
   subsequent `INavigationService` request was silently ignored ("Ignoring this
   navigation request as another request is already in progress"). Any *real*
   navigation (tab switch, pop) fires `Navigated` and unsticks it. This was the
   root cause of the "all navigation buttons dead until a manual tab switch"
   symptom (reproduced 2026-11-13). **Fixed in the library (2026-11-14):**
   `NavigationService.ShellNavigating` now detects no-op navigations (target
   location == current location) and leaves the navigation state untouched
   (pinned by `NavigationServiceNoOpNavigationTests`); until that release ships —
   and as defense against agent/MAUI churn — the suite still works around it (see 3).
3. **`reset_to` is 100% app-UI-driven and never issues `ui navigate`.** It unwinds
   stray dialogs, popups (`Close using IPopupService` / `Close Popup`), pushed
   pages (plain `Navigate Backwards`, first tree-order match), and modals
   (`Close All Modals (if any)`), then switches tabs via DevFlow's stable
   `Tab_<name>` elements — a real Shell navigation through the app's own tab bar.
   If already on the target tab with nothing to unwind, it performs an *un-wedge
   dance* (real tab switch away and back) so a wedged `IsNavigating` flag left by
   an interrupted flow is cleared. `_tap_tab` checks the agent's route first and
   skips the no-op tap (a tap on the already-active tab is reported as
   `{"success":false}` and the CLI exits non-zero), tolerating the reported failure
   when the route already confirms the target tab.
4. **CT 17 vs nucleus popup lifecycle order.** For `AdvancedPopup` shown via the
   CT 17 `IPopupService`, `NucleusMvvmPopupBehavior.OnPopupOpened` fires
   `OnOpened()` **before** `Prepare()`, so the state label reads
   `State: Default, Opened, Prepared` — the reverse of the nucleus-service path
   (`State: Default, Prepared, Opened`, see `popup_advanced.sh`). Both orders are
   deterministic library behavior, and each flow pins its own order.
5. **A sample bug the suite caught.** `PopupTabViewModel.ShowBackgroundThreadPopup`
   built the `IsMainThread = …` navigation parameter but never passed it to
   `ShowPopupAsync<SimplePopup>` (the popup showed its "Sample Text" fallback).
   Fixed to pass the parameter (2026-11-14); `popup_background_thread.sh` pins it.

## Catalyst native-dialog contract (why the flows look the way they do)

Spike #1 (re-confirmed during implementation) established, for Catalyst native
NSAlerts:

- `ui alert detect`/`ui alert dismiss` report **false** — DevFlow is blind to the
  alert *as an alert*;
- the alert's **buttons are registered as native `DialogAction` elements** and are
  tappable — this is how every dialog is dismissed here;
- the alert **message body is not queryable** — so flows assert on the button set
  and the follow-up state (e.g. the follow-up alert's `Okay` button) rather than on
  message text;
- prompt text fields are native `UITextField`s, fillable via `ui fill`; buttons are
  `OK`/`Cancel`.

## Future CI job (no workflow file in this repo yet)

Per the research decision ("structure now, enable when ready"; consistent with
`plan.md`), the workflow file itself is deliberately not authored. When it is, it is
a drop-in **macOS runner** job:

```yaml
jobs:
  ui-tests:
    runs-on: macos-latest          # the only runner that can host Catalyst
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with: { dotnet-version: 11.x }
      - run: dotnet workload install maui
      - run: tests/ui/run-all.sh    # self-contained: build → launch → wait → flows → teardown
```

An **iOS simulator** variant (`simctl` bootstrap + launch + `maui devflow … --device <UDID>`)
is the alternate host if Catalyst ever becomes unavailable in CI; it is documented
here but not built (Catalyst is the validated host, per spike #1).

## Notes

- The suite is a debug-bench for the sample, not a shipped artifact: nothing under
  `tests/ui/` is packed, referenced, or part of the library's public surface.
- `Details.xaml` data labels use plain `Text` + `StringFormat` (not `FormattedText`
  spans) so their values are exposed to the DevFlow registry and assertable — do not
  convert them back.
- The agent package is preview-only (pinned in the sample csproj). If
  `Microsoft.Maui.DevFlow.Agent` churns, bump-and-verify the pin first; the driver
  targets the stable CLI surface by design (XCUITest remains the documented
  fallback, see `decision-ui-automation-devflow.md`).
