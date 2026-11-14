# UI regression suite (DevFlow)

Script-driven UI regression suite for the `Mvvm.Nucleus.Maui` sample app, driven
against the running **Mac Catalyst** app through the DevFlow in-app agent
(`maui devflow … --ci --json`).

Companion to the unit lane in `tests/Mvvm.Nucleus.Maui.UnitTests`.

## Prerequisites

- macOS (the only host that can run Mac Catalyst).
- .NET 11 SDK + MAUI workloads for `net11.0-maccatalyst`.
- The `maui` DevFlow CLI — `~/.dotnet/tools/maui`.
- `jq` (preinstalled on GitHub macOS runners; locally: `brew install jq`).
- `bash` (macOS stock 3.2 works — the suite avoids bash-4-only features).

The sample app embeds the DevFlow agent **Debug-only**
(`Microsoft.Maui.DevFlow.Agent` with `Condition="'$(Configuration)' == 'Debug'"`
in the sample csproj, registered in `#if DEBUG`, and
`com.apple.security.network.server` in
`Platforms/MacCatalyst/Entitlements.Debug.plist`).

## How to run

```bash
tests/Mvvm.Nucleus.Maui.UITests/run-all.sh
```

The runner builds the sample (Debug, `net11.0-maccatalyst`), launches the bundle
via LaunchServices (`open` — see "Operational caveats"), starts the DevFlow
broker, waits for the agent, runs a recovery pass (land on a known healthy tab),
executes the 22 flows in **4 tab-ordered groups** (one `maui devflow batch`
session per group) and quits the app (the broker daemon is left running —
stopping it is a recovery step, not a teardown step).

Fail-fast is at the **group** level: a group gets up to 3 escalating attempts
(run as-is → recovery pass → app relaunch); a group that fails all three is
attributed to a specific flow and the suite stops there.
Per-flow results are reported as `PASS` / `FAIL` / `SKIP`.

Skip the build when iterating (the Debug bundle must already exist):

```bash
DF_SKIP_BUILD=1 tests/Mvvm.Nucleus.Maui.UITests/run-all.sh
```

Run a single flow standalone (app running + agent connected):

```bash
bash -c 'source tests/Mvvm.Nucleus.Maui.UITests/lib/devflow.sh; source tests/Mvvm.Nucleus.Maui.UITests/nav_by_type.sh
         printf "%s\n" "${DF_CMDS[@]}" | maui devflow batch --delay 300'
```

## Timing model

Two independent guards bound how fast mutations can be issued:

1. **DevFlow mutation lease** — app-wide, per *session*. A mutating call
   (tap/fill/alert-dismiss) claims an app-wide lease for the calling session and
   each mutation refreshes it. The lease expires ~10s after the holder's last
   mutation (6s still conflicts; 10s is clean). Each `maui devflow <cmd>`
   process is its own session, so two separate CLI processes mutating <10s apart
   conflict (`"Another DevFlow session is driving this app (MAUI CLI)"`).
   `maui devflow batch` is ONE long-lived session: its mutations keep refreshing
   the lease, so an entire group of flows runs back-to-back with **no**
   per-step lease wait. Only *between* two sessions must the previous holder's
   lease expire first (the runner sleeps `DF_LEASE_GAP` before every batch).
2. **Library navigation debounce** (app-side, `NucleusMvvmCore`) —
   `NucleusMvvmOptions.IgnoreNavigationWithinMilliseconds` (250ms) +
   `IgnoreNavigationWhenInProgress`. A navigation request arriving <250ms after
   the previous one is ignored, and a request that lands after the previous one
   completed but inside the window can leave `IsNavigating` stuck `true`
   (see `NavigationService.SetIsNavigatingOrIgnoreRequest`), rejecting **all**
   further navigation until the app restarts. The driver therefore keeps a
   uniform `DF_BATCH_DELAY` (300ms) floor between every pair of batch commands —
   non-navigation mutations aren't debounced, so the floor only costs a little
   time and makes the rule trivially uniform.

Consequences, baked into the design:

- Flow scripts are **pure declarations** (see the emitter table below); the
  runner assembles each group's declarations into a static command file and
  executes it in one batch session, so intra-group pacing is `DF_BATCH_DELAY`
  only.
- Tab switches are emitted **only when the tab actually changes** (a tap on the
  already-active tab is a DevFlow no-op that reports `success:false`); the
  assembler tracks each flow's declared start/end tab, so e.g. `nav_switch_tabs`
  (ends on PopupTab) hands straight into the popup group with no switch at all.
- The 3-attempt escalation ends with an **app relaunch** precisely because an
  `IsNavigating` wedge (or any app-side state) is the one failure class that no
  amount of driver retry can clear.

## Driver API (`lib/devflow.sh`)

Flow scripts source `lib/devflow.sh` and use the **emitter** helpers (they append
to the group's command file via `DF_CMDS`; nothing is executed at declaration
time):

