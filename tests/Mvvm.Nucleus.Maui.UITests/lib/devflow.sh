#!/usr/bin/env bash
#
# lib/devflow.sh — shared DevFlow driver helpers for the Mvvm.Nucleus.Maui UI
# regression suite (tests/Mvvm.Nucleus.Maui.UITests/). Sourced by run-all.sh and every flow script.
#
# TIMING MODEL
# ------------
# Two independent guards bound how fast mutations can be issued:
#
#  1. DevFlow MUTATION LEASE — app-wide, per-SESSION.
#     - A mutating call (tap/fill/navigate/alert-dismiss) claims an app-wide
#       lease for the CALLING SESSION. Each mutation refreshes it.
#     - The lease expires ~10s after the last mutating call by its holder
#       (6s after a mutation still conflicts; 10s is clean).
#     - Each `maui devflow <cmd>` process is its own session, so two separate
#       CLI processes issuing mutations <10s apart conflict with
#       "Another DevFlow session is driving this app (MAUI CLI)".
#     - `maui devflow batch` (one long-lived process) is ONE session: its
#       mutations refresh the lease for itself, so a whole group of flows can
#       run back-to-back with NO inter-step lease wait. Only between two
#       separate batch processes (or single-CLI mutations) must we wait for
#       the previous holder's lease to expire (~10s).
#
#  2. LIBRARY NAVIGATION DEBOUNCE (app-side, NucleusMvvmCore) —
#     `IgnoreNavigationWithinMilliseconds` (250ms) +
#     `IgnoreNavigationWhenInProgress`. A navigation request arriving <250ms
#     after the previous one is IGNORED, and (see NavigationService.
#     SetIsNavigatingOrIgnoreRequest) it can leave IsNavigating stuck True,
#     rejecting ALL subsequent navigation until the app restarts. So the
#       driver must keep >=250ms between consecutive navigation-inducing taps.
#     We use DF_BATCH_DELAY (default 300ms) between every pair of batch
#       commands as a uniform, safe floor (non-navigation mutations are not
#       debounced, so this only costs us a little time).
#
# CONSEQUENCES FOR THE DRIVER
# ---------------------------
#  - Flows within one group are assembled into ONE `maui devflow batch`
#    invocation (one session). Mutations refresh the lease; no per-step wait.
#  - Between two groups (two batch processes) we sleep DF_LEASE_GAP (default
#    10s) so the previous session's lease has expired before the next
#    session's first mutation.
#  - The pre-suite recovery pass (reset_to / stray-dialog / popup / modal
#    cleanup) uses single-CLI calls with conflict-driven retry (poll, not a
#    blind sleep), since it is a rare path and may need several mutations.
#
# Catalyst native-dialog behavior:
#   - `ui alert detect`/`ui alert dismiss` report false for Catalyst native NSAlerts,
#     but the alert's buttons ARE registered as native `DialogAction` elements and
#     `ui tap <id>` dismisses them reliably. The alert message BODY is not queryable
#     (flows assert on the button set / follow-up state instead).
#   - Prompts: buttons are native "OK"/"Cancel" DialogActions; the prompt text field
#     is a native `UITextField` (role "dialog") fillable via `ui fill --type UITextField`.
#
# Tunables (overridable via environment):
#   DF_BATCH_DELAY      ms between two batch commands (default 300; must be > the
#                       library's 250ms navigation debounce)
#   DF_LEASE_GAP        seconds to wait between two separate batch/single-CLI
#                       mutating sessions (default 10; the lease TTL)
#   DF_WAIT_TIMEOUT     default wait-until timeout in seconds (default 10)
#   DF_MAX_ATTEMPTS     attempts per single-CLI mutating call (default 4)
#   DF_RETRY_WAIT_SECS  wait before retrying a single-CLI lease conflict (default 3)
#   DF_SKIP_BUILD       set to 1 to skip the dotnet build (dev iteration only)
DF_BATCH_DELAY="${DF_BATCH_DELAY:-300}"
DF_LEASE_GAP="${DF_LEASE_GAP:-10}"
DF_WAIT_TIMEOUT="${DF_WAIT_TIMEOUT:-10}"
DF_MAX_ATTEMPTS="${DF_MAX_ATTEMPTS:-4}"
DF_RETRY_WAIT_SECS="${DF_RETRY_WAIT_SECS:-3}"

