# Building and Testing

How to build this repository and run its two test lanes (unit + UI). Written for
**contributors** and **AI agents** alike: every command is literal and has been
verified on the host this repo is developed on (macOS arm64, .NET 11). Where
behavior is host- or version-specific, the doc says so instead of assuming.

## Repository layout

| Path | What it is | Target framework(s) |
|---|---|---|
| `Mvvm.Nucleus.Maui/` | **The library** — MVVM framework for MAUI; published as the NuGet package `Mvvm.Nucleus.Maui` | `net11.0`, `net11.0-android`, `net11.0-ios`, `net11.0-maccatalyst`, `net11.0-windows10.0.19041.0` |
| `Mvvm.Nucleus.Maui.Sample/` | **Sample app** — exercises every feature; the DevFlow automation agent is embedded **Debug-only** here | `net11.0-android`, `net11.0-ios`, `net11.0-maccatalyst` (+ `net11.0-windows10.0.19041.0` on Windows) |
| `tests/Mvvm.Nucleus.Maui.UnitTests/` | **Unit lane** — plain xUnit, 65 tests | `net11.0` |
| `tests/ui/` | **UI lane** — 22-flow script-driven DevFlow regression suite (not a .NET project) | runs the **Mac Catalyst** sample |

Solution: `Mvvm.Nucleus.Maui.sln` (the three projects above).

> **For AI agents:** do not infer target frameworks or package pins from memory.
> They are authoritative in the `.csproj` files (library
> `Mvvm.Nucleus.Maui/Mvvm.Nucleus.Maui.csproj`, sample
> `Mvvm.Nucleus.Maui.Sample/Mvvm.Nucleus.Maui.Sample.csproj`, tests
> `tests/Mvvm.Nucleus.Maui.UnitTests/Mvvm.Nucleus.Maui.UnitTests.csproj`). The
> "why" for non-obvious choices lives in the RPI docs at the repo root
> (`decision-*.md`, `research-testingOptions.md`, `plan*.md`).

## Prerequisites