| Helper | Purpose (emits) |
|---|---|
| `flow_begin <tab>` | declare the tab this flow must start on (assembler inserts a tab switch if the previous flow ended elsewhere) |
| `flow_end <tab>` | declare the tab this flow ends on (default: start tab) |
| `tap_text <label>` | `MAUI tap --text "<label>"` — text-based tap scoped to `Button`; labels in this suite are unique per presented context |
| `wait_text <text>` / `assert_text` | `MAUI query --text … --wait-until exists` |
| `wait_dialog <btn>` / `assert_dialog` | same, scoped to `DialogAction` — i.e. the alert/prompt/action sheet is up |
| `dismiss_dialog <btn>` | `MAUI tap --text "<btn>" --type DialogAction` — tap the dialog's native button |
| `enter_prompt <value> [ok]` | wait for the native `UITextField`, `MAUI fill`, then tap `OK` (Catalyst prompt buttons are `OK`/`Cancel`) |
| `assert_absent <text>` | `MAUI query --text … --wait-until gone` |

Recovery/setup path (single-CLI, used by `run-all.sh`'s pre-suite and
mid-suite recovery, not the fast path):

| Helper | Purpose |
|---|---|
| `df <step> <args…>` | `maui devflow <args> --ci --json` with conflict-aware retry: driving-lease conflict → wait + retry; broker lease-authority hiccup → broker stop/start + retry; stale element/epoch → `ui tree` refresh + retry; `success:false` → reactivate the app window + retry |
| `ensure_broker` / `wait_agent` | broker + agent health |
| `reset_to <tab>` | RECOVERY PASS only: land on a known, healthy tab using the app's own UI — dismiss stray dialogs, close open popups, pop pushed pages, close modals, then tab-bar switch (skips the tap when the route already confirms the target tab); single-CLI, lease-aware via `df` |
| `batch_run <cmdfile>` | execute a command file in ONE `maui devflow batch --delay $DF_BATCH_DELAY` session (JSONL on stdout, fail-fast) |

Tunables via environment:

| Variable | Default | Meaning |
|---|---|---|
| `DF_BATCH_DELAY` | 300 | ms between two batch commands; **must stay above the library's 250ms navigation debounce** |
| `DF_LEASE_GAP` | 10 | seconds waited before each batch so the previous session's mutation lease has expired (the lease TTL) |
| `DF_WAIT_TIMEOUT` | 10 | default `--wait-until` timeout (seconds) |
| `DF_MAX_ATTEMPTS` | 4 | attempts per single-CLI mutating call (recovery path) |
| `DF_RETRY_WAIT_SECS` | 3 | wait before retrying a single-CLI lease conflict |
| `DF_SKIP_BUILD` | 0 | set to `1` to skip the dotnet build (iteration only; the Debug bundle must exist) |

## Operational caveats

1. **Launch via LaunchServices (`open`), never by executing the bundle binary
   directly.** A Catalyst app started from a terminal ends up with a non-key
   window that silently swallows DevFlow-synthesized touches — the CLI reports
   `{"success":true}`, but the button's command never runs. Side effect: the
   app's `AddDebug()` log lines (stderr) are only visible in that
   terminal-launched mode, so the running suite has no live app log; flows
   assert on UI state instead. `run-all.sh` handles this end to end:
   - **Select the right bundle, or `open` fails with error -54
     (`procNotFound`).** Both the Debug and Release builds of
     `net11.0-maccatalyst` emit a `Mvvm.Nucleus.Maui.Sample.app` bundle
     (Release adds `maccatalyst-x64`, `maccatalyst-arm64`, and a universal
     one). A broad `find … | head -1` then non-deterministically grabs a
     **Release x64** bundle, which cannot launch on an arm64 host → -54.
     `run-all.sh` therefore restricts the search to `bin/Debug` — only the
     Debug build embeds the DevFlow agent, and it yields exactly one
     `net11.0-maccatalyst` bundle (the host arch), so the selection is
     unambiguous. It also runs
     `…/LaunchServices…/lsregister -f <bundle>` before `open` to defend
     against stale same-bundle-ID LS registrations (idempotent).
   - It hard-kills any stale app instance before launch, launches via `open`
     with one retry, verifies the agent's `processId` is alive after
     connecting, and tears down deterministically (graceful quit → wait loop
     → SIGKILL).
   - The `df` recovery driver also self-recovers a non-key window: if a
     tap/fill is reported `{"success":false}` (the touch was synthesized but
     not accepted) it reactivates the app to make its window key again and
     retries.
2. **Active-tab no-op tap is a failure.** Tapping the already-active tab is a
   DevFlow no-op that reports `{"success":false}` and exits non-zero — inside a
   batch that would fail the whole session. The assembler therefore emits a tab
   switch only when a flow's declared start tab differs from the tab the
   previous flow ended on, and `reset_to` checks the agent's route before
   tapping a tab.
3. **CT 17 vs nucleus popup lifecycle order.** For `AdvancedPopup` shown via
   the CT 17 `IPopupService`, `NucleusMvvmPopupBehavior.OnPopupOpened` fires
   `OnOpened()` **before** `Prepare()`, so the state label reads
   `State: Default, Opened, Prepared` — the reverse of the nucleus-service path
   (`State: Default, Prepared, Opened`, see `popup_advanced.sh`). Both orders
   are deterministic library behavior, and each flow pins its own order.
