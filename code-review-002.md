# Code Review

**Reviewer:** AI
**Date:** 2026-11-13
**Reviewing:** Uncommitted changes — `Mvvm.Nucleus.Maui/Mvvm.Nucleus.Maui.csproj` (+3 additions), `Mvvm.Nucleus.Maui.sln` (test project entry), `tests/Mvvm.Nucleus.Maui.UnitTests/**` (new project: csproj, AssemblyInfo, 11 helper files, 10 test files, 65 tests), `devlog-002.md`

## Summary
A tightly-scoped, zero-library-source-change test suite: the library diff is exactly the plan's three csproj additions (net11.0 TFM, `InternalsVisibleTo`, TFM-scoped `CA1416` NoWarn), and the entire behavioral contract of the library is pinned by 65 passing tests in a plain-`net11.0` xUnit project. Every claim in the devlog was independently re-verified during this review: 65/65 green in Debug **and** Release, no new warnings beyond the pre-existing NU1902 (library) and pre-existing CS0618 (platform TFMs, unchanged count), all three non-Windows platform TFMs build with 0 errors, and the packed nupkg differs from the 0.8.0 baseline **only** by the added `lib/net11.0/` (dll + xml). Both plan-documented fallbacks were resolved in favor of keeping the tests (no skips in the suite). One issue was found and fixed during review (sln configuration bloat); no High items remain.

## What Works Well
- **Zero library code change** — confirmed by diff: only the csproj (TFM + IVT + scoped NoWarn) and sln were touched on the library side. The IVT seam is name-fixed (not a wildcard) and exposes only `Initialize(IServiceProvider)` + the `Shell` setter, exactly as the adopted decision specifies.
- **Determinism is engineered, not hoped for**: per-test cores (no `NavigationService` subscription leaks), `DisableTestParallelization` with the rationale documented in `AssemblyInfo.cs`, and reflection reset of the process-wide debounce static. Proven by two consecutive green runs.
- **The reflection helpers fail loudly**: every helper throws a diagnostic `InvalidOperationException` naming the missing field/ctor/method rather than no-op'ing silently (the one intentional no-op — `CoreEvents` when nothing is subscribed — matches C# event semantics).
- **Test code hygiene**: xunit-analyzer-clean (the `xUnit2029` trap handled with `DoesNotContain(collection, predicate)`), the CS0618 suppressions are pragma-scoped per usage with a rationale comment, non-test helper types are `internal`/`private nested`, and the `NavigationMode` ambiguity is resolved with named using-aliases and explanatory comments.
- **Package shape verified**: the 0.8.0 nupkg diff is exactly `+lib/net11.0/{dll,xml}` — platform asset version suffixes (android37.0/ios27.2/maccatalyst27.2/windows10.0.19041) are byte-identical to the baseline.
- **Docs**: the devlog captures the MAUI 11 event-field shape, the xunit API traps, and the namespace traps — all of which would otherwise have been lost.

## Issues

### High
- None.

### Medium
- None.

### Low
- [x] **sln configuration bloat (found & fixed during review)**: `dotnet sln add` on this SDK also injected `Debug|x64`/`Debug|x86`/`Release|x64`/`Release|x86` solution configuration platforms (mapping all three projects to `Any CPU`) — 40 extra diff lines beyond the plan's "one project entry". Removed; sln now parses (`dotnet sln list`) and the diff is exactly the project + `tests` folder + Any CPU mappings + nested section. Note for the future: any `dotnet sln` command in this environment may re-normalize the config list.
- [ ] **Reflection brittleness (accepted trade-off)**: `PageEvents`/`CoreEvents`/`ShellNavigatingEventArgsBuilder`/`NavigationServiceStateReset` and the route-parsing test depend on MAUI 11 / library-internal member shapes (same-name event fields, `_isNavigatingLastTriggeredUtc`, `GetQueryParameterDictionary`). A future MAUI upgrade or library refactor will break them at *runtime* with a diagnostic exception, not a compile error. This is inherent to the adopted "no library change" seam and acceptable for tests (they fail loudly, and the suite documents the expected shapes in comments), but the MAUI-upgrade checklist should include "check TestHelpers".
- [ ] **`IVT` ships in the package**: the compiled `InternalsVisibleTo` attribute is embedded in the shipped `net11.0` (and all platform) DLLs. Name-fixed, exposes only two internals, explicitly adopted in the decision doc — no action; noted so a future "trim the package" effort knows why it's there.
- [ ] **`NavigationServiceRouteParsingTests.Parse`** invokes the private method via `method.Invoke`, so a genuine throw inside the method would surface as `TargetInvocationException` wrapping (cosmetic; the method under test never throws by contract).
- [ ] **Options-defaults test is a single `[Fact]`** rather than the plan's "table-driven" `[Theory]` — all 14 documented defaults are asserted (the intent), just not data-row-per-default. Fine.
- [ ] **Pre-existing, not introduced here**: the library csproj's duplicated `net11.0-windows10.0.19041.0` TFM entry (already recorded in devlog-001 Known Quirks).

## Recommendation
- [x] Ready for human review
- [ ] Needs revision (see issues above)