# --- diagnostics ---------------------------------------------------------------

_df_log()  { printf '[devflow] %s\n' "$*" >&2; }

_df_fail() {
    local msg="$1"; shift
    printf '[FAIL] %s\n' "$msg" >&2
    while IFS= read -r line; do
        printf '       %s\n' "$line" >&2
    done <<< "$*"
    return 1
}

# --- single-CLI (recovery/setup path) ------------------------------------------
#
# df <step> <devflow subcommand + args...>
# Runs `maui devflow <args> --ci --json`, applying the recovery rules for
# single-CLI calls (used by the pre-suite recovery pass, not the fast path):
#   - driving-lease conflict -> wait + retry (poll, up to DF_MAX_ATTEMPTS)
#   - broker lease authority -> broker stop/start + agent wait + retry
#   - stale element/epoch    -> `ui tree` refresh + retry
#   - success:false          -> reactivate app window + retry
#   - anything else          -> fail with the step name
# stdout carries the command's JSON output; diagnostics go to stderr.
df() {
    local step="${1:-devflow call}"; shift
    local attempt=1 out rc recoverable
    while :; do
        if out="$(maui devflow "$@" --ci --json 2>&1)"; then
            printf '%s\n' "$out"
            return 0
        fi
        rc=$?
        recoverable=0
        case "$out" in
            *"Another DevFlow session"*)
                _df_log "${step}: driving-lease conflict (attempt ${attempt}/${DF_MAX_ATTEMPTS}); waiting ${DF_RETRY_WAIT_SECS}s"
                sleep "$DF_RETRY_WAIT_SECS"
                recoverable=1
                ;;
            *"lease authority"*)
                _df_log "${step}: broker lease authority unavailable (attempt ${attempt}/${DF_MAX_ATTEMPTS}); restarting broker"
                maui devflow broker stop >/dev/null 2>&1
                maui devflow broker start >/dev/null 2>&1
                maui devflow wait --timeout 30 >/dev/null 2>&1
                recoverable=1
                ;;
            *"stale"* | *"epoch"* | *"registry"*)
                _df_log "${step}: stale element/epoch suspected (attempt ${attempt}/${DF_MAX_ATTEMPTS}); refreshing tree"
                maui devflow ui tree --depth 1 --ci --json >/dev/null 2>&1 || true
                sleep "$DF_RETRY_WAIT_SECS"
                recoverable=1
                ;;
            *'"success":false'*)
                _df_log "${step}: action reported success:false (attempt ${attempt}/${DF_MAX_ATTEMPTS}); reactivating app window"
                osascript -e 'tell application "Mvvm.Nucleus.Maui.Sample" to activate' >/dev/null 2>&1 || true
                sleep 2
                recoverable=1
                ;;
        esac
        if (( recoverable == 0 || attempt >= DF_MAX_ATTEMPTS )); then
            _df_fail "${step}: 'maui devflow $*' failed (attempt ${attempt}/${DF_MAX_ATTEMPTS})" "$out"
            return 1
        fi
        attempt=$(( attempt + 1 ))
    done
}

# --- broker / agent ------------------------------------------------------------

ensure_broker() {
    local out running
    out="$(maui devflow broker status --json 2>&1)" || true
    running="$(printf '%s' "$out" | jq -r '.running // false' 2>/dev/null)" || running="false"
    if [[ "$running" != "true" ]]; then
        _df_log "broker not running; starting"
        maui devflow broker start || return 1
    fi
    return 0
}

wait_agent() {
    local timeout="${1:-60}"
    local deadline=$(( $(date +%s) + timeout ))
    while :; do
        if maui devflow wait --timeout 5 --json >/dev/null 2>&1; then
            return 0
        fi
        if (( $(date +%s) >= deadline )); then
            _df_fail "wait_agent: no DevFlow agent registered within ${timeout}s"
            return 1
        fi
    done
}

