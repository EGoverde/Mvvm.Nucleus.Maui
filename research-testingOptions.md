# Research: Introducing Testing to Mvvm.Nucleus.Maui

**Requester:** Erik
**Date:** 2026-11-13

## Requirements

### Original Request
"Introduce testing to this project. Unit tests where they make sense (unsure if there's much value here, but perhaps). But likely some form of UI testing — the samples project already allows me to test most functionality manually. Let's research the options and where value can be added."

Follow-up: ideally testing could be part of a pipeline, but depends on practicality. If very involved it can start locally, but structure it with future CI in mind.

### Context
- This repo is the **`Mvvm.Nucleus.Maui` library** (NuGet 0.8.0, consumed by third-party MAUI apps) plus a **`Mvvm.Nucleus.Maui.Sample`** app that serves as the manual test bench.
- The project just completed the **.NET 11 / MAUI 11 / CommunityToolkit.Maui 17.0.0** upgrade (see `research.md`, completed 2026-11-13). Library targets: `net11.0-android/ios/maccatalyst/windows` — **no plain `net11.0` TFM exists** in the library.
- **No CI exists** (no `.github/workflows/`, no pipeline config at all).
- **DevFlow is already part of this environment**: `maui devflow` CLI v0.1.0-preview.14.26555.2 installed, MCP server configured in `opencode.json`, DevFlow skills present in the workspace. However, the **sample app has not been onboarded** (no `Microsoft.Maui.DevFlow.*` package reference, no `AddMauiDevFlowAgent()`).
- Repo conventions: inline `PackageReference` versions (no Central Package Management, no `Directory.Build.props`, no `global.json`).
- The library's public API is consumed by third parties — test-support changes should not expand or change the public surface more than necessary.

