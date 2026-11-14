# Decision: Catalyst minimum raised to 17.0 for the .NET 11 / Xcode 27.2 upgrade

**Status:** Accepted
**Date:** 2026-11-13
**Deciders:** Erik

## Context
Moving both the `Mvvm.Nucleus.Maui` library and the `Sample` app from .NET 10 to .NET 11 on the installed **Xcode 27.2** toolchain (Apple 27 SDK). The Apple 27 SDK enforces deployment minima of **iOS 15.0** and **Catalyst 17.0 (macOS 14)**.

The project currently sets `SupportedOSPlatformVersion` to:
- iOS **15.0** (both projects) — already at the SDK floor → **no change needed**.
- Mac Catalyst **15.0** (both projects) — **below** the SDK floor → **must be raised to 17.0** to build `net11.0-maccatalyst` on this toolchain.

Catalyst 15.0 maps to macOS 12 and 16.0 to macOS 13. Raising to 17.0 therefore **drops macOS 12 and 13 Catalyst users**. Per the Xcode 27 migration guidance this is a lost-support change that needs an explicit decision, not something implied by "upgrade to .NET 11."

## Options

### Option 1: Accept Catalyst 17.0 and proceed to .NET 11
Raise `SupportedOSPlatformVersion` (maccatalyst) from 15.0 to 17.0 in **both** projects and complete the .NET 11 + scene migration.

**Pros:**
- Required to build `net11.0-maccatalyst` on the installed Xcode 27.2 SDK — no workaround.
- Aligns with the .NET 11 GA toolchain, current Xcode, and CommunityToolkit.Maui 17.0.0 (which targets `net11.0-maccatalyst27.2`).
- Removes an out-of-support macOS tier.

**Cons:**
- Drops macOS 12 and 13 Catalyst users.
- Affects library consumers, not just the sample (the library's maccatalyst minimum changes too).

### Option 2: Keep Catalyst 15.0 and stay on .NET 10
Do not raise the minimum; retain the current .NET 10 / older-Xcode toolchain.

**Pros:**
- Preserves macOS 12/13 Catalyst support.

**Cons:**
- Blocks the entire .NET 11 + scene-lifecycle work that was requested.
- .NET 10 is superseded; no scene-adoption path on the installed toolchain.

## Decision
**Accepted: Option 1** — Erik approved (2026-11-13) raising the Catalyst minimum to 17.0 in both projects and proceeding to .NET 11, including dropping macOS 12/13 Mac Catalyst users.

## Consequences
### Positive
- Clean, fully-supported .NET 11 + Xcode 27.2 build for all Apple TFMs.
- Consistent, current dependency set (MAUI 11.0.10, CommunityToolkit.Maui 17.0.0).

### Negative
- macOS 12 and 13 Mac Catalyst users can no longer run builds from this repo.
- Library consumers moving to this package version inherit the 17.0 minimum (relevant to the library version-bump decision).
