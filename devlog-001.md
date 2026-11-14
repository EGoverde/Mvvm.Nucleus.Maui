# Devlog: .NET 11 / MAUI 11 / CommunityToolkit.Maui 17 Upgrade (0.8.0)

**Date:** 2026-11-13
**Implementing:** plan.md

## What Was Done
- Library (`Mvvm.Nucleus.Maui`) and Sample csprojs: all TFMs `net10.0-*` → `net11.0-*`; package version `0.7.0` → `0.8.0`; dependencies raised to MAUI `11.0.10`, CommunityToolkit.Maui `17.0.0`, CommunityToolkit.Mvvm `8.4.2` (Sample: Logging.Debug `11.0.0`).
- Sample adopted the Xcode 27 UIKit scene lifecycle fast path: `Platforms/iOS/SceneDelegate.cs` + `Platforms/MacCatalyst/SceneDelegate.cs` (`MauiUISceneDelegate`) and a single-scene `UIApplicationSceneManifest` in both Info.plists (`plutil -lint` clean; `AppDelegate`/`MauiProgram` untouched).
- Built library + sample cleanly (0 errors) for `net11.0-android`, `net11.0-ios`, `net11.0-maccatalyst`; library `net11.0-windows10.0.19041.0` also builds on macOS (see Tricky Parts 4).
- Verified built bundles: scene manifest + `UISceneDelegateClassName = SceneDelegate` present in both iOS and Catalyst `Info.plist`; Catalyst `LSMinimumSystemVersion` = `14.0`.
- Lifecycle audit (`audit-lifecycle.mjs`) reports `simple-migration-candidate` with SceneDelegate + manifest for both heads — expected clean result.
- Packed `Mvvm.Nucleus.Maui.0.8.0.nupkg`/`.snupkg` to repo root; verified: 4 TFMs (`net11.0-android37.0`, `-ios27.2`, `-maccatalyst27.2`, `-windows10.0.19041`), DLL+XML only per TFM (same shape as 0.7.0), nuspec deps MAUI 11.0.10 / CT.Maui 17.0.0 / CT.Mvvm 8.4.2.
- `MIGRATIONS.md`: new top section "Nucleus 0.8.0" (requirements, migration steps, raised OS floors, CT 13→17 note, scene-lifecycle adoption, package contents).
- **CommunityToolkit.Maui 17.0.0 required zero signature fixes** — the popup surface Nucleus implements against (`IPopupService`, `Popup`, `Popup<T>`, `PopupOptions`, `IPopupPrepare(Async)`) is unchanged. The only warnings are the pre-existing 0.7.0 `IPrepare(Async)`-vs-obsolete-`IInitializable(Async)` pair. Nothing to enumerate in MIGRATIONS.md beyond "no changes required".

## Tricky Parts
1. **`android` workload install required sudo**: `dotnet` is root-owned at `/usr/local/share/dotnet`; `NETSDK1147` blocked even `-f net11.0-ios` builds because restore evaluates all TFMs. User ran `sudo dotnet workload install android` manually.
2. **`.NET 11 / Android SDK 37.2.31 enforces min API 24** (`XA4216`, hardcoded in `Microsoft.Android.Build.Tasks.dll`): projects had `SupportedOSPlatformVersion` 21.0 → raised to 24.0 in both csprojs (user-approved, drops Android 5/6).
3. **`dotnet pack -p:TargetFrameworks` is unparseable**: the MSBuild CLI splits `-p:` values on `;` (`MSB1006: Property is not valid`), and `dotnet pack` doesn't accept response files. This whole line of work (env vars, `@rsp`, `CustomAfterMicrosoftCommonTargets` temp-import) turned out to be unnecessary — see Deviations 1 — but note for the future: the `CustomAfterMicrosoftCommonTargets` import hook **did not fire at all** on SDK 11.0.100 even when the file existed (verified via `-getProperty` + `-pp`); don't rely on it for evaluation-time overrides.
4. **Windows TFM build fails on macOS with CT.Maui 17**: the package's Windows assets ship `.pri` content whose expansion invokes `makepri.exe` (Win32 binary, `Exec format error`). Fix: `-p:AppxGeneratePriEnabled=false` (skips expansion; build then succeeds) and `-p:IncludeProjectPriFile=false` (prevents `NU5026` — NuGet's `_AddPriFileToPackBuildOutput` unconditionally packs the project `.pri` otherwise). The Nucleus package ships DLL+XML only, identical to 0.7.0, so no content is lost. Both flags are now in `package.sh` with an explanatory comment.

## Decisions Made
- **Android floor 21.0 → 24.0** in both csprojs: toolchain-enforced on .NET 11 SDK (API 24 / Android 7.0 minimum), same pattern as the approved Catalyst 17.0 decision. Recorded in MIGRATIONS.md.
- **0.8.0 ships all 4 TFMs including Windows** — same shape as 0.7.0 (see Deviations 1).

## Deviations from Plan
1. **Plan §7 assumed a 3-TFM pack (android/ios/maccatalyst only, "no Windows assets") — misread, corrected.** The "no Windows" scope applies to the *sample* only; the library package is "as before". Verified against `package.sh` and the 0.7.0 nupkg (4 TFMs incl. `net10.0-windows10.0.19041`). The "Windows asset gap" consequence noted in research/plan is therefore moot.
2. **Android minimum raised to 24.0** — the plan matrix assumed 21.0 could be kept; the .NET 11 Android SDK enforces 24.0. Approved by the user mid-implementation (same pattern as the pre-approved Catalyst 17.0 floor).
3. **`package.sh` now passes two extra properties** (`AppxGeneratePriEnabled=false`, `IncludeProjectPriFile=false`) — required on non-Windows hosts since CT.Maui 17's Windows `.pri` assets can't be expanded with `makepri.exe`.

## Known Quirks (pre-existing, not fixed)
- Library csproj lists `net11.0-windows10.0.19041.0` in the base `TargetFrameworks` **and** again in the `IsOSPlatform('windows')` conditional line — duplicate entry carried over from 0.7.0. Harmless (MSBuild dedupes) but should be cleaned up eventually.

## Next Steps
- None for implementation. Remaining process: gated self-review of the changes (`/rpi-reviewing-gated`), then commit and publish the 0.8.0 package.