### Open Questions
1. ~~**DevFlow agent ↔ MAUI 11 compatibility (gating).**~~ → **RESOLVED (spike #1, 2026-11-13): YES.** The net10-line `0.1.0-preview.14.26555.2` agent compiles, runs, and fully drives the net11.0-maccatalyst sample app (connect, tree, navigate, tap, assert, popups, dialogs). See *Spike results* below.
2. ~~**Unit-test host runtime.**~~ → **RESOLVED (spike #2, 2026-11-13): plain `net11.0`.** A platform-TFM testhost does **not** work under `dotnet test` (Catalyst SDK app-bundle pipeline crashes; android needs a device). But the library can target plain `net11.0` (all deps ship it, code has no platform conditionals), and a plain-`net11.0` xunit project runs natively with `dotnet test`. See *Spike results*. **Adopting this adds a TFM to the shipped package — a decision point for the user.**
3. ~~**How to initialize `NucleusMvvmCore` in tests.**~~ → **RESOLVED (spike #3, 2026-11-13): minimal `InternalsVisibleTo` is sufficient.** `NucleusMvvmCore` initializes in a bare `net11.0` host via `internal Initialize(IServiceProvider)` + the `internal` `Shell` setter, both exposed to the test assembly with `InternalsVisibleTo Include="Mvvm.Nucleus.Maui.UnitTests"` (no `MauiApp` bootstrap needed). `NucleusMvvmPageBehavior` subscribes to the **public** `Page` events and its handlers read from `NucleusMvvmCore.Current` + `BindingContext` (the event-args are ignored), so events are raised through the backing-field delegate with the non-public event-args ctors invoked via reflection. All lifecycle routing verified. See *Spike results*.
4. ~~**DevFlow alert handling on Catalyst.**~~ → **RESOLVED WITH CAVEAT (spike #1, 2026-11-13).** `ui alert detect`/`ui alert dismiss` do **not** work for Catalyst native NSAlerts (both report false), but the alert's buttons *are* registered as native `DialogAction` elements — `ui tap <id>` dismisses them reliably. The alert **message text is not queryable** (only buttons). So: dialog *flows* are automatable (trigger → detect button → tap → verify follow-up state), but we cannot assert on the alert message body. See *Spike results*.
5. **CI runner shape.** DevFlow needs a *running* app. Realistic options: GitHub **macOS runner** launching the Catalyst app (`open`), or an **iOS simulator** via `simctl` + DevFlow. Ubuntu runners cannot host Catalyst. → Verify later; structure for it now.
6. **Sample-ViewModel unit tests.** The sample's ViewModels are thin wrappers around the services (the very thing under test). Probably little value — confirm and skip.

## System Architecture

### Related Components

**Testable surface of the library** (assessed per component):

| Component | Unit-testable without a running app? | Notes |
|---|---|---|
| `Compatibility.NavigationParameters` | ✅ Yes, pure C# | Prism-compat contract: `this[missingKey]` returns `null` instead of throwing. This exact null semantics is the reason the class exists — a regression would silently break migrating consumers. |
| `Compatibility.CompatibilityExtensions` (`IsBackNavigation` / `GetNavigationMode`) | ✅ Yes, pure C# | Reads `NavigationMode` from a dictionary; trivial to pin down. |
| `Extensions.DictionaryExtensions` (`GetValueOrDefault<T>` / `GetStructOrDefault<T>` / `GetShellNavigationSource`) | ✅ Yes, pure C# | Public API every consumer uses for navigation parameters; wrong-type/missing-key/fallback edge cases are cheap to test. |
| `NucleusMvvmOptions` defaults | ✅ Yes, trivial | Cheap guard that documented defaults (`IgnoreNavigationWithinMilliseconds = 250`, etc.) don't silently change. |
| `NavigationService.SetIsNavigatingOrIgnoreRequest` | ⚠️ Coupled | The flagship "debounce double navigation" logic is `protected virtual` and needs options + a logger, but the constructor subscribes to `NucleusMvvmCore.Current.Shell` → requires core initialized with a Shell. Also note the static `_isNavigatingLastTriggeredUtc` (shared across instances, test-order hazards). |
| `NavigationService.GetQueryParameterDictionary` (route `?key=val` parsing) | ⚠️ Private | Only reachable via `NavigateToRouteAsync` (touches `Shell.Current`) — or extracted. |
| `NucleusMvvmPageBehavior` (lifecycle routing: `IPrepare(Async)` → `IRefresh(Async)` → `IInitializable` → `INavigatedAware`, `IDestructible`, appear/disappear, Shell double-`NavigatedTo` workaround) | ⚠️ Coupled | **The heart of the library** — when each callback fires is its core value. Page events (`Appearing`, `NavigatedTo`, …) are public and raisable in a test host, but the behavior reads `NucleusMvvmCore.Current.NavigationParameters` and `AppResumed/AppStopped`. Needs core initialized (open question 3). |
| `ViewFactory` / `PopupViewFactory` | ⚠️ Coupled | DI + `NucleusMvvmCore.Current` + behavior attachment; mostly thin. Best validated by UI tests. |
| `PopupService` / `PageDialogService` / `NucleusWindowCreator` | ❌ No meaningful unit value | Thin wrappers over `Shell`/`CommunityToolkit`/`MainThread` + a real `Page`. Their contracts are exactly what UI tests should assert against. |
| `AppBuilderExtensions.UseNucleusMvvm` (DI wiring, route registration, CT popup compat) | ⚠️ Coupled | Best validated by the sample app starting up and working (UI lane). |

**Sample app (the UI test fixture)** — small and already well-organized:
- 3 tabs (`NavigationTab`, `PopupTab`, `DialogTab`) + global route `Details` + 4 popups (`Simple`, `Advanced` with return value, `Singleton`, `Legacy` via CT V1 compat service).
- `Details` page displays `PageIdentifier`, `NavigationParameterData`, `OnPrepareData`, `OnRefreshData` as labels → **assertable via visual tree** without screenshots.
- `NavigationTabViewModel` covers: navigate by type/route/params/query-string, modal, modal+`NavigationPage` stack, background-thread navigation, double-trigger debounce (`NavigateMultipleTriggers`).
- `PopupTabViewModel` covers: all 4 popup flavors, `IPopupPrepare` data flow, return values, CT `IPopupService` compat, CT V1 compat service, background-thread popup.
- `DialogTabViewModel` covers: alert, confirm, prompt, action sheet, background-thread alert.
- `DetailsViewModel` covers: `IConfirmNavigation` (back with confirm), `NavigateBackTwice`, `CloseModal`, `CloseAllModal`, deeper push/modal.

**DevFlow tooling (already present in this environment):**
- CLI: `maui devflow ui {tap, navigate, query, element, property, assert, alert, gesture, screenshot, hit-test, …}`, `batch` (JSONL commands on stdin → JSONL on stdout), `wait`, `diagnose`, `agent`, `broker`, `--ci` (non-interactive, fail fast), `--json`.
- MCP server: same command surface (configured in `opencode.json`).
- Onboarding requirements (per `maui-devflow-onboard` skill): add `Microsoft.Maui.DevFlow.Agent` (standard MAUI flavor), register `builder.AddMauiDevFlowAgent()` inside `#if DEBUG` in the sample's `MauiProgram.cs`, and for **Mac Catalyst Debug** add the entitlement `com.apple.security.network.server`. Verify via `maui devflow diagnose` / `list` / `ui tree`.
- **Constraint found:** the agent package is **preview-only and net10-line** (see open question 1).

**CI status:** none exists. Any pipeline must be built from scratch (GitHub Actions assumed, repo is public on GitHub).

### Data Flow

Two independent test lanes:

1. **Unit lane:** `xunit` test project → ProjectReference to `Mvvm.Nucleus.Maui` → requires a platform TFM (e.g. `net11.0-maccatalyst` or `net11.0-android`) → `dotnet test` runs the managed `testhost` with that platform's runtime. No device/simulator needed for the managed parts, *if* the TFM's runtime can run bare on the host (open question 2).
2. **UI lane:** sample app runs (Catalyst locally, or simulator) with the DevFlow agent embedded (`#if DEBUG`) → agent exposes an HTTP JSON API (broker, default port 9223) → driven by `maui devflow` CLI (scriptable via `batch`/`--ci`) or the MCP server. Flow: launch app → `ui tap` / `ui navigate` → `ui assert` on element properties → `ui alert` for dialogs → next step.

**Static dependency graph (the main unit-testing obstacle):**
`NucleusMvvmCore.Current` (static singleton, `internal Initialize`, `Shell`/`Window` getters throw if not registered) ← `NavigationService` (ctor subscribes `Current.Shell` events; `Shell.Current` for `GoToAsync`), `NucleusMvvmPageBehavior` (reads `Current.NavigationParameters`, `Current.AppResumed/AppStopped`), `PopupService`/`PageDialogService` (`Current.Shell`/`Current.CurrentPage` + `MainThread`), `ViewFactory`/`PopupViewFactory` (`Current.NavigationParameters`). Plus `Shell.Current` and `MainThread` everywhere, and `Routing.RegisterRoute` (static) inside `UseNucleusMvvm`.
Consequence: **anything that touches Shell or MainThread belongs in the UI lane**; only the pure-C# surface and the event-routing logic (behaviors) are candidates for the unit lane, and the latter needs the core initialized in the test host.

### Constraints
- **Platform-TFM-only library** → unit test project must pick a platform TFM; a plain `net11.0` test project is impossible while referencing this library.
- **CI runner pairing:** macOS runners for Catalyst/simulator lanes; a maccatalyst testhost cannot run on ubuntu runners; an android-TFM testhost needs the Android workload on whatever runner executes it.
- **Preview tooling risk:** DevFlow agent is `0.1.x-preview` and net10-line (open question 1). Mitigation: keep the UI suite script-based (`batch` JSONL / CLI sequences) so the driver layer (CLI) is stable even if the MCP surface moves.
- **Debug-only agent:** the DevFlow agent must never ship in Release or in the library itself (it would affect all consumers) — sample app only, inside `#if DEBUG`.
- **Catalyst Debug entitlement** (`com.apple.security.network.server`) required for the in-app HTTP server.
- **Consumer-facing API:** prefer zero public API changes for test support. `InternalsVisibleTo` is the least-invasive seam; refactoring `NavigationService` debounce logic into a pure, separately testable unit is the alternative (open question 3).
- **Static test state:** `NavigationService._isNavigatingLastTriggeredUtc` is `static` — debounce tests must handle shared state between tests (or another argument for extracting it).
- **Windows TFM** is declared but unsupported on this host; unit test project should not require building it.

## Options

### Option A — Unit tests only
xUnit project for the pure-C# surface (compat contracts, dictionary extensions, options defaults) + behavior lifecycle tests if the core can be initialized in a test host (seam decision required).
- **Catches:** regressions in the public compatibility contracts and the lifecycle-callback routing (the library's core value) *before* a running app is involved.
- **Misses:** everything that needs Shell/handlers/popup runtime — i.e. most of what has historically regressed (CT popup major jumps).
- **Effort:** low–medium (one test project + spike on TFM/host + one seam decision).

### Option B — UI tests only (DevFlow)
Onboard the DevFlow agent (DEBUG-only) and script the sample flows (navigation, modals, popups, dialogs, parameters, debounce, confirm-navigation) via `maui devflow` CLI/`batch`.
- **Catches:** exactly the integration surface that has historically broken (CT popup rewrites, Shell navigation quirks like the double-`NavigatedTo` workaround, popup open/close event bugs Nucleus explicitly works around).
- **Misses:** fast feedback on pure logic; slower to run than unit tests.
- **Effort:** low–medium *if* open question 1 passes (package add + entitlement + scripts). High if the agent can't run on MAUI 11.

### Option C — Unit + UI (recommended shape)
Both lanes: unit lane as the fast/cheap safety net on contracts + lifecycle logic; DevFlow lane as the integration regression suite. This matches the `maui-unit-testing` skill's boundary guide (unit for state logic; running-app behavior for handlers/navigation/popups).

### Option D — XCUITest / Appium alternatives (for the UI lane)
- **XCUITest (Catalyst):** Apple-native, CI-robust (`xcodebuild test`), works regardless of DevFlow's preview status. **But:** Swift test code in a C# repo, xcodeproj/scheme management for a MAUI single project, and the heaviest authoring cost here.
- **Appium / UIAutomator (Android emulator):** most mature third-party ecosystem, but the most moving parts (emulator images in CI, driver versioning, page-object layer) and Android is a secondary target on this machine.
- **Verdict:** neither is the natural fit for *this* repo (C#, macOS-first, DevFlow already in the environment). Keep XCUITest as the fallback if DevFlow fails the MAUI 11 spike.

### Where value can be added (value matrix)

| Test type | Regressions it catches | Evidence of need | Effort | CI fit |
|---|---|---|---|---|
| Unit: compat contracts (`NavigationParameters`, `IsBackNavigation`, dictionary extensions) | Silent breaking of Prism-migrating consumers' expectations | Public API, documented contract, zero test coverage today | Very low | Good (platform TFM on macOS runner) |
| Unit: behavior lifecycle routing (Prepare/Refresh/Destructible/confirm-nav) | Wrong-order or missed callbacks — the library's core promise | Core of the library; complex event ordering + workarounds | Medium (needs core-init seam) | Good |
| Unit: navigation debounce logic | Double-navigation protection (flagship feature) regressing | Public feature with documented options + bypass param; `static` state hazard | Low–medium (extraction or core-init) | Good |
| UI: navigation flows (push/pop/modal/close-all/tabs/params/query) | Shell integration, `NavigationPage` modal stack, parameter passing, `IConfirmNavigation` | Historically the flakiest area; sample exists precisely for this | Low–medium (if DevFlow works) | Good (macOS runner, Catalyst or simulator) |
| UI: popup flows (all 4 flavors, return values, singleton reuse, CT compat, V1 compat) | **Biggest proven risk:** CT 12→13 V2 rewrite forced a 0.6.0 release; 13→17 was forced by .NET 11 | `MIGRATIONS.md`; `research.md` | Low–medium (if DevFlow works) | Good |
| UI: dialogs (alert/confirm/prompt/action sheet, background-thread) | `CurrentPage` resolution + MainThread marshalling | Sample has dedicated flows | Low (if `ui alert` works on Catalyst — open question 4) | Good |
| Unit: sample ViewModels | — | Thin wrappers over the services under test; low value | — | Skip |

## Prior Art
- **Repo's own regression history:** CommunityToolkit.Maui 12.x's V2 popup breaking change forced a significant Nucleus rework in 0.6.0 (`MIGRATIONS.md`); the .NET 11 move forced CT 13→17 (today's `research.md`). Every CT/MAUI major jump is a popup-surface risk → the strongest argument for an automated integration suite over manual-only testing.
- **`maui-unit-testing` skill** (workspace): boundary guide — unit tests for ViewModel/state logic without a device; device/running-app automation for handlers, native controls, end-to-end flows. Anti-patterns: no `MauiProgram.CreateMauiApp()` in unit tests, no UI assertions in unit tests, no `MainThread` dispatch in synchronous logic under test.
- **DevFlow (dotnet/maui-labs):** in-app agent + CLI/MCP automation; this workspace already ships its skills and MCP config. `ui assert`/`ui query`/`ui alert` map directly onto the sample's assertable labels and native dialogs.
- **Prism's `NavigationParameters`** — the class Nucleus mimics deliberately returns `null` for missing keys; Nucleus' compatibility class documents the same contract, which is exactly the kind of behavior unit tests should pin.
- **GitHub Actions macOS runners** — the only realistic CI home for both the maccatalyst testhost and Catalyst/simulator UI runs (unverified, open question 5).

## Recommendation (staged, structure-for-CI)
1. **Gating spikes (cheap, do first):**
   - DevFlow agent in the MAUI 11 sample: package add + `#if DEBUG` + Catalyst entitlement → build, run, `maui devflow diagnose`, drive one tab, try `ui alert` (answers open questions 1 & 4).
   - Minimal xUnit project (platform TFM) with one trivial test → `dotnet test` on this Mac (answers open question 2; pick TFM: maccatalyst vs android).
   - Core-init probe: can `NucleusMvvmCore` be initialized in the test host (via `InternalsVisibleTo` or `UseNucleusMvvm` bootstrap) and does `NucleusMvvmPageBehavior` route a raised `NavigatedTo` correctly (open question 3)?
2. **`tests/Mvvm.Nucleus.Maui.UnitTests`** — compat contracts, dictionary extensions, options defaults first; then debounce logic and behavior lifecycle tests depending on spike 3 outcome (seam decision: `InternalsVisibleTo` vs small extraction).
3. **UI suite** — keep DevFlow (if spike 1 passes): script the flows as data (CLI/`batch` JSONL), stored as files under `tests/ui/` so the same scripts run locally (CLI/MCP) and in CI (`--ci` mode); document the flow set from the value matrix above.
4. **CI (structure now, enable when ready)** — GitHub Actions: macOS runner job(s): build library (Apple + Android TFMs), unit tests, UI suite (Catalyst via `open` or iOS simulator); separate android build job on ubuntu if desired. Keep Windows excluded (existing unsupported-TFM policy).

> **Note:** the plan below splits into **two separate plans** — see *Plan task breakdown (two separate plans)* near the end for the concrete, ordered steps each should take (Plan 1 = unit lane, Plan 2 = UI lane).

## Spike results

### Spike #1 — DevFlow agent in the MAUI 11 sample (2026-11-13): ✅ PASS (gating question resolved)

**Setup applied during the spike (reverted afterward — re-apply in Plan 2):**
- `Microsoft.Maui.DevFlow.Agent` `0.1.0-preview.14.26555.2` added to the sample `.csproj` with `Condition="'$(Configuration)' == 'Debug'"`.
- `builder.AddMauiDevFlowAgent()` registered inside the existing `#if DEBUG` block in `MauiProgram.cs`.
- New `Platforms/MacCatalyst/Entitlements.Debug.plist` (sandbox ON + `network.client` + `network.server` — Option B) referenced via a `CodeSignEntitlements` `PropertyGroup` conditioned on maccatalyst + Debug.

**Verified (all on `net11.0-maccatalyst`, app launched via `open`):**

| Capability | Result | Notes |
|---|---|---|
| Build (net10-line agent ↔ MAUI 11) | ✅ | 0 errors; only pre-existing obsolete-API warnings. |
| Agent connect + broker | ✅ | Agent port 9223, broker 19223; `diagnose` healthy; `frameworkVersion: 11.0.0`. |
| Visual tree | ✅ | Full tree readable (`Window → AppShell → TabBar → pages → Buttons/Labels`), with `captureEpoch`/`registryGeneration` refresh semantics. |
| Navigation | ✅ | `ui navigate //Tabs/...` works; button tap pushes `Details` (global route) fine. |
| Assertions | ✅ | `ui assert Text "…"` passes and fails correctly; exit code 1 on failure (CI-friendly). |
| Popups (CT 17 `toolkit:Popup`) | ✅ | Full round trip: open `AdvancedPopup` → lifecycle label reads "State: Default, Prepared, Opened" → navigation parameter ("Text from navigation parameters.") rendered → closed via `IPopupService` with a result → follow-up alert appeared (flow completed) → dismissed. |
| Dialogs (Catalyst native NSAlert) | ⚠️ Partial | `ui alert detect` → `detected: false`, `ui alert dismiss "Okay"` → `dismissed: false` — DevFlow is blind to the native alert *as an alert*. **But** the alert's "Okay" button appears in the element registry as a native `DialogAction` element and `ui tap <id>` dismisses it reliably. Alert message body text is not queryable. |
| `FormattedText` spans | ❌ Invisible | Labels using `Label.FormattedText` (the `Details` page data labels) expose **no text** to the registry — `ui query --text "View Identifier"` → `[]`. UI assertions on those values are impossible as-is. |

**Operational caveats (scripting implications):**
- **Sequential execution required.** Rapid/parallel DevFlow CLI invocations trigger `Another DevFlow session is driving this app (MAUI CLI)`; recoverable by refreshing the tree (`ui tree`) and retrying.
- **Broker lease hiccup.** Hit once: `The DevFlow broker lease authority is temporarily unavailable.` (non-retryable per the error). Recovered with `maui devflow broker stop` + `start`; the running app's agent auto-re-registered after ~3s. → CI scripts need a broker-health check + retry wrapper.
- **Element IDs are epoch-scoped.** IDs from `ui query` go stale across navigation/state changes; refresh with `ui tree` (or re-query) after any error suggesting it.

**Verdict:** DevFlow is **viable for the UI lane on MAUI 11** (Catalyst). It covers the full value matrix (navigation, popups, dialogs, assertions). The dialog and `FormattedText` gaps are workable: dialogs via the `DialogAction` tap pattern, and `Details` data labels converted from `FormattedText` spans to plain `Text` labels (a sample-only change; the data values are plain strings there). XCUITest remains the fallback if the preview package churns.

**Sample-app adjustment needed for assertability (to be part of implementation, not the spike):** convert `Details.xaml` data labels from `Label.FormattedText` spans to `Text` bindings so their values are queryable/assertable.

### Spike #2 — xUnit host for the unit lane (2026-11-13): ✅ PASS (with a library decision point)

**Question:** which TFM can actually run a bare `testhost` under `dotnet test` on this Mac (and on GitHub runners)?

**Attempt 1 — `net11.0-maccatalyst` test project: ❌ dead end.**
- First tripped the Catalyst SDK's app-bundle validations: `MT7199` (scene manifest), then a required `ApplicationId`/bundle identifier.
- Suppressed those and it then crashed inside the SDK itself: `ProcessRuntimeLibraries` task throws `KeyNotFoundException` (and, with a `RuntimeIdentifier` added, fails outright). The MacCatalyst SDK assumes a real app; a bare testhost is not one.
- **Conclusion: a plain platform-TFM xunit project cannot be run via `dotnet test`.** (android TFM would need a device/emulator to host the testhost — not viable for fast local/CI runs.)

**Attempt 2 — plain `net11.0`: ✅ works.**
- **All three dependencies ship a plain `net11.0` group:** `Microsoft.Maui.Controls` 11.0.10 ✅, `CommunityToolkit.Maui` 17.0.0 ✅, `CommunityToolkit.Mvvm` 8.4.2 (via netstandard) ✅. So the MAUI core API surface (Shell, Page, Popup, etc.) is available without a platform.
- **The library code has no platform conditionals** (no `#if`/`Essentials`/`DeviceInfo`/`RuntimePlatform` anywhere) — it is pure C# over the MAUI core. So it compiles for plain `net11.0`.
- With `net11.0` added to the library's `TargetFrameworks`, a plain `net11.0` xunit project built and **`dotnet test` ran natively: `Passed! Failed: 0, Passed: 2`** (real tests: `NavigationParameters` null/round-trip contract).
- Only new diagnostics: the pre-existing CS0618 obsolete warnings, plus one `CA1416` (platform-attribute) warning on `UseMauiCommunityToolkit` — benign for the plain target, suppressible.

**Verdict:** the clean, CI-friendly unit host is **plain `net11.0`**. This is a **decision point**: adopting it means **adding a `net11.0` TFM to the shipped `Mvvm.Nucleus.Maui` package** (a new asset in the nupkg). That is a public-packaging change, so it needs the user's sign-off before it's treated as permanent. It also has a bonus: the pure-C# core becomes consumable/testable from plain .NET contexts.

**Working-tree state: REVERTED after the spike (finding preserved here — re-apply in Plan 1).** The spike added a plain `net11.0` TFM to the library `TargetFrameworks` and created `tests/Mvvm.Nucleus.Maui.UnitTests` (plain `net11.0`) with 2 passing `NavigationParameters` tests; both were reverted to keep the tree clean for planning. Re-apply during implementation of Plan 1.

### Spike #3 — Core-init + behavior-lifecycle probe (2026-11-13): ✅ PASS (seam approach confirmed)

**Question (open question 3):** can `NucleusMvvmCore` be initialized in the bare `net11.0` test host, and does `NucleusMvvmPageBehavior` route page lifecycle events to ViewModel callbacks — and what seam do the tests need?

**Approach proven:**
- **Seam = minimal `InternalsVisibleTo`.** Added `<InternalsVisibleTo Include="Mvvm.Nucleus.Maui.UnitTests" />` to the library csproj. Only two `internal` members are needed: `NucleusMvvmCore.Initialize(IServiceProvider)` and the `Shell` setter. No `MauiApp`/`CreateMauiApp` bootstrap is required — `NucleusMvvmCore` is constructed directly with a real `Application`, an `IViewFactory` stub, `NucleusMvvmOptions`, and `NullLogger<T>.Instance`.
- **`NucleusMvvmCore` init + Shell registration work in the bare host.** `core.Initialize(sp)` sets the static `Current`; `core.Shell = new Shell()` just hooks the `Shell.Navigating`/`Navigated` events (no `Window` needed). Verified `IsInitialized`, `Current`, and `Current.Shell` all resolve.
- **The behavior is fully drivable without a running app.** It subscribes to the **public** `Page.Appearing`/`Disappearing`/`NavigatedTo`/`NavigatedFrom`/`NavigatingFrom` events, and every handler reads from `NucleusMvvmCore.Current.NavigationParameters` + the `BindingContext` — **the event-args parameter is never read**. So the exact MAUI event-args instances don't matter for the callbacks.
- **Event-args construction gotcha (discovered & solved):** `NavigatedToEventArgs`/`NavigatedFromEventArgs`/`NavigatingFromEventArgs` (MAUI 11) all have **non-public** ctors `(Page, NavigationType)` and are raised only by MAUI internals; `Page.Appearing`/`Disappearing` are plain `EventHandler` (`EventArgs.Empty`). The probe raises the events through the `Page` event **backing-field delegate** and constructs the nav event-args via their non-public ctors through reflection. This is the reusable pattern for behavior-lifecycle tests.

**Tests written (all pass — `Failed: 0, Passed: 6`):**
| Test | Verifies |
|---|---|
| `Core_Initializes_AndRegistersShell_InBareTestHost` | Core init + Shell registration in the bare host. |
| `Behavior_DoubleNavigatedTo_OnlyFirstIsProcessed_ShellWorkaround` | The `_isNavigatedTo && !_isNavigatedFrom` guard: a second `NavigatedTo` (Shell's NavigationPage+Modal double-trigger) is dropped → 1 `OnNavigatedTo`, 0 `Refresh`. |
| `Behavior_RenavigationAfterNavigatedFrom_TriggersRefresh_ButNotInit` | To → From → To: 2 `OnNavigatedTo`, 1 `OnNavigatedFrom`, 1 `Refresh` (re-init is NOT re-run). |
| `Behavior_Appearing_RoutesToLifecycleAware_FirstAppearingOnlyOnce` | Appearing ×2 → 2 `OnAppearing`, exactly 1 `OnFirstAppearing`. |

**Verdict:** the unit lane reaches the **coupled core + behavior layer**, not just the pure contracts. The seam is a single `InternalsVisibleTo` line (no API surface change, no public seam extraction). Behavior-lifecycle tests are therefore viable and are a **high-value** addition (this is exactly the Shell double-trigger / Refresh-on-renavigation logic that historically regresses).

**Working-tree state: REVERTED after the spike (finding preserved here — re-apply in Plan 1).** The spike added `<InternalsVisibleTo Include="Mvvm.Nucleus.Maui.UnitTests" />` to the library csproj and `tests/Mvvm.Nucleus.Maui.UnitTests/CoreInitializationTests.cs` (4 tests) + fakes (`NoopViewFactory`, `NoopServiceProvider`, `RecordingViewModel`, `PageEvents` reflection raiser); 6 tests passed, warning-clean. All reverted to keep the tree clean for planning. Re-apply during implementation of Plan 1.

## Plan task breakdown (two separate plans)

The work splits into **two independent plans**, each sourced from this document. The spikes above de-risked both lanes; the spike code was **reverted** (tree is clean at HEAD), so each plan **re-applies** its share of it during implementation. The two plans do not block each other and share the CI scaffold plus the two adopted decisions.

**Adopted decisions both plans rely on:**
- Plain `net11.0` test TFM — `decision-net11-test-tfm.md` (adds a plain `net11.0` asset to the shipped package).
- Minimal `InternalsVisibleTo` test seam — `decision-test-seam-internalsvisiblet.md` (exposes only `NucleusMvvmCore.Initialize(IServiceProvider)` + the `Shell` setter).

### Plan 1 — Unit tests (fast lane)
**Purpose:** pin the pure-C# contracts and the behavior-lifecycle routing (the library's core value) without a running app.
**Builds on:** spike #2 (plain `net11.0` host), spike #3 (`InternalsVisibleTo` seam; behavior is drivable via public `Page` events; nav event-args raised via reflection), the value-matrix unit rows.
**Implementation steps (in order):**
1. Add plain `net11.0` to the library `TargetFrameworks` (spike #2) — `Mvvm.Nucleus.Maui/Mvvm.Nucleus.Maui.csproj`.
2. Add `<InternalsVisibleTo Include="Mvvm.Nucleus.Maui.UnitTests" />` (spike #3) — same csproj.
3. Create `tests/Mvvm.Nucleus.Maui.UnitTests` (plain `net11.0`, xUnit; `.csproj` + `.sln` entry).
4. Rebuild the spike baseline as the starting point: spike #2's 2 `NavigationParameters` tests + spike #3's 4 behavior tests (6 passing).
5. Add the remaining unit coverage: `Extensions.DictionaryExtensions` edge cases (wrong-type/missing-key/fallback), `NucleusMvvmOptions` defaults, `CompatibilityExtensions` (`IsBackNavigation`/`GetNavigationMode`), then `NavigationService` debounce (`SetIsNavigatingOrIgnoreRequest`) handling the shared `static _isNavigatingLastTriggeredUtc` state, and the full behavior-lifecycle matrix (`IPrepare(Async)`→`IRefresh(Async)`→`IInitializable`→`INavigatedAware`/`IDestructible`, appear/disappear, double-`NavigatedTo`, `IConfirmNavigation`).
6. Keep the reusable harness from spike #3: the `PageEvents` reflection raiser (non-public event-args ctors → backing-field delegate) + fakes (`NoopViewFactory`, `NoopServiceProvider`, `RecordingViewModel`).
7. CI: a macOS runner job — `dotnet test tests/Mvvm.Nucleus.Maui.UnitTests` (runs natively on plain `net11.0`; no device needed).

**Files touched:** library csproj; new `tests/Mvvm.Nucleus.Maui.UnitTests/…`; `.sln` (test project); GitHub Actions workflow.
**Notes:** suppress the pre-existing CS0618 + the one `CA1416` on `UseMauiCommunityToolkit` for a warning-clean build; debounce tests must isolate the shared `static` state (or extract the logic — the seam-decision doc notes the extraction alternative).

### Plan 2 — UI tests (DevFlow integration lane)
**Purpose:** regression-test the exact integration surface that has historically broken (Shell navigation, CT popup round-trips, dialogs, parameter passing) against the running sample app.
**Builds on:** spike #1 (DevFlow drives the MAUI 11 sample end-to-end; dialog + `FormattedText` caveats), the value-matrix UI rows.
**Implementation steps (in order):**
1. Re-onboard DevFlow (spike #1) in `Mvvm.Nucleus.Maui.Sample/…`: Debug-conditioned `Microsoft.Maui.DevFlow.Agent` `0.1.0-preview.14.26555.2` PackageReference + `#if DEBUG builder.AddMauiDevFlowAgent()` in `MauiProgram.cs` + `Platforms/MacCatalyst/Entitlements.Debug.plist` (sandbox ON + `network.client` + `network.server`) wired via a `CodeSignEntitlements` PropertyGroup.
2. Sample-only assertability fix: convert `Details.xaml` data labels from `Label.FormattedText` spans to plain `Text` bindings so their values are queryable (spike #1 `FormattedText` finding).
3. Author the UI suite as **data/scripts** under `tests/ui/` (CLI / `batch` JSONL), one script per value-matrix flow: navigation (push/pop/modal/close-all/tabs/params/query), all 4 popups (open/close/result/singleton/CT compat/V1 compat), dialogs (alert/confirm/prompt/action sheet via the `DialogAction`-tap pattern), debounce, `IConfirmNavigation`.
4. Driver robustness wrapper: sequential-only execution, broker-health check + retry (broker lease hiccup), refresh tree (`ui tree`) on epoch-scoped ID errors.
5. CI: a macOS runner job — build the sample (Catalyst) → `open` the `.app` → run the `tests/ui/` scripts with `maui devflow --ci`; (iOS simulator via `simctl` is the alternative host).

**Files touched:** sample csproj, `MauiProgram.cs`, Catalyst entitlements, `Details.xaml`; new `tests/ui/…`; GitHub Actions workflow.
**Notes:** DevFlow agent is **debug-only and sample-only** (never Release, never the library); XCUITest remains the fallback if the preview package churns (spike #1).

### Shared: CI scaffold (both plans)
- GitHub Actions (repo is public, no CI today). A **macOS runner** hosts both the plain-`net11.0` unit testhost and the Catalyst/simulator UI runs (ubuntu cannot host Catalyst).
- Keep **Windows excluded** (existing unsupported-TFM policy). An optional android build job can live on ubuntu if desired.
- Structure both jobs now; enable the UI job once the sample + agent are stable in CI.

### Spike work log (code added during research → reverted; re-apply per plan)
| Spike | Code change (reverted) | Re-applied by |
|---|---|---|
| #1 | Sample Debug DevFlow: PackageReference + `#if DEBUG` registration + Catalyst entitlements | Plan 2, step 1 |
| #2 | Library `net11.0` TFM; `tests/…UnitTests` project (2 `NavigationParameters` tests) | Plan 1, steps 1 & 3–4 |
| #3 | Library `InternalsVisibleTo`; `CoreInitializationTests.cs` (4 tests) + fakes + `PageEvents` raiser | Plan 1, steps 2, 4 & 6 |

## Out of scope (this research phase)
- **Full implementation** — the complete unit suite (dictionary extensions, options defaults, debounce/popup logic), the DevFlow UI scripts under `tests/ui/`, and the CI workflow. The spikes have de-risked all three lanes; the remaining work is execution, not investigation.
- **Decisions recorded:** the `net11.0` test TFM (`decision-net11-test-tfm.md`) and the minimal `InternalsVisibleTo` test seam (`decision-test-seam-internalsvisiblet.md`) are both **adopted**. The UI-automation choice (DevFlow primary, XCUITest fallback) is settled by spike #1's PASS but is not yet a formal decision record — Plan 2 should capture it via the decision-record workflow.
- **Sample-ViewModel unit tests** (open question 6) — thin wrappers around the services; low value, expected to be skipped.
- Android-first or Windows UI testing (secondary/unsupported targets).
