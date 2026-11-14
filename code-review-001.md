# Code Review

**Reviewer:** AI
**Date:** 2026-11-13
**Reviewing:** Uncommitted changes — `Mvvm.Nucleus.Maui.csproj`, `Mvvm.Nucleus.Maui.Sample.csproj`, both Sample `Info.plist`s, `Platforms/{iOS,MacCatalyst}/SceneDelegate.cs` (new), `package.sh`, `MIGRATIONS.md`, `devlog-001.md`, plus the packed `Mvvm.Nucleus.Maui.0.8.0.{nupkg,snupkg}`

## Summary
A tightly-scoped toolchain upgrade (0.7.0 → 0.8.0) with an exactly matching diff: version/TFM bumps in two csprojs, the documented Xcode 27 scene-lifecycle fast path in the sample, two documented `package.sh` flags, and a new `MIGRATIONS.md` section. Every claimed result was independently re-verified: builds green on all three non-Windows TFMs plus the Windows TFM, built-bundle plists carry the scene manifest with the correct delegate name, Catalyst floor reads 14.0, and the 0.8.0 nupkg is structurally identical to 0.7.0 (4 TFMs, DLL+XML, correct dependency groups). No High issues.

## What Works Well
- **Diff is surgical** — nothing outside the plan changed; the duplicated-Windows-TFM quirk was preserved rather than fixed, as instructed.
- **Scene adoption follows the fast path exactly**: `MauiUISceneDelegate` subclasses with `[Register("SceneDelegate")]` (name matches `UISceneDelegateClassName`), single-scene manifest, no `UISceneStoryboardFile`, `AppDelegate`/`MauiProgram` untouched.
- **Verification went beyond source**: built bundles were inspected directly (`plutil -p`), so the manifest is confirmed in the *shipped* artifact, not just the .plist.
- **Package continuity**: 0.8.0 ships the same 4-TFM DLL+XML shape as 0.7.0 with the correct MAUI 11.0.10 / CT.Maui 17.0.0 / CT.Mvvm 8.4.2 dependency groups — the 3-TFM pack in plan §7 was caught as a misread and corrected before shipping.
- **`MIGRATIONS.md`** gives consumers actionable steps, an OS-floor table, and an explicit "no popup API changes" note; **`devlog-001.md`** records all four tricky parts, the three deviations, and the known quirk — including the failed `CustomAfterMicrosoftCommonTargets` hook, which is useful institutional knowledge.
- `package.sh` changes carry a comment explaining *why* the two `.pri` flags exist, so they won't be "cleaned up" by mistake.

## Issues

### High
- None.

### Medium
- [ ] **Windows TFM was never built by a full Windows toolchain.** It compiles cleanly on macOS, but `.pri` expansion was skipped via `-p:AppxGeneratePriEnabled=false`. Risk is near-zero (Nucleus ships no XAML/.pri resources, and the 0.7.0 Windows build had the same shape), but if a Windows-only consumer hits an issue, "built on macOS" is the first thing to rule out. Suggest a Windows CI job or a one-time Windows-host build before publishing.

### Low
- [ ] **`package.sh` pins `IncludeProjectPriFile=false` globally.** Harmless today (the library has no `.pri`), but if a `.pri` resource is ever added to the library, it will be silently excluded from the package on *every* host, not just macOS. The comment covers intent; consider restricting the two flags to non-Windows hosts in a future pass.
- [ ] **Pre-existing duplicated Windows TFM** in the library csproj (base list + `IsOSPlatform('windows')` conditional). Flagged in the devlog; MSBuild dedupes so it's inert, but it's a tripwire for anyone editing TFMs later.
- [ ] **`MIGRATIONS.md` 0.8.0 section lists MAUI `11.0.10` as the floor**, matching the nuspec `>=` dependency. Fine, but note consumers on MAUI `11.0.x < 11.0.10` will be pulled up by NuGet — worth remembering if a support question comes in.
- [ ] The 0.8.0 nupkg/snupkg are gitignored (consistent with how 0.7.0 was handled) — they exist only as local artifacts; publishing is a separate manual step.

## Recommendation
[x] Ready for human review
[ ] Needs revision (see issues above)
