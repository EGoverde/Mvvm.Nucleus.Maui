# Decision: Add a plain net11.0 TFM to the library to enable the unit-test lane

**Status:** Accepted
**Date:** 2026-11-13
**Deciders:** Erik

## Context
Introducing unit testing (see `research-testingOptions.md`) requires a test host that can run under `dotnet test`. Spike #2 established:

- The library currently has **no plain `net11.0` TFM** — only `net11.0-android/ios/maccatalyst/windows`.
- A test project referencing the library must therefore use a **platform TFM** (NuGet TFM compatibility forbids a plain `net11.0` consumer referencing a platform-only producer).
- **Platform-TFM testhosts do not work under `dotnet test`:** a `net11.0-maccatalyst` test project crashes inside the MacCatalyst SDK's app-bundle pipeline (`ProcessRuntimeLibraries` KeyNotFoundException / MT7199 scene-manifest gate); an android testhost needs a device/emulator to run.
- **Plain `net11.0` works:** all three dependencies (`Microsoft.Maui.Controls` 11.0.10, `CommunityToolkit.Maui` 17.0.0, `CommunityToolkit.Mvvm` 8.4.2) ship plain `net11.0` assets, the library code has **zero** platform conditionals (no `#if`/`Essentials`/`DeviceInfo`), and a plain-`net11.0` xunit project built and ran natively (`Passed! Failed: 0, Passed: 2`).

The unit lane is therefore only viable if the library also targets plain `net11.0`.

## Options

### Option 1: Add a `net11.0` TFM to the library
Add `net11.0` to the library's `TargetFrameworks`. The test project uses plain `net11.0` and runs on the native testhost.

**Pros:**
- Enables the entire unit lane locally **and on any CI runner** (ubuntu or macOS), unlike the Catalyst testhost.
- Bonus: the pure-C# core becomes consumable from plain .NET contexts (e.g. desktop/non-MAUI test harnesses).
- Additive-only packaging change — existing platform consumers are unaffected.

**Cons:**
- New asset in the shipped nupkg — a public-packaging surface change (minor, but public).
- One more TFM to build/verify on every change (CI time).
- One new `CA1416` platform-attribute warning on the plain target (benign, suppressible).

### Option 2: Keep the library platform-only; drop the unit lane
Revert the TFM change and rely solely on the UI (DevFlow) lane for automated coverage.

**Pros:**
- No public packaging change; no extra TFM to maintain.

**Cons:**
- **Blocks the unit lane entirely** — platform testhosts can't run under `dotnet test`, and an android testhost needs a device.
- Loses fast, cheap feedback on the pure-C# contracts (the Prism-compat `NavigationParameters` null semantics, dictionary extensions, debounce logic) that have historically regressed.

## Decision
**Accepted: Option 1** — Erik approved (2026-11-13) adding a plain `net11.0` TFM to the library to enable the unit-test lane, accepting the minor additive packaging surface change.

## Consequences
### Positive
- Clean, CI-friendly unit host on any runner.
- Pure-C# core testable and consumable outside MAUI platform contexts.
- Unit lane can pin the compatibility contracts and lifecycle/debounce logic before they reach a running app.

### Negative
- The shipped package now includes a `net11.0` asset (public surface grows, additively).
- Build matrix grows by one TFM.
- The plain target surfaces a `CA1416` warning on `UseMauiCommunityToolkit` that should be suppressed to keep the plain build warning-clean.
