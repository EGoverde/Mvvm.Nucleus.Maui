# Plan: UI Tests (DevFlow integration lane) for Mvvm.Nucleus.Maui

**Status:** Draft
**Author:** Erik
**Created:** 2026-11-13

## Summary

Onboard the DevFlow agent into the sample app (sample-only, Debug-only, re-applying spike #1 from `research-testingOptions.md`), convert the sample's `Details.xaml` data labels from `FormattedText` to plain `Text` so they are assertable, and author a script-driven UI regression suite under `tests/ui/` — one bash script per flow over navigation, all four popup flavors, native dialogs (via the spike-verified `DialogAction`-tap pattern), double-navigation debounce, and confirm-navigation — driven against the running Mac Catalyst app with `maui devflow ui … --ci --json`. The unit lane (`plan.md`) is already implemented (65 green tests, devlog-002); this is the remaining lane. CI workflow files are out of scope (matching `plan.md`), but every script is CI-ready.

## Motivation

- The library's biggest proven regression surface is integration, not logic: CommunityToolkit popup rewrites forced Nucleus reworks (0.6.0, and the CT 13→17 jump during the .NET 11 upgrade), and the Shell double-`NavigatedTo` workaround exists for a MAUI quirk. Unit tests (done) pin the contracts and lifecycle routing; only a running-app suite pins the navigation/popup/dialog integration end-to-end.
- The sample app is already a purpose-built manual test bench (3 tabs + global `Details` route + 4 popup flavors + 5 dialog triggers). Spike #1 proved DevFlow can drive all of it on MAUI 11/Catalyst, including a full CT 17 popup round-trip.
- The suite catches exactly what has historically broken on every MAUI/CT major jump, and runs locally today with zero additional infrastructure.
- Scripts run with `--ci --json` and a sequential/broker-safe driver, so a future CI job on a macOS runner is a drop-in (per the shared CI scaffold in the research; no workflow file in this plan, at Erik's request — consistent with `plan.md`).

## Goals

- DevFlow agent embedded in the sample app, **Debug-only and sample-only** (re-applied from spike #1), verified healthy via `maui devflow diagnose` on `net11.0-maccatalyst`.
- `Details.xaml` data labels assertable via the visual tree (plain `Text`, spike #1 `FormattedText` finding).
- A green, deterministic UI suite under `tests/ui/` (one script per flow), covering the value-matrix UI rows:
  1. Navigation: by type, by global route, modal, modal + `NavigationPage` stack, parameters (via prompt), query-string, background-thread, double-trigger debounce, tab switch.
  2. Popups: simple (no VM), advanced (lifecycle state + parameter + `IPopupService` close with result), singleton (state persists across reopen), CommunityToolkit `IPopupService` compat, CT V1 compat (`LegacyPopup`), background-thread.
  3. Dialogs: alert, alert-from-background-thread, confirm, prompt, action sheet — all dismissed/asserted via the `DialogAction`-tap pattern.
  4. `Details` confirm-navigation: `IConfirmNavigationAsync` deny (back canceled) and allow (back proceeds).
  5. `Details` double-back (`Navigate Backwards (twice)` → `../..`).
- Driver robustness wrapper (shell): sequential execution, broker health/recovery, tree-refresh-on-stale-ID retry — absorbing the three operational caveats found in spike #1.
- Structured for future CI: every command runs with `--ci --json`; a single `run-all.sh` entry point; documented prerequisites (no workflow files authored).
- The library is untouched (no changes to `Mvvm.Nucleus.Maui`); the sample's Release build stays free of DevFlow.

## Non-Goals

- **CI workflow files** — no GitHub Actions yml in this plan (skipped at Erik's request, consistent with `plan.md` step 8 / research "structure now, enable when ready"). `tests/ui/README.md` documents the future macOS-runner job instead.
- **Unit lane** — implemented (`plan.md` / devlog-002); this plan must not regress it (verification step re-runs it).
- **Library changes** — no code, API, or packaging changes in `Mvvm.Nucleus.Maui`.
- **Android / iOS-simulator / Windows as hosts** — Catalyst is the validated host (spike #1); the iOS simulator is documented as the CI alternative, not built; Windows stays excluded per repo policy.
- **XCUITest** — fallback only per `decision-ui-automation-devflow.md`; not implemented here.
- **Screenshot/visual regression** — assertions are tree/text/property-based (spike-validated); no image comparison.
- **Sample-ViewModel unit tests** — skipped per research open question 6 (thin wrappers; covered by these integration flows).
- **Cascading popup** (`AdvancedPopup` → "Open another popup") — not in the value matrix; optional add-on after the suite is green.

## Technical Design

### Current state (grounded, 2026-11-13)

- Unit lane: `tests/Mvvm.Nucleus.Maui.UnitTests` (65 tests, green, committed — devlog-002). Library already carries the plain `net11.0` TFM + `InternalsVisibleTo`.
- Sample app: `net11.0-android/ios/maccatalyst` (+windows on Windows hosts), `Microsoft.Maui.Controls` 11.0.10, CT.Maui 17.0.0 via the library, `ApplicationId com.companyname.Mvvm.Nucleus.Maui.Sample`, Catalyst minimum 17.0 (`decision-catalyst-os-minimum.md`).
- DevFlow CLI `0.1.0-preview.14.26555.2` installed; MCP configured; **sample not onboarded** (no package ref, no `AddMauiDevFlowAgent()`).
- Routes (spike-verified navigable): `//Tabs/NavigationTab`, `//Tabs/PopupTab`, `//Tabs/DialogTab`; global route `Details`.
- Existing `Platforms/MacCatalyst/Entitlements.plist`: sandbox ON + `network.client`.

### 1. DevFlow onboarding (re-apply spike #1; sample-only, Debug-only)

- `Mvvm.Nucleus.Maui.Sample.csproj`:
  ```xml
  <PackageReference Include="Microsoft.Maui.DevFlow.Agent" Version="0.1.0-preview.14.26555.2" Condition="'$(Configuration)' == 'Debug'" />
  ```
- `MauiProgram.cs` — inside the existing `#if DEBUG` block (next to `builder.Logging.AddDebug()`):
  ```csharp
  builder.AddMauiDevFlowAgent();
  ```
- New `Mvvm.Nucleus.Maui.Sample/Platforms/MacCatalyst/Entitlements.Debug.plist` — the existing entitlements (sandbox ON + `network.client`) plus `com.apple.security.network.server` (required for the in-app agent HTTP server), wired in the sample csproj:
  ```xml
  <PropertyGroup Condition="'$(TargetFramework)' == 'net11.0-maccatalyst' AND '$(Configuration)' == 'Debug'">
    <CodeSignEntitlements>Platforms/MacCatalyst/Entitlements.Debug.plist</CodeSignEntitlements>
  </PropertyGroup>
  ```
- **Verify (spike #1 exit criteria):** `dotnet build -c Debug -f net11.0-maccatalyst` → 0 errors; launch the bundle; `maui devflow diagnose` healthy (agent 9223, broker 19223); `maui devflow ui tree` shows `Window → AppShell → TabBar → …`.

### 2. `Details.xaml` assertability fix (sample-only)

Convert the five `Label.FormattedText` labels to plain `Text` + `StringFormat` (spike #1: `FormattedText` spans expose **no text** to the registry; the values are plain strings here). Exact replacements:

| Current (FormattedString spans) | New |
|---|---|
| `"View Identifier: "` + `{Binding PageIdentifier}` | `Text="{Binding PageIdentifier, StringFormat='View Identifier: {0}'}"` |
| `"Page Identifier: "` + `{Binding PageIdentifier}` | `Text="{Binding PageIdentifier, StringFormat='Page Identifier: {0}'}"` |
| `"Parameters: "` + `{Binding NavigationParameterData}` | `Text="{Binding NavigationParameterData, StringFormat='Parameters: {0}'}"` |
| `"OnPrepare: "` + `{Binding OnPrepareData}` | `Text="{Binding OnPrepareData, StringFormat='OnPrepare: {0}'}"` |
| `"OnRefresh: "` + `{Binding OnRefreshData}` | `Text="{Binding OnRefreshData, StringFormat='OnRefresh: {0}'}"` |

Trade-off: the bold label prefixes become plain text (cosmetic, sample-only). All other labels in the sample are already plain `Text` (verified: tabs, popups use `Text`/`StringFormat` bindings — `State: {0}`, `Value: {0}` are already queryable).

### 3. Suite layout (`tests/ui/`)

Scripts, not a dotnet project — the driver is the DevFlow CLI; zero C# surface, runs on macOS today and on a GitHub macOS runner unchanged. `jq` is the only external dependency (preinstalled on GitHub macOS runners; `brew install jq` locally if absent — noted in the README).

```
tests/ui/
├── README.md                  # prerequisites, how to run, future CI job notes
├── run-all.sh                 # entry point: build → launch → wait → flows (sequential) → teardown
├── lib/
│   └── devflow.sh             # shared helpers (see §4)
├── nav_by_type.sh
├── nav_by_route.sh
├── nav_modal.sh
├── nav_modal_stack.sh
├── nav_params_prompt.sh
├── nav_query.sh
├── nav_background_thread.sh
├── nav_debounce.sh
├── nav_switch_tabs.sh
├── popup_simple.sh
├── popup_advanced.sh
├── popup_singleton.sh
├── popup_ct_compat.sh
├── popup_v1_compat.sh
├── popup_background_thread.sh
├── dialog_alert.sh
├── dialog_alert_background.sh
├── dialog_confirm.sh
├── dialog_prompt.sh
├── dialog_action_sheet.sh
├── details_confirm_navigation.sh
└── details_back_twice.sh
```

`run-all.sh` responsibilities (order matters — sequential execution is a DevFlow constraint from spike #1):
1. `dotnet build Mvvm.Nucleus.Maui.Sample -f net11.0-maccatalyst -c Debug` (0 errors required).
2. Locate the bundle (`find … -path '*net11.0-maccatalyst*' -name 'Mvvm.Nucleus.Maui.Sample.app'`), `open` it.
3. `ensure_broker` + `wait_agent` (helper, with timeout).
4. Execute flow scripts in the order above, **sequentially**, each starting from a known tab; fail-fast on first failing flow (print the flow + failing step); per-flow PASS/FAIL summary.
5. Teardown: quit the app (`osascript`); leave the broker daemon running (harmless; stopping it is the recovery step, not the teardown step).

### 4. Driver helper (`lib/devflow.sh`)

All flows call these — they encapsulate the spike #1 operational caveats:

- `df <subcommand…>` — runs `maui devflow <…> --ci --json` (plus `--agent-port` auto-discovery); on a stale-element/epoch error → `maui devflow ui tree` (refresh) and retry **once**; on a broker-lease error → `maui devflow broker stop && start` + `wait_agent` + retry **once** (spike #1: the error is non-retryable inline but recovered via broker restart); anything else → fail with the step name.
- `ensure_broker` — `maui devflow broker start` (idempotent check first).
- `wait_agent <timeout_s>` — `maui devflow wait` / `agent list` polling until the sample agent is registered and `diagnose` healthy.
- `reset_to <NavigationTab|PopupTab|DialogTab>` — `df ui navigate //Tabs/<tab>` (spike-verified), then a tree refresh.
- `tap_text <label> [kind]` — `ui query --text` → pick the match (optionally filtered by element kind, e.g. `Button`) → `ui tap <id>`; disambiguation rule for repeated labels (e.g. `Close using IPopupService` present in both a popup and, behind it, the tab): prefer the match under the currently-presented overlay/popup subtree when the query returns >1.
- `assert_text <substring>` — `ui query --text` must return ≥1 element (equivalent to spike #1's `ui assert Text`; exit-code-friendly).
- `assert_absent <text>` — for negative checks (e.g. no second `Details` after the debounce flow's single back).
- `dismiss_dialog <button_label>` — spike #1 Catalyst pattern: try `ui alert detect` → `ui alert dismiss` first (works off-Catalyst / for in-app dialogs); fall back to `tap_text <button_label>` (the alert's button is registered as a native `DialogAction` element — reliable on Catalyst).
- `enter_prompt <value> [ok_label]` — find the prompt's text field, `ui fill <id> <value>`, then `dismiss_dialog <ok_label>` (exact Catalyst OK/Cancel labels confirmed at implementation time via `ui tree` while a prompt is open — see Open Notes).

### 5. Flow set (concrete steps and assertions)

Distinctive test values are used so assertions can't collide with defaults (`Sample Text`, `Unset`).

**Navigation (start on `//Tabs/NavigationTab`)**

| Script | Steps |
|---|---|
| `nav_by_type.sh` | `tap_text "Navigation by Type"` → `assert_text "Welcome to the details page."` → `tap_text "Navigate Backwards"` → `assert_text "Navigation by Type"` (back on tab) |
| `nav_by_route.sh` | same shape via `"Navigation by Global Route"` |
| `nav_modal.sh` | `tap_text "Navigation by Modal"` → `assert_text "Welcome to the details page."` (modal presented) → `tap_text "Close Current Modal (if any)"` → `assert_text "Navigation by Modal"` |
| `nav_modal_stack.sh` | `tap_text "Navigation by Modal + Stack"` → assert details → `tap_text "Close All Modals (if any)"` → assert tab content |
| `nav_params_prompt.sh` | `tap_text "Navigation with Parameters"` → prompt appears (`assert_text "Enter a value for the parameter."`) → `enter_prompt "UI-Param-42"` → `assert_text "Parameters: Sample : UI-Param-42"` → back |
| `nav_query.sh` | `tap_text "Navigation with Query"` → `enter_prompt "UI-Query-77"` → `assert_text "Parameters: Sample : UI-Query-77"` (query string parsed into the parameter dictionary) → back |
| `nav_background_thread.sh` | `tap_text "Navigation from background thread"` → `assert_text "Parameters: Sample : IsMainThread = False"` (MainThread marshalling) → back |
| `nav_debounce.sh` | `tap_text "Navigation ignoring multiple requests"` (two concurrent navigations; debounce must drop the second) → `assert_text "Welcome to the details page."` → **one** `tap_text "Navigate Backwards"` → `assert_text "Navigation by Type"` (a double-push would leave us on a second `Details` → failure) |
| `nav_switch_tabs.sh` | `tap_text "Switch Tabs"` → `assert_text "Show Popup without ViewModel"` (PopupTab active) |

**Popups (start on `//Tabs/PopupTab`)**

| Script | Steps |
|---|---|
| `popup_simple.sh` | `tap_text "Show Popup without ViewModel"` → `assert_text "Text from navigation parameters."` → `tap_text "Close using IPopupService"` → `assert_text "Popup was closed"` → `dismiss_dialog "Okay"` |
| `popup_advanced.sh` | `tap_text "Show Popup with ViewModel"` → `assert_text "State: Default, Prepared, Opened"` (lifecycle order) + `assert_text "Text from navigation parameters."` → `tap_text "Close using IPopupService"` → `assert_text "result was 'Closed using IPopupService.'"` (return value round-trip) → `dismiss_dialog "Okay"` |
| `popup_singleton.sh` | `tap_text "Show Popup as Singleton"` → `assert_text "Value: Unset"` → `tap_text "Set Value"` → `enter_prompt "Persist-42"` → `assert_text "Value: Persist-42"` → `tap_text "Close Popup"` → `dismiss_dialog "Okay"` → **reopen** `tap_text "Show Popup as Singleton"` → `assert_text "Value: Persist-42"` (singleton state survived close+reopen) → close + dismiss |
| `popup_ct_compat.sh` | `tap_text "Use CommunityToolkit.PopupService"` → `assert_text "State: Default, Prepared, Opened"` (CT 17 `IPopupService` drives the advanced popup) → close via service → `assert_text "result was 'Closed using IPopupService.'"` → dismiss |
| `popup_v1_compat.sh` | `tap_text "Show Compatibility Popup"` → `tap_text "Close using IPopupService"` (LegacyPopup, CT V1 compat service) → `assert_text "result was 'Closed using IPopupService.'"` → dismiss |
| `popup_background_thread.sh` | `tap_text "Show Popup from background thread"` → `assert_text "IsMainThread = False"` (SimplePopup `Init` param + thread marshalling) → `tap_text "Close using IPopupService"` (no follow-up alert in this flow) |

**Dialogs (start on `//Tabs/DialogTab`; all dismissals via `dismiss_dialog`)**

| Script | Steps |
|---|---|
| `dialog_alert.sh` | `tap_text "Display Alert"` → `assert_text "This is an alert"` → `dismiss_dialog "Okay"` |
| `dialog_alert_background.sh` | `tap_text "Display Alert (background thread)"` → `assert_text "triggered from another thread"` → `dismiss_dialog "Okay"` |
| `dialog_confirm.sh` | `tap_text "Display Confirm"` → `dismiss_dialog "Confirm"` → `assert_text "You choose to confirm as input."` → `dismiss_dialog "Okay"` |
| `dialog_prompt.sh` | `tap_text "Display Prompt"` → `enter_prompt "UI-Prompt-Value"` → `assert_text "You choose 'UI-Prompt-Value' as input."` → `dismiss_dialog "Okay"` |
| `dialog_action_sheet.sh` | `tap_text "Display Action Sheet"` → `dismiss_dialog "Option Two"` → `assert_text "You choose 'Option Two' as input."` → `dismiss_dialog "Okay"` |

**Details (start on `//Tabs/NavigationTab`)**

| Script | Steps |
|---|---|
| `details_confirm_navigation.sh` | `tap_text "Navigation by Type"` → `tap_text "Navigate Backwards (confirm)"` → confirm alert → `dismiss_dialog "Cancel"` → **still on Details** (`assert_text "Welcome to the details page."`; `IConfirmNavigationAsync` deny cancels the pop) → `tap_text "Navigate Backwards (confirm)"` again (re-arms the flag) → `dismiss_dialog "Confirm"` → `assert_text "Navigation by Type"` (allow proceeds with the pop) |
| `details_back_twice.sh` | `tap_text "Navigation by Type"` → `tap_text "Show More Details (Regular)"` (two `Details` deep) → `tap_text "Navigate Backwards (twice)"` (`../..` route) → `assert_text "Navigation by Type"` (both levels popped in one request) |

**Total: 22 flows.** Each flow ends in its starting state (tab restored, modals/alerts closed), keeping the suite order-independent within the fixed `run-all.sh` order.

### 6. CI readiness (no workflow files)

- Every CLI invocation goes through `df` with `--ci --json` (non-interactive, fail-fast) — the exact mode GitHub Actions would use.
- `run-all.sh` is self-contained: build → launch → wait → flows → teardown; prerequisites are only the .NET 11 SDK, the MAUI workloads for `net11.0-maccatalyst`, and `jq`.
- `tests/ui/README.md` documents the future job (macOS runner — the only runner that can host Catalyst; ubuntu cannot): install SDK/workloads → `tests/ui/run-all.sh`; and notes the iOS-simulator variant (`simctl` + `--device`) as the alternate host. Per the shared CI scaffold in the research, the workflow file itself is deferred (consistent with `plan.md`).

### 7. Verification exit criteria

1. `tests/ui/run-all.sh` → **22/22 PASS** locally on this Mac (Catalyst, Debug).
2. Run `run-all.sh` **twice back-to-back** (second run reuses/restarts the broker and relaunches the app) to prove the wrapper absorbs the broker-lease and epoch-ID caveats — no manual intervention.
3. `dotnet build Mvvm.Nucleus.Maui.Sample -c Release -f net11.0-maccatalyst` succeeds **without** the DevFlow package (Debug condition) and the Release bundle contains no `*DevFlow*` framework/dll (grep the bundle's `Contents/Frameworks`).
4. `dotnet test tests/Mvvm.Nucleus.Maui.UnitTests -c Release` → still **65/65** (unit lane unregressed — this plan touches no library code).
5. Debug sample build adds no new warnings beyond the pre-existing CS0618 set (devlog-002 baseline).

## Implementation Approach

Order matters: onboard → assertable → driver → flows outward from the highest-risk surface (popups) is *not* chosen; instead navigation first because it's the most deterministic host for validating the driver, then popups (biggest historical risk), then dialogs, then the `Details` flows.

1. **DevFlow onboarding (spike #1 re-apply)** — sample csproj (Debug-conditioned `Microsoft.Maui.DevFlow.Agent` 0.1.0-preview.14.26555.2 + `CodeSignEntitlements` property group), `MauiProgram.cs` (`#if DEBUG` → `builder.AddMauiDevFlowAgent();`), new `Entitlements.Debug.plist`. Verify per §1 exit criteria (build, launch, `diagnose`, `ui tree`).
2. **`Details.xaml` assertability fix** — the five label conversions from §2; rebuild; spot-check one value via `ui query --text "Parameters:"` after navigating to `Details`.
3. **Driver + runner skeleton** — `lib/devflow.sh` (all helpers from §4), `run-all.sh` (build/launch/wait/sequential/teardown), `README.md`. Smoke-test: a throwaway flow that navigates tabs, asserts one label, and dismisses an alert — the three driver archetypes.
4. **Navigation flows** — the 9 `nav_*.sh` scripts (§5); get `run-all.sh` green over navigation before adding more.
5. **Popup flows** — the 6 `popup_*.sh` scripts (§5); confirm the `tap_text` overlay-disambiguation rule holds for `Close using IPopupService` (popup vs. behind-tab).
6. **Dialog flows** — the 5 `dialog_*.sh` scripts (§5); confirm the Catalyst prompt's OK/Cancel button labels with `ui tree` while a prompt is open and pin them in `enter_prompt`/`dismiss_dialog` calls.
7. **Details flows** — `details_confirm_navigation.sh`, `details_back_twice.sh`.
8. **Final verification** — §7 exit criteria, in order (full suite ×2, Release bundle check, unit suite re-run, warning diff).

**Files touched:**
- `Mvvm.Nucleus.Maui.Sample/Mvvm.Nucleus.Maui.Sample.csproj` (Debug package ref + entitlements property group).
- `Mvvm.Nucleus.Maui.Sample/MauiProgram.cs` (one line in the `#if DEBUG` block).
- `Mvvm.Nucleus.Maui.Sample/Platforms/MacCatalyst/Entitlements.Debug.plist` (new).
- `Mvvm.Nucleus.Maui.Sample/Views/Details.xaml` (five label conversions).
- `tests/ui/**` (new: README, run-all.sh, lib/devflow.sh, 22 flow scripts).
- No changes to `Mvvm.Nucleus.Maui`, the test project, or the solution.

## Alternatives Considered

- **XCUITest** (research option D / decision option 2) — Apple-native and CI-robust, but Swift in a C# repo plus xcodeproj/scheme management on a MAUI single project is the heaviest authoring cost; kept as the documented fallback (see `decision-ui-automation-devflow.md`) if the preview agent churns.
- **Appium / Android emulator** (research option D) — most mature third-party ecosystem but the most moving parts (emulator images in CI, driver versioning, page-object layer), and Android is a secondary target here. Rejected per research.
- **`maui devflow batch` (JSONL) as the sole script format** — atomic single requests, but loses per-step retry, per-step error attribution, and can't interleave app-level steps (launch, broker recovery); the CLI-per-step driver with a retry wrapper is strictly more robust here. `batch` remains available for a flow if it ever needs atomicity.
- **A C# xunit-style "UI test project" that shelled out to the CLI** — would add a build host, package refs, and test-runner semantics on top of scripts that already fail-fast with exit codes; zero benefit for a macOS/Catalyst-only suite. Rejected.
- **Stable `AutomationId`s on all sample elements** — would harden selectors, but spike #1 proved text-based query/assert/tap works end-to-end, and the sample's button/label texts are already unique per presented context (with the one overlay rule in §4). Adding IDs to ~30 elements is sample churn without proven need; revisit only if text queries prove ambiguous in practice.
- **iOS simulator as the primary host** — spike #1 validated Catalyst (and it's the app's first-party desktop target); `simctl`-based runs are documented in the README as the CI alternative, not built.
- **Screenshots for assertions** — DevFlow supports `ui screenshot`, but text/property assertions give precise, diffable failures; image comparison adds no value for this suite's contracts.

## Open Notes for Implementation

- **Catalyst native-prompt button labels** (`OK`/`Cancel` vs. `Done` etc.) must be confirmed with `ui tree` while a prompt is open (step 6); they are parameters to `dismiss_dialog`/`enter_prompt`, so adjusting them is a one-line change per flow.
- **`jq` availability:** verify on this Mac (`brew install jq` if absent) and document it; GitHub macOS runners ship it.
- **Agent package is preview-only/net10-line:** version is pinned in the csproj; if a newer `Microsoft.Maui.DevFlow.Agent` appears, bump-and-verify before the suite changes (driver is CLI-stable by design).
- **`tap_text` disambiguation:** if `ui query --text "Close using IPopupService"` ever returns both the popup button and a behind-tab button, the helper's overlay-preference rule (§4) handles it; if it proves unreliable, add an `AutomationId` to the popup close buttons *only* (the one justified case for the rejected alternative).
- **`run-all.sh` app quit:** use `osascript -e 'quit app "Mvvm.Nucleus.Maui.Sample"'`; fall back to `pkill` if the bundle name differs from the display name.
- The suite is a **debug-bench for the sample**, not a shipped artifact: nothing under `tests/ui/` is packed, referenced, or part of the library's public surface.