| Tool | Version (verified) | Check | Install |
|---|---|---|---|
| .NET SDK | 11.0.100 | `dotnet --version` | [dotnet 11](https://dotnet.microsoft.com/download) |
| MAUI workloads | android, ios, maccatalyst | `dotnet workload list` | `dotnet workload install maui` |
| Xcode (macOS) | for `net11.0-maccatalyst` | `xcodebuild -version` | App Store / `xcode-select` |
| `maui` DevFlow CLI | `0.1.0-preview.14.26555.2` | `maui --version` | only needed for the **UI lane** |
| `jq` | 1.7.1 | `jq --version` | `brew install jq` (macOS) — **UI lane only** |

The **unit lane** needs only the .NET SDK (the library compiles to `net11.0`).
The **UI lane** additionally needs macOS + Xcode (Catalyst), the `maui` CLI, and
`jq`.

## Building

### On Windows (all target frameworks)

```bash
dotnet build Mvvm.Nucleus.Maui.sln -c Debug
```

The Windows TFM (`net11.0-windows10.0.19041.0`) only builds where the Windows
SDK tools are available.

### On macOS / Linux (per-target-framework)

The full-solution build **fails** on the library's `net11.0-windows10.0.19041.0`
target because Windows MSIX PRI expansion cannot run off-Windows. This is an
**environmental** limit, not a code defect — the exact errors are:

```
error MSB6003: The specified task executable "MakePri.exe" could not be run.
            ... Win32Exception (8) ... Exec format error   [net11.0-windows10.0.19041.0]
error APPX0002: Task 'WinAppSdkExpandPriContent' failed. ...   [net11.0-windows10.0.19041.0]
```

Build the target you need instead (each is self-contained and verified):

```bash
# Library, one TFM at a time (pick the platform you care about)
dotnet build Mvvm.Nucleus.Maui/Mvvm.Nucleus.Maui.csproj -c Debug -f net11.0
dotnet build Mvvm.Nucleus.Maui/Mvvm.Nucleus.Maui.csproj -c Debug -f net11.0-maccatalyst

# Sample app (this is what the UI lane builds)
dotnet build Mvvm.Nucleus.Maui.Sample/Mvvm.Nucleus.Maui.Sample.csproj -c Debug -f net11.0-maccatalyst
```

**Run the sample app** (manual inspection, any platform with a device/simulator):

```bash
dotnet run --project Mvvm.Nucleus.Maui.Sample -f net11.0-maccatalyst
```

> The sample embeds the DevFlow agent **only in Debug** (`Microsoft.Maui.DevFlow.Agent`
> behind `Condition="'$(Configuration)' == 'Debug'"`, registered in `#if DEBUG`,
> with `com.apple.security.network.server` in
> `Platforms/MacCatalyst/Entitlements.Debug.plist`). **Release builds contain no
> agent** — that invariant is an explicit exit criterion, do not break it.

## Running the unit lane

```bash
dotnet test tests/Mvvm.Nucleus.Maui.UnitTests/Mvvm.Nucleus.Maui.UnitTests.csproj -c Debug
```

**Expected:** `Passed! - Failed: 0, Passed: 65, Skipped: 0, Total: 65`.

The lane is `net11.0`-only (a plain xUnit project that references the library), so
it runs on any host with the .NET SDK. There are no skips in the suite — if a
test is skipped or fails, that is a signal to investigate, not to ignore.

## Running the UI lane (DevFlow)

Full operational reference (prerequisites, driver helpers, tunables, operational
caveats, the future CI job): **[`tests/ui/README.md`](tests/ui/README.md)**.
The essentials:

**Prereqs:** macOS + Xcode (Catalyst), the `maui` DevFlow CLI, `jq`. The sample
must be built **Debug** (that is the only configuration with the agent).

**Run the whole suite** (self-contained: builds the sample, launches via
LaunchServices, starts the broker, waits for the agent, runs the 22 flows
sequentially, tears down):

```bash
tests/ui/run-all.sh
```

**Expected:** `RESULT: 22/22 passed` (the exit criterion is **22/22 twice in a
row**, to prove the suite is order-independent and stable).

Flows: navigation ×9, popups ×6, dialogs ×5, details ×2. Every flow is a
standalone script under `tests/ui/` and can be run by itself **only if** the app
is already running and the agent connected:

```bash
tests/ui/nav_by_type.sh        # example: a single flow
```

Every `maui devflow` call goes through the `df` wrapper in
[`tests/ui/lib/devflow.sh`](tests/ui/lib/devflow.sh) with `--ci --json`. That
wrapper encodes the non-obvious runtime constraints (driving-lease spacing +
retry, broker recovery, stale-epoch refresh, and the app-UI-only `reset_to`).
**If a flow fails, read `tests/ui/README.md` "Operational caveats" before
changing code** — most UI-lane failures are environmental (launch, key-window,
lease) rather than app regressions.

### UI-lane quick troubleshooting

| Symptom | Cause / fix |
|---|---|
| `open … failed with error -54` | Wrong/stale bundle selected. `run-all.sh` scopes the search to `bin/Debug` and re-registers via `lsregister -f`. See `tests/ui/README.md` caveat #1. |
| Tap reports `{"success":true}` but nothing happens | Non-key window (app not launched via LaunchServices). `run-all.sh` launches with `open`; the `df` driver reactivates + retries on `{"success":false}`. |
| `run-all.sh` killed mid-run by a shell timeout | Run it detached / without a foreground timeout — the harness kills the process group on timeout. The suite takes ~30–45 min. |

## Committing

- Keep the library (`Mvvm.Nucleus.Maui/`) free of UI-lane concerns; the agent is
  sample-only and Debug-only.
- The `Details.xaml` data labels use plain `Text` + `StringFormat` (not
  `FormattedText`) so their values are assertable — do not convert them back.
- The RPI workflow docs at the root (`plan*.md`, `research*.md`, `decision-*.md`,
  `devlog-*.md`, `code-review-*.md`) are tracked on purpose (see `.gitignore`)
  and should stay in the repo.

## Verified baseline (2026-11-14)

- Unit lane: **65/65** (Debug and Release).
- UI lane: **22/22 ×2 back-to-back**.
- Release Catalyst bundle: **no `*DevFlow*` artifacts**, no DevFlow strings in any
  Mach-O.
- No new compiler warnings beyond the pre-existing `CS0618` (CommunityToolkit V1
  compat) and `NU1902` (`Microsoft.Build.Tasks.Git`) baseline.
