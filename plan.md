# Plan: Unit Tests (fast lane) for Mvvm.Nucleus.Maui

**Status:** Draft
**Author:** Erik
**Created:** 2026-11-13

## Summary

Introduce a plain-`net11.0` xUnit test project (`tests/Mvvm.Nucleus.Maui.UnitTests`) that pins the library's pure-C# public contracts (Prism compatibility, dictionary extensions, options defaults) and — using the adopted minimal `InternalsVisibleTo` seam — drives the coupled core + behavior layer (lifecycle-callback routing, the Shell double-`NavigatedTo` workaround, navigation debounce, confirm-navigation) without a running app. Both gating spikes passed and their code was reverted; this plan re-applies their findings step by step. The UI (DevFlow) lane is explicitly out of scope and will get its own plan.

## Motivation

- The library is public (NuGet 0.8.0, third-party consumers) and has **zero automated tests**. Its value is behavioral contracts — the Prism-migration null semantics, lifecycle callback ordering, double-navigation protection — that regress silently.
- Repo history proves the risk: CommunityToolkit popup rewrites forced Nucleus reworks (0.6.0, and the CT 13→17 jump during the .NET 11 upgrade). Every major jump touches the same surface.
- The pure-C# surface and the event-routing logic are testable **without** Shell/`MainThread`/a running app (spikes #2 and #3), giving fast, cheap feedback before the UI lane (separate plan) covers the integration surface.
- The suite is structured with future CI in mind: plain `dotnet test` with only the .NET SDK — no MAUI workloads, no device — so it can move to a pipeline later without changes. (CI workflow itself is out of scope for this plan.)

## Goals

- A green, warning-clean unit test suite running natively under `dotnet test` on this Mac (plain `net11.0`, .NET SDK 11.0.100).
- Coverage of:
  1. `Compatibility.NavigationParameters` null-indexer contract + ctors.
  2. `CompatibilityExtensions.IsBackNavigation` / `GetNavigationMode`.
  3. `Extensions.DictionaryExtensions` (`GetValueOrDefault<T>`, `GetStructOrDefault<T>`, `GetShellNavigationSource`) edge cases.
  4. `NucleusMvvmOptions` documented defaults.
  5. `NucleusMvvmCore` initialization seam behavior (init, Shell registration, parameter fallbacks).
  6. `ViewFactory.ConfigureView` creation lane: ViewModel binding, `IPrepare(Async)` invocation, behavior attachment (idempotent), `WrapInNavigationPage`, `NavigatingPresentationMode`.
  7. `NucleusMvvmPageBehavior` full lifecycle matrix: first-vs-renavigation routing (`IInitializable(Async)` vs `IRefresh(Async)` branches), Shell double-`NavigatedTo` workaround, `IPageLifecycleAware` first-appearing-once, `INavigatedAware` to/from/navigating-from, `IApplicationLifecycleAware` subscribe/unsubscribe, `IDestructible` + `DestroyAfterNavigatedFrom` cleanup.
  8. `NavigationService.SetIsNavigatingOrIgnoreRequest` debounce matrix (bypass param, in-progress, time window, options-off) — including deterministic handling of the shared `static _isNavigatingLastTriggeredUtc`.
  9. `NavigationService` confirm-navigation (`IConfirmNavigation(Async)`) via a reflected `ShellNavigatingEventArgs`, and `GetQueryParameterDictionary` route parsing.
