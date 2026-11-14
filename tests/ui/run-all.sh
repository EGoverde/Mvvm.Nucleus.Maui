#!/usr/bin/env bash
#
# run-all.sh — entry point for the Mvvm.Nucleus.Maui UI regression suite.
#
# Pipeline (order matters — sequential execution is a DevFlow constraint, spike #1):
#   1. build the sample (net11.0-maccatalyst, Debug — the only configuration that
#      embeds the DevFlow agent)
#   2. launch the app bundle
#   3. ensure the broker + wait for the agent
#   4. execute the flow scripts, sequentially, fail-fast on the first failure
#   5. teardown: quit the app; leave the broker daemon running (stopping it is the
#      recovery step, not the teardown step)
#
# Prerequisites: .NET 11 SDK, MAUI workloads for net11.0-maccatalyst, the
# `maui` DevFlow CLI, `jq`. See README.md.

set -uo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
UI="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# shellcheck source=lib/devflow.sh
source "${UI}/lib/devflow.sh"

cd "$ROOT"

# --- 1. build ------------------------------------------------------------------
echo "== 1/5 build (net11.0-maccatalyst, Debug) =="
if ! dotnet build Mvvm.Nucleus.Maui.Sample -f net11.0-maccatalyst -c Debug -v minimal; then
    echo "FATAL: build failed" >&2
    exit 1
fi

# --- 2. launch ------------------------------------------------------------------
echo "== 2/5 launch =="
# Kill any stale instance from a previous run/session. A zombie registration
# makes `open` refuse with LaunchServices error -54, and a leftover (background,
# non-key-window) instance would otherwise be the one driven — its synthesized
# taps are silently swallowed. Hard-kill, then wait for full exit.
APP_NAME="Mvvm.Nucleus.Maui.Sample"
pkill -f "$APP_NAME" 2>/dev/null || true
for _ in 1 2 3 4 5 6 7 8 9 10; do
    pgrep -f "$APP_NAME" >/dev/null || break
    sleep 1
done
if pgrep -f "$APP_NAME" >/dev/null; then
    pkill -9 -f "$APP_NAME" 2>/dev/null || true
    sleep 2
fi
# Restrict the search to bin/Debug: ONLY the Debug build embeds the DevFlow agent,
# and the Release build (same net11.0-maccatalyst TFM) produces additional .app
# bundles (maccatalyst-x64 / maccatalyst-arm64 / universal) that a broader
# `find ... | head -1` can pick non-deterministically — an x64 Release bundle
# cannot launch on an arm64 host (LaunchServices error -54, the run-2 / 2026-11-14
# failure). The Debug build yields exactly one net11.0-maccatalyst bundle (the host
# arch), so this is unambiguous.
APP="$(find Mvvm.Nucleus.Maui.Sample/bin/Debug -path '*net11.0-maccatalyst*' -name 'Mvvm.Nucleus.Maui.Sample.app' -maxdepth 6 -type d | head -1)"
if [[ -z "$APP" ]]; then
    echo "FATAL: sample .app bundle not found" >&2
    exit 1
fi
# Absolute path: LaunchServices and `open` both resolve more reliably against it.
APP="$(cd "$(dirname "$APP")" && pwd)/$(basename "$APP")"
# Force-register the freshly-built bundle with LaunchServices so `open` resolves
# the bundle ID to THIS path. The LS database accumulates stale same-bundle-ID
# registrations across .NET SDK upgrades (net9/net10 build artifacts observed),
# and `open` then resolves to a stale path it cannot launch -> error -54
# (procNotFound). `lsregister -f` re-points the registration at the current
# bundle (idempotent; the real fix for the -54 that failed runs on 2026-11-14).
LSREG="/System/Library/Frameworks/CoreServices.framework/Frameworks/LaunchServices.framework/Support/lsregister"
"$LSREG" -f "$APP" 2>/dev/null \
    || _df_log "warning: lsregister -f failed (continuing; 'open' may still work)"
# LaunchServices (`open`) is REQUIRED: it starts the app with a key window,
# which is the only state that accepts DevFlow-synthesized touches (README
# "Operational caveats" #1). Retry once if the first attempt is refused.
if ! open "$APP"; then
    sleep 3
    open "$APP" || { echo "FATAL: 'open' refused to launch the app bundle (LaunchServices error)" >&2; exit 1; }
fi

# --- 3. broker + agent ----------------------------------------------------------
echo "== 3/5 broker + agent =="
ensure_broker || { echo "FATAL: broker unavailable" >&2; exit 1; }
wait_agent 90 || { echo "FATAL: agent never connected" >&2; exit 1; }
# Guard against a stale registration: the agent's process must actually be alive.
AGENT_PID="$(maui devflow wait --timeout 5 --json 2>/dev/null | jq -r '.processId // empty' 2>/dev/null)"
if [[ -z "$AGENT_PID" ]] || ! kill -0 "$AGENT_PID" 2>/dev/null; then
    echo "FATAL: agent registered but its process (${AGENT_PID:-unknown}) is not alive" >&2
    exit 1
fi
_df_log "agent connected (pid ${AGENT_PID})"

# --- 4. flows (sequential, fail-fast) -------------------------------------------
echo "== 4/5 flows =="
FLOWS=(
    # Navigation (start on //Tabs/NavigationTab)
    nav_by_type
    nav_by_route
    nav_modal
    nav_modal_stack
    nav_params_prompt
    nav_query
    nav_background_thread
    nav_debounce
    nav_switch_tabs
    # Popups (start on //Tabs/PopupTab)
    popup_simple
    popup_advanced
    popup_singleton
    popup_ct_compat
    popup_v1_compat
    popup_background_thread
    # Dialogs (start on //Tabs/DialogTab)
    dialog_alert
    dialog_alert_background
    dialog_confirm
    dialog_prompt
    dialog_action_sheet
    # Details (start on //Tabs/NavigationTab)
    details_confirm_navigation
    details_back_twice
)

pass=0
failed=()
for flow in "${FLOWS[@]}"; do
    echo
    echo "--- ${flow} ---"
    if bash "${UI}/${flow}.sh"; then
        pass=$(( pass + 1 ))
        echo "PASS ${flow}"
    else
        echo "FAIL ${flow}"
        failed+=("$flow")
        break # fail-fast
    fi
done

# --- 5. teardown -----------------------------------------------------------------
echo
echo "== 5/5 teardown =="
# Deterministic teardown: graceful quit -> wait for full exit -> SIGKILL.
# A leftover instance breaks the NEXT run's launch (LaunchServices error -54)
# and, if it survives in the background, its non-key window silently swallows
# DevFlow-synthesized taps (the run-2 failure mode, 2026-11-14).
osascript -e 'quit app "Mvvm.Nucleus.Maui.Sample"' >/dev/null 2>&1 || true
for _ in 1 2 3 4 5 6; do
    pgrep -f "Mvvm.Nucleus.Maui.Sample" >/dev/null || break
    sleep 1
done
if pgrep -f "Mvvm.Nucleus.Maui.Sample" >/dev/null; then
    pkill -9 -f "Mvvm.Nucleus.Maui.Sample" 2>/dev/null || true
    sleep 1
fi
# The broker daemon is left running on purpose (stopping it is the recovery step).

echo
total="${#FLOWS[@]}"
echo "RESULT: ${pass}/${total} passed${failed:+; failed at: ${failed[0]}}"
if (( ${#failed[@]} == 0 )); then
    exit 0
fi
exit 1
