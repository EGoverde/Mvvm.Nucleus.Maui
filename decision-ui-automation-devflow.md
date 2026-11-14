# Decision: DevFlow as the UI-automation driver (XCUITest as fallback)

**Status:** Accepted
**Date:** 2026-11-13
**Deciders:** Erik

## Context
`research-testingOptions.md` establishes two test lanes. The unit lane is implemented (`plan.md`, devlog-002: 65 green tests). The UI lane needs an automation driver for the integration surface (Shell navigation, CT 17 popup round-trips, native dialogs, parameter passing) — the exact area that historically regressed (CT 12→13 V2 rewrite, CT 13→17).

DevFlow (dotnet/maui-labs) is already installed in this environment (CLI `0.1.0-preview.14.26555.2`, MCP server configured, skills present). Spike #1 (2026-11-13) verified end-to-end on `net11.0-maccatalyst` (MAUI 11): build, agent connect, visual tree, navigation, assertions, CT 17 popup round-trip. Known caveats: DevFlow is blind to Catalyst native NSAlerts *as alerts* (but their buttons are tappable `DialogAction` elements), `FormattedText` spans expose no text (sample-only fix planned), and the agent package is preview-only/net10-line.

## Options

### Option 1: DevFlow (script-driven via `maui devflow` CLI)
Embed the `Microsoft.Maui.DevFlow.Agent` in the sample (Debug-only) and script the flows as shell scripts calling `maui devflow ui … --ci --json` under `tests/ui/`.

**Pros:**
- Proven by spike #1 on this exact toolchain (MAUI 11 / Catalyst / CT 17): tree, navigate, tap, assert, popup round-trip all work.
- C# repo, macOS-first, zero new languages; DevFlow already installed and configured here.
- Driver layer is the stable CLI surface — the suite survives MCP-surface churn; `--ci --json` makes it CI-ready.
- Lowest authoring cost of the three options; flows stay data/scripts (no test framework to maintain).

**Cons:**
- Preview-only, net10-line package (`0.1.0-preview.14.26555.2`) — version churn risk (mitigated: pinned version, XCUITest fallback).
- Catalyst native alerts must be handled via the `DialogAction`-tap pattern (alert message text not assertable).
- `FormattedText` labels invisible to the registry (sample-only conversion required).
- Operational flakiness (broker lease, epoch-scoped IDs) needs a retry/refresh wrapper.

### Option 2: XCUITest
Apple-native UI tests via `xcodebuild test`; CI-robust regardless of DevFlow's preview status; can read native alert text.

**Pros:**
- No third-party preview dependency; Apple-supported.
- Full access to native alert text/buttons.

**Cons:**
- Swift test code in a C# repo; xcodeproj/scheme management on top of a MAUI single project.
- Heaviest authoring cost of the three options (research verdict: not the natural fit for this repo).

### Option 3: Appium / UIAutomator (Android emulator)
Most mature third-party ecosystem.

**Pros:**
- Large ecosystem, cross-platform.

**Cons:**
- Most moving parts (emulator images in CI, driver versioning, page-object layer); Android is a secondary target on this machine (research verdict: rejected).

## Decision
**Accepted: Option 1** — Erik approved (2026-11-13) DevFlow as the UI-automation driver, with **Option 2 (XCUITest) as the documented fallback** if the preview package churns to the point where the suite can no longer run. Option 3 rejected per research. The choice is grounded in spike #1's PASS on MAUI 11/Catalyst, not assumption.

## Consequences
### Positive
- The entire UI value matrix (navigation, 4 popup flavors, dialogs, debounce, confirm-navigation) becomes an automated regression suite with the lowest authoring cost.
- Same scripts run locally and (later) on a GitHub macOS runner — no driver rewrite needed for CI.
- The sample app doubles as a permanent UI-test fixture.

### Negative
- A preview-only, net10-line package sits in the sample (Debug-only; never in the library, never in Release).
- Catalyst alert *message* text stays unassertable (buttons and follow-up state are); `FormattedText` content stays invisible (sample labels must use plain `Text`).
- If DevFlow churns past usability, the suite must be re-driven on XCUITest (fallback, not expected).