- Re-apply the spike baselines (spike #2: 2 `NavigationParameters` tests; spike #3: 4 core/behavior tests) as the starting green set.

## Non-Goals

- **UI/DevFlow testing** — separate plan (its own `plan-*.md`), per the research task breakdown.
- `PopupService` / `PageDialogService` / `NucleusWindowCreator` / `NucleusMvvmPopupBehavior` — thin wrappers over `Shell`/`Popup`/`MainThread`; their contracts belong to the UI lane (research value matrix: "no meaningful unit value").
- Sample-app ViewModel unit tests — thin wrappers over the services under test (research open question 6: skip).
- Obsolete surface beyond what the behavior matrix already exercises (`NucleusViewModel`/`INucleusViewModel` base-class busy-flag logic) — optional at most, not a goal.
- **CI workflow files** — no GitHub Actions yml in this plan (skipped at Erik's request). The suite is still structured for future CI: plain `dotnet test` with only the SDK, no workloads or device, so a job can be added later as-is.
- No version bump, no package publish (the new `net11.0` asset simply joins the next release), no changes to `package.sh`.
- No Windows/Android-specific unit runs (Windows TFM stays unsupported here; plain `net11.0` is the host).
- No new public API (seam is `InternalsVisibleTo` only, per adopted decision).

## Technical Design

### Host and seams (both adopted decisions, already signed off)

- **TFM:** add plain `net11.0` to the library `TargetFrameworks` (`decision-net11-test-tfm.md`). All three dependencies ship plain `net11.0` assets; library code has zero platform conditionals. Spike #2 proved `dotnet test` runs natively (`Passed! Failed: 0, Passed: 2`).
- **Seam:** `<InternalsVisibleTo Include="Mvvm.Nucleus.Maui.UnitTests" />` (`decision-test-seam-internalsvisiblet.md`). Only two internals are needed: `NucleusMvvmCore.Initialize(IServiceProvider)` and the `Shell` setter. No `MauiApp` bootstrap.
- **Warning hygiene:** the plain target surfaces one new `CA1416` on `UseMauiCommunityToolkit` (benign) — suppress **conditionally for `net11.0` only** (csproj `NoWarn` conditioned on `$(TargetFramework) == 'net11.0'` or a scoped `#pragma`); keep existing platform builds unchanged. Existing `CS0618` (obsolete `IInitializable*` usage) should stay suppressed the same way it is today — do not widen suppressions globally.

### Test project

- `tests/Mvvm.Nucleus.Maui.UnitTests/` — `Microsoft.NET.Sdk`, `TargetFramework net11.0`, `IsPackable false`, `Nullable enable`, `ImplicitUsings enable`.
- Packages (inline versions, repo convention; latest stable at implementation time): `xunit` **2.9.3**, `xunit.runner.visualstudio` 2.8.x (matching), `Microsoft.NET.Test.Sdk` **18.12.0**. (No xunit v3 line on nuget.org in this environment — stay on the 2.9.x line spike #2 validated.)
- `ProjectReference` to `..\..\Mvvm.Nucleus.Maui\Mvvm.Nucleus.Maui.csproj` (flows `Microsoft.Maui.Controls` / `CommunityToolkit.Maui` / `CommunityToolkit.Mvvm` transitively; `Microsoft.Extensions.DependencyInjection` needed for real `ServiceCollection` fakes — add the Abstractions package explicitly if not flowed).
- **Disable test parallelization** (`[assembly: CollectionBehavior(DisableTestParallelization = true)]` in `AssemblyInfo.cs`). Rationale: two process-wide statics — `NucleusMvvmCore._current` (shared init) and `NavigationService._isNavigatingLastTriggeredUtc` (debounce timing). The suite is small (< ~70 tests, sub-second each); serialization is the deterministic choice.
- Added to `Mvvm.Nucleus.Maui.sln`.

### Shared harness (`TestHelpers/`)

Re-apply spike #3's proven pattern (reverted from the tree; rebuild from the research notes):

- **`NucleusTestHarness`** — per fixture: `new Application()`, a `NoopViewFactory` (or real `ViewFactory` where the test needs it), `NucleusMvvmOptions` (fresh instance per test where the test mutates it), `NullLogger<T>.Instance` (or `RecordingLogger` where a log line is the assertion), then `core.Initialize(serviceProvider)` + `core.Shell = new Shell()`. Because `_current` is process-wide, exactly one init per process is fine (idempotent across tests); tests that assert "throws before registration" run in a dedicated class that never inits.
- **`PageEvents`** — raiser for `Page` lifecycle events through the event **backing-field delegate**: `Appearing`/`Disappearing` use `EventArgs.Empty`; `NavigatedTo`/`NavigatedFrom`/`NavigatingFrom` have **non-public** `(Page, NavigationType)` ctors (MAUI 11) — construct via reflection, then invoke the backing-field delegate. The behavior never reads the event-args, so identity doesn't matter.
- **`ShellNavigatingEventArgsBuilder`** — same reflection pattern for `Shell`'s `Navigating` event: construct `ShellNavigatingEventArgs` via its non-public ctor `(Shell current, Shell? target, ShellNavigationSource source)` and invoke the `Navigating` backing-field delegate. Used by the confirm-navigation tests.
- **`NavigationServiceStateReset`** — deterministic isolation for the debounce tests: via reflection set `NavigationService._isNavigatingLastTriggeredUtc` (static) back to `default` and the instance `_isNavigating` back to `false` before each test. This removes any need for sleep-based timing and makes the matrix order-independent (defense in depth alongside disabled parallelization).
- **`RecordingViewModel`** — implements `INavigatedAware`, `IRefresh`, `IRefreshAsync`, `IInitializable`, `IInitializableAsync`, `IPageLifecycleAware`, `IApplicationLifecycleAware`, `IDestructible`, `IConfirmNavigation`, `IConfirmNavigationAsync`; records an ordered `List<string>` of callback names + the parameters received; async callbacks complete a `TaskCompletionSource` so tests can await `async void` work routed through `RunTaskInVoidAndTrackException`.
- **`RecordingLogger<T>`** — captures `LogLevel` + message for assertions (e.g. "Shell Navigation Canceled." proves the `e.Cancel()` path without relying on `ShellNavigatingEventArgs` exposing cancellation state).
- **`NoopViewFactory` / `NoopServiceProvider`** — minimal `IViewFactory` and `IServiceProvider` stubs for core-only fixtures; use a real `ServiceCollection` where `ViewFactory` tests need `ActivatorUtilities.CreateInstance` resolution.

### Test matrix (concrete assertions to pin)

**`Compatibility/NavigationParametersTests`** — the Prism contract that justifies the class's existence:
- `this[missingKey]` returns `null` (does **not** throw `KeyNotFoundException`; must hold when accessed through a `NavigationParameters`-typed reference so the `new` indexer wins over `Dictionary.this[]`).
- `this[presentKey]` returns the stored value; `TryGetValue(missing)` → `false`.
- Ctor from `IDictionary<string, object>` and from `IEnumerable<KeyValuePair<string, object>>` copies entries.
- Type checks: `is INavigationParameters`, `is Dictionary<string, object>`.

**`Compatibility/CompatibilityExtensionsTests`:**
- `IsBackNavigation()` true iff stored `CompatibilityNavigationParameters.NavigationMode == NavigationMode.Back`; false for `New`; false (→ default `New`) when the key is missing.
- `GetNavigationMode()` returns stored value; defaults to `NavigationMode.New` when missing or wrong type.

**`Extensions/DictionaryExtensionsTests`:**
- `GetValueOrDefault<T>`: present + correct type → value; missing → explicit fallback; missing + no fallback → `null`; wrong type → fallback.
- `GetStructOrDefault<T>`: present + correct type → value; missing → fallback; wrong type → fallback.
- `GetShellNavigationSource()`: stored source returned; missing → `ShellNavigationSource.Unknown`; wrong type → `Unknown`.

**`Configuration/NucleusMvvmOptionsTests`:**
- Table-driven assertion of every documented default: `AddQueryParametersToDictionary == true`, `AlwaysDisableNavigationAnimation == false`, `IgnoreNavigationWhenInProgress == true`, `IgnoreNavigationWithinMilliseconds == 250`, `UseDeconstructPageOnDestroy == true`, `UseDeconstructPopupOnDestroy == true`, `UseShellNavigationQueryParameters == true`, `UseConfirmNavigationForAllNavigationRequests == false`, `UseCommunityToolkitPopupServiceCompatibility == true`, `UseAlternativePopupOpenedAndClosedEvents == true`, `CommunityToolkitV1PopupServicePopupOptions == null`, `ViewMappings`/`PopupMappings` empty.

**`Core/NucleusMvvmCoreTests`:**
- `Initialize` sets `IsInitialized`, `Current` resolves, `ServiceProvider` is the passed provider.
- `Shell` getter throws `InvalidOperationException` before registration (dedicated no-init class); after `Shell = new Shell()` it resolves.
- `NavigationParameters` setter with `null` falls back to an empty (non-null) dictionary.

**`Factories/ViewFactoryTests`** (harness + real `ServiceCollection`):
- Page with registered `ViewMapping` and no `BindingContext` → `BindingContext` is an instance of the mapped ViewModel type (with that type registered in the provider).
- `Page` → exactly one `NucleusMvvmPageBehavior` attached with `Page` set; calling `ConfigureView` twice does not add a second.
- `IPrepare` implemented on both element and binding context → both `Prepare` calls receive `core.NavigationParameters` (set a sentinel key and assert).
- `IPrepareAsync` → `PrepareAsync` completes (await the `TaskCompletionSource`).
- `NucleusNavigationParameters.WrapInNavigationPage == true` → returned element is a `NavigationPage` wrapping the page.
- `NucleusNavigationParameters.NavigatingPresentationMode` present → `Shell.GetPresentationMode(page)` equals the requested `PresentationMode` (applied to the wrapper too when wrapping).
- Non-`Page` element with a `Page` parent → behavior attached to the page with `Element` set (verify `Element.Parent` settable in the bare host first; if not, drop this one test — the creation lane is still covered).

**`Behaviors/NucleusMvvmPageBehaviorTests`** (the heart of the library — pin callback ordering):
- First `NavigatedTo` with `IInitializable` → `Init` called, `IRefresh.Refresh` **not** called, `INavigatedAware.OnNavigatedTo` called.
- Second `NavigatedTo` without `NavigatedFrom` in between (Shell NavigationPage+Modal double-trigger) → **dropped**: no second `OnNavigatedTo`, no `Refresh` (spike #3 test).
- To → From → To → exactly 2 `OnNavigatedTo`, 1 `OnNavigatedFrom`, 1 `Refresh`, 1 `Init` (spike #3 test).
- `IInitializable` **without** `IRefresh`, on renavigation → `IInitializable.Refresh` called (the `else if` branch).
- `IInitializableAsync` **without** `IRefreshAsync`, on renavigation → `RefreshAsync` called (await via TCS).
- `IRefreshAsync`-only VM, on renavigation → `RefreshAsync` called; init never implied.
- `Appearing` ×2 → 2 `OnAppearing`, exactly 1 `OnFirstAppearing` (spike #3 test); `Disappearing` → `OnDisappearing`.
- `NavigatingFrom` → `OnNavigatingFrom`.
- `NavigatedFrom` → `OnNavigatedFrom`, and after it `core.AppResumed`/`core.AppStopped` no longer route to `OnResume`/`OnSleep` (unsubscribed).
- Before `NavigatedFrom`: after the first `NavigatedTo`, `core.AppResumed` → `OnResume`; `core.AppStopped` → `OnSleep`.
- `DestroyAfterNavigatedFrom = true` + `IDestructible` → `Destroy()` called on `NavigatedFrom`; with `UseDeconstructPageOnDestroy == true` the behavior is removed from `Page.Behaviors` and `BindingContext` cleared; flag `false` → no `Destroy`.
- The exact `IDictionary<string, object>` set on `core.NavigationParameters` is the one received by each callback (sentinel key).

**`Services/NavigationServiceDebounceTests`** (via `TestNavigationService : NavigationService` exposing the `protected virtual SetIsNavigatingOrIgnoreRequest`):
- Bypass: `navigationParameters[{DoNotIgnoreThisNavigationRequest} = true]` → allowed even when a request is in progress.
- In-progress (defaults): first request allowed, immediate second → ignored (assert via return value + `IsNavigatingChanged` event sequence).
- Time window (`IgnoreNavigationWhenInProgress = false`, window e.g. 100 ms): first allowed, immediate second → ignored as too-soon (assert `RecordingLogger` warning mentions the window).
- Options off (`IgnoreNavigationWhenInProgress = false`, window 0): consecutive requests all allowed.
- **State isolation:** `NavigationServiceStateReset` before each test (see harness) — no sleeps, deterministic ordering.

**`Services/NavigationServiceConfirmNavigationTests`** (`ShellNavigatingEventArgsBuilder` + `RecordingViewModel` + `RecordingLogger`):
- `Shell` with a `CurrentPage` whose `BindingContext` is an `IConfirmNavigation` denying `NavigationDirection.Back` for a `Pop` source → navigation canceled (assert via the "Shell Navigation Canceled." log line and/or `IsNavigating == false`); the fake records it received `Back`.
- Allowing `IConfirmNavigation` → no cancel log.
- Source mapping: `Pop`/`PopToRoot` → `Back`, `Push` → `Forwards` (assert on the direction the fake receives).
- Guard: source other than Push/Pop/PopToRoot with `UseConfirmNavigationForAllNavigationRequests == false` → `CanNavigate` never called; `== true` → it is called.
- Async branch: `IConfirmNavigationAsync` denying → canceled, deferral completed. **If `GetDeferral()`/`Cancel()` prove non-functional in the bare host**, keep the sync-branch tests, mark the async one `Skip` with a comment pointing at the UI lane (the sample's `Details` back-with-confirm flow covers it), and note it in the devlog.

**`Services/NavigationServiceRouteParsingTests`:**
- `GetQueryParameterDictionary` (reflected private call): `"Details?Id=42&Name=Foo"` → `{Id: "42", Name: "Foo"}`; malformed parts (no `=`) skipped; no query → empty; unparseable route → empty. (Pure string parsing — valuable and unreachable any other way without Shell/`MainThread`.)

**Optional (low priority, only if cheap):** `NucleusViewModel` double-init guard and `IsBusy` aggregation — it is `[Obsolete]` and thin; do not expand scope for it.

## Implementation Approach

Order matters: host first, then green baseline, then coverage outward.

1. **Library csproj (`Mvvm.Nucleus.Maui/Mvvm.Nucleus.Maui.csproj`)** — re-apply spike #2/#3:
   - Add `net11.0` to `TargetFrameworks`.
   - Add `<InternalsVisibleTo Include="Mvvm.Nucleus.Maui.UnitTests" />`.
   - Suppress the plain-target `CA1416` (`UseMauiCommunityToolkit`) conditionally for `net11.0` only; verify no *other* new warnings on the plain TFM.
   - Verify: `dotnet build Mvvm.Nucleus.Maui -c Release` is warning-clean for `net11.0` (and still builds the platform TFMs on this Mac: maccatalyst at minimum).
2. **Test project skeleton** — create `tests/Mvvm.Nucleus.Maui.UnitTests/`:
   - `Mvvm.Nucleus.Maui.UnitTests.csproj` (net11.0, `xunit` 2.9.3, `xunit.runner.visualstudio` 2.8.x, `Microsoft.NET.Test.Sdk` 18.12.0, `ProjectReference` to the library, `IsPackable false`).
   - `AssemblyInfo.cs` with `CollectionBehavior(DisableTestParallelization = true)`.
   - `TestHelpers/`: `NucleusTestHarness`, `PageEvents` (reflection raiser), `ShellNavigatingEventArgsBuilder`, `NavigationServiceStateReset`, `RecordingViewModel`, `RecordingLogger<T>`, `NoopViewFactory`, `NoopServiceProvider`.
   - `dotnet sln Mvvm.Nucleus.Maui.sln add tests/Mvvm.Nucleus.Maui.UnitTests/Mvvm.Nucleus.Maui.UnitTests.csproj`.
3. **Baseline green (re-apply spikes)** — `Compatibility/NavigationParametersTests` (spike #2's 2 tests, expanded per the matrix) + `Core/NucleusMvvmCoreTests` + the 4 spike #3 behavior tests. Run `dotnet test` → all green before adding more.
4. **Pure-contract tests** (no harness needed): `CompatibilityExtensionsTests`, `DictionaryExtensionsTests`, `NucleusMvvmOptionsTests`, `NavigationServiceRouteParsingTests`.
5. **Creation-lane tests**: `ViewFactoryTests`.
6. **Behavior matrix**: full `NucleusMvvmPageBehaviorTests` (superset of the spike baseline).
7. **Service lane**: `NavigationServiceDebounceTests`, `NavigationServiceConfirmNavigationTests` (with the documented fallback for the async/deferral branch if the bare host can't support it).
8. **Final verification**:
   - `dotnet test tests/Mvvm.Nucleus.Maui.UnitTests -c Release` → 100% pass, no warnings.
   - `dotnet build` the library for the platform TFMs (maccatalyst, ios, android on this Mac) to prove the new TFM didn't disturb them.
   - Confirm public API surface unchanged apart from the new package asset: `dotnet pack` the library and diff the nupkg folder list against `Mvvm.Nucleus.Maui.0.8.0.nupkg` (expect the added `lib/net11.0/` entry and nothing else).
   - Re-run `dotnet test` twice back-to-back to check for order/state flakiness (the static-state traps).

**Files touched:**
- `Mvvm.Nucleus.Maui/Mvvm.Nucleus.Maui.csproj` (3 small additions).
- `tests/Mvvm.Nucleus.Maui.UnitTests/**` (new project: csproj, AssemblyInfo, ~8 test classes, ~8 helpers).
- `Mvvm.Nucleus.Maui.sln` (one project entry).

## Alternatives Considered

- **Refactoring the debounce logic into a pure, separately testable unit** (the extraction alternative noted in `research-testingOptions.md` and the seam decision doc) — rejected: it would change library code for testability; the adopted `InternalsVisibleTo` seam + a test-assembly probe subclass (`TestNavigationService`) reach `protected virtual SetIsNavigatingOrIgnoreRequest` with zero library change. The shared `static` state is handled test-side (`NavigationServiceStateReset`), keeping the production code untouched.
- **Bootstrapping a `MauiApp` (`CreateMauiApp`) in tests** — rejected (seam decision doc, Option 2): pulls platform DI into a bare host, exactly the anti-pattern the MAUI unit-testing guidance warns against.
- **Sleep-based timing isolation for debounce tests** — rejected in favor of the reflection state reset: deterministic, no CI-time flakiness, suite stays fast. (Disabled parallelization is kept anyway, because `NucleusMvvmCore._current` is also process-wide.)
- **Platform-TFM test project (`net11.0-maccatalyst`)** — dead end proven by spike #2 (Catalyst SDK app-bundle pipeline crashes on a bare testhost); android would need a device. Plain `net11.0` is the only viable host (adopted decision).
- **xunit v3** — not on nuget.org in this environment (latest `xunit` is 2.9.3); stay on the validated 2.9.x line.
- **Testing sample ViewModels / popup-dialog-window services** — skipped per research value matrix (thin wrappers; their contracts are the UI lane's job, planned separately).

## Open Notes for Implementation

- `ShellNavigatingEventArgs` members (`CanCancel`, `Cancel()`, `GetDeferral()`) must be smoke-tested in the bare host during Step 3/7; the confirm-navigation async branch has a documented skip-fallback.
- `Element.Parent` settable-ness in the bare host must be verified for the `ViewFactory` non-page test; drop that single test if it isn't, don't refactor for it.
- The exact `xunit.runner.visualstudio` version should be the latest 2.8.x compatible with `xunit` 2.9.3 (`dotnet add package` will resolve it).