# --- recovery pass (pre-suite, single-CLI, rare path) --------------------------

_dismiss_stray_dialogs() {
    local actions count label fields i
    for i in 1 2 3; do
        actions="$(df "stray-dialog scan" ui query --type DialogAction --json)" || actions="[]"
        count="$(printf '%s' "$actions" | jq -r 'if type == "array" then length else 0 end' 2>/dev/null)" || count=0
        [[ "$count" =~ ^[0-9]+$ ]] || count=0
        [[ "$count" == "0" ]] && return 0
        fields="$(df "stray-field scan" ui query --type UITextField --json)" || fields="[]"
        if [[ "$(printf '%s' "$fields" | jq -r 'if type == "array" then length else 0 end' 2>/dev/null)" != "0" || count -gt 1 ]]; then
            tap_cmd "Cancel" || _df_log "stray dialog: 'Cancel' not found"
        else
            label="$(printf '%s' "$actions" | jq -r 'if type == "array" and length > 0 then .[0].text else "" end' 2>/dev/null)"
            tap_cmd "$label" || _df_log "stray dialog: could not tap '${label}'"
        fi
    done
    return 0
}

_current_route() {
    local out
    out="$(maui devflow agent status --json 2>/dev/null)" || out=""
    printf '%s' "$out" | jq -r '.route // ""' 2>/dev/null
}

_tap_tab() {
    local tab="$1"
    if [[ "$(_current_route)" == "//Tabs/${tab}" ]]; then
        _df_log "tab switch to ${tab}: already here; no-op tap skipped"
        return 0
    fi
    if df "tab switch to ${tab}" ui tap "Tab_${tab}" >/dev/null; then
        return 0
    fi
    if [[ "$(_current_route)" == "//Tabs/${tab}" ]]; then
        _df_log "tab switch to ${tab}: tap reported not-OK, but the route confirms we are here"
        return 0
    fi
    return 1
}

# A single-CLI mutating tap (recovery path; lease-aware via df).
tap_cmd() {
    local text="$1" id
    id="$(df "query '${text}'" ui query --text "$text" --json | jq -r 'if type == "array" and length > 0 then .[-1].id else "" end' 2>/dev/null)"
    if [[ -z "$id" ]]; then
        return 1
    fi
    df "tap '${text}'" ui tap "$id" >/dev/null
}

