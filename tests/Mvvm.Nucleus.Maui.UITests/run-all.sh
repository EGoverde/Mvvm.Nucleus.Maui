#!/usr/bin/env bash
#
# run-all.sh — entry point for the Mvvm.Nucleus.Maui UI regression suite.
#
# EXECUTION MODEL (grouped batches; see lib/devflow.sh for the timing rationale)
# ---------------------------------------------------------------------------
# The suite's 22 flows are split into 4 groups (one per Shell tab). A group is
# assembled into a static file of `MAUI ...` batch commands and executed in ONE
# `maui devflow batch` session (one DevFlow session = one mutation-lease holder,
# whose mutations keep refreshing the lease), so flows inside a group run
# back-to-back with only DF_BATCH_DELAY (300ms) between commands. Between two
# groups we sleep DF_LEASE_GAP (10s) so the previous session's lease has
# expired before the next session's first mutation.
#
# Pipeline (order matters — sequential execution is a DevFlow constraint):
#   1. build the sample (net11.0-maccatalyst, Debug — the only configuration that
#      embeds the DevFlow agent; skippable with DF_SKIP_BUILD=1 for iteration)
#   2. launch the app bundle (LaunchServices, key window required)
#   3. ensure the broker + wait for the agent + verify the agent process is alive
#   4. recovery pass: land on a known, healthy tab (single-CLI, conflict-aware)
#   5. run the groups, each with up to 3 escalating attempts:
#        attempt 1: run the batch as-is
#        attempt 2: single-CLI recovery pass (stray popups/modals/dialogs, tab)
#        attempt 3: relaunch the app, wait for the agent, recovery pass, re-run
#      A group failure is attributed to the flow containing the failing command
#      (per-flow PASS/FAIL/SKIP reporting); the suite is fail-fast at the group
#      level (a later group starts from a broken app state at best).
#   6. teardown: quit the app; leave the broker daemon running (stopping it is
#      the recovery step, not the teardown step)
#
# Prerequisites: .NET 11 SDK, MAUI workloads for net11.0-maccatalyst, the
# `maui` DevFlow CLI, `jq`. See README.md.

set -uo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
UI="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# shellcheck source=lib/devflow.sh
source "${UI}/lib/devflow.sh"

WORK="$(mktemp -d "${TMPDIR:-/tmp}/dfsuite.XXXXXX")"
trap 'rm -rf "$WORK"' EXIT

cd "$ROOT"

# --- 1. build ------------------------------------------------------------------
if [[ "${DF_SKIP_BUILD:-0}" == "1" ]]; then
    echo "== 1/6 build: SKIPPED (DF_SKIP_BUILD=1) =="
else
    echo "== 1/6 build (net11.0-maccatalyst, Debug) =="
    if ! dotnet build Mvvm.Nucleus.Maui.Sample -f net11.0-maccatalyst -c Debug -v minimal; then
        echo "FATAL: build failed" >&2
        exit 1
    fi
fi

# --- 2. launch ------------------------------------------------------------------
echo "== 2/6 launch =="
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
launch_and_wait_agent() {
    ensure_broker || { echo "FATAL: broker unavailable" >&2; return 1; }
    wait_agent 90 || { echo "FATAL: agent never connected" >&2; return 1; }
    # Guard against a stale registration: the agent's process must actually be alive.
    local agent_pid
    agent_pid="$(maui devflow wait --timeout 5 --json 2>/dev/null | jq -r '.processId // empty' 2>/dev/null)"
    if [[ -z "$agent_pid" ]] || ! kill -0 "$agent_pid" 2>/dev/null; then
        echo "FATAL: agent registered but its process (${agent_pid:-unknown}) is not alive" >&2
        return 1
    fi
    _df_log "agent connected (pid ${agent_pid})"
    return 0
}

# relaunch_app — kill + relaunch the sample and wait for a healthy agent.
# Used as the final escalation when a group has failed twice (covers app-side
# wedges like a stuck navigation flag that only a restart clears).
relaunch_app() {
    pkill -f "$APP_NAME" 2>/dev/null || true
    for _ in 1 2 3 4 5 6 7 8 9 10; do
        pgrep -f "$APP_NAME" >/dev/null || break
        sleep 1
    done
    if pgrep -f "$APP_NAME" >/dev/null; then
        pkill -9 -f "$APP_NAME" 2>/dev/null || true
        sleep 2
    fi
    "$LSREG" -f "$APP" 2>/dev/null || true
    open "$APP" || { _df_log "relaunch_app: 'open' refused"; return 1; }
    launch_and_wait_agent || return 1
    reset_to NavigationTab || { _df_log "relaunch_app: recovery pass reported problems"; return 1; }
    return 0
}
echo "== 3/6 broker + agent =="
launch_and_wait_agent || exit 1

