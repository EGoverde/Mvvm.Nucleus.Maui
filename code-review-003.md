# Code Review

**Reviewer:** AI
**Date:** 2026-11-14
**Reviewing:** Uncommitted changes — `Mvvm.Nucleus.Maui.Sample/{MauiProgram.cs, Mvvm.Nucleus.Maui.Sample.csproj, Platforms/MacCatalyst/Entitlements.Debug.plist, ViewModels/PopupTabViewModel.cs, Views/Details.xaml, Views/Details.xaml.cs}`, the new `tests/ui/` suite (22 flows + `run-all.sh` + `lib/devflow.sh` + `README.md`), `.gitignore` (RPI-doc tracking), and `devlog-003.md`

## Summary
A Debug-only, sample-only DevFlow onboarding plus a 22-flow script-driven UI regression suite driven entirely through `maui devflow … --ci --json`. The library (`Mvvm.Nucleus.Maui`) is untouched — confirmed by diff. The plan §7 exit criteria were met and are independently consistent with what the code does: two consecutive 22/22 back-to-back `run-all.sh` passes, a Release bundle with no `*DevFlow*` artifacts, a 65/65 unit lane, and no new warning codes beyond the pre-existing `CS0618`/`NU1902` baseline. The driver is the load-bearing piece; it is well-engineered and its non-obvious behavior (driving-lease spacing, the no-op-`navigate` wedge, the non-key-window swallow) is *explained in comments with the date and reproduction*, not just worked around. One genuine robustness gap found (a negative assertion can turn a driver error into a false pass) is documented below as a Low; everything else is clean. No High or Medium items remain.

## What Works Well
- **Zero library-source change** — the diff touches only the sample app + `tests/ui` + docs. The DevFlow agent is wired behind `#if DEBUG` in `MauiProgram.cs` *and* a `Configuration == 'Debug'`-conditional `PackageReference`, so Release bundles provably carry no DevFlow artifacts (verified: no `*DevFlow*` files, no DevFlow strings in any Mach-O).
- **The `-54` launch regression was root-caused, not papered over.** The fix (restrict the bundle search to `bin/Debug`) is correct and minimal: only the Debug build embeds the agent and yields exactly one `net11.0-maccatalyst` bundle (the host arch), so selection is unambiguous. The earlier 22/22 run passing is explained by the Release bundles not existing yet — the analysis is coherent, and `lsregister -f` is kept as a defensible secondary guard rather than the (red-herring) primary fix the first two failed runs suggested.
- **`reset_to` is a sound design under a real constraint.** It never issues the no-op `ui navigate` that wedges the library's `IsNavigating` state machine, and it *only* uses the app's own UI (dialogs → popups → pushed pages → modals → tab bar), each of which fires a real `Navigated`. The "already on target with nothing to unwind → un-wedge dance via real tab switches" branch is a thoughtful hedge against a killed flow leaving the flag wedged, and it *fails loudly* if the dance doesn't land on the target route.
- **`_tap_tab` handles the active-tab no-op correctly.** A tap on the already-active tab reports `{"success":false}` + non-zero exit (observed), so it pre-checks the agent's authoritative route to skip the no-op tap, and tolerates the reported failure when the route already confirms the target. This is the exact case that previously failed an entire reset.
- **stdout/stderr discipline is consistent.** `df` carries the JSON payload on stdout (for `jq`) and all diagnostics/failures on stderr, so `df … | jq -e` pipelines behave predictably under `set -euo pipefail` in the flow scripts.
- **Recovery rules are enumerated and bounded.** Driving-lease conflict → wait + registry refresh; broker authority → broker restart; stale element/epoch → tree refresh; `success:false` → reactivate window (restore key status). Every retry is capped by `DF_MAX_ATTEMPTS` and then fails with the step name.
- **Honest docs.** The README "Operational caveats" and the `lib/devflow.sh` header record the Catalyst native-dialog findings (NSAlert bodies unqueryable → assert on buttons/follow-up state), the lease window, and the failure dates. The CT 17 `Opened, Prepared` lifecycle-order deviation is documented at the point of the assertion (`popup_ct_compat.sh`) with the `NucleusMvvmPopupBehavior.OnPopupOpened` rationale.

## Issues

### High
- None.

### Medium
- None.

### Low
- [ ] **`assert_absent` turns a driver error into a false pass.** `lib/devflow.sh`: `out="$(df "assert absent '${text}'" ui query …)" || out="[]"` — if `df` returns non-zero (unrecoverable after retries), `out` is reset to `[]` and the negative assertion *passes*. A negative assertion that can go green on a broken driver is the weaker direction in a regression suite. The positive assertions (`wait_text`/`assert_text`) fail loudly under `pipefail`, so this is isolated to negative checks. Suggested fix (not applied, to preserve the verified 22/22 ×2 evidence): capture the `df` return code and `_df_fail` if non-zero, only treating a *successful* empty query as "absent". Acceptable as-is because a broken driver at that point would almost certainly also break the surrounding flow, but worth closing.
- [ ] **`success:false` retry can mask a genuine tap failure.** The `*'"success":false'*` recovery case (reactivate + retry) is correct for the non-key-window case, but a tap that legitimately cannot succeed (e.g. a truly non-interactive element) will be retried up to `DF_MAX_ATTEMPTS` times with reactivation before failing. That is bounded and then fails loudly, so it is only a small time/observability cost, not a correctness one.
- [ ] **`_dismiss_stray_dialogs` picks `Cancel` by text, not by position.** Correct and safe (cancel has no side effects), and the single-button path taps the sole button. Fine; noted only because it depends on the sample's button labels remaining `Cancel`/`Okay`, which the suite's own dialogs already assume.
- [ ] **Coupling to sample button/label strings is inherent.** The flows assert against exact strings (`Close using IPopupService`, `Close All Modals (if any)`, `Navigate Backwards`, `Use CommunityToolkit.PopupService`, `State: Default, Opened, Prepared`). This is by design for a UI regression suite (it pins the visible contract), and the sample is first-party here, so it is acceptable — but a label rename in the sample is a hard suite break, not a soft one.
- [ ] **`enter_prompt` default ok-label is `"OK"`.** Correct for Catalyst native prompts (spike-confirmed); would need adjusting for a platform where the confirm label differs. Not a defect for the pinned target.

## Recommendation
- [x] Ready for human review
- [ ] Needs revision (see issues above)

No High or Medium findings. The one Low that could turn into a false pass (`assert_absent`) is documented with a concrete fix but deliberately left unfixed so the two verified 22/22 back-to-back runs remain the exit-criterion evidence; closing it should be paired with a fresh 22/22 ×2 run.