# reset_to <tab> — RECOVERY PASS: land on a known, healthy tab using the app's
# own UI (single-CLI; the rare pre-suite path). In flow scripts this is now a
# pure DECLARATION of the flow's required start tab (see the emitter below);
# the actual recovery is done once, before the suite, by run-all.sh calling
# this function.
reset_to() {
    local tab="${1:?reset_to: tab name required}"
    local i current
    for i in 1 2 3 4; do
        _dismiss_stray_dialogs
        if [[ -n "$(df "q" ui query --text "Close using IPopupService" --json | jq -r 'if type=="array" and length>0 then .[-1].id else "" end' 2>/dev/null)" ]]; then
            tap_cmd "Close using IPopupService"; sleep 2; continue
        fi
        if [[ -n "$(df "q" ui query --text "Close Popup" --json | jq -r 'if type=="array" and length>0 then .[-1].id else "" end' 2>/dev/null)" ]]; then
            tap_cmd "Close Popup"; sleep 2; continue
        fi
        current="$(_current_route)"
        if [[ "$current" == "//Tabs/"*/* ]]; then
            if [[ -n "$(df "q" ui query --text "Navigate Backwards" --json | jq -r 'if type=="array" and length>0 then .[0].id else "" end' 2>/dev/null)" ]]; then
                tap_cmd "Navigate Backwards"; sleep 2; continue
            fi
        fi
        if [[ -n "$(df "q" ui query --text "Close All Modals (if any)" --json | jq -r 'if type=="array" and length>0 then .[-1].id else "" end' 2>/dev/null)" ]]; then
            tap_cmd "Close All Modals (if any)"; sleep 2; continue
        fi
        break
    done
    _dismiss_stray_dialogs
    current="$(_current_route)"
    if [[ "$current" != "//Tabs/${tab}" ]]; then
        _tap_tab "$tab"
    fi
    return 0
}

# --- FLOW EMITTER (fast path) --------------------------------------------------
#
# Flows are assembled into a static list of `MAUI ...` commands (one per line)
# that is executed by a single `maui devflow batch` invocation (one session,
# one lease holder). The helpers below APPEND to the global DF_CMDS array
# instead of executing, so a flow script is a pure declaration.
#
# q <s> — quote a string for the batch line parser (double quotes).
q() { printf '"%s"' "$1"; }

# _emit <line> — append one batch command line.
_emit() { DF_CMDS+=("$1"); }

# tap_text <label> — tap a page Button whose text contains the label.
# `--type Button` is REQUIRED: the CLI's `--text` match is a SUBSTRING match
# (e.g. `query --text "Nav"` matches every Navigation* button), so the type
# filter keeps page-button taps out of the dialog-button space.
tap_text() {
    _emit "MAUI tap --text $(q "$1") --type Button"
    return 0
}
tap_text_first() { tap_text "$@"; }

# wait_text <text> — wait until an element with the exact text exists.
wait_text() {
    _emit "MAUI query --text $(q "$1") --wait-until exists --timeout ${DF_WAIT_TIMEOUT}"
    return 0
}
assert_text() { wait_text "$1"; }

# wait_dialog <button_label> — wait until a native DialogAction with the label
# is registered (the alert/prompt/action sheet is up).
# `--type DialogAction` is REQUIRED: `--text` is a substring match, so a bare
# `query --text "Confirm"` matches the PAGE's "Display Confirm" button and
# returns success before the dialog is actually up. Scoping to DialogAction
# makes the wait only resolve once the native alert/prompt button is registered.
wait_dialog() {
    _emit "MAUI query --type DialogAction --text $(q "$1") --wait-until exists --timeout ${DF_WAIT_TIMEOUT}"
    return 0
}
assert_dialog() { wait_dialog "$1"; }

# dismiss_dialog <button_label> — tap the dialog's button (registered as a
# native DialogAction element on Catalyst). `--type DialogAction` is REQUIRED:
# `--text` is a substring match and a dialog label can collide with a page
# button (e.g. `tap --text "Confirm"` would hit the page's "Display Confirm"
# button instead of the alert's "Confirm"). The type+text combination resolves
# within DialogActions.
dismiss_dialog() {
    _emit "MAUI tap --text $(q "$1") --type DialogAction"
    return 0
}

# enter_prompt <value> [ok_label] — fill the prompt's text field and confirm.
enter_prompt() {
    local value="$1" ok_label="${2:-OK}"
    _emit "MAUI query --type UITextField --wait-until exists --timeout ${DF_WAIT_TIMEOUT}"
    _emit "MAUI fill --type UITextField $(q "$value")"
    _emit "MAUI tap --text $(q "$ok_label") --type DialogAction"
    return 0
}

# assert_absent <text> — wait until no element with the exact text exists.
assert_absent() {
    _emit "MAUI query --text $(q "$1") --wait-until gone --timeout ${DF_WAIT_TIMEOUT}"
    return 0
}

# flow_begin <tab> — declare the tab this flow must start on (the assembler
# inserts a tab switch if the previous flow ended elsewhere).
flow_begin() {
    FLOW_START_TAB="${1:?flow_begin: tab required}"
    return 0
}
# flow_end <tab> — declare the tab this flow ends on (default: FLOW_START_TAB).
flow_end() {
    FLOW_END_TAB="${1:-$FLOW_START_TAB}"
    return 0
}

# --- batch runner ---------------------------------------------------------------

# batch_run <cmdfile> — execute the commands in <cmdfile> (one per line) in ONE
# `maui devflow batch` session. JSONL responses go to stdout. Returns 0 iff
# every command exited 0 (the batch is fail-fast: it stops at the first
# non-zero command, which is then the last JSONL line).
batch_run() {
    local cmdfile="$1"
    maui devflow batch --delay "$DF_BATCH_DELAY" < "$cmdfile"
}