# --- helpers: flow declaration + group assembly ---------------------------------

# flow_decl <flow> — run a flow script's declarations in a subshell and print
# them as records:
#   BEGIN\t<start tab>
#   END\t<end tab>        (default: start tab)
#   CMD\t<batch command>  (one per line, in order)
# The flow scripts are pure declarations (see lib/devflow.sh emitter helpers),
# so executing them is side-effect-free and cheap.
flow_decl() {
    local flow="$1"
    (
        set -u
        . "${UI}/lib/devflow.sh"
        . "${UI}/${flow}.sh"
        printf 'BEGIN\t%s\n' "${FLOW_START_TAB:-}"
        printf 'END\t%s\n' "${FLOW_END_TAB:-${FLOW_START_TAB:-}}"
        if [[ -n "${DF_CMDS[@]+x}" ]]; then
            local c
            for c in "${DF_CMDS[@]}"; do
                printf 'CMD\t%s\n' "$c"
            done
        fi
    )
}

# assemble_group <label> <start tab> <flow...>
# Appends the group's commands to $1's cmdfile ($WORK/cmd-<label>):
#   - a `MAUI tap Tab_<tab>` line before any flow whose start tab differs from
#     the tab the previous flow ended on (an active-tab no-op tap is a failure,
#     so switches are only emitted when the tab actually changes)
# and records per-flow ranges in GN/G_NAMES/G_RSTART for result attribution.
assemble_group() {
    local label="$1" start_tab="$2"; shift 2
    local cmdfile="$WORK/cmd-${label}"
    local out begin end flow
    : > "$cmdfile"
    GN=0; G_NAMES=(); G_RSTART=()

    local current="$start_tab"
    for flow in "$@"; do
        if ! out="$(flow_decl "$flow")"; then
            _df_fail "assemble_group ${label}: flow '${flow}' failed to declare (bash syntax/source error)"
            return 1
        fi
        begin="$(printf '%s\n' "$out" | sed -n 's/^BEGIN\t//p' | head -1)"
        end="$(printf '%s\n' "$out" | sed -n 's/^END\t//p' | head -1)"
        if [[ -z "$begin" || -z "$end" ]]; then
            _df_fail "assemble_group ${label}: flow '${flow}' is missing flow_begin"
            return 1
        fi
        local nlines
        nlines=$(wc -l < "$cmdfile")
        if [[ "$current" != "$begin" ]]; then
            printf 'MAUI tap Tab_%s\n' "$begin" >> "$cmdfile"
            nlines=$(( nlines + 1 ))
            _df_log "group ${label}: tab switch to ${begin} before ${flow}"
            current="$begin"
        fi
        G_NAMES[GN]="$flow"
        G_RSTART[GN]=$nlines
        printf '%s\n' "$out" | sed -n 's/^CMD\t//p' >> "$cmdfile"
        GN=$(( GN + 1 ))
        current="$end"
    done
    return 0
}

# parse_group <outfile> <total> — inspect the batch JSONL output.
# Sets: PG_FAILED (0/1), PG_FAIL_FLOW, PG_FAIL_CMD, PG_FAIL_OUT, PG_FAIL_LINE.
parse_group() {
    local outfile="$1" total="$2"
    local line lineno=0 first_fail=-1 fail_line=""
    PG_FAILED=0; PG_FAIL_FLOW=""; PG_FAIL_CMD=""; PG_FAIL_OUT=""; PG_FAIL_LINE=""
    while IFS= read -r line; do
        case "$line" in
            *'"exit_code":0'*) : ;;
            *) first_fail=$lineno; fail_line="$line"; break ;;
        esac
        lineno=$(( lineno + 1 ))
    done < "$outfile"
    if (( first_fail < 0 && lineno == total )); then
        return 0
    fi
    PG_FAILED=1
    if (( first_fail < 0 )); then
        PG_FAIL_FLOW="(unknown)"
        PG_FAIL_CMD="(no JSONL output — the batch session itself failed)"
        PG_FAIL_OUT="$(head -c 400 "$outfile" 2>/dev/null)"
        return 1
    fi
    if [[ -n "$fail_line" ]]; then
        PG_FAIL_CMD="$(printf '%s' "$fail_line" | jq -r '.command // empty' 2>/dev/null)"
        PG_FAIL_OUT="$(printf '%s' "$fail_line" | jq -r '.output // empty' 2>/dev/null)"
    fi
    # Attribute the failing line (0-based) to the flow whose range contains it.
    local i
    PG_FAIL_FLOW="${G_NAMES[GN-1]}"
    for (( i = 0; i < GN; i++ )); do
        if (( G_RSTART[i] <= first_fail )); then
            PG_FAIL_FLOW="${G_NAMES[i]}"
        else
            break
        fi
    done
    return 1
}

