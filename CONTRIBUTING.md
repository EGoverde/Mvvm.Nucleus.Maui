# Building and Testing

How to build this repository and run its two test lanes (unit + UI). A MacOS environment is assumed.

## Repository layout

| Path | What it is | Target framework(s) |
|---|---|---|
| `Mvvm.Nucleus.Maui/` | **The library** — MVVM framework for MAUI; published as the NuGet package `Mvvm.Nucleus.Maui` | `net11.0`, `net11.0-android`, `net11.0-ios`, `net11.0-maccatalyst`, `net11.0-windows10.0.19041.0` |
| `Mvvm.Nucleus.Maui.Sample/` | **Sample app** — exercises every feature; the DevFlow automation agent is embedded **Debug-only** here | `net11.0-android`, `net11.0-ios`, `net11.0-maccatalyst` (+ `net11.0-windows10.0.19041.0` on Windows) |
| `tests/Mvvm.Nucleus.Maui.UnitTests/` | **Unit lane** — plain xUnit | `net11.0` |
| `tests/Mvvm.Nucleus.Maui.UITests/` | **UI lane** — script-driven DevFlow regression suite (not a .NET project) | runs the **Mac Catalyst** sample |

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
> `Platforms/MacCatalyst/Entitlements.Debug.plist`).

## Running the unit lane

```bash
dotnet test tests/Mvvm.Nucleus.Maui.UnitTests/Mvvm.Nucleus.Maui.UnitTests.csproj -c Debug
```

**Expected:** `Passed! - Failed: 0, Passed: 65, Skipped: 0, Total: 65`.

## Running the UI lane (DevFlow)

Full operational reference (prerequisites, driver helpers, tunables, operational
caveats, the future CI job): **[`tests/Mvvm.Nucleus.Maui.UnitTests//README.md`](tests/Mvvm.Nucleus.Maui.UnitTests//README.md)**.
The essentials:

**Prereqs:** macOS + Xcode (Catalyst), the `maui` DevFlow CLI, `jq`. The sample
must be built **Debug** (that is the only configuration with the agent).

**Run the whole suite** (self-contained: builds the sample, launches via
LaunchServices, starts the broker, waits for the agent, runs the 22 flows
sequentially, tears down):

```bash
tests/Mvvm.Nucleus.Maui.UnitTests//run-all.sh
```

**Expected:** `RESULT: 22/22 passed` (the exit criterion is **22/22 twice in a
row**, to prove the suite is order-independent and stable).

Flows: navigation ×9, popups ×6, dialogs ×5, details ×2. Every flow is a
standalone script under `tests/Mvvm.Nucleus.Maui.UnitTests//` and can be run by itself **only if** the app
is already running and the agent connected:

```bash
tests/Mvvm.Nucleus.Maui.UnitTests//nav_by_type.sh        # example: a single flow
```