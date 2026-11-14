# Decision: Expose NucleusMvvmCore test seams via a single InternalsVisibleTo

**Status:** Accepted
**Date:** 2026-11-13
**Deciders:** Erik

## Context
Introducing unit testing (see `research-testingOptions.md`) requires driving the coupled core + behavior layer — not just the pure contracts. Spike #3 established that the two `internal` members tests must reach are:

- `NucleusMvvmCore.Initialize(IServiceProvider)` — sets the static `Current` and the `IServiceProvider`.
- The `NucleusMvvmCore.Shell` **setter** — registers a `Shell` (hooks its `Navigating`/`Navigated` events).

The library currently has **no `InternalsVisibleTo`** and **no public initializer seam**. `NucleusMvvmCore`'s constructor is public, but `Initialize`/`Shell` are `internal`, so a test assembly cannot complete initialization or register a Shell through the public API alone.

The unit host is the **plain `net11.0`** test project (see `decision-net11-test-tfm.md`). In that host there is no running `Application`/DI container, so the seam must be reachable without bootstrapping a `MauiApp`.

## Options

### Option 1: Minimal `InternalsVisibleTo` for the test assembly
Add `<InternalsVisibleTo Include="Mvvm.Nucleus.Maui.UnitTests" />` to the library csproj. Tests call `new NucleusMvvmCore(...)` (public ctor) then `core.Initialize(sp)` and `core.Shell = shell` (both `internal`, now visible).

**Pros:**
- **No public API surface change** — `InternalsVisibleTo` is a compile-time, test-only grant; it is invisible to production consumers of the package.
- **No `MauiApp` bootstrap** — avoids pulling platform DI/services into a bare `net11.0` host, which would fail or be brittle.
- **One line**, trivially revertible; no library code refactor.
- Proven: spike #3 passes 6/6 tests with this exact seam.

**Cons:**
- Couples the library to the **name** of the test assembly (if the test assembly is renamed, the attribute must follow).
- The two `internal` members become reachable by the test assembly (a very small, bounded surface).

### Option 2: Bootstrap a full `MauiApp` in the test host
Call `MauiProgram.CreateMauiApp()` / build a `MauiAppBuilder` in the test to obtain the composed `NucleusMvvmCore`.

**Pros:**
- Exercises the real composition root.

**Cons:**
- `CreateMauiApp` pulls in platform services and DI that **do not resolve in a bare `net11.0` host** (no platform, no `IApplicationWindow`/lifecycle) — this is exactly the pattern the MAUI unit-testing guidance warns against.
- Far heavier, brittle, and slower; couples unit tests to app startup.
- Not viable for the fast unit lane.

### Option 3: Introduce an explicit test seam (small public/internal abstraction)
Extract an `INucleusCoreInitializer` (or similar) and register it in DI, so tests can initialize through a public contract.

**Pros:**
- A "real" public seam; no `InternalsVisibleTo`.

**Cons:**
- **Grows the public API surface** of a shipped library for a testing convenience.
- Requires refactoring `NucleusMvvmCore`/the DI wiring — more invasive and higher risk for marginal benefit.
- Overkill given only two members are needed.

## Decision
**Accepted: Option 1** — Erik approved (2026-11-13) exposing the test seams via a single `InternalsVisibleTo Include="Mvvm.Nucleus.Maui.UnitTests"` line in the library csproj. This is the minimal, non-public, revertible seam that enables the coupled core + behavior unit tests without bootstrapping a `MauiApp` or growing the public API.

## Consequences
### Positive
- The unit lane can cover the coupled core + behavior layer (core init, Shell registration, the Shell double-`NavigatedTo` guard, `Refresh`-on-renavigation, `OnFirstAppearing`-once) with a single additive line.
- No public API growth; production consumers are unaffected.
- Tests stay fast and platform-free (plain `net11.0`).

### Negative
- The library now references the test-assembly name in an `InternalsVisibleTo` attribute — must be kept in sync if the test assembly is renamed.
- The two `internal` members (`Initialize`, `Shell` setter) are now reachable by the test assembly (bounded, intended).