# --- 4. recovery pass (single-CLI, pre-suite) -----------------------------------
echo "== 4/6 recovery pass =="
if ! reset_to NavigationTab; then
    echo "FATAL: recovery pass could not land on NavigationTab" >&2
    exit 1
fi

# --- 5. groups -------------------------------------------------------------------
echo "== 5/6 groups =="

# Flow order within a group follows the original suite order (run-2026-11-14).
# Groups are tab-ordered so that, where a flow ends on another tab
# (nav_switch_tabs -> PopupTab), the next group needs no switch at all.
GROUP_LABELS=( Navigation Popup Dialog Details )
GROUP_FLOWS_0=( nav_by_type nav_by_route nav_modal nav_modal_stack nav_params_prompt nav_query nav_background_thread nav_debounce nav_switch_tabs )
GROUP_FLOWS_1=( popup_simple popup_advanced popup_singleton popup_ct_compat popup_v1_compat popup_background_thread )
GROUP_FLOWS_2=( dialog_alert dialog_alert_background dialog_confirm dialog_prompt dialog_action_sheet )
GROUP_FLOWS_3=( details_confirm_navigation details_back_twice )

# Per-flow results: "name:PASS|FAIL|SKIP"
RESULTS=()
SUITE_FAILED=0

# run_group <label> <flows...> — assemble + execute one group (3 escalating
# attempts); records per-flow results in RESULTS.
run_group() {
    local label="$1"; shift
    local flows=( "$@" )
    local outfile="$WORK/out-${label}"
    local errfile="$WORK/err-${label}"
    local attempt=1 group_ok=1

    while (( attempt <= 3 )); do
        # Start-tab for this attempt's assembly:
        #  - attempt 1: the tab the PREVIOUS group ended on (NavigationTab for
        #    the first group — the pre-suite recovery pass guarantees it)
        #  - attempt 2/3: NavigationTab — the recovery pass / relaunch always
        #    lands the app there, so the assembler inserts the needed switch.
        local start_tab
        if (( attempt == 1 )); then
            start_tab="$RUN_PREV_END"
        else
            start_tab="NavigationTab"
        fi
        if ! assemble_group "$label" "$start_tab" "${flows[@]}"; then
            group_ok=0; PG_FAIL_FLOW=""; break
        fi
        local total
        total=$(wc -l < "$WORK/cmd-${label}")
        local t0 t1
        t0=$(date +%s)
        # Lease guard: the previous mutating session (recovery pass or prior
        # group's batch) must have let its lease expire before our first mutation.
        _df_log "group ${label}: waiting ${DF_LEASE_GAP}s for the previous session's lease to expire (attempt ${attempt}/3)"
        sleep "$DF_LEASE_GAP"
        batch_run "$WORK/cmd-${label}" > "$outfile" 2> "$errfile" || true
        t1=$(date +%s)

        if parse_group "$outfile" "$total"; then
            local i
            for (( i = 0; i < GN; i++ )); do
                RESULTS+=("${G_NAMES[i]}:PASS")
            done
            echo "PASS group ${label} (${#flows[@]} flows, $(( t1 - t0 ))s including the ${DF_LEASE_GAP}s lease gap)"
            return 0
        fi

        # --- failure ---
        local failed_flow="$PG_FAIL_FLOW"
        _df_log "group ${label} attempt ${attempt}/3 FAILED at flow '${failed_flow}':"
        printf '       command: %s\n' "${PG_FAIL_CMD:-unknown}" >&2
        printf '%s\n' "${PG_FAIL_OUT:-}" | head -20 | sed 's/^/       /' >&2
        if (( attempt == 3 )); then
            group_ok=0
            break
        fi
        if [[ "${PG_FAIL_OUT:-}" == *"Another DevFlow session"* ]]; then
            _df_log "group ${label}: driving-lease conflict; another session still holds the lease — waiting and retrying"
            sleep "$DF_LEASE_GAP"
        elif (( attempt == 1 )); then
            # The batch session's mutation lease is still live right after the
            # session ends (TTL ~10s from its last mutation), and the recovery
            # pass is single-CLI mutations — wait it out first, or the recovery
            # taps will burn their retries on conflicts.
            _df_log "group ${label}: waiting ${DF_LEASE_GAP}s for the batch session's lease, then recovery pass"
            sleep "$DF_LEASE_GAP"
            reset_to NavigationTab || _df_log "group ${label}: recovery pass reported problems (continuing; retry may still fail)"
        else
            _df_log "group ${label}: recovery did not help — relaunching the app before the final attempt"
            relaunch_app || _df_log "group ${label}: relaunch reported problems (continuing; final attempt may fail)"
        fi
        attempt=$(( attempt + 1 ))
    done

    # Record per-flow results for the failed group: flows before the failing
    # one PASSED (they executed cleanly); the failing one FAILED; the rest SKIPPED.
    # If assembly itself failed (empty PG_FAIL_FLOW), nothing executed: all SKIP.
    local i done_at=0
    for (( i = 0; i < ${#flows[@]}; i++ )); do
        local name="${flows[i]}"
        if [[ -z "$PG_FAIL_FLOW" ]]; then
            RESULTS+=("${name}:SKIP")
        elif [[ "$name" == "$PG_FAIL_FLOW" ]]; then
            RESULTS+=("${name}:FAIL")
            done_at=1
        elif (( done_at )); then
            RESULTS+=("${name}:SKIP")
        else
            RESULTS+=("${name}:PASS")
        fi
    done
    SUITE_FAILED=1
    if [[ -z "$PG_FAIL_FLOW" ]]; then
        echo "FAIL group ${label}: group assembly/declaration failed (harness error — see log above)"
    else
        echo "FAIL group ${label}: flow '${PG_FAIL_FLOW}' failed after 3 attempts (see log above); later flows skipped"
    fi
    return 1
}

RUN_PREV_END="NavigationTab"
for gi in 0 1 2 3; do
    label="${GROUP_LABELS[gi]}"
    case "$gi" in
        0) flows=( "${GROUP_FLOWS_0[@]}" ) ;;
        1) flows=( "${GROUP_FLOWS_1[@]}" ) ;;
        2) flows=( "${GROUP_FLOWS_2[@]}" ) ;;
        3) flows=( "${GROUP_FLOWS_3[@]}" ) ;;
    esac
    if ! run_group "$label" "${flows[@]}"; then
        # Fail-fast at the group level: a broken app state would poison the
        # remaining groups. (Per-flow results up to here are still reported.)
        break
    fi
    # Track the tab this group ended on for the next group's switch logic.
    last_flow="${flows[${#flows[@]}-1]}"
    RUN_PREV_END="$(flow_decl "$last_flow" | sed -n 's/^END\t//p' | head -1)"
    [[ -n "$RUN_PREV_END" ]] || RUN_PREV_END="NavigationTab"
done

# --- 6. teardown ------------------------------------------------------------------
echo
echo "== 6/6 teardown =="
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

# --- report ------------------------------------------------------------------------
echo
total=22
pass=0; fail=0; skip=0
for r in "${RESULTS[@]}"; do
    name="${r%%:*}"; status="${r##*:}"
    case "$status" in
        PASS) printf 'PASS  %s\n' "$name"; pass=$(( pass + 1 )) ;;
        FAIL) printf 'FAIL  %s\n' "$name"; fail=$(( fail + 1 )) ;;
        SKIP) printf 'SKIP  %s\n' "$name"; skip=$(( skip + 1 )) ;;
    esac
done
if (( skip == 0 && fail == 0 )); then
    echo
    echo "RESULT: ${pass}/${total} passed"
    exit 0
fi
echo
echo "RESULT: ${pass}/${total} passed, ${fail} failed, ${skip} skipped"
exit 1
